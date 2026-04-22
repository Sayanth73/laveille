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
    /// Écran partie — hub qui route vers un sous-panel par GamePhase.
    /// Pattern : Build statique dans BuildUI (header + phaseRoot + feed),
    /// et rebuild du contenu de phaseRoot à chaque changement de Phase.
    /// Les updates fréquents (timer, compteurs de votes) passent par Update() sans rebuild.
    public class GameScreen : ScreenBase
    {
        GameState _gs;
        LobbyState _lobby;
        FusionRoomManager _room;

        RectTransform _headerRoot;
        TextMeshProUGUI _phaseTitle;
        TextMeshProUGUI _phaseTimer;
        RectTransform _phaseRoot;
        TextMeshProUGUI _feedText;

        GamePhase _renderedPhase = (GamePhase)(-1); // force rebuild au premier Update
        int _renderedDayCount = -1;

        // État interactif par phase — reset lors du rebuild
        TextMeshProUGUI _distributionAckText;
        Button _distributionAckBtn;
        TextMeshProUGUI _voyanteResultText;
        Toggle _sorciereSaveToggle;
        Button _sorciereLockBtn;
        Button _dayAbstainBtn;
        Button _quitBtn;

        // Story 5.6 — Écran noir UX (yeux fermés numériques)
        CanvasGroup _nightOverlay;
        float _overlayAlpha;
        bool _wasLocalActive = true;
        const float OverlayFadeSeconds = 0.5f;

        // Story 5.8 — Crosshair central pour viser en FPV
        Image _crosshair;

        protected override void BuildUI()
        {
            UIFactory.CreateScreenCanvas("GameCanvas", out var canvasGo);
            canvasGo.transform.SetParent(transform, false);
            // Fond transparent + scrim léger : la 3D (socles, avatars, feu) sert de backdrop pour le HUD.
            Root = UIFactory.CreateFullscreen(canvasGo.transform, "GameRoot", new Color(0f, 0f, 0f, 0f));
            // Important : le Root est un Image transparent plein écran. Par défaut Unity
            // met raycastTarget=true, ce qui fait que IsPointerOverGameObject() renvoie
            // toujours true et bloque le drag caméra FPV + le raycast 3D de sélection.
            Root.GetComponent<UnityEngine.UI.Image>().raycastTarget = false;
            var scrim = UIFactory.CreateFullscreen(Root, "Scrim", new Color(0.043f, 0.070f, 0.141f, 0.40f));
            scrim.GetComponent<UnityEngine.UI.Image>().raycastTarget = false;

            BuildHeader();
            BuildPhaseRoot();
            BuildFeed();
            BuildCrosshair();
            BuildNightOverlay();
        }

        // Story 5.8 — Point de mire central pour l'interaction FPV.
        void BuildCrosshair()
        {
            var go = new GameObject("Crosshair", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(Root, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(22f, 22f);
            rt.anchoredPosition = Vector2.zero;
            _crosshair = go.GetComponent<Image>();
            _crosshair.color = new Color(1f, 1f, 1f, 0.7f);
            _crosshair.raycastTarget = false;
            // Cercle simple créé via sprite default — fallback solide si sprite absent.
            _crosshair.sprite = null;
            // Petit point : on désactive et on utilise un second enfant "dot" (12 px)
            _crosshair.enabled = false;

            var dot = new GameObject("Dot", typeof(RectTransform), typeof(Image));
            dot.transform.SetParent(go.transform, false);
            var drt = dot.GetComponent<RectTransform>();
            drt.anchorMin = drt.anchorMax = new Vector2(0.5f, 0.5f);
            drt.pivot = new Vector2(0.5f, 0.5f);
            drt.sizeDelta = new Vector2(8f, 8f);
            var di = dot.GetComponent<Image>();
            di.color = new Color(1f, 0.85f, 0.55f, 0.9f);
            di.raycastTarget = false;
        }

        // Story 5.6 — "Yeux fermés numériques" : pendant la nuit, les joueurs dont
        // le rôle n'est pas actif voient un fondu vers noir (fade 500 ms). Le rôle
        // appelé garde son UI visible. Un léger haptic signale l'activation.
        void BuildNightOverlay()
        {
            var go = new GameObject("NightOverlay", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            go.transform.SetParent(Root, false);
            var rt = go.GetComponent<RectTransform>();
            FullFill(rt);
            go.GetComponent<Image>().color = Color.black;
            _nightOverlay = go.GetComponent<CanvasGroup>();
            _nightOverlay.alpha = 0f;
            _nightOverlay.blocksRaycasts = false;
            _nightOverlay.interactable = false;

            var hint = UIFactory.CreateText(rt, "🌙  Ferme les yeux…", UIFactory.TextStyle.Body,
                new Color(0.35f, 0.35f, 0.4f, 1f));
            hint.rectTransform.anchorMin = new Vector2(0f, 0.46f);
            hint.rectTransform.anchorMax = new Vector2(1f, 0.54f);
            hint.rectTransform.offsetMin = Vector2.zero;
            hint.rectTransform.offsetMax = Vector2.zero;

            // L'overlay doit rester au-dessus de tous les panels de phase.
            go.transform.SetAsLastSibling();
        }

        void BuildHeader()
        {
            _headerRoot = UIFactory.CreatePanel(Root, "Header", DesignTokens.Colors.Night700);
            _headerRoot.anchorMin = new Vector2(0f, 1f);
            _headerRoot.anchorMax = new Vector2(1f, 1f);
            _headerRoot.pivot = new Vector2(0.5f, 1f);
            _headerRoot.sizeDelta = new Vector2(0f, 140f);

            _phaseTitle = UIFactory.CreateText(_headerRoot, "…", UIFactory.TextStyle.H1,
                DesignTokens.Colors.Moon100, TextAlignmentOptions.Center);
            _phaseTitle.rectTransform.anchorMin = new Vector2(0f, 0.45f);
            _phaseTitle.rectTransform.anchorMax = new Vector2(1f, 1f);
            _phaseTitle.rectTransform.offsetMin = new Vector2(24f, 0f);
            _phaseTitle.rectTransform.offsetMax = new Vector2(-24f, -8f);

            _phaseTimer = UIFactory.CreateText(_headerRoot, "--", UIFactory.TextStyle.H3,
                DesignTokens.Colors.Moon300, TextAlignmentOptions.Center);
            _phaseTimer.rectTransform.anchorMin = new Vector2(0f, 0f);
            _phaseTimer.rectTransform.anchorMax = new Vector2(1f, 0.45f);
            _phaseTimer.rectTransform.offsetMin = new Vector2(24f, 8f);
            _phaseTimer.rectTransform.offsetMax = new Vector2(-24f, 0f);

            _quitBtn = UIFactory.CreateButton(_headerRoot, "Quitter", UIFactory.ButtonStyle.Ghost,
                new Vector2(160f, 64f));
            var qrt = _quitBtn.GetComponent<RectTransform>();
            qrt.anchorMin = new Vector2(1f, 0.5f);
            qrt.anchorMax = new Vector2(1f, 0.5f);
            qrt.pivot = new Vector2(1f, 0.5f);
            qrt.anchoredPosition = new Vector2(-16f, 0f);
            _quitBtn.onClick.AddListener(OnQuit);
        }

        void BuildPhaseRoot()
        {
            _phaseRoot = UIFactory.CreatePanel(Root, "PhaseRoot", new Color(0, 0, 0, 0));
            _phaseRoot.GetComponent<UnityEngine.UI.Image>().raycastTarget = false;
            _phaseRoot.anchorMin = new Vector2(0f, 0.22f);
            _phaseRoot.anchorMax = new Vector2(1f, 1f);
            _phaseRoot.offsetMin = new Vector2(0f, 0f);
            _phaseRoot.offsetMax = new Vector2(0f, -140f);
        }

        void BuildFeed()
        {
            var feedPanel = UIFactory.CreatePanel(Root, "Feed", DesignTokens.Colors.Night700);
            feedPanel.anchorMin = new Vector2(0f, 0f);
            feedPanel.anchorMax = new Vector2(1f, 0.22f);
            feedPanel.offsetMin = Vector2.zero;
            feedPanel.offsetMax = Vector2.zero;

            _feedText = UIFactory.CreateText(feedPanel, "", UIFactory.TextStyle.Caption,
                DesignTokens.Colors.Moon300, TextAlignmentOptions.BottomLeft);
            _feedText.rectTransform.anchorMin = Vector2.zero;
            _feedText.rectTransform.anchorMax = Vector2.one;
            _feedText.rectTransform.offsetMin = new Vector2(16f, 12f);
            _feedText.rectTransform.offsetMax = new Vector2(-16f, -12f);
            _feedText.enableWordWrapping = true;
        }

        public override void OnShow()
        {
            _room = GameServices.EnsureExists().Room;
            _gs = FindFirstObjectByType<GameState>();
            _lobby = FindFirstObjectByType<LobbyState>();
            _renderedPhase = (GamePhase)(-1);
            _renderedDayCount = -1;
            ResetPhaseRefs();
        }

        public override void OnHide()
        {
            ResetPhaseRefs();
        }

        void ResetPhaseRefs()
        {
            _distributionAckText = null;
            _distributionAckBtn = null;
            _voyanteResultText = null;
            _sorciereSaveToggle = null;
            _sorciereLockBtn = null;
            _dayAbstainBtn = null;
            _loupStatusText = null;
            _dayVoteStatusText = null;
        }

        // =====================================================================

        void Update()
        {
            if (_gs == null || _gs.Object == null || !_gs.Object.IsValid)
            {
                _gs = FindFirstObjectByType<GameState>();
                if (_gs == null) return;
            }
            if (_lobby == null || _lobby.Object == null || !_lobby.Object.IsValid)
            {
                _lobby = FindFirstObjectByType<LobbyState>();
            }
            if (_room?.Runner == null) return;

            if (_gs.Phase != _renderedPhase || _gs.DayCount != _renderedDayCount)
            {
                RebuildForPhase(_gs.Phase);
                _renderedPhase = _gs.Phase;
                _renderedDayCount = _gs.DayCount;
            }
            UpdateHeader();
            UpdatePhase();
            UpdateFeed();
            UpdateNightOverlay();
            UpdateCrosshair();
            ApplyVoiceGate(); // appel chaque frame — réagit aussi aux morts mid-phase
        }

        void UpdateCrosshair()
        {
            if (_crosshair == null) return;
            bool show = IsPickPhaseActive();
            // Activer/désactiver le GameObject parent (contient le Dot)
            if (_crosshair.transform.parent is Transform t && t == Root) { }
            var parent = _crosshair.gameObject;
            if (parent.activeSelf != show) parent.SetActive(show);
        }

        bool IsPickPhaseActive()
        {
            if (_gs == null) return false;
            var role = LocalRole();
            return _gs.Phase switch
            {
                GamePhase.NightVoyante  => role == RoleId.Voyante && IsLocalAlive() && !_gs.VoyanteLockedThisNight,
                GamePhase.NightLoups    => role == RoleId.LoupGarou && IsLocalAlive() && !_gs.LoupVoteLocked.Get(LocalPlayerId),
                GamePhase.NightSorciere => role == RoleId.Sorciere && IsLocalAlive()
                                           && _gs.SorciereHasDeathPotion && !_gs.SorciereLockedThisNight,
                GamePhase.DayDebate or GamePhase.DayVote => IsLocalAlive(),
                GamePhase.HunterShot    => _gs.HunterPendingShooter == LocalPlayerId,
                _                       => false,
            };
        }

        void UpdateNightOverlay()
        {
            if (_nightOverlay == null) return;
            bool shouldBlackOut = IsNightPhase(_gs.Phase) && !IsLocalActiveInPhase(_gs.Phase);
            float target = shouldBlackOut ? 1f : 0f;
            _overlayAlpha = Mathf.MoveTowards(_overlayAlpha, target, Time.deltaTime / OverlayFadeSeconds);
            _nightOverlay.alpha = _overlayAlpha;
            _nightOverlay.blocksRaycasts = _overlayAlpha > 0.5f;

            // Haptic léger quand mon rôle vient d'activer pendant la nuit (fade in).
            // `Handheld.Vibrate` n'existe que sur iOS/Android — no-op sur Standalone.
            bool isActiveNow = !shouldBlackOut;
            if (!_wasLocalActive && isActiveNow && IsNightPhase(_gs.Phase))
            {
#if UNITY_IOS || UNITY_ANDROID
                Handheld.Vibrate();
#endif
            }
            _wasLocalActive = isActiveNow;
        }

        bool IsNightPhase(GamePhase p) =>
            p == GamePhase.NightVoyante || p == GamePhase.NightLoups ||
            p == GamePhase.NightSorciere || p == GamePhase.NightResolve;

        bool IsLocalActiveInPhase(GamePhase p)
        {
            if (!IsLocalAlive()) return false;
            var role = LocalRole();
            return p switch
            {
                GamePhase.NightVoyante  => role == RoleId.Voyante,
                GamePhase.NightLoups    => role == RoleId.LoupGarou,
                GamePhase.NightSorciere => role == RoleId.Sorciere,
                GamePhase.NightResolve  => false, // cinématique — personne n'agit
                GamePhase.HunterShot    => _gs.HunterPendingShooter == LocalPlayerId,
                _                       => true,
            };
        }

        /// MVP : mute/unmute en fonction de la phase + statut vivant.
        /// Day = vivants parlent ; Night/Distribution/Hunter = silence pour tous.
        /// (Groupe vocal wolves-only et salon morts déférés post-MVP : nécessite
        /// Photon Voice InterestGroup + OpChangeGroups, complexité non-triviale.)
        void ApplyVoiceGate()
        {
            var voice = GameServices.Instance?.Voice;
            if (voice == null) return;
            bool canTalk = _gs.Phase == GamePhase.DayDebate
                           || _gs.Phase == GamePhase.DayVote;
            canTalk &= IsLocalAlive();
            voice.SetMuted(!canTalk);
        }

        void UpdateHeader()
        {
            _phaseTitle.text = PhaseTitleFor(_gs.Phase, _gs.DayCount);
            _phaseTitle.color = PhaseColorFor(_gs.Phase);
            var remaining = _gs.PhaseTimer.RemainingTime(_gs.Runner) ?? 0f;
            _phaseTimer.text = remaining > 0 ? $"{Mathf.CeilToInt(remaining)}s" : "—";
        }

        void UpdateFeed()
        {
            var msgs = _gs.ReadFeedChronological().ToList();
            // afficher les 6 derniers pour ne pas déborder
            int start = Mathf.Max(0, msgs.Count - 6);
            _feedText.text = string.Join("\n", msgs.Skip(start));
        }

        // =====================================================================
        // Phase routing
        // =====================================================================

        void RebuildForPhase(GamePhase p)
        {
            for (int i = _phaseRoot.childCount - 1; i >= 0; i--)
                Destroy(_phaseRoot.GetChild(i).gameObject);
            ResetPhaseRefs();

            switch (p)
            {
                case GamePhase.Idle:             BuildIdlePanel(); break;
                case GamePhase.Distribution:     BuildDistributionPanel(); break;
                case GamePhase.NightVoyante:     BuildVoyantePanel(); break;
                case GamePhase.NightLoups:       BuildLoupsPanel(); break;
                case GamePhase.NightSorciere:    BuildSorcierePanel(); break;
                case GamePhase.NightResolve:     BuildNightWaitPanel("La nuit s'achève…"); break;
                case GamePhase.DayDebate:        BuildDayDebatePanel(); break;
                case GamePhase.DayVote:          BuildDayVotePanel(); break;
                case GamePhase.HunterShot:       BuildHunterPanel(); break;
                case GamePhase.RevealDeath:      BuildNightWaitPanel("…"); break;
                case GamePhase.Ended:            BuildEndPanel(); break;
            }
        }

        void UpdatePhase()
        {
            switch (_gs.Phase)
            {
                case GamePhase.Distribution: UpdateDistributionPanel(); break;
                case GamePhase.NightVoyante: UpdateVoyantePanel(); break;
                case GamePhase.NightLoups:   UpdateLoupsPanel(); break;
                case GamePhase.NightSorciere: UpdateSorcierePanel(); break;
                case GamePhase.DayDebate:    UpdateDayDebatePanel(); break;
                case GamePhase.DayVote:      UpdateDayVotePanel(); break;
                case GamePhase.HunterShot:   UpdateHunterPanel(); break;
            }
        }

        // =====================================================================
        // Idle (pré-démarrage)
        // =====================================================================

        void BuildIdlePanel()
        {
            var t = UIFactory.CreateText(_phaseRoot, "Préparation de la partie…",
                UIFactory.TextStyle.H1, DesignTokens.Colors.Moon300);
            FullFill(t.rectTransform);
        }

        // =====================================================================
        // Story 3.1 — Distribution
        // =====================================================================

        void BuildDistributionPanel()
        {
            var role = LocalRole();
            var center = UIFactory.CreatePanel(_phaseRoot, "Role", new Color(0, 0, 0, 0));
            center.GetComponent<UnityEngine.UI.Image>().raycastTarget = false;
            center.anchorMin = new Vector2(0.5f, 0.5f);
            center.anchorMax = new Vector2(0.5f, 0.5f);
            center.pivot = new Vector2(0.5f, 0.5f);
            center.sizeDelta = new Vector2(900f, 900f);

            var emoji = UIFactory.CreateText(center, RoleCatalog.EmojiFor(role),
                UIFactory.TextStyle.Display, DesignTokens.Colors.Moon100);
            emoji.fontSize = 220f;
            emoji.rectTransform.anchorMin = new Vector2(0f, 0.55f);
            emoji.rectTransform.anchorMax = new Vector2(1f, 0.9f);
            emoji.rectTransform.offsetMin = Vector2.zero;
            emoji.rectTransform.offsetMax = Vector2.zero;

            var nameT = UIFactory.CreateText(center, $"Tu es {RoleCatalog.LabelFor(role)}",
                UIFactory.TextStyle.Display, RoleCatalog.IsWolf(role) ? DesignTokens.Colors.Blood500 : DesignTokens.Colors.Fire500);
            nameT.rectTransform.anchorMin = new Vector2(0f, 0.4f);
            nameT.rectTransform.anchorMax = new Vector2(1f, 0.55f);
            nameT.rectTransform.offsetMin = Vector2.zero;
            nameT.rectTransform.offsetMax = Vector2.zero;

            var desc = UIFactory.CreateText(center, RoleDescriptionFor(role),
                UIFactory.TextStyle.Body, DesignTokens.Colors.Moon300);
            desc.rectTransform.anchorMin = new Vector2(0f, 0.22f);
            desc.rectTransform.anchorMax = new Vector2(1f, 0.4f);
            desc.rectTransform.offsetMin = new Vector2(48f, 0f);
            desc.rectTransform.offsetMax = new Vector2(-48f, 0f);

            _distributionAckBtn = UIFactory.CreateButton(center, "J'ai compris",
                UIFactory.ButtonStyle.Primary, new Vector2(560f, 104f));
            var brt = _distributionAckBtn.GetComponent<RectTransform>();
            brt.anchorMin = new Vector2(0.5f, 0.1f);
            brt.anchorMax = new Vector2(0.5f, 0.1f);
            brt.pivot = new Vector2(0.5f, 0.5f);
            brt.anchoredPosition = Vector2.zero;
            _distributionAckBtn.onClick.AddListener(OnAckRole);

            _distributionAckText = UIFactory.CreateText(center, "",
                UIFactory.TextStyle.Body, DesignTokens.Colors.Moon300);
            _distributionAckText.rectTransform.anchorMin = new Vector2(0f, 0f);
            _distributionAckText.rectTransform.anchorMax = new Vector2(1f, 0.06f);
            _distributionAckText.rectTransform.offsetMin = Vector2.zero;
            _distributionAckText.rectTransform.offsetMax = Vector2.zero;
        }

        void UpdateDistributionPanel()
        {
            int pid = LocalPlayerId;
            bool acked = pid >= 0 && pid < GameState.MaxPlayers && _gs.RoleAcked.Get(pid);
            if (_distributionAckBtn != null) _distributionAckBtn.interactable = !acked;
            if (_distributionAckText != null)
            {
                int total = 0, ackedCount = 0;
                for (int i = 0; i < GameState.MaxPlayers; i++)
                {
                    if (!_gs.Alive.Get(i)) continue;
                    total++;
                    if (_gs.RoleAcked.Get(i)) ackedCount++;
                }
                _distributionAckText.text = acked
                    ? $"En attente des autres joueurs : {ackedCount} / {total}"
                    : "Lis bien ton rôle — il ne te sera pas re-montré.";
            }
        }

        void OnAckRole()
        {
            if (_gs == null || _room?.Runner == null) return;
            _gs.Rpc_AckRole(_room.Runner.LocalPlayer);
        }

        // =====================================================================
        // Story 3.4 — Voyante
        // =====================================================================

        void BuildVoyantePanel()
        {
            var role = LocalRole();
            if (role != RoleId.Voyante || !IsLocalAlive())
            {
                BuildNightWaitPanel("🌙 La Voyante scrute quelqu'un…");
                return;
            }

            BuildPickHint("🔮 Vise un joueur pour découvrir son rôle",
                DesignTokens.Colors.Crystal500);

            _voyanteResultText = UIFactory.CreateText(_phaseRoot, "",
                UIFactory.TextStyle.H2, DesignTokens.Colors.Crystal500);
            _voyanteResultText.rectTransform.anchorMin = new Vector2(0f, 0.35f);
            _voyanteResultText.rectTransform.anchorMax = new Vector2(1f, 0.55f);
            _voyanteResultText.rectTransform.offsetMin = new Vector2(24f, 0f);
            _voyanteResultText.rectTransform.offsetMax = new Vector2(-24f, 0f);
        }

        void UpdateVoyantePanel()
        {
            if (LocalRole() != RoleId.Voyante || !IsLocalAlive()) return;
            if (_gs.VoyanteLockedThisNight && _gs.VoyanteTargetThisNight >= 0 && _voyanteResultText != null)
            {
                var t = _gs.VoyanteTargetThisNight;
                var r = _gs.GetRoleOf(t);
                _voyanteResultText.text = $"{PseudoOf(t)} est {RoleCatalog.EmojiFor(r)} {RoleCatalog.LabelFor(r)}";
            }
        }

        // =====================================================================
        // Story 3.3 — Loups-Garous (vote privé)
        // =====================================================================

        TextMeshProUGUI _loupStatusText;

        void BuildLoupsPanel()
        {
            var role = LocalRole();
            if (role != RoleId.LoupGarou || !IsLocalAlive())
            {
                BuildNightWaitPanel("🐺 Les Loups-Garous choisissent leur victime…");
                return;
            }

            BuildPickHint("🐺 Vise une cible — re-vise pour verrouiller",
                DesignTokens.Colors.Blood500);

            _loupStatusText = UIFactory.CreateText(_phaseRoot, "",
                UIFactory.TextStyle.H3, DesignTokens.Colors.Moon100);
            _loupStatusText.rectTransform.anchorMin = new Vector2(0f, 0.3f);
            _loupStatusText.rectTransform.anchorMax = new Vector2(1f, 0.5f);
            _loupStatusText.rectTransform.offsetMin = new Vector2(24f, 0f);
            _loupStatusText.rectTransform.offsetMax = new Vector2(-24f, 0f);
        }

        void UpdateLoupsPanel()
        {
            if (LocalRole() != RoleId.LoupGarou || !IsLocalAlive()) return;
            if (_loupStatusText == null) return;

            var tally = new Dictionary<int, int>();
            for (int i = 0; i < GameState.MaxPlayers; i++)
            {
                if (!_gs.Alive.Get(i)) continue;
                if (_gs.GetRoleOf(i) != RoleId.LoupGarou) continue;
                int t = _gs.LoupVoteTarget.Get(i);
                if (t < 0) continue;
                if (!tally.ContainsKey(t)) tally[t] = 0;
                tally[t]++;
            }
            bool locked = _gs.LoupVoteLocked.Get(LocalPlayerId);
            int myTarget = _gs.LoupVoteTarget.Get(LocalPlayerId);
            string status = myTarget >= 0
                ? (locked ? $"Verrouillé sur {PseudoOf(myTarget)}" : $"Vise : {PseudoOf(myTarget)} — re-vise pour verrouiller")
                : "Aucune cible visée";
            if (tally.Count > 0)
            {
                var parts = new List<string>();
                foreach (var kv in tally) parts.Add($"{PseudoOf(kv.Key)} ({kv.Value})");
                status += "\nVotes Loups : " + string.Join(" • ", parts);
            }
            _loupStatusText.text = status;
        }

        // =====================================================================
        // Story 3.5 — Sorcière
        // =====================================================================

        void BuildSorcierePanel()
        {
            var role = LocalRole();
            if (role != RoleId.Sorciere || !IsLocalAlive())
            {
                BuildNightWaitPanel("🧪 La Sorcière consulte ses potions…");
                return;
            }

            var title = UIFactory.CreateText(_phaseRoot, "🧪 Tes potions",
                UIFactory.TextStyle.H1, DesignTokens.Colors.Poison500);
            title.rectTransform.anchorMin = new Vector2(0f, 0.88f);
            title.rectTransform.anchorMax = new Vector2(1f, 1f);
            title.rectTransform.offsetMin = new Vector2(24f, 0f);
            title.rectTransform.offsetMax = new Vector2(-24f, -16f);

            // Info cible Loups
            int loupTarget = _gs.LoupsTargetResolved;
            var info = UIFactory.CreateText(_phaseRoot,
                loupTarget >= 0
                    ? $"Les Loups ont désigné : {PseudoOf(loupTarget)}"
                    : "Les Loups n'ont désigné personne cette nuit.",
                UIFactory.TextStyle.Body, DesignTokens.Colors.Moon300);
            info.rectTransform.anchorMin = new Vector2(0f, 0.78f);
            info.rectTransform.anchorMax = new Vector2(1f, 0.88f);
            info.rectTransform.offsetMin = new Vector2(24f, 0f);
            info.rectTransform.offsetMax = new Vector2(-24f, 0f);

            // Potion de vie : toggle sauvetage
            var saveRow = UIFactory.CreatePanel(_phaseRoot, "SaveRow", DesignTokens.Colors.Night700);
            saveRow.anchorMin = new Vector2(0.05f, 0.68f);
            saveRow.anchorMax = new Vector2(0.95f, 0.78f);
            saveRow.offsetMin = Vector2.zero;
            saveRow.offsetMax = Vector2.zero;

            var saveLbl = UIFactory.CreateText(saveRow,
                _gs.SorciereHasLifePotion
                    ? (loupTarget >= 0 ? $"Sauver {PseudoOf(loupTarget)} (potion de vie)" : "Potion de vie — aucune cible à sauver")
                    : "Potion de vie déjà utilisée",
                UIFactory.TextStyle.Body, DesignTokens.Colors.Moon100, TextAlignmentOptions.MidlineLeft);
            saveLbl.rectTransform.anchorMin = new Vector2(0f, 0f);
            saveLbl.rectTransform.anchorMax = new Vector2(0.7f, 1f);
            saveLbl.rectTransform.offsetMin = new Vector2(16f, 0f);
            saveLbl.rectTransform.offsetMax = Vector2.zero;

            var saveToggleGo = new GameObject("SaveToggle", typeof(RectTransform), typeof(Toggle), typeof(Image));
            saveToggleGo.transform.SetParent(saveRow, false);
            var strt = saveToggleGo.GetComponent<RectTransform>();
            strt.anchorMin = new Vector2(0.75f, 0.2f);
            strt.anchorMax = new Vector2(0.95f, 0.8f);
            strt.offsetMin = Vector2.zero;
            strt.offsetMax = Vector2.zero;
            _sorciereSaveToggle = saveToggleGo.GetComponent<Toggle>();
            var timg = saveToggleGo.GetComponent<Image>();
            timg.color = DesignTokens.Colors.Night900;
            _sorciereSaveToggle.targetGraphic = timg;
            _sorciereSaveToggle.isOn = false;
            _sorciereSaveToggle.interactable = _gs.SorciereHasLifePotion && loupTarget >= 0;
            var toggleLabel = UIFactory.CreateText(saveToggleGo.transform, _sorciereSaveToggle.isOn ? "OUI" : "NON",
                UIFactory.TextStyle.H3, DesignTokens.Colors.Moon100);
            toggleLabel.rectTransform.anchorMin = Vector2.zero;
            toggleLabel.rectTransform.anchorMax = Vector2.one;
            toggleLabel.rectTransform.offsetMin = Vector2.zero;
            toggleLabel.rectTransform.offsetMax = Vector2.zero;
            _sorciereSaveToggle.onValueChanged.AddListener(on =>
            {
                toggleLabel.text = on ? "OUI" : "NON";
                timg.color = on ? DesignTokens.Colors.Fire500 : DesignTokens.Colors.Night900;
            });

            // Potion de mort : picking 3D direct (vise un joueur + click = empoisonne + lock)
            var killTitle = UIFactory.CreateText(_phaseRoot,
                _gs.SorciereHasDeathPotion
                    ? "🧪 Potion de mort — vise un joueur pour l'empoisonner (ou passe)"
                    : "Potion de mort épuisée",
                UIFactory.TextStyle.H3, DesignTokens.Colors.Moon100);
            killTitle.rectTransform.anchorMin = new Vector2(0f, 0.55f);
            killTitle.rectTransform.anchorMax = new Vector2(1f, 0.65f);
            killTitle.rectTransform.offsetMin = new Vector2(24f, 0f);
            killTitle.rectTransform.offsetMax = new Vector2(-24f, 0f);

            _sorciereLockBtn = UIFactory.CreateButton(_phaseRoot, "Passer sans empoisonner",
                UIFactory.ButtonStyle.Primary, new Vector2(700f, 104f));
            var lrt = _sorciereLockBtn.GetComponent<RectTransform>();
            lrt.anchorMin = new Vector2(0.5f, 0.05f);
            lrt.anchorMax = new Vector2(0.5f, 0.05f);
            lrt.pivot = new Vector2(0.5f, 0.5f);
            lrt.anchoredPosition = Vector2.zero;
            _sorciereLockBtn.onClick.AddListener(OnSorciereLock);
        }

        void UpdateSorcierePanel()
        {
            if (LocalRole() != RoleId.Sorciere || !IsLocalAlive()) return;
            if (_sorciereLockBtn != null)
            {
                bool locked = _gs.SorciereLockedThisNight;
                _sorciereLockBtn.interactable = !locked;
                var lbl = _sorciereLockBtn.GetComponentInChildren<TextMeshProUGUI>();
                if (lbl != null) lbl.text = locked ? "Tour validé ✅" : "Passer sans empoisonner";
            }
        }

        void OnSorciereLock()
        {
            if (_gs.SorciereLockedThisNight) return;
            bool save = _sorciereSaveToggle != null && _sorciereSaveToggle.isOn;
            // Passer sans tuer : killTarget = -1. Le RPC prendra en compte le save éventuel.
            _gs.Rpc_SorciereAction(_room.Runner.LocalPlayer, save, -1, true);
        }

        // =====================================================================
        // Stories 3.6 + 3.7 — Phase jour fusionnée : débat + vote en temps réel
        // Vote public pendant le débat, compteurs live, abstention possible.
        // =====================================================================

        TextMeshProUGUI _dayVoteStatusText;

        void BuildDayDebatePanel()
        {
            BuildPickHint(IsLocalAlive()
                    ? "☀️ Vise un joueur pour voter — re-vise pour changer"
                    : "☀️ Tu es mort(e) — spectateur",
                DesignTokens.Colors.Fire500);

            _dayVoteStatusText = UIFactory.CreateText(_phaseRoot, "",
                UIFactory.TextStyle.H3, DesignTokens.Colors.Moon100);
            _dayVoteStatusText.rectTransform.anchorMin = new Vector2(0f, 0.25f);
            _dayVoteStatusText.rectTransform.anchorMax = new Vector2(1f, 0.45f);
            _dayVoteStatusText.rectTransform.offsetMin = new Vector2(24f, 0f);
            _dayVoteStatusText.rectTransform.offsetMax = new Vector2(-24f, 0f);

            _dayAbstainBtn = UIFactory.CreateButton(_phaseRoot, "S'abstenir",
                UIFactory.ButtonStyle.Ghost, new Vector2(700f, 96f));
            var art = _dayAbstainBtn.GetComponent<RectTransform>();
            art.anchorMin = new Vector2(0.5f, 0.05f);
            art.anchorMax = new Vector2(0.5f, 0.05f);
            art.pivot = new Vector2(0.5f, 0.5f);
            art.anchoredPosition = Vector2.zero;
            _dayAbstainBtn.onClick.AddListener(OnDayAbstain);
        }

        void UpdateDayDebatePanel()
        {
            if (_dayVoteStatusText == null) return;
            var tally = new Dictionary<int, int>();
            for (int i = 0; i < GameState.MaxPlayers; i++)
            {
                if (!_gs.Alive.Get(i)) continue;
                int t = _gs.DayVoteTarget.Get(i);
                if (t < 0) continue;
                if (!tally.ContainsKey(t)) tally[t] = 0;
                tally[t]++;
            }
            int myVote = _gs.DayVoteTarget.Get(LocalPlayerId);
            string status = myVote == -2
                ? "Tu t'abstiens."
                : (myVote >= 0 ? $"Tu votes : {PseudoOf(myVote)}" : "Tu n'as pas encore voté.");
            if (tally.Count > 0)
            {
                var parts = new List<string>();
                foreach (var kv in tally) parts.Add($"{PseudoOf(kv.Key)} ({kv.Value})");
                status += "\n" + string.Join(" • ", parts);
            }
            _dayVoteStatusText.text = status;

            if (_dayAbstainBtn != null)
            {
                _dayAbstainBtn.interactable = IsLocalAlive();
                var lbl = _dayAbstainBtn.GetComponentInChildren<TextMeshProUGUI>();
                if (lbl != null) lbl.text = myVote == -2 ? "Abstention ✅" : "S'abstenir";
            }
        }

        // DayVote (legacy) : phase deprecated, backend ne l'atteint plus.
        // Garde défensive au cas où un état networked périmé arriverait ici.
        void BuildDayVotePanel()  => BuildDayDebatePanel();
        void UpdateDayVotePanel() => UpdateDayDebatePanel();

        void OnDayAbstain()
        {
            if (!IsLocalAlive()) return;
            _gs.Rpc_DayVote(_room.Runner.LocalPlayer, -2);
        }

        // =====================================================================
        // Story 3.10 — Chasseur
        // =====================================================================

        void BuildHunterPanel()
        {
            bool isShooter = _gs.HunterPendingShooter == LocalPlayerId;
            if (!isShooter)
            {
                var who = _gs.HunterPendingShooter;
                BuildNightWaitPanel(who >= 0
                    ? $"🎯 {PseudoOf(who)} (Chasseur) vise une dernière cible…"
                    : "🎯 Un Chasseur va tirer…");
                return;
            }

            BuildPickHint("🎯 Tu es mort — vise ta dernière cartouche",
                DesignTokens.Colors.Blood500);
        }

        void UpdateHunterPanel() { }

        // =====================================================================
        // Story 3.8 — Fin de partie
        // =====================================================================

        void BuildEndPanel()
        {
            string campText = _gs.WinnerCamp switch
            {
                0 => "🏆 Le Village l'emporte !",
                1 => "🐺 Les Loups-Garous l'emportent !",
                2 => "🪦 Match nul — aucun survivant.",
                _ => "Partie terminée.",
            };
            Color campColor = _gs.WinnerCamp switch
            {
                0 => DesignTokens.Colors.Fire500,
                1 => DesignTokens.Colors.Blood500,
                _ => DesignTokens.Colors.Moon300,
            };

            var panel = UIFactory.CreatePanel(_phaseRoot, "EndPanel", new Color(0, 0, 0, 0));
            panel.GetComponent<UnityEngine.UI.Image>().raycastTarget = false;
            panel.anchorMin = Vector2.zero;
            panel.anchorMax = Vector2.one;
            panel.offsetMin = Vector2.zero;
            panel.offsetMax = Vector2.zero;

            var title = UIFactory.CreateText(panel, campText, UIFactory.TextStyle.Display, campColor);
            title.rectTransform.anchorMin = new Vector2(0f, 0.7f);
            title.rectTransform.anchorMax = new Vector2(1f, 0.88f);
            title.rectTransform.offsetMin = Vector2.zero;
            title.rectTransform.offsetMax = Vector2.zero;

            // Récap : rôles de tous
            var list = BuildPlayerScrollList(panel, 0.18f, 0.7f);
            for (int pid = 0; pid < GameState.MaxPlayers; pid++)
            {
                var r = _gs.Roles.Get(pid);
                if (r < 0) continue;
                var role = (RoleId)r;
                var row = UIFactory.CreatePanel(list, $"End_{pid}", DesignTokens.Colors.Night700);
                row.sizeDelta = new Vector2(0f, 72f);
                row.gameObject.AddComponent<LayoutElement>().preferredHeight = 72f;
                var alive = _gs.Alive.Get(pid);
                var t = UIFactory.CreateText(row,
                    $"{(alive ? "💚" : "💀")} {PseudoOf(pid)} — {RoleCatalog.EmojiFor(role)} {RoleCatalog.LabelFor(role)}",
                    UIFactory.TextStyle.Body, DesignTokens.Colors.Moon100, TextAlignmentOptions.MidlineLeft);
                t.rectTransform.anchorMin = new Vector2(0f, 0f);
                t.rectTransform.anchorMax = new Vector2(1f, 1f);
                t.rectTransform.offsetMin = new Vector2(16f, 0f);
                t.rectTransform.offsetMax = new Vector2(-16f, 0f);
            }

            var home = UIFactory.CreateButton(panel, "Retour à l'accueil",
                UIFactory.ButtonStyle.Primary, new Vector2(560f, 96f));
            var hrt = home.GetComponent<RectTransform>();
            hrt.anchorMin = new Vector2(0.5f, 0.05f);
            hrt.anchorMax = new Vector2(0.5f, 0.05f);
            hrt.pivot = new Vector2(0.5f, 0.5f);
            hrt.anchoredPosition = Vector2.zero;
            home.onClick.AddListener(OnQuit);
        }

        // =====================================================================
        // Attente générique (pour joueurs non-actifs dans une phase)
        // =====================================================================

        void BuildNightWaitPanel(string msg)
        {
            var panel = UIFactory.CreatePanel(_phaseRoot, "Wait", new Color(0, 0, 0, 0));
            panel.GetComponent<UnityEngine.UI.Image>().raycastTarget = false;
            panel.anchorMin = Vector2.zero;
            panel.anchorMax = Vector2.one;
            panel.offsetMin = Vector2.zero;
            panel.offsetMax = Vector2.zero;

            var t = UIFactory.CreateText(panel, msg, UIFactory.TextStyle.H1, DesignTokens.Colors.Moon300);
            t.rectTransform.anchorMin = new Vector2(0f, 0.3f);
            t.rectTransform.anchorMax = new Vector2(1f, 0.7f);
            t.rectTransform.offsetMin = new Vector2(48f, 0f);
            t.rectTransform.offsetMax = new Vector2(-48f, 0f);

            if (!IsLocalAlive())
            {
                var dead = UIFactory.CreateText(panel,
                    "Tu es mort(e) — tu observes la veillée en silence.",
                    UIFactory.TextStyle.Body, DesignTokens.Colors.Fog500);
                dead.rectTransform.anchorMin = new Vector2(0f, 0.15f);
                dead.rectTransform.anchorMax = new Vector2(1f, 0.25f);
                dead.rectTransform.offsetMin = new Vector2(48f, 0f);
                dead.rectTransform.offsetMax = new Vector2(-48f, 0f);
            }
        }

        // =====================================================================
        // Helpers
        // =====================================================================

        int LocalPlayerId => _room?.Runner != null ? _room.Runner.LocalPlayer.PlayerId : -1;
        RoleId LocalRole() => _gs.GetRoleOf(LocalPlayerId);
        bool IsLocalAlive() => _gs.IsAlive(LocalPlayerId);

        string PseudoOf(int pid)
        {
            if (_lobby != null && pid >= 0 && pid < LobbyState.MaxPlayers)
            {
                var s = _lobby.Pseudos[pid].ToString();
                if (!string.IsNullOrEmpty(s)) return s;
            }
            return $"Joueur-{pid}";
        }

        string AvatarPrefix(int pid) => pid == LocalPlayerId ? "👉 " : "";

        // Story 5.8 — Bandeau d'indication FPV. Laisse la zone centrale visible sur la
        // 3D (pas de fond plein) : juste un titre tout en haut du phaseRoot.
        void BuildPickHint(string text, Color color)
        {
            var t = UIFactory.CreateText(_phaseRoot, text, UIFactory.TextStyle.H2, color);
            t.rectTransform.anchorMin = new Vector2(0f, 0.88f);
            t.rectTransform.anchorMax = new Vector2(1f, 1f);
            t.rectTransform.offsetMin = new Vector2(24f, 0f);
            t.rectTransform.offsetMax = new Vector2(-24f, -16f);
        }

        RectTransform BuildPlayerScrollList(RectTransform parent, float yMin, float yMax)
        {
            var listGo = new GameObject("List", typeof(RectTransform), typeof(VerticalLayoutGroup));
            listGo.transform.SetParent(parent, false);
            var rt = listGo.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.05f, yMin);
            rt.anchorMax = new Vector2(0.95f, yMax);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var vlg = listGo.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = DesignTokens.Spacing.Sm;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childControlHeight = false;
            vlg.childControlWidth = true;
            return rt;
        }

        static void FullFill(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        string PhaseTitleFor(GamePhase p, int day) => p switch
        {
            GamePhase.Idle          => "Préparation…",
            GamePhase.Distribution  => "🎭 Distribution des rôles",
            GamePhase.NightVoyante  => $"🌙 Nuit {day} — Voyante",
            GamePhase.NightLoups    => $"🌙 Nuit {day} — Loups-Garous",
            GamePhase.NightSorciere => $"🌙 Nuit {day} — Sorcière",
            GamePhase.NightResolve  => "🌙 Résolution de la nuit",
            GamePhase.DayDebate     => $"☀️ Jour {day} — Débat & Vote",
            GamePhase.DayVote       => $"☀️ Jour {day} — Débat & Vote",
            GamePhase.HunterShot    => "🎯 Tir du Chasseur",
            GamePhase.RevealDeath   => "💀 …",
            GamePhase.Ended         => "🏁 Fin de partie",
            _                       => p.ToString(),
        };

        Color PhaseColorFor(GamePhase p) => p switch
        {
            GamePhase.NightVoyante  => DesignTokens.Colors.Crystal500,
            GamePhase.NightLoups    => DesignTokens.Colors.Blood500,
            GamePhase.NightSorciere => DesignTokens.Colors.Poison500,
            GamePhase.DayDebate     => DesignTokens.Colors.Fire500,
            GamePhase.DayVote       => DesignTokens.Colors.Fire500,
            GamePhase.HunterShot    => DesignTokens.Colors.Blood500,
            GamePhase.Ended         => DesignTokens.Colors.Gold500,
            _                       => DesignTokens.Colors.Moon100,
        };

        string RoleDescriptionFor(RoleId r) => r switch
        {
            RoleId.Villageois  => "Tu es un simple villageois. Démasque les Loups avant qu'ils ne t'éliminent.",
            RoleId.LoupGarou   => "Chaque nuit, vote avec tes frères loups pour dévorer un villageois. Le jour, fais-toi passer pour innocent.",
            RoleId.Voyante     => "Chaque nuit, tu peux découvrir le rôle véritable d'un autre joueur. Orient le village sans te faire démasquer.",
            RoleId.Sorciere    => "Tu possèdes deux potions : une de vie (sauver la victime des Loups) et une de mort (éliminer un joueur). Utilise-les avec sagesse.",
            RoleId.Chasseur    => "Quand tu meurs, tu tires une dernière cartouche sur le joueur de ton choix avant de disparaître.",
            RoleId.Cupidon     => "Dès la première nuit, désigne deux amoureux. S'ils s'entretuent ou si l'un meurt, l'autre le suit.",
            RoleId.PetiteFille => "La nuit, tu peux espionner les Loups — mais s'ils te repèrent, tu seras leur prochaine victime.",
            RoleId.Salvateur   => "Chaque nuit, protège un joueur (toi-même inclus) ; il ne pourra pas mourir des crocs des Loups cette nuit-là.",
            RoleId.Maire       => "Ton vote compte double. Tu désignes ton successeur à ta mort.",
            RoleId.Ange        => "Fais-toi lyncher par le village dès le premier jour pour gagner seul. Ou rejoue comme un villageois si tu échoues.",
            _                  => "",
        };

        // =====================================================================

        void OnQuit()
        {
            _room?.LeaveRoom();
            NavigationService.Instance.Show<HomeScreen>();
        }
    }
}
