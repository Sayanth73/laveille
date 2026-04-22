using System.Collections;
using LaVeillee.Core;
using LaVeillee.Networking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LaVeillee.UI
{
    /// Mode Solo (dev) — contourne le minimum 5 joueurs pour tester chaque rôle /
    /// phase sans ouvrir 5 instances.
    ///
    /// Flow :
    ///   1) Pick rôle local + phase de départ.
    ///   2) Lancer → CreateRoom ("SOLO_XXXXXX") → attend LobbyState/GameState →
    ///      GameState.DevStartSolo() → GameScreen.
    ///
    /// Pas de vrais autres joueurs : 4 bots virtuels (Alive + RoleAcked) pour que
    /// le moteur de phases ne bloque pas. Leurs votes restent à -1.
    public class DevSoloScreen : ScreenBase
    {
        static readonly RoleId[] Roles =
        {
            RoleId.Villageois, RoleId.LoupGarou, RoleId.Voyante,
            RoleId.Sorciere, RoleId.Chasseur,
        };

        static readonly (GamePhase phase, string label)[] Phases =
        {
            (GamePhase.Distribution, "Distribution"),
            (GamePhase.NightVoyante, "🌙 Nuit — Voyante"),
            (GamePhase.NightLoups,   "🌙 Nuit — Loups"),
            (GamePhase.NightSorciere,"🌙 Nuit — Sorcière"),
            (GamePhase.DayDebate,    "☀️ Jour — Débat & Vote"),
            (GamePhase.HunterShot,   "🎯 Tir Chasseur"),
        };

        RoleId _selectedRole = RoleId.LoupGarou;
        GamePhase _selectedPhase = GamePhase.DayDebate;

        TextMeshProUGUI _statusText;
        Button _launchBtn;
        Button _backBtn;
        readonly System.Collections.Generic.Dictionary<RoleId, Button> _roleBtns = new();
        readonly System.Collections.Generic.Dictionary<GamePhase, Button> _phaseBtns = new();

        bool _launching;

        protected override void BuildUI()
        {
            UIFactory.CreateScreenCanvas("DevSoloCanvas", out var canvasGo);
            canvasGo.transform.SetParent(transform, false);
            Root = UIFactory.CreateFullscreen(canvasGo.transform, "DevSoloRoot", DesignTokens.Colors.Night900);

            BuildHeader();
            BuildRoleSection();
            BuildPhaseSection();
            BuildFooter();
        }

        void BuildHeader()
        {
            var title = UIFactory.CreateText(Root, "🛠 Mode Solo (dev)", UIFactory.TextStyle.H1,
                DesignTokens.Colors.Fire500);
            title.rectTransform.anchorMin = new Vector2(0f, 1f);
            title.rectTransform.anchorMax = new Vector2(1f, 1f);
            title.rectTransform.pivot = new Vector2(0.5f, 1f);
            title.rectTransform.anchoredPosition = new Vector2(0f, -48f);
            title.rectTransform.sizeDelta = new Vector2(0f, 72f);

            var sub = UIFactory.CreateText(Root,
                "Lance la partie en solo avec 4 bots. Tu peux tester chaque rôle et chaque phase.",
                UIFactory.TextStyle.Body, DesignTokens.Colors.Moon300);
            sub.rectTransform.anchorMin = new Vector2(0f, 1f);
            sub.rectTransform.anchorMax = new Vector2(1f, 1f);
            sub.rectTransform.pivot = new Vector2(0.5f, 1f);
            sub.rectTransform.anchoredPosition = new Vector2(0f, -128f);
            sub.rectTransform.sizeDelta = new Vector2(-64f, 56f);
        }

        void BuildRoleSection()
        {
            var label = UIFactory.CreateText(Root, "Ton rôle", UIFactory.TextStyle.H3,
                DesignTokens.Colors.Moon100);
            label.rectTransform.anchorMin = new Vector2(0f, 1f);
            label.rectTransform.anchorMax = new Vector2(1f, 1f);
            label.rectTransform.pivot = new Vector2(0.5f, 1f);
            label.rectTransform.anchoredPosition = new Vector2(0f, -216f);
            label.rectTransform.sizeDelta = new Vector2(-64f, 48f);

            var row = UIFactory.CreatePanel(Root, "RolesRow", new Color(0, 0, 0, 0));
            row.anchorMin = new Vector2(0.5f, 1f);
            row.anchorMax = new Vector2(0.5f, 1f);
            row.pivot = new Vector2(0.5f, 1f);
            row.anchoredPosition = new Vector2(0f, -280f);
            row.sizeDelta = new Vector2(1000f, 420f);

            var vlg = row.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = DesignTokens.Spacing.Sm;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;

            foreach (var r in Roles)
            {
                var btn = UIFactory.CreateButton(row, $"{RoleCatalog.EmojiFor(r)}  {RoleCatalog.LabelFor(r)}",
                    UIFactory.ButtonStyle.Secondary, new Vector2(0f, 72f));
                var le = btn.gameObject.AddComponent<LayoutElement>();
                le.preferredHeight = 72f;
                var captured = r;
                btn.onClick.AddListener(() => { _selectedRole = captured; RefreshHighlights(); });
                _roleBtns[r] = btn;
            }
        }

        void BuildPhaseSection()
        {
            var label = UIFactory.CreateText(Root, "Phase de départ", UIFactory.TextStyle.H3,
                DesignTokens.Colors.Moon100);
            label.rectTransform.anchorMin = new Vector2(0f, 1f);
            label.rectTransform.anchorMax = new Vector2(1f, 1f);
            label.rectTransform.pivot = new Vector2(0.5f, 1f);
            label.rectTransform.anchoredPosition = new Vector2(0f, -740f);
            label.rectTransform.sizeDelta = new Vector2(-64f, 48f);

            var row = UIFactory.CreatePanel(Root, "PhasesRow", new Color(0, 0, 0, 0));
            row.anchorMin = new Vector2(0.5f, 1f);
            row.anchorMax = new Vector2(0.5f, 1f);
            row.pivot = new Vector2(0.5f, 1f);
            row.anchoredPosition = new Vector2(0f, -800f);
            row.sizeDelta = new Vector2(1000f, 500f);

            var vlg = row.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = DesignTokens.Spacing.Sm;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;

            foreach (var (p, lbl) in Phases)
            {
                var btn = UIFactory.CreateButton(row, lbl,
                    UIFactory.ButtonStyle.Secondary, new Vector2(0f, 72f));
                var le = btn.gameObject.AddComponent<LayoutElement>();
                le.preferredHeight = 72f;
                var captured = p;
                btn.onClick.AddListener(() => { _selectedPhase = captured; RefreshHighlights(); });
                _phaseBtns[p] = btn;
            }
        }

        void BuildFooter()
        {
            _statusText = UIFactory.CreateText(Root, "", UIFactory.TextStyle.Caption,
                DesignTokens.Colors.Moon300);
            _statusText.rectTransform.anchorMin = new Vector2(0f, 0f);
            _statusText.rectTransform.anchorMax = new Vector2(1f, 0f);
            _statusText.rectTransform.pivot = new Vector2(0.5f, 0f);
            _statusText.rectTransform.anchoredPosition = new Vector2(0f, 280f);
            _statusText.rectTransform.sizeDelta = new Vector2(-64f, 60f);

            _backBtn = UIFactory.CreateButton(Root, "Retour", UIFactory.ButtonStyle.Ghost,
                new Vector2(400f, 96f));
            var brt = _backBtn.GetComponent<RectTransform>();
            brt.anchorMin = new Vector2(0.5f, 0f);
            brt.anchorMax = new Vector2(0.5f, 0f);
            brt.pivot = new Vector2(0.5f, 0f);
            brt.anchoredPosition = new Vector2(-260f, 128f);
            _backBtn.onClick.AddListener(() => NavigationService.Instance.Show<HomeScreen>());

            _launchBtn = UIFactory.CreateButton(Root, "Lancer",
                UIFactory.ButtonStyle.Primary, new Vector2(400f, 96f));
            var lrt = _launchBtn.GetComponent<RectTransform>();
            lrt.anchorMin = new Vector2(0.5f, 0f);
            lrt.anchorMax = new Vector2(0.5f, 0f);
            lrt.pivot = new Vector2(0.5f, 0f);
            lrt.anchoredPosition = new Vector2(260f, 128f);
            _launchBtn.onClick.AddListener(OnLaunch);
        }

        public override void OnShow()
        {
            _launching = false;
            _statusText.text = "";
            _launchBtn.interactable = true;
            RefreshHighlights();
        }

        void RefreshHighlights()
        {
            foreach (var kv in _roleBtns)
            {
                var img = kv.Value.GetComponent<Image>();
                if (img != null)
                    img.color = (kv.Key == _selectedRole)
                        ? DesignTokens.Colors.Fire500
                        : DesignTokens.Colors.Night700;
            }
            foreach (var kv in _phaseBtns)
            {
                var img = kv.Value.GetComponent<Image>();
                if (img != null)
                    img.color = (kv.Key == _selectedPhase)
                        ? DesignTokens.Colors.Fire500
                        : DesignTokens.Colors.Night700;
            }
        }

        void OnLaunch()
        {
            if (_launching) return;
            _launching = true;
            _launchBtn.interactable = false;
            _statusText.text = "🔌 Connexion Photon…";
            StartCoroutine(LaunchCoroutine());
        }

        IEnumerator LaunchCoroutine()
        {
            var services = GameServices.EnsureExists();
            var room = services.Room;

            // Room unique pour éviter une collision avec une autre partie
            var roomId = "SOLO" + Random.Range(100000, 999999);
            bool createdFired = false, errored = false;
            System.Action<string> onCreated = _ => createdFired = true;
            System.Action<RoomError> onError = _ => errored = true;
            room.RoomCreated += onCreated;
            room.Error       += onError;

            room.CreateRoom(roomId, 25);

            float timeout = 15f;
            while (!createdFired && !errored && timeout > 0f)
            {
                timeout -= Time.unscaledDeltaTime;
                yield return null;
            }
            room.RoomCreated -= onCreated;
            room.Error       -= onError;

            if (errored || !createdFired)
            {
                _statusText.text = "❌ Échec connexion Photon — vérifie ton réseau.";
                _launching = false;
                _launchBtn.interactable = true;
                yield break;
            }

            _statusText.text = "📡 Attente du lobby…";
            LobbyState lobby = null;
            GameState gs = null;
            timeout = 10f;
            while (timeout > 0f)
            {
                lobby = FindFirstObjectByType<LobbyState>();
                gs = FindFirstObjectByType<GameState>();
                bool lobbyOk = lobby != null && lobby.Object != null && lobby.Object.IsValid;
                bool gsOk    = gs != null && gs.Object != null && gs.Object.IsValid && gs.HasStateAuthority;
                if (lobbyOk && gsOk) break;
                timeout -= Time.unscaledDeltaTime;
                yield return null;
            }
            if (lobby == null || gs == null || !gs.HasStateAuthority)
            {
                _statusText.text = "❌ Lobby/GameState non synchronisé — réessaie.";
                _launching = false;
                _launchBtn.interactable = true;
                yield break;
            }

            // Pseudo local → slot dans lobby
            var id = PlayerIdentityService.Current;
            lobby.Rpc_RegisterPlayer(room.Runner.LocalPlayer, id.Pseudo, id.AvatarColorSeed);

            // Laisse un tick passer pour que le RPC s'applique
            yield return null;

            gs.DevStartSolo(lobby, _selectedRole, _selectedPhase);
            _statusText.text = "🎬 En route…";
            yield return null;

            NavigationService.Instance.Show<GameScreen>();
        }
    }
}
