using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LaVeillee.UI
{
    /// Helpers statiques pour construire une UI cohérente avec DesignTokens.
    /// Alternative aux prefabs : on monte les écrans en code (sans dépendance à un
    /// Editor pre-processing), ce qui rend les scènes reproduisibles depuis rien
    /// et évite les prefabs cassés quand Unity régénère ses GUIDs.
    public static class UIFactory
    {
        public enum ButtonStyle { Primary, Secondary, Danger, Ghost }
        public enum TextStyle { Display, H1, H2, H3, Body, Caption, Label }

        public static Canvas CreateScreenCanvas(string name, out GameObject canvasGo)
        {
            canvasGo = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f); // iPhone portrait baseline
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            // Need an EventSystem for input — check scene or add if missing.
            if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var es = new GameObject("EventSystem",
                    typeof(UnityEngine.EventSystems.EventSystem),
                    typeof(UnityEngine.EventSystems.StandaloneInputModule));
                Object.DontDestroyOnLoad(es);
            }

            return canvas;
        }

        public static RectTransform CreatePanel(Transform parent, string name, Color bg)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            go.GetComponent<Image>().color = bg;
            return rt;
        }

        public static RectTransform CreateFullscreen(Transform parent, string name, Color bg)
        {
            var rt = CreatePanel(parent, name, bg);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return rt;
        }

        public static TextMeshProUGUI CreateText(Transform parent, string content, TextStyle style, Color? color = null, TextAlignmentOptions align = TextAlignmentOptions.Center)
        {
            var go = new GameObject($"Text_{style}", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.font = EnsureDefaultFont();
            tmp.text = content;
            tmp.color = color ?? DesignTokens.Colors.Moon100;
            tmp.alignment = align;
            tmp.enableWordWrapping = true;
            tmp.raycastTarget = false;
            ApplyTextStyle(tmp, style);
            return tmp;
        }

        static TMP_FontAsset _defaultFontAsset;

        /// TMP a besoin d'un FontAsset sinon le texte ne rend rien. On by-pass complètement
        /// TMP_Settings (dont le getter NPE si TMP Essentials pas importé) et on fabrique
        /// systématiquement un TMP_FontAsset dynamique depuis LegacyRuntime.ttf.
        static TMP_FontAsset EnsureDefaultFont()
        {
            if (_defaultFontAsset != null) return _defaultFontAsset;

            var builtinFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                              ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (builtinFont == null)
            {
                Debug.LogError("[UIFactory] Aucune font Unity built-in trouvée — texte ne rendra pas.");
                return null;
            }

            _defaultFontAsset = TMP_FontAsset.CreateFontAsset(builtinFont);
            _defaultFontAsset.name = "LaVeilleeRuntimeTMPFont";
            return _defaultFontAsset;
        }

        static void ApplyTextStyle(TextMeshProUGUI tmp, TextStyle s)
        {
            switch (s)
            {
                case TextStyle.Display:
                    tmp.fontSize = DesignTokens.Text.Display;
                    tmp.fontStyle = FontStyles.Bold;
                    tmp.characterSpacing = -0.5f;
                    break;
                case TextStyle.H1:
                    tmp.fontSize = DesignTokens.Text.H1;
                    tmp.fontStyle = FontStyles.Bold;
                    break;
                case TextStyle.H2:
                    tmp.fontSize = DesignTokens.Text.H2;
                    tmp.fontStyle = FontStyles.Bold;
                    break;
                case TextStyle.H3:
                    tmp.fontSize = DesignTokens.Text.H3;
                    tmp.fontStyle = FontStyles.Normal;
                    break;
                case TextStyle.Body:
                    tmp.fontSize = DesignTokens.Text.Body;
                    tmp.fontStyle = FontStyles.Normal;
                    break;
                case TextStyle.Caption:
                    tmp.fontSize = DesignTokens.Text.Caption;
                    tmp.fontStyle = FontStyles.Normal;
                    break;
                case TextStyle.Label:
                    tmp.fontSize = DesignTokens.Text.Label;
                    tmp.fontStyle = FontStyles.Bold;
                    tmp.characterSpacing = 1f;
                    tmp.text = tmp.text.ToUpperInvariant();
                    break;
            }
        }

        /// Crée un bouton tokenisé. Retourne le composant Button — caller wire le onClick.
        public static Button CreateButton(Transform parent, string label, ButtonStyle style, Vector2 size)
        {
            var go = new GameObject($"Btn_{label}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.sizeDelta = size;

            var img = go.GetComponent<Image>();
            var btn = go.GetComponent<Button>();

            Color bg, textColor, outlineColor;
            bool hasOutline, hasHalo;
            switch (style)
            {
                case ButtonStyle.Primary:
                    bg = DesignTokens.Colors.Fire500;
                    textColor = DesignTokens.Colors.Moon100;
                    outlineColor = DesignTokens.Colors.Fire300;
                    hasOutline = false;
                    hasHalo = true;
                    break;
                case ButtonStyle.Secondary:
                    bg = new Color(0, 0, 0, 0); // transparent
                    textColor = DesignTokens.Colors.Moon100;
                    outlineColor = DesignTokens.Colors.Moon100;
                    hasOutline = true;
                    hasHalo = false;
                    break;
                case ButtonStyle.Danger:
                    bg = DesignTokens.Colors.Blood500;
                    textColor = DesignTokens.Colors.Moon100;
                    outlineColor = DesignTokens.Colors.Moon100;
                    hasOutline = false;
                    hasHalo = false;
                    break;
                default: // Ghost
                    bg = new Color(0, 0, 0, 0);
                    textColor = DesignTokens.Colors.Moon300;
                    outlineColor = DesignTokens.Colors.Fog500;
                    hasOutline = false;
                    hasHalo = false;
                    break;
            }

            img.color = bg;

            var colors = btn.colors;
            colors.normalColor      = Color.white;
            colors.highlightedColor = new Color(1.05f, 1.05f, 1.05f, 1f);
            colors.pressedColor     = new Color(0.85f, 0.85f, 0.85f, 1f);
            colors.selectedColor    = Color.white;
            colors.disabledColor    = DesignTokens.Colors.Fog500;
            btn.colors = colors;

            if (hasOutline)
            {
                var outline = go.AddComponent<Outline>();
                outline.effectColor = outlineColor;
                outline.effectDistance = new Vector2(2f, -2f);
            }

            if (hasHalo)
            {
                // Approximation du `shadow.glow.fire` (blur=16, opacity=0.6) avec un Shadow
                // component. Pas de vrai blur sans shader custom — on accepte l'approx.
                var glow = go.AddComponent<Shadow>();
                glow.effectColor = new Color(DesignTokens.Colors.Fire300.r,
                                             DesignTokens.Colors.Fire300.g,
                                             DesignTokens.Colors.Fire300.b, 0.6f);
                glow.effectDistance = new Vector2(0f, 0f);
                glow.useGraphicAlpha = false;
            }

            var text = CreateText(go.transform, label, TextStyle.H3, textColor, TextAlignmentOptions.Center);
            var textRt = text.rectTransform;
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(DesignTokens.Spacing.Md, DesignTokens.Spacing.Sm);
            textRt.offsetMax = new Vector2(-DesignTokens.Spacing.Md, -DesignTokens.Spacing.Sm);
            return btn;
        }

        /// Avatar placeholder : carré arrondi coloré avec l'initiale du pseudo.
        /// (Vrai cercle demanderait un sprite importé — post-MVP.)
        public static RectTransform CreateAvatar(Transform parent, string pseudo, int colorSeed, float size)
        {
            var go = new GameObject("Avatar", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.sizeDelta = new Vector2(size, size);

            var img = go.GetComponent<Image>();
            img.color = DesignTokens.Colors.AvatarFor(colorSeed);
            // Pas de sprite = carré plein de la couleur du joueur. Les sprites
            // built-in d'Unity 6 ("UI/Skin/Knob.psd" etc.) ne sont plus disponibles.
            img.sprite = null;
            img.type = Image.Type.Simple;

            var initial = string.IsNullOrWhiteSpace(pseudo) ? "?" : char.ToUpperInvariant(pseudo.Trim()[0]).ToString();
            var text = CreateText(go.transform, initial, TextStyle.H2, DesignTokens.Colors.Moon100);
            var textRt = text.rectTransform;
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;
            return rt;
        }
    }
}
