using LaVeillee.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LaVeillee.UI
{
    /// Story 2.1 — Écran d'accueil joueur.
    /// 2 CTA principaux (Créer / Rejoindre), avatar + pseudo top-right, menu hamburger top-left.
    public class HomeScreen : ScreenBase
    {
        Button _createBtn;
        Button _joinBtn;
        Button _menuBtn;
        TextMeshProUGUI _pseudoText;
        GameObject _menuSheet;

        protected override void BuildUI()
        {
            UIFactory.CreateScreenCanvas("HomeCanvas", out var canvasGo);
            canvasGo.transform.SetParent(transform, false);
            // Fond transparent → la Main Camera affiche la scène 3D (feu de camp + skybox) derrière.
            Root = UIFactory.CreateFullscreen(canvasGo.transform, "HomeRoot", new Color(0f, 0f, 0f, 0f));
            // Scrim sombre pour garantir la lisibilité du texte par-dessus la 3D.
            var scrim = UIFactory.CreateFullscreen(Root, "Scrim", new Color(0.043f, 0.070f, 0.141f, 0.55f));
            scrim.GetComponent<UnityEngine.UI.Image>().raycastTarget = false;

            BuildTopBar();
            BuildHero();
            BuildCTAs();
            BuildMenuSheet();
        }

        void BuildTopBar()
        {
            // Hamburger top-left
            _menuBtn = UIFactory.CreateButton(Root, "≡", UIFactory.ButtonStyle.Ghost, new Vector2(72f, 72f));
            var mrt = _menuBtn.GetComponent<RectTransform>();
            mrt.anchorMin = new Vector2(0f, 1f);
            mrt.anchorMax = new Vector2(0f, 1f);
            mrt.pivot = new Vector2(0f, 1f);
            mrt.anchoredPosition = new Vector2(DesignTokens.Spacing.Md, -DesignTokens.Spacing.Md);
            _menuBtn.onClick.AddListener(ToggleMenu);

            // Avatar + pseudo top-right
            var id = PlayerIdentityService.Current;
            var headerGroup = UIFactory.CreatePanel(Root, "HeaderGroup", new Color(0, 0, 0, 0));
            headerGroup.anchorMin = new Vector2(1f, 1f);
            headerGroup.anchorMax = new Vector2(1f, 1f);
            headerGroup.pivot = new Vector2(1f, 1f);
            headerGroup.anchoredPosition = new Vector2(-DesignTokens.Spacing.Md, -DesignTokens.Spacing.Md);
            headerGroup.sizeDelta = new Vector2(360f, 72f);

            // Avatar à droite du groupe
            var avatar = UIFactory.CreateAvatar(headerGroup, id.Pseudo, id.AvatarColorSeed, 72f);
            avatar.anchorMin = new Vector2(1f, 0.5f);
            avatar.anchorMax = new Vector2(1f, 0.5f);
            avatar.pivot = new Vector2(1f, 0.5f);
            avatar.anchoredPosition = Vector2.zero;

            // Pseudo à gauche de l'avatar
            _pseudoText = UIFactory.CreateText(headerGroup, id.Pseudo, UIFactory.TextStyle.H3,
                DesignTokens.Colors.Moon100, TextAlignmentOptions.MidlineRight);
            var prt = _pseudoText.rectTransform;
            prt.anchorMin = new Vector2(0f, 0f);
            prt.anchorMax = new Vector2(1f, 1f);
            prt.pivot = new Vector2(1f, 0.5f);
            prt.offsetMin = new Vector2(0f, 0f);
            prt.offsetMax = new Vector2(-84f, 0f); // leave 72 + 12 pad for avatar
        }

        void BuildHero()
        {
            var hero = UIFactory.CreatePanel(Root, "Hero", new Color(0, 0, 0, 0));
            hero.anchorMin = new Vector2(0.5f, 0.5f);
            hero.anchorMax = new Vector2(0.5f, 0.5f);
            hero.pivot = new Vector2(0.5f, 0.5f);
            hero.anchoredPosition = new Vector2(0f, 300f);
            hero.sizeDelta = new Vector2(900f, 280f);

            var title = UIFactory.CreateText(hero, "La Veillée", UIFactory.TextStyle.Display,
                DesignTokens.Colors.Fire500);
            var trt = title.rectTransform;
            trt.anchorMin = new Vector2(0f, 0.5f);
            trt.anchorMax = new Vector2(1f, 1f);
            trt.offsetMin = Vector2.zero;
            trt.offsetMax = Vector2.zero;
            title.fontSize = 96f; // hero scale over the base display token

            var subtitle = UIFactory.CreateText(hero, "Loups-Garous de Thiercelieux", UIFactory.TextStyle.Body,
                DesignTokens.Colors.Moon300);
            var srt = subtitle.rectTransform;
            srt.anchorMin = new Vector2(0f, 0f);
            srt.anchorMax = new Vector2(1f, 0.5f);
            srt.offsetMin = Vector2.zero;
            srt.offsetMax = Vector2.zero;
            subtitle.fontSize = 28f;
        }

        void BuildCTAs()
        {
            var ctaGroup = UIFactory.CreatePanel(Root, "CTAGroup", new Color(0, 0, 0, 0));
            ctaGroup.anchorMin = new Vector2(0.5f, 0f);
            ctaGroup.anchorMax = new Vector2(0.5f, 0f);
            ctaGroup.pivot = new Vector2(0.5f, 0f);
            ctaGroup.anchoredPosition = new Vector2(0f, 380f);
            ctaGroup.sizeDelta = new Vector2(760f, 260f);

            _createBtn = UIFactory.CreateButton(ctaGroup, "Créer une partie",
                UIFactory.ButtonStyle.Primary, new Vector2(760f, 110f));
            var crt = _createBtn.GetComponent<RectTransform>();
            crt.anchorMin = new Vector2(0.5f, 1f);
            crt.anchorMax = new Vector2(0.5f, 1f);
            crt.pivot = new Vector2(0.5f, 1f);
            crt.anchoredPosition = Vector2.zero;
            _createBtn.onClick.AddListener(OnCreate);

            _joinBtn = UIFactory.CreateButton(ctaGroup, "Rejoindre une partie",
                UIFactory.ButtonStyle.Secondary, new Vector2(760f, 110f));
            var jrt = _joinBtn.GetComponent<RectTransform>();
            jrt.anchorMin = new Vector2(0.5f, 0f);
            jrt.anchorMax = new Vector2(0.5f, 0f);
            jrt.pivot = new Vector2(0.5f, 0f);
            jrt.anchoredPosition = Vector2.zero;
            _joinBtn.onClick.AddListener(OnJoin);

            // Mode Solo (dev) — sous les CTAs principaux, discret mais accessible.
            var soloBtn = UIFactory.CreateButton(Root, "🛠 Solo (dev)",
                UIFactory.ButtonStyle.Ghost, new Vector2(400f, 72f));
            var srt = soloBtn.GetComponent<RectTransform>();
            srt.anchorMin = new Vector2(0.5f, 0f);
            srt.anchorMax = new Vector2(0.5f, 0f);
            srt.pivot = new Vector2(0.5f, 0f);
            srt.anchoredPosition = new Vector2(0f, 260f);
            soloBtn.onClick.AddListener(() => NavigationService.Instance.Show<DevSoloScreen>());
        }

        void BuildMenuSheet()
        {
            _menuSheet = UIFactory.CreatePanel(Root, "MenuSheet", new Color(0, 0, 0, 0.85f)).gameObject;
            var srt = _menuSheet.GetComponent<RectTransform>();
            srt.anchorMin = Vector2.zero;
            srt.anchorMax = Vector2.one;
            srt.offsetMin = Vector2.zero;
            srt.offsetMax = Vector2.zero;
            _menuSheet.SetActive(false);

            var panel = UIFactory.CreatePanel(_menuSheet.transform, "SheetPanel", DesignTokens.Colors.Night700);
            panel.anchorMin = new Vector2(0.5f, 0.5f);
            panel.anchorMax = new Vector2(0.5f, 0.5f);
            panel.sizeDelta = new Vector2(640f, 520f);

            var title = UIFactory.CreateText(panel, "Menu", UIFactory.TextStyle.H1);
            title.rectTransform.anchorMin = new Vector2(0f, 1f);
            title.rectTransform.anchorMax = new Vector2(1f, 1f);
            title.rectTransform.pivot = new Vector2(0.5f, 1f);
            title.rectTransform.anchoredPosition = new Vector2(0f, -32f);
            title.rectTransform.sizeDelta = new Vector2(0f, 48f);

            var y = -120f;
            float spacing = 96f;
            AddMenuItem(panel, "Paramètres", new Vector2(0f, y), () =>
            {
                Debug.Log("[Home] Paramètres — non-implémenté (backlog)");
                ToggleMenu();
            });
            y -= spacing;
            AddMenuItem(panel, "Mentions légales", new Vector2(0f, y), () =>
            {
                Debug.Log("[Home] Mentions légales — non-implémenté (backlog)");
                ToggleMenu();
            });
            y -= spacing;
            AddMenuItem(panel, "Déconnexion", new Vector2(0f, y), () =>
            {
                Debug.Log("[Home] Déconnexion — Sign in with Apple bloqué ADP (Story 1.2)");
                ToggleMenu();
            });

            var close = UIFactory.CreateButton(panel, "Fermer", UIFactory.ButtonStyle.Ghost, new Vector2(560f, 72f));
            var crt = close.GetComponent<RectTransform>();
            crt.anchorMin = new Vector2(0.5f, 0f);
            crt.anchorMax = new Vector2(0.5f, 0f);
            crt.pivot = new Vector2(0.5f, 0f);
            crt.anchoredPosition = new Vector2(0f, 24f);
            close.onClick.AddListener(ToggleMenu);
        }

        static void AddMenuItem(RectTransform parent, string label, Vector2 pos, System.Action onClick)
        {
            var btn = UIFactory.CreateButton(parent, label, UIFactory.ButtonStyle.Ghost, new Vector2(560f, 72f));
            var rt = btn.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = pos;
            btn.onClick.AddListener(() => onClick());
        }

        void ToggleMenu() => _menuSheet.SetActive(!_menuSheet.activeSelf);

        void OnCreate()
        {
            Debug.Log("[Home] → Créer");
            NavigationService.Instance.Show<CreateRoomScreen>();
        }

        void OnJoin()
        {
            Debug.Log("[Home] → Rejoindre");
            NavigationService.Instance.Show<JoinRoomScreen>();
        }

        public override void OnShow()
        {
            // Refresh pseudo au cas où modifié (menu param)
            _pseudoText.text = PlayerIdentityService.Current.Pseudo;
        }
    }
}
