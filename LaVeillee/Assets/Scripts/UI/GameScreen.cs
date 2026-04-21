using LaVeillee.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LaVeillee.UI
{
    /// Stub d'écran partie — Épopée 3 implémentera le vrai déroulé nuit/jour.
    /// Présent ici pour que le countdown du lobby (Story 2.6) puisse naviguer
    /// quelque part sans crash.
    public class GameScreen : ScreenBase
    {
        Button _quitBtn;

        protected override void BuildUI()
        {
            UIFactory.CreateScreenCanvas("GameCanvas", out var canvasGo);
            canvasGo.transform.SetParent(transform, false);
            Root = UIFactory.CreateFullscreen(canvasGo.transform, "GameRoot", DesignTokens.Colors.Night900);

            var center = UIFactory.CreatePanel(Root, "Center", new Color(0, 0, 0, 0));
            center.anchorMin = new Vector2(0.5f, 0.5f);
            center.anchorMax = new Vector2(0.5f, 0.5f);
            center.pivot = new Vector2(0.5f, 0.5f);
            center.sizeDelta = new Vector2(900f, 600f);

            var title = UIFactory.CreateText(center, "🌙 Partie en cours", UIFactory.TextStyle.Display,
                DesignTokens.Colors.Fire500);
            var trt = title.rectTransform;
            trt.anchorMin = new Vector2(0f, 0.65f);
            trt.anchorMax = new Vector2(1f, 0.9f);
            trt.offsetMin = Vector2.zero;
            trt.offsetMax = Vector2.zero;

            var sub = UIFactory.CreateText(center,
                "Le vrai jeu arrive à l'Épopée 3 (rôles, nuit/jour, votes).",
                UIFactory.TextStyle.Body, DesignTokens.Colors.Moon300);
            var srt = sub.rectTransform;
            srt.anchorMin = new Vector2(0f, 0.4f);
            srt.anchorMax = new Vector2(1f, 0.6f);
            srt.offsetMin = Vector2.zero;
            srt.offsetMax = Vector2.zero;

            _quitBtn = UIFactory.CreateButton(center, "Quitter la partie",
                UIFactory.ButtonStyle.Danger, new Vector2(520f, 96f));
            var qrt = _quitBtn.GetComponent<RectTransform>();
            qrt.anchorMin = new Vector2(0.5f, 0.1f);
            qrt.anchorMax = new Vector2(0.5f, 0.1f);
            qrt.pivot = new Vector2(0.5f, 0.5f);
            qrt.anchoredPosition = Vector2.zero;
            _quitBtn.onClick.AddListener(OnQuit);
        }

        void OnQuit()
        {
            var services = GameServices.Instance;
            services?.Room?.LeaveRoom();
            NavigationService.Instance.Show<HomeScreen>();
        }
    }
}
