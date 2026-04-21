using LaVeillee.Networking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LaVeillee.UI
{
    /// Story 2.6 — Configuration timers nuit/jour + mode Campfire/Remote.
    public class TimersDialog : MonoBehaviour
    {
        LobbyState _lobby;
        Slider _nightSlider;
        Slider _daySlider;
        TextMeshProUGUI _nightValue;
        TextMeshProUGUI _dayValue;
        Toggle _campfireToggle;
        Toggle _remoteToggle;

        public static void Open(LobbyState lobby)
        {
            var go = new GameObject("[TimersDialog]");
            var d = go.AddComponent<TimersDialog>();
            d._lobby = lobby;
            d.Build();
        }

        void Build()
        {
            UIFactory.CreateScreenCanvas("TimersCanvas", out var canvasGo);
            canvasGo.transform.SetParent(transform, false);
            canvasGo.GetComponent<Canvas>().sortingOrder = 500;
            var root = UIFactory.CreateFullscreen(canvasGo.transform, "Backdrop",
                new Color(0f, 0f, 0f, 0.85f));

            var panel = UIFactory.CreatePanel(root, "Panel", DesignTokens.Colors.Night700);
            panel.anchorMin = new Vector2(0.5f, 0.5f);
            panel.anchorMax = new Vector2(0.5f, 0.5f);
            panel.pivot = new Vector2(0.5f, 0.5f);
            panel.sizeDelta = new Vector2(960f, 1100f);

            var title = UIFactory.CreateText(panel, "Timers & mode", UIFactory.TextStyle.H1);
            title.rectTransform.anchorMin = new Vector2(0f, 1f);
            title.rectTransform.anchorMax = new Vector2(1f, 1f);
            title.rectTransform.pivot = new Vector2(0.5f, 1f);
            title.rectTransform.anchoredPosition = new Vector2(0f, -32f);
            title.rectTransform.sizeDelta = new Vector2(0f, 56f);

            BuildSlider(panel, "Nuit (par rôle)", _lobby.NightDurationSeconds, 60, 300, 0.65f,
                out _nightSlider, out _nightValue);
            BuildSlider(panel, "Jour (débat)", _lobby.DayDurationSeconds, 120, 600, 0.5f,
                out _daySlider, out _dayValue);

            _nightSlider.onValueChanged.AddListener(v => _nightValue.text = FormatSeconds((int)v));
            _daySlider.onValueChanged.AddListener(v => _dayValue.text = FormatSeconds((int)v));

            // Mode toggles — ToggleGroup pour exclusivité
            var group = panel.gameObject.AddComponent<ToggleGroup>();

            BuildModeToggle(panel, "🔥 Campfire (1 tél au centre)",
                "Requiert que tout le monde soit dans la même pièce — Bluetooth activé.",
                0.3f, group, _lobby.ModeCampfire, out _campfireToggle);
            BuildModeToggle(panel, "📱 Remote (chacun son tél)",
                "Chacun joue depuis son téléphone, peu importe la distance. Vocal via internet.",
                0.12f, group, !_lobby.ModeCampfire, out _remoteToggle);

            var cancel = UIFactory.CreateButton(panel, "Annuler", UIFactory.ButtonStyle.Secondary,
                new Vector2(400f, 88f));
            var crt = cancel.GetComponent<RectTransform>();
            crt.anchorMin = new Vector2(0.5f, 0f);
            crt.anchorMax = new Vector2(0.5f, 0f);
            crt.pivot = new Vector2(0.5f, 0f);
            crt.anchoredPosition = new Vector2(-220f, 48f);
            cancel.onClick.AddListener(Close);

            var validate = UIFactory.CreateButton(panel, "Valider", UIFactory.ButtonStyle.Primary,
                new Vector2(400f, 88f));
            var vrt = validate.GetComponent<RectTransform>();
            vrt.anchorMin = new Vector2(0.5f, 0f);
            vrt.anchorMax = new Vector2(0.5f, 0f);
            vrt.pivot = new Vector2(0.5f, 0f);
            vrt.anchoredPosition = new Vector2(220f, 48f);
            validate.onClick.AddListener(Validate);
        }

        void BuildSlider(RectTransform parent, string label, int initial, int min, int max,
                         float anchorY, out Slider slider, out TextMeshProUGUI value)
        {
            var row = UIFactory.CreatePanel(parent, $"Slider_{label}", new Color(0, 0, 0, 0));
            row.anchorMin = new Vector2(0f, anchorY);
            row.anchorMax = new Vector2(1f, anchorY + 0.12f);
            row.offsetMin = new Vector2(24f, 0f);
            row.offsetMax = new Vector2(-24f, 0f);

            var labelText = UIFactory.CreateText(row, label, UIFactory.TextStyle.H3,
                DesignTokens.Colors.Moon100, TextAlignmentOptions.MidlineLeft);
            labelText.rectTransform.anchorMin = new Vector2(0f, 0.5f);
            labelText.rectTransform.anchorMax = new Vector2(0.6f, 1f);
            labelText.rectTransform.offsetMin = Vector2.zero;
            labelText.rectTransform.offsetMax = Vector2.zero;

            value = UIFactory.CreateText(row, FormatSeconds(initial), UIFactory.TextStyle.H3,
                DesignTokens.Colors.Fire500, TextAlignmentOptions.MidlineRight);
            value.rectTransform.anchorMin = new Vector2(0.75f, 0.5f);
            value.rectTransform.anchorMax = new Vector2(1f, 1f);
            value.rectTransform.offsetMin = Vector2.zero;
            value.rectTransform.offsetMax = Vector2.zero;

            // Slider — Unity's built-in slider via components
            var sliderGo = new GameObject("Slider", typeof(RectTransform));
            sliderGo.transform.SetParent(row, false);
            var srt = sliderGo.GetComponent<RectTransform>();
            srt.anchorMin = new Vector2(0f, 0f);
            srt.anchorMax = new Vector2(1f, 0.5f);
            srt.offsetMin = Vector2.zero;
            srt.offsetMax = Vector2.zero;

            // Background
            var bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(sliderGo.transform, false);
            var bgrt = bg.GetComponent<RectTransform>();
            bgrt.anchorMin = new Vector2(0f, 0.4f);
            bgrt.anchorMax = new Vector2(1f, 0.6f);
            bgrt.offsetMin = Vector2.zero;
            bgrt.offsetMax = Vector2.zero;
            bg.GetComponent<Image>().color = DesignTokens.Colors.Night900;

            // Fill area
            var fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(sliderGo.transform, false);
            var fart = fillArea.GetComponent<RectTransform>();
            fart.anchorMin = new Vector2(0f, 0.4f);
            fart.anchorMax = new Vector2(1f, 0.6f);
            fart.offsetMin = new Vector2(8f, 0f);
            fart.offsetMax = new Vector2(-8f, 0f);
            var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(fillArea.transform, false);
            var fillrt = fill.GetComponent<RectTransform>();
            fillrt.anchorMin = Vector2.zero;
            fillrt.anchorMax = new Vector2(0.5f, 1f);
            fillrt.offsetMin = Vector2.zero;
            fillrt.offsetMax = Vector2.zero;
            fill.GetComponent<Image>().color = DesignTokens.Colors.Fire500;

            // Handle area
            var handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
            handleArea.transform.SetParent(sliderGo.transform, false);
            var hart = handleArea.GetComponent<RectTransform>();
            hart.anchorMin = Vector2.zero;
            hart.anchorMax = Vector2.one;
            hart.offsetMin = new Vector2(16f, 0f);
            hart.offsetMax = new Vector2(-16f, 0f);
            var handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
            handle.transform.SetParent(handleArea.transform, false);
            var hrt = handle.GetComponent<RectTransform>();
            hrt.sizeDelta = new Vector2(32f, 48f);
            handle.GetComponent<Image>().color = DesignTokens.Colors.Moon100;

            slider = sliderGo.AddComponent<Slider>();
            slider.targetGraphic = handle.GetComponent<Image>();
            slider.fillRect = fillrt;
            slider.handleRect = hrt;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = min;
            slider.maxValue = max;
            slider.wholeNumbers = true;
            slider.value = initial;
        }

        void BuildModeToggle(RectTransform parent, string label, string desc, float anchorY,
                             ToggleGroup group, bool isOn, out Toggle toggle)
        {
            var row = UIFactory.CreatePanel(parent, $"Toggle_{label}",
                DesignTokens.Colors.Night900);
            row.anchorMin = new Vector2(0f, anchorY);
            row.anchorMax = new Vector2(1f, anchorY + 0.15f);
            row.offsetMin = new Vector2(24f, 0f);
            row.offsetMax = new Vector2(-24f, 0f);

            var toggleGo = new GameObject("Toggle", typeof(RectTransform), typeof(Toggle));
            toggleGo.transform.SetParent(row, false);
            var trt = toggleGo.GetComponent<RectTransform>();
            trt.anchorMin = new Vector2(0f, 0.5f);
            trt.anchorMax = new Vector2(0f, 0.5f);
            trt.pivot = new Vector2(0f, 0.5f);
            trt.anchoredPosition = new Vector2(16f, 0f);
            trt.sizeDelta = new Vector2(48f, 48f);

            var bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(toggleGo.transform, false);
            var bgrt = bg.GetComponent<RectTransform>();
            bgrt.anchorMin = Vector2.zero;
            bgrt.anchorMax = Vector2.one;
            bgrt.offsetMin = Vector2.zero;
            bgrt.offsetMax = Vector2.zero;
            bg.GetComponent<Image>().color = DesignTokens.Colors.Night700;

            var checkmark = new GameObject("Checkmark", typeof(RectTransform), typeof(Image));
            checkmark.transform.SetParent(bg.transform, false);
            var crt = checkmark.GetComponent<RectTransform>();
            crt.anchorMin = Vector2.zero;
            crt.anchorMax = Vector2.one;
            crt.offsetMin = new Vector2(8f, 8f);
            crt.offsetMax = new Vector2(-8f, -8f);
            checkmark.GetComponent<Image>().color = DesignTokens.Colors.Fire500;

            toggle = toggleGo.GetComponent<Toggle>();
            toggle.targetGraphic = bg.GetComponent<Image>();
            toggle.graphic = checkmark.GetComponent<Image>();
            toggle.group = group;
            toggle.isOn = isOn;

            var labelText = UIFactory.CreateText(row, label, UIFactory.TextStyle.H3,
                DesignTokens.Colors.Moon100, TextAlignmentOptions.MidlineLeft);
            labelText.rectTransform.anchorMin = new Vector2(0f, 0.5f);
            labelText.rectTransform.anchorMax = new Vector2(1f, 1f);
            labelText.rectTransform.offsetMin = new Vector2(88f, 0f);
            labelText.rectTransform.offsetMax = Vector2.zero;

            var descText = UIFactory.CreateText(row, desc, UIFactory.TextStyle.Caption,
                DesignTokens.Colors.Moon300, TextAlignmentOptions.TopLeft);
            descText.rectTransform.anchorMin = new Vector2(0f, 0f);
            descText.rectTransform.anchorMax = new Vector2(1f, 0.5f);
            descText.rectTransform.offsetMin = new Vector2(88f, 8f);
            descText.rectTransform.offsetMax = new Vector2(-16f, 0f);
        }

        static string FormatSeconds(int sec) => $"{sec / 60}:{(sec % 60):D2}";

        void Validate()
        {
            _lobby.SetTimersAndMode((int)_nightSlider.value, (int)_daySlider.value, _campfireToggle.isOn);
            Close();
        }

        void Close() => Destroy(gameObject);
    }
}
