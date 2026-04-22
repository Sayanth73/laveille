using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using LaVeillee.Core;
using LaVeillee.Networking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LaVeillee.UI
{
    /// Story 2.4/2.5/2.6/2.7 — Lobby pré-partie (panel).
    /// Attend que la LobbyState scene-placed soit spawnée par Fusion (après room join),
    /// puis push l'identité locale et rend la liste + contrôles host.
    public class LobbyScreen : ScreenBase
    {
        // UI refs
        TextMeshProUGUI _codeText;
        TextMeshProUGUI _counterText;
        TextMeshProUGUI _pausedOverlayText;
        TextMeshProUGUI _waitingText;
        GameObject _pausedOverlay;
        GameObject _waitingOverlay;
        RectTransform _playerListContainer;
        Button _shareBtn;
        Button _copyBtn;
        Button _quitBtn;
        Button _pauseBtn;
        Button _startBtn;
        Button _composeBtn;
        Button _timersBtn;

        LobbyState _lobby;
        FusionRoomManager _room;
        bool _pseudoPushed;

        readonly Dictionary<int, GameObject> _playerRows = new();

        protected override void BuildUI()
        {
            UIFactory.CreateScreenCanvas("LobbyCanvas", out var canvasGo);
            canvasGo.transform.SetParent(transform, false);
            // Fond transparent + scrim pour laisser voir la scène 3D (village nocturne) derrière.
            Root = UIFactory.CreateFullscreen(canvasGo.transform, "LobbyRoot", new Color(0f, 0f, 0f, 0f));
            var scrim = UIFactory.CreateFullscreen(Root, "Scrim", new Color(0.043f, 0.070f, 0.141f, 0.55f));
            scrim.GetComponent<UnityEngine.UI.Image>().raycastTarget = false;

            BuildHeader();
            BuildCodeSection();
            BuildPlayerList();
            BuildHostControls();
            BuildPausedOverlay();
            BuildWaitingOverlay();
        }

        void BuildHeader()
        {
            var header = UIFactory.CreatePanel(Root, "Header", new Color(0, 0, 0, 0));
            header.anchorMin = new Vector2(0f, 1f);
            header.anchorMax = new Vector2(1f, 1f);
            header.pivot = new Vector2(0.5f, 1f);
            header.sizeDelta = new Vector2(0f, 120f);
            header.anchoredPosition = new Vector2(0f, 0f);

            var title = UIFactory.CreateText(header, "La Veillée", UIFactory.TextStyle.H1,
                DesignTokens.Colors.Fire500, TextAlignmentOptions.Center);
            title.rectTransform.anchorMin = Vector2.zero;
            title.rectTransform.anchorMax = Vector2.one;
            title.rectTransform.offsetMin = Vector2.zero;
            title.rectTransform.offsetMax = Vector2.zero;

            _quitBtn = UIFactory.CreateButton(header, "Quitter", UIFactory.ButtonStyle.Ghost,
                new Vector2(180f, 72f));
            var qrt = _quitBtn.GetComponent<RectTransform>();
            qrt.anchorMin = new Vector2(1f, 0.5f);
            qrt.anchorMax = new Vector2(1f, 0.5f);
            qrt.pivot = new Vector2(1f, 0.5f);
            qrt.anchoredPosition = new Vector2(-DesignTokens.Spacing.Lg, 0f);
            _quitBtn.onClick.AddListener(OnQuit);

            _pauseBtn = UIFactory.CreateButton(header, "Pause", UIFactory.ButtonStyle.Ghost,
                new Vector2(180f, 72f));
            var prt = _pauseBtn.GetComponent<RectTransform>();
            prt.anchorMin = new Vector2(0f, 0.5f);
            prt.anchorMax = new Vector2(0f, 0.5f);
            prt.pivot = new Vector2(0f, 0.5f);
            prt.anchoredPosition = new Vector2(DesignTokens.Spacing.Lg, 0f);
            _pauseBtn.onClick.AddListener(OnPauseToggle);
        }

        void BuildCodeSection()
        {
            var panel = UIFactory.CreatePanel(Root, "CodePanel", DesignTokens.Colors.Night700);
            panel.anchorMin = new Vector2(0.5f, 1f);
            panel.anchorMax = new Vector2(0.5f, 1f);
            panel.pivot = new Vector2(0.5f, 1f);
            panel.anchoredPosition = new Vector2(0f, -160f);
            panel.sizeDelta = new Vector2(960f, 220f);

            var label = UIFactory.CreateText(panel, "Code de partie", UIFactory.TextStyle.Caption,
                DesignTokens.Colors.Moon300);
            label.rectTransform.anchorMin = new Vector2(0f, 0.7f);
            label.rectTransform.anchorMax = new Vector2(1f, 1f);
            label.rectTransform.offsetMin = new Vector2(24f, 0f);
            label.rectTransform.offsetMax = new Vector2(-24f, 0f);

            _codeText = UIFactory.CreateText(panel, "------", UIFactory.TextStyle.Display,
                DesignTokens.Colors.Fire500);
            _codeText.fontSize = 80f;
            _codeText.characterSpacing = 12f;
            _codeText.rectTransform.anchorMin = new Vector2(0f, 0.2f);
            _codeText.rectTransform.anchorMax = new Vector2(0.55f, 0.8f);
            _codeText.rectTransform.offsetMin = new Vector2(24f, 0f);
            _codeText.rectTransform.offsetMax = Vector2.zero;

            _copyBtn = UIFactory.CreateButton(panel, "📋 Copier", UIFactory.ButtonStyle.Secondary,
                new Vector2(240f, 88f));
            var crt = _copyBtn.GetComponent<RectTransform>();
            crt.anchorMin = new Vector2(0.6f, 0.55f);
            crt.anchorMax = new Vector2(0.6f, 0.55f);
            crt.pivot = new Vector2(0f, 0.5f);
            crt.anchoredPosition = Vector2.zero;
            _copyBtn.onClick.AddListener(OnCopyCode);

            _shareBtn = UIFactory.CreateButton(panel, "🔗 Partager", UIFactory.ButtonStyle.Primary,
                new Vector2(240f, 88f));
            var srt = _shareBtn.GetComponent<RectTransform>();
            srt.anchorMin = new Vector2(0.6f, 0.15f);
            srt.anchorMax = new Vector2(0.6f, 0.15f);
            srt.pivot = new Vector2(0f, 0.5f);
            srt.anchoredPosition = Vector2.zero;
            _shareBtn.onClick.AddListener(OnShareLink);
        }

        void BuildPlayerList()
        {
            var panel = UIFactory.CreatePanel(Root, "PlayersPanel", DesignTokens.Colors.Night700);
            panel.anchorMin = new Vector2(0.5f, 0.5f);
            panel.anchorMax = new Vector2(0.5f, 0.5f);
            panel.pivot = new Vector2(0.5f, 0.5f);
            panel.anchoredPosition = new Vector2(0f, 20f);
            panel.sizeDelta = new Vector2(960f, 780f);

            _counterText = UIFactory.CreateText(panel, "0 / 25 joueurs", UIFactory.TextStyle.H3,
                DesignTokens.Colors.Moon300, TextAlignmentOptions.Left);
            _counterText.rectTransform.anchorMin = new Vector2(0f, 1f);
            _counterText.rectTransform.anchorMax = new Vector2(1f, 1f);
            _counterText.rectTransform.pivot = new Vector2(0f, 1f);
            _counterText.rectTransform.anchoredPosition = new Vector2(24f, -20f);
            _counterText.rectTransform.sizeDelta = new Vector2(-48f, 48f);

            var listContainerGo = new GameObject("PlayerList", typeof(RectTransform), typeof(VerticalLayoutGroup));
            listContainerGo.transform.SetParent(panel, false);
            _playerListContainer = listContainerGo.GetComponent<RectTransform>();
            _playerListContainer.anchorMin = new Vector2(0f, 0f);
            _playerListContainer.anchorMax = new Vector2(1f, 1f);
            _playerListContainer.offsetMin = new Vector2(24f, 24f);
            _playerListContainer.offsetMax = new Vector2(-24f, -80f);

            var vlg = listContainerGo.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = DesignTokens.Spacing.Sm;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childControlHeight = false;
            vlg.childControlWidth = true;
        }

        void BuildHostControls()
        {
            var panel = UIFactory.CreatePanel(Root, "HostControls", new Color(0, 0, 0, 0));
            panel.anchorMin = new Vector2(0.5f, 0f);
            panel.anchorMax = new Vector2(0.5f, 0f);
            panel.pivot = new Vector2(0.5f, 0f);
            panel.anchoredPosition = new Vector2(0f, 60f);
            panel.sizeDelta = new Vector2(960f, 280f);

            _composeBtn = UIFactory.CreateButton(panel, "Configurer les rôles",
                UIFactory.ButtonStyle.Secondary, new Vector2(960f, 84f));
            SetAnchorTop(_composeBtn.GetComponent<RectTransform>(), Vector2.zero);
            _composeBtn.onClick.AddListener(() => CompositionDialog.Open(_lobby, _room, ActivePlayerCount));

            _timersBtn = UIFactory.CreateButton(panel, "Timers & mode",
                UIFactory.ButtonStyle.Secondary, new Vector2(960f, 84f));
            SetAnchorTop(_timersBtn.GetComponent<RectTransform>(), new Vector2(0f, -98f));
            _timersBtn.onClick.AddListener(() => TimersDialog.Open(_lobby));

            _startBtn = UIFactory.CreateButton(panel, "Démarrer la partie",
                UIFactory.ButtonStyle.Primary, new Vector2(960f, 110f));
            var srt = _startBtn.GetComponent<RectTransform>();
            srt.anchorMin = new Vector2(0.5f, 0f);
            srt.anchorMax = new Vector2(0.5f, 0f);
            srt.pivot = new Vector2(0.5f, 0f);
            srt.anchoredPosition = Vector2.zero;
            _startBtn.onClick.AddListener(OnStartGame);
        }

        static void SetAnchorTop(RectTransform rt, Vector2 pos)
        {
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = pos;
        }

        void BuildPausedOverlay()
        {
            _pausedOverlay = UIFactory.CreatePanel(Root, "PausedOverlay",
                new Color(0f, 0f, 0f, 0.85f)).gameObject;
            var prt = _pausedOverlay.GetComponent<RectTransform>();
            prt.anchorMin = Vector2.zero;
            prt.anchorMax = Vector2.one;
            prt.offsetMin = Vector2.zero;
            prt.offsetMax = Vector2.zero;
            _pausedOverlay.SetActive(false);

            _pausedOverlayText = UIFactory.CreateText(_pausedOverlay.transform,
                "⏸ Partie en pause", UIFactory.TextStyle.H1, DesignTokens.Colors.Moon100);
            var trt = _pausedOverlayText.rectTransform;
            trt.anchorMin = new Vector2(0f, 0.5f);
            trt.anchorMax = new Vector2(1f, 0.7f);
            trt.offsetMin = Vector2.zero;
            trt.offsetMax = Vector2.zero;

            var resume = UIFactory.CreateButton(_pausedOverlay.transform, "Reprendre",
                UIFactory.ButtonStyle.Primary, new Vector2(480f, 96f));
            var rrt = resume.GetComponent<RectTransform>();
            rrt.anchorMin = new Vector2(0.5f, 0.3f);
            rrt.anchorMax = new Vector2(0.5f, 0.3f);
            rrt.pivot = new Vector2(0.5f, 0.5f);
            rrt.anchoredPosition = Vector2.zero;
            resume.onClick.AddListener(OnPauseToggle);
        }

        void BuildWaitingOverlay()
        {
            _waitingOverlay = UIFactory.CreatePanel(Root, "WaitingOverlay",
                DesignTokens.Colors.Night900).gameObject;
            var rt = _waitingOverlay.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            _waitingText = UIFactory.CreateText(_waitingOverlay.transform, "Synchronisation du lobby…",
                UIFactory.TextStyle.H1, DesignTokens.Colors.Moon100);
            var trt = _waitingText.rectTransform;
            trt.anchorMin = new Vector2(0f, 0.4f);
            trt.anchorMax = new Vector2(1f, 0.6f);
            trt.offsetMin = Vector2.zero;
            trt.offsetMax = Vector2.zero;
            _waitingOverlay.SetActive(true);
        }

        // --- Lifecycle ------------------------------------------------------

        public override void OnShow()
        {
            _room = GameServices.EnsureExists().Room;
            _pseudoPushed = false;
            _waitingOverlay.SetActive(true);
            if (_room != null) _room.RoomLeft += OnRoomLeft;
            StartCoroutine(WaitForLobbyState());
        }

        public override void OnHide()
        {
            if (_room != null) _room.RoomLeft -= OnRoomLeft;
            _lobby = null;
        }

        IEnumerator WaitForLobbyState()
        {
            float timeout = 10f;
            while (timeout > 0f)
            {
                _lobby = FindFirstObjectByType<LobbyState>();
                if (_lobby != null && _lobby.Object != null && _lobby.Object.IsValid) break;
                yield return null;
                timeout -= Time.deltaTime;
            }
            if (_lobby == null)
            {
                _waitingText.text = "Impossible de synchroniser le lobby — réessaie.";
                Debug.LogError("[LobbyScreen] LobbyState non trouvé — scene-placed manquant ?");
                yield break;
            }
            _waitingOverlay.SetActive(false);
        }

        // --- Polling --------------------------------------------------------

        float _nextDiagLog;

        void Update()
        {
            if (Time.unscaledTime >= _nextDiagLog)
            {
                _nextDiagLog = Time.unscaledTime + 1f;
                var runner = _room?.Runner;
                int active = runner?.ActivePlayers?.Count() ?? -1;
                Debug.Log($"[LobbyScreen][DIAG] lobby={(_lobby != null)} lobbyObj={(_lobby?.Object != null)} lobbyValid={(_lobby?.Object != null && _lobby.Object.IsValid)} room={(_room != null)} runner={(runner != null)} running={(runner?.IsRunning == true)} localId={runner?.LocalPlayer.PlayerId} sessionName={_room?.CurrentRoomId} activePlayers={active}");
            }

            if (_room?.Runner == null) return;

            // RefreshCode ne dépend que de la session — on le laisse tourner même si
            // LobbyState n'est pas encore propagé par Fusion.
            RefreshCode();

            // Rpc_RegisterPlayer + reste exigent un NetworkBehaviour initialisé. Si
            // FindFirstObjectByType a trouvé une instance scene-placed dont le
            // NetworkObject n'est pas encore bound, on skip proprement.
            bool lobbyReady = _lobby != null && _lobby.Object != null && _lobby.Object.IsValid;
            if (!lobbyReady)
            {
                // Re-try find au cas où Fusion a respawn / réattaché l'instance.
                _lobby = FindFirstObjectByType<LobbyState>();
                lobbyReady = _lobby != null && _lobby.Object != null && _lobby.Object.IsValid;
                if (!lobbyReady) return;
            }

            if (!_pseudoPushed)
            {
                var id = PlayerIdentityService.Current;
                _lobby.Rpc_RegisterPlayer(_room.Runner.LocalPlayer, id.Pseudo, id.AvatarColorSeed);
                _pseudoPushed = true;
            }

            RefreshPlayers();
            RefreshPaused();
            RefreshHostControls();
            CheckCountdown();
        }

        void RefreshCode()
        {
            var code = _room?.CurrentRoomId ?? "------";
            if (_codeText.text != code) _codeText.text = code;
        }

        int ActivePlayerCount => _room?.Runner?.ActivePlayers?.Count() ?? 0;

        bool IsLocalHost()
        {
            if (_lobby == null || _room?.Runner == null) return false;
            return _lobby.HostPlayerRef == _room.Runner.LocalPlayer;
        }

        void RefreshPlayers()
        {
            var activeIds = new HashSet<int>();
            foreach (var p in _room.Runner.ActivePlayers)
                activeIds.Add(p.PlayerId);

            _counterText.text = $"{activeIds.Count} / 25 joueurs";

            var toRemove = new List<int>();
            foreach (var kv in _playerRows)
                if (!activeIds.Contains(kv.Key)) toRemove.Add(kv.Key);
            foreach (var id in toRemove)
            {
                Destroy(_playerRows[id]);
                _playerRows.Remove(id);
                if (_lobby.HasStateAuthority) _lobby.ClearPlayerSlot(id);
            }

            foreach (var p in _room.Runner.ActivePlayers)
            {
                var row = _playerRows.TryGetValue(p.PlayerId, out var existing)
                    ? existing
                    : CreatePlayerRow(p);
                UpdatePlayerRow(row, p);
            }

            if (_lobby.HasStateAuthority)
            {
                bool currentHostStillActive = activeIds.Contains(_lobby.HostPlayerRef.PlayerId);
                if (!currentHostStillActive)
                {
                    int lowest = int.MaxValue;
                    PlayerRef newHost = default;
                    foreach (var p in _room.Runner.ActivePlayers)
                        if (p.PlayerId < lowest) { lowest = p.PlayerId; newHost = p; }
                    if (lowest != int.MaxValue) _lobby.PromoteHost(newHost);
                }
            }
        }

        GameObject CreatePlayerRow(PlayerRef player)
        {
            var row = UIFactory.CreatePanel(_playerListContainer, $"Row_{player.PlayerId}",
                DesignTokens.Colors.Night900);
            row.sizeDelta = new Vector2(0f, 96f);
            var le = row.gameObject.AddComponent<LayoutElement>();
            le.preferredHeight = 96f;

            var avatar = UIFactory.CreateAvatar(row, "?", player.PlayerId, 80f);
            avatar.anchorMin = new Vector2(0f, 0.5f);
            avatar.anchorMax = new Vector2(0f, 0.5f);
            avatar.pivot = new Vector2(0f, 0.5f);
            avatar.anchoredPosition = new Vector2(12f, 0f);

            var pseudoText = UIFactory.CreateText(row, "?", UIFactory.TextStyle.H3,
                DesignTokens.Colors.Moon100, TextAlignmentOptions.MidlineLeft);
            pseudoText.name = "Pseudo";
            pseudoText.rectTransform.anchorMin = new Vector2(0f, 0f);
            pseudoText.rectTransform.anchorMax = new Vector2(1f, 1f);
            pseudoText.rectTransform.offsetMin = new Vector2(108f, 8f);
            pseudoText.rectTransform.offsetMax = new Vector2(-260f, -8f);

            var hostBadge = UIFactory.CreateText(row, "HOST", UIFactory.TextStyle.Label,
                DesignTokens.Colors.Gold500, TextAlignmentOptions.Center);
            hostBadge.name = "HostBadge";
            hostBadge.rectTransform.anchorMin = new Vector2(1f, 0.5f);
            hostBadge.rectTransform.anchorMax = new Vector2(1f, 0.5f);
            hostBadge.rectTransform.pivot = new Vector2(1f, 0.5f);
            hostBadge.rectTransform.anchoredPosition = new Vector2(-140f, 0f);
            hostBadge.rectTransform.sizeDelta = new Vector2(96f, 40f);

            var kick = UIFactory.CreateButton(row, "Kick", UIFactory.ButtonStyle.Danger,
                new Vector2(120f, 64f));
            kick.name = "Kick";
            var krt = kick.GetComponent<RectTransform>();
            krt.anchorMin = new Vector2(1f, 0.5f);
            krt.anchorMax = new Vector2(1f, 0.5f);
            krt.pivot = new Vector2(1f, 0.5f);
            krt.anchoredPosition = new Vector2(-12f, 0f);
            kick.onClick.AddListener(() =>
            {
                if (IsLocalHost() && _lobby.HasStateAuthority)
                {
                    Debug.Log($"[Lobby] Kick player {player.PlayerId}");
                    _lobby.Rpc_Kick(player);
                }
            });

            _playerRows[player.PlayerId] = row.gameObject;
            return row.gameObject;
        }

        void UpdatePlayerRow(GameObject row, PlayerRef player)
        {
            var pseudoSlot = player.PlayerId;
            var pseudo = pseudoSlot >= 0 && pseudoSlot < LobbyState.MaxPlayers
                ? _lobby.Pseudos[pseudoSlot].ToString()
                : "";
            if (string.IsNullOrEmpty(pseudo)) pseudo = $"Joueur-{player.PlayerId}";

            var pseudoText = row.transform.Find("Pseudo")?.GetComponent<TextMeshProUGUI>();
            if (pseudoText != null) pseudoText.text = pseudo;

            bool isHost = player == _lobby.HostPlayerRef;
            var hostBadge = row.transform.Find("HostBadge")?.GetComponent<TextMeshProUGUI>();
            if (hostBadge != null) hostBadge.gameObject.SetActive(isHost);

            bool isSelf = player == _room.Runner.LocalPlayer;
            bool showKick = IsLocalHost() && !isSelf;
            var kick = row.transform.Find("Kick")?.GetComponent<Button>();
            if (kick != null) kick.gameObject.SetActive(showKick);
        }

        void RefreshPaused()
        {
            bool paused = _lobby?.Paused ?? false;
            if (_pausedOverlay.activeSelf != paused) _pausedOverlay.SetActive(paused);
            if (paused)
            {
                var who = _lobby.PausedBy.ToString();
                _pausedOverlayText.text = string.IsNullOrEmpty(who)
                    ? "⏸ Partie en pause"
                    : $"⏸ Partie en pause par {who}";
            }
        }

        void RefreshHostControls()
        {
            bool isHost = IsLocalHost();
            _startBtn.gameObject.SetActive(isHost);
            _composeBtn.gameObject.SetActive(isHost);
            _timersBtn.gameObject.SetActive(isHost);
            bool canStart = isHost && _lobby.CompositionValidated && ActivePlayerCount >= 5 && !_lobby.GameStarting;
            _startBtn.interactable = canStart;
            if (_lobby.GameStarting)
            {
                _startBtn.GetComponentInChildren<TextMeshProUGUI>().text = "Démarrage…";
            }
        }

        bool _gameStartTriggered;

        void CheckCountdown()
        {
            if (_lobby.GameStarting && _lobby.StartCountdown.Expired(_lobby.Runner))
            {
                if (!_gameStartTriggered)
                {
                    _gameStartTriggered = true;
                    // Host déclenche la distribution des rôles AVANT la navigation, pour
                    // que GameState.Phase soit déjà Distribution quand les clients
                    // affichent GameScreen (sinon RoleRevealScreen verrait Phase=Idle
                    // et flickerait).
                    var gs = FindFirstObjectByType<GameState>();
                    if (gs != null && gs.Object != null && gs.Object.IsValid && IsLocalHost())
                    {
                        Debug.Log("[Lobby] Host → GameState.HostStartGame()");
                        gs.HostStartGame(_lobby);
                    }
                }
                Debug.Log("[Lobby] Countdown expiré — transition vers GameScreen.");
                NavigationService.Instance.Show<GameScreen>();
            }
        }

        // --- Actions --------------------------------------------------------

        void OnQuit()
        {
            _room?.LeaveRoom();
            NavigationService.Instance.Show<HomeScreen>();
        }

        void OnPauseToggle()
        {
            if (_lobby == null || _room?.Runner == null) return;
            var pseudo = PlayerIdentityService.Current.Pseudo;
            _lobby.Rpc_RequestPauseToggle(pseudo);
        }

        void OnCopyCode()
        {
            GUIUtility.systemCopyBuffer = _codeText.text;
            Debug.Log($"[Lobby] Code copié : {_codeText.text}");
        }

        void OnShareLink()
        {
            var code = _room?.CurrentRoomId ?? "";
            var link = $"laveillee://join/{code}";
            GUIUtility.systemCopyBuffer = link;
            Debug.Log($"[Lobby] Lien copié (share sheet iOS à intégrer post-MVP) : {link}");
        }

        void OnStartGame()
        {
            if (!IsLocalHost() || !_lobby.HasStateAuthority) return;
            if (ActivePlayerCount < 5)
            {
                Debug.Log("[Lobby] 5 joueurs minimum — démarrage refusé.");
                return;
            }
            _lobby.StartGame();
        }

        void OnRoomLeft(RoomLeaveReason reason)
        {
            Debug.Log($"[LobbyScreen] RoomLeft : {reason} — retour Home.");
            NavigationService.Instance.Show<HomeScreen>();
        }
    }
}
