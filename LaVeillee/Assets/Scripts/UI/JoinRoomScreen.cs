using LaVeillee.Core;
using LaVeillee.Networking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LaVeillee.UI
{
    /// Story 2.3 — Écran de saisie de code pour rejoindre une partie.
    /// Input numérique 6 chiffres, paste presse-papier, erreurs FR.
    public class JoinRoomScreen : ScreenBase
    {
        TMP_InputField _codeInput;
        Button _joinBtn;
        Button _pasteBtn;
        Button _backBtn;
        TextMeshProUGUI _errorText;
        bool _listenersWired;

        protected override void BuildUI()
        {
            UIFactory.CreateScreenCanvas("JoinRoomCanvas", out var canvasGo);
            canvasGo.transform.SetParent(transform, false);
            Root = UIFactory.CreateFullscreen(canvasGo.transform, "JoinRoomRoot", DesignTokens.Colors.Night900);

            // Back button top-left
            _backBtn = UIFactory.CreateButton(Root, "←", UIFactory.ButtonStyle.Ghost, new Vector2(96f, 72f));
            var bkrt = _backBtn.GetComponent<RectTransform>();
            bkrt.anchorMin = new Vector2(0f, 1f);
            bkrt.anchorMax = new Vector2(0f, 1f);
            bkrt.pivot = new Vector2(0f, 1f);
            bkrt.anchoredPosition = new Vector2(DesignTokens.Spacing.Md, -DesignTokens.Spacing.Md);
            _backBtn.onClick.AddListener(() => NavigationService.Instance.Show<HomeScreen>());

            var center = UIFactory.CreatePanel(Root, "Center", new Color(0, 0, 0, 0));
            center.anchorMin = new Vector2(0.5f, 0.5f);
            center.anchorMax = new Vector2(0.5f, 0.5f);
            center.pivot = new Vector2(0.5f, 0.5f);
            center.sizeDelta = new Vector2(900f, 700f);

            var title = UIFactory.CreateText(center, "Rejoindre une partie", UIFactory.TextStyle.H1);
            var trt = title.rectTransform;
            trt.anchorMin = new Vector2(0f, 0.85f);
            trt.anchorMax = new Vector2(1f, 1f);
            trt.offsetMin = Vector2.zero;
            trt.offsetMax = Vector2.zero;

            var sub = UIFactory.CreateText(center, "Entre le code à 6 chiffres partagé par ton ami",
                UIFactory.TextStyle.Body, DesignTokens.Colors.Moon300);
            var srt = sub.rectTransform;
            srt.anchorMin = new Vector2(0f, 0.72f);
            srt.anchorMax = new Vector2(1f, 0.82f);
            srt.offsetMin = Vector2.zero;
            srt.offsetMax = Vector2.zero;

            BuildCodeInput(center);

            _pasteBtn = UIFactory.CreateButton(center, "Coller depuis le presse-papier",
                UIFactory.ButtonStyle.Ghost, new Vector2(560f, 72f));
            var prt = _pasteBtn.GetComponent<RectTransform>();
            prt.anchorMin = new Vector2(0.5f, 0.42f);
            prt.anchorMax = new Vector2(0.5f, 0.42f);
            prt.pivot = new Vector2(0.5f, 0.5f);
            prt.anchoredPosition = Vector2.zero;
            _pasteBtn.onClick.AddListener(PasteCode);

            _errorText = UIFactory.CreateText(center, "", UIFactory.TextStyle.Body, DesignTokens.Colors.Blood500);
            var ert = _errorText.rectTransform;
            ert.anchorMin = new Vector2(0f, 0.25f);
            ert.anchorMax = new Vector2(1f, 0.35f);
            ert.offsetMin = Vector2.zero;
            ert.offsetMax = Vector2.zero;

            _joinBtn = UIFactory.CreateButton(center, "Rejoindre",
                UIFactory.ButtonStyle.Primary, new Vector2(760f, 110f));
            var jrt = _joinBtn.GetComponent<RectTransform>();
            jrt.anchorMin = new Vector2(0.5f, 0.05f);
            jrt.anchorMax = new Vector2(0.5f, 0.05f);
            jrt.pivot = new Vector2(0.5f, 0.5f);
            jrt.anchoredPosition = Vector2.zero;
            _joinBtn.onClick.AddListener(OnJoin);
        }

        void BuildCodeInput(RectTransform parent)
        {
            var inputBg = UIFactory.CreatePanel(parent, "InputBg", DesignTokens.Colors.Night700);
            inputBg.anchorMin = new Vector2(0.5f, 0.55f);
            inputBg.anchorMax = new Vector2(0.5f, 0.55f);
            inputBg.pivot = new Vector2(0.5f, 0.5f);
            inputBg.sizeDelta = new Vector2(560f, 140f);

            var go = new GameObject("CodeInput", typeof(RectTransform), typeof(CanvasRenderer),
                typeof(Image), typeof(TMP_InputField));
            go.transform.SetParent(inputBg, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var bgImg = go.GetComponent<Image>();
            bgImg.color = new Color(0, 0, 0, 0);

            _codeInput = go.GetComponent<TMP_InputField>();
            _codeInput.contentType = TMP_InputField.ContentType.IntegerNumber;
            _codeInput.characterLimit = 6;
            _codeInput.keyboardType = TouchScreenKeyboardType.NumberPad;

            // Text area viewport (required by TMP_InputField)
            var textArea = new GameObject("TextArea", typeof(RectTransform), typeof(RectMask2D));
            textArea.transform.SetParent(go.transform, false);
            var tart = textArea.GetComponent<RectTransform>();
            tart.anchorMin = Vector2.zero;
            tart.anchorMax = Vector2.one;
            tart.offsetMin = new Vector2(16f, 8f);
            tart.offsetMax = new Vector2(-16f, -8f);
            _codeInput.textViewport = tart;

            var textComponent = UIFactory.CreateText(textArea.transform, "", UIFactory.TextStyle.Display,
                DesignTokens.Colors.Moon100);
            textComponent.name = "TextComponent";
            textComponent.fontSize = 72f;
            textComponent.characterSpacing = 12f;
            textComponent.alignment = TextAlignmentOptions.Center;
            var tcrt = textComponent.rectTransform;
            tcrt.anchorMin = Vector2.zero;
            tcrt.anchorMax = Vector2.one;
            tcrt.offsetMin = Vector2.zero;
            tcrt.offsetMax = Vector2.zero;
            _codeInput.textComponent = textComponent;

            var placeholder = UIFactory.CreateText(textArea.transform, "••••••", UIFactory.TextStyle.Display,
                DesignTokens.Colors.Fog500);
            placeholder.name = "Placeholder";
            placeholder.fontSize = 72f;
            placeholder.characterSpacing = 12f;
            placeholder.alignment = TextAlignmentOptions.Center;
            var pcrt = placeholder.rectTransform;
            pcrt.anchorMin = Vector2.zero;
            pcrt.anchorMax = Vector2.one;
            pcrt.offsetMin = Vector2.zero;
            pcrt.offsetMax = Vector2.zero;
            _codeInput.placeholder = placeholder;
        }

        public override void OnShow()
        {
            _errorText.text = "";
            _codeInput.text = "";
            // Pre-fill from deeplink if any
            if (!string.IsNullOrEmpty(DeeplinkHandler.PendingCode))
            {
                _codeInput.text = DeeplinkHandler.PendingCode;
                DeeplinkHandler.ConsumePending();
            }
            _codeInput.Select();
        }

        public override void OnHide()
        {
            UnwireRoomListeners();
        }

        void PasteCode()
        {
            var raw = GUIUtility.systemCopyBuffer ?? "";
            var digits = new System.Text.StringBuilder();
            foreach (var c in raw)
            {
                if (char.IsDigit(c)) digits.Append(c);
                if (digits.Length == 6) break;
            }
            _codeInput.text = digits.ToString();
        }

        void OnJoin()
        {
            var code = _codeInput.text?.Trim() ?? "";
            if (code.Length != 6)
            {
                _errorText.text = "Le code doit faire exactement 6 chiffres.";
                return;
            }
            _errorText.text = "Connexion à la partie…";
            _joinBtn.interactable = false;
            var services = GameServices.EnsureExists();
            WireRoomListeners(services.Room);
            services.Room.JoinRoom(code);
        }

        void WireRoomListeners(FusionRoomManager room)
        {
            if (_listenersWired) return;
            room.RoomJoined += OnRoomJoined;
            room.Error       += OnError;
            _listenersWired = true;
        }

        void UnwireRoomListeners()
        {
            if (!_listenersWired) return;
            if (GameServices.Instance?.Room is { } room)
            {
                room.RoomJoined -= OnRoomJoined;
                room.Error       -= OnError;
            }
            _listenersWired = false;
        }

        void OnRoomJoined(string roomId)
        {
            _errorText.text = "";
            UnwireRoomListeners();
            NavigationService.Instance.Show<LobbyScreen>();
        }

        void OnError(RoomError error)
        {
            UnwireRoomListeners();
            _joinBtn.interactable = true;
            _errorText.text = error switch
            {
                RoomError.RoomNotFound  => "Cette partie n'existe pas ou est terminée.",
                RoomError.RoomFull      => "Partie complète, désolé !",
                RoomError.Timeout       => "Connexion réseau perdue — réessaie.",
                RoomError.ConnectionFailed => "Impossible de joindre le serveur — réessaie.",
                _                       => "Quelque chose s'est mal passé — réessaie.",
            };
        }
    }
}
