using LaVeillee.Core;
using LaVeillee.Networking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LaVeillee.UI
{
    /// Story 2.2 — Écran de création de partie (transitoire).
    /// Affiche loading pendant CreateRoom, passe en Lobby sur succès, montre
    /// erreur FR + retry sur échec réseau.
    public class CreateRoomScreen : ScreenBase
    {
        TextMeshProUGUI _statusText;
        Button _retryBtn;
        Button _backBtn;
        bool _listenersWired;

        protected override void BuildUI()
        {
            UIFactory.CreateScreenCanvas("CreateRoomCanvas", out var canvasGo);
            canvasGo.transform.SetParent(transform, false);
            Root = UIFactory.CreateFullscreen(canvasGo.transform, "CreateRoomRoot", DesignTokens.Colors.Night900);

            var center = UIFactory.CreatePanel(Root, "Center", new Color(0, 0, 0, 0));
            center.anchorMin = new Vector2(0.5f, 0.5f);
            center.anchorMax = new Vector2(0.5f, 0.5f);
            center.pivot = new Vector2(0.5f, 0.5f);
            center.sizeDelta = new Vector2(800f, 500f);

            var title = UIFactory.CreateText(center, "Création de la partie…", UIFactory.TextStyle.H1);
            var trt = title.rectTransform;
            trt.anchorMin = new Vector2(0f, 0.7f);
            trt.anchorMax = new Vector2(1f, 1f);
            trt.offsetMin = Vector2.zero;
            trt.offsetMax = Vector2.zero;

            _statusText = UIFactory.CreateText(center, "Connexion au serveur Photon", UIFactory.TextStyle.Body,
                DesignTokens.Colors.Moon300);
            var srt = _statusText.rectTransform;
            srt.anchorMin = new Vector2(0f, 0.4f);
            srt.anchorMax = new Vector2(1f, 0.65f);
            srt.offsetMin = Vector2.zero;
            srt.offsetMax = Vector2.zero;

            _retryBtn = UIFactory.CreateButton(center, "Réessayer",
                UIFactory.ButtonStyle.Primary, new Vector2(400f, 96f));
            var rrt = _retryBtn.GetComponent<RectTransform>();
            rrt.anchorMin = new Vector2(0.5f, 0.15f);
            rrt.anchorMax = new Vector2(0.5f, 0.15f);
            rrt.pivot = new Vector2(0.5f, 0.5f);
            rrt.anchoredPosition = new Vector2(-220f, 0f);
            _retryBtn.onClick.AddListener(StartCreate);

            _backBtn = UIFactory.CreateButton(center, "Retour",
                UIFactory.ButtonStyle.Secondary, new Vector2(400f, 96f));
            var brt = _backBtn.GetComponent<RectTransform>();
            brt.anchorMin = new Vector2(0.5f, 0.15f);
            brt.anchorMax = new Vector2(0.5f, 0.15f);
            brt.pivot = new Vector2(0.5f, 0.5f);
            brt.anchoredPosition = new Vector2(220f, 0f);
            _backBtn.onClick.AddListener(() => NavigationService.Instance.Show<HomeScreen>());

            SetButtonsVisible(false);
        }

        public override void OnShow()
        {
            StartCreate();
        }

        public override void OnHide()
        {
            UnwireRoomListeners();
        }

        void StartCreate()
        {
            SetButtonsVisible(false);
            _statusText.text = "Connexion au serveur Photon…";
            var services = GameServices.EnsureExists();
            WireRoomListeners(services.Room);
            services.Room.CreateRoom();
        }

        void WireRoomListeners(FusionRoomManager room)
        {
            if (_listenersWired) return;
            room.RoomCreated += OnRoomCreated;
            room.Error       += OnError;
            _listenersWired = true;
        }

        void UnwireRoomListeners()
        {
            if (!_listenersWired) return;
            if (GameServices.Instance?.Room is { } room)
            {
                room.RoomCreated -= OnRoomCreated;
                room.Error       -= OnError;
            }
            _listenersWired = false;
        }

        void OnRoomCreated(string roomId)
        {
            _statusText.text = $"Room {roomId} prête — transition vers le lobby…";
            UnwireRoomListeners();
            NavigationService.Instance.Show<LobbyScreen>();
        }

        void OnError(RoomError error)
        {
            UnwireRoomListeners();
            _statusText.text = error switch
            {
                RoomError.InvalidAppId    => "AppId Photon invalide — vérifie la config.",
                RoomError.Timeout         => "Connexion réseau perdue — réessaie.",
                RoomError.ConnectionFailed=> "Impossible de joindre le serveur — réessaie.",
                _                         => "Quelque chose s'est mal passé — réessaie.",
            };
            SetButtonsVisible(true);
        }

        void SetButtonsVisible(bool visible)
        {
            _retryBtn.gameObject.SetActive(visible);
            _backBtn.gameObject.SetActive(visible);
        }
    }
}
