using System.Text;
using LaVeillee.Networking;
using Photon.Voice.Fusion;
using Photon.Voice.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace LaVeillee.UI
{
    /// Controller for the DevTestRoom scene.
    /// Wires the room/voice UI to FusionRoomManager + PhotonVoiceManager.
    public class DevTestRoomController : MonoBehaviour
    {
        [SerializeField] InputField _roomIdInput;
        [SerializeField] Button _createButton;
        [SerializeField] Button _joinButton;
        [SerializeField] Button _leaveButton;
        [SerializeField] Button _quitButton;
        [SerializeField] Button _muteButton;
        [SerializeField] Text _muteLabel;
        [SerializeField] Text _statusText;
        [SerializeField] Text _voiceStatusText;

        FusionRoomManager _room;
        PhotonVoiceManager _voice;

        void Awake()
        {
            var go = new GameObject("FusionRoomManager");
            DontDestroyOnLoad(go);

            // Order matters: PhotonVoiceManager → RequireComponent chain pulls in
            // Recorder + FusionVoiceClient + NetworkRunner. Then FusionRoomManager
            // reuses the same NetworkRunner via GetComponent in EnsureRunner.
            _voice = go.AddComponent<PhotonVoiceManager>();
            _room = go.AddComponent<FusionRoomManager>();

            // SpeakerPrefab: build a template GameObject that FusionVoiceClient clones for each
            // incoming remote voice stream. Must stay active so cloned instances inherit active
            // state — Unity's Instantiate preserves active flag, and a disabled AudioSource
            // refuses to play. The template itself does nothing audible because no remote voice
            // is linked to it.
            var voiceClient = go.GetComponent<FusionVoiceClient>();
            if (voiceClient.SpeakerPrefab == null)
            {
                var speakerTemplate = new GameObject("SpeakerTemplate", typeof(AudioSource), typeof(Speaker));
                speakerTemplate.transform.SetParent(go.transform, false);
                voiceClient.SpeakerPrefab = speakerTemplate;
            }

            _room.RoomCreated  += OnRoomCreated;
            _room.RoomJoined   += OnRoomJoined;
            _room.RoomLeft     += OnRoomLeft;
            _room.PlayerJoined += OnPlayerChanged;
            _room.PlayerLeft   += OnPlayerChanged;
            _room.Error        += OnError;

            _voice.Connected               += OnVoiceConnected;
            _voice.Disconnected            += OnVoiceDisconnected;
            _voice.SpeakerStartedTalking   += OnSpeakerStarted;
            _voice.SpeakerStoppedTalking   += OnSpeakerStopped;
            _voice.MicPermissionChanged    += OnMicPermissionChanged;
            _voice.Error                   += OnVoiceError;

            _createButton.onClick.AddListener(() =>
            {
                var rid = string.IsNullOrWhiteSpace(_roomIdInput.text) ? null : _roomIdInput.text.Trim().ToUpperInvariant();
                _room.CreateRoom(rid);
                SetStatus($"Création room {(rid ?? "auto")}…");
                RequestMicIfNeeded();
            });

            _joinButton.onClick.AddListener(() =>
            {
                var rid = _roomIdInput.text?.Trim().ToUpperInvariant();
                _room.JoinRoom(rid);
                SetStatus($"Connexion à room {rid}…");
                RequestMicIfNeeded();
            });

            _leaveButton.onClick.AddListener(() =>
            {
                _room.LeaveRoom();
                SetStatus("Déconnexion…");
            });

            _quitButton.onClick.AddListener(() =>
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            });

            if (_muteButton != null)
            {
                _muteButton.onClick.AddListener(() =>
                {
                    _voice.SetMuted(!_voice.IsMuted);
                    RefreshMuteLabel();
                });
            }
            RefreshMuteLabel();
            SetVoiceStatus("Vocal : en attente.");
            SetStatus("Prêt. Crée ou rejoins une room.");
        }

        void RequestMicIfNeeded()
        {
            if (_voice.MicPermission == MicPermissionStatus.Granted) return;
            _voice.RequestMicPermission(status =>
            {
                if (status == MicPermissionStatus.Denied)
                    SetVoiceStatus("Vocal : micro refusé. Active-le dans Réglages → La Veillée.");
                else
                    SetVoiceStatus("Vocal : micro autorisé.");
                RefreshMuteLabel();
            });
        }

        void RefreshMuteLabel()
        {
            if (_muteLabel == null) return;
            _muteLabel.text = _voice.IsMuted ? "🔇 Activer micro" : "🎙 Couper micro";
        }

        void OnRoomCreated(string roomId)
        {
            _roomIdInput.text = roomId;
            RefreshStatus($"Room créée : {roomId}");
        }

        void OnRoomJoined(string roomId)    => RefreshStatus($"Rejoint : {roomId}");
        void OnRoomLeft(RoomLeaveReason r)  => SetStatus($"Quitté : {r}");
        void OnPlayerChanged(PlayerHandle _) => RefreshStatus($"Room : {_room.CurrentRoomId}");
        void OnError(RoomError e)           => SetStatus($"⚠ Erreur room : {e}");

        void OnVoiceConnected()             => SetVoiceStatus("Vocal : connecté ✓");
        void OnVoiceDisconnected(VoiceLeaveReason r) => SetVoiceStatus($"Vocal : déconnecté ({r})");
        void OnSpeakerStarted(int playerId) => RefreshVoiceStatus();
        void OnSpeakerStopped(int playerId) => RefreshVoiceStatus();
        void OnMicPermissionChanged(MicPermissionStatus s) => SetVoiceStatus($"Vocal : permission micro = {s}");
        void OnVoiceError(VoiceError e)     => SetVoiceStatus($"⚠ Erreur vocal : {e}");

        void RefreshStatus(string header)
        {
            var sb = new StringBuilder();
            sb.AppendLine(header);
            sb.AppendLine($"Local : {_room.LocalPlayer}");
            sb.AppendLine("Joueurs :");
            foreach (var p in _room.GetPlayersInRoom())
                sb.AppendLine($"  • {p}");
            SetStatus(sb.ToString());
        }

        void RefreshVoiceStatus()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Vocal : {(_voice.IsConnected ? "connecté" : "hors-ligne")} · {(_voice.IsMuted ? "muet" : "ouvert")}");
            if (_voice.CurrentSpeakers.Count > 0)
            {
                sb.Append("Parle : ");
                foreach (var pid in _voice.CurrentSpeakers) sb.Append($"#{pid} ");
            }
            else
            {
                sb.Append("Personne ne parle.");
            }
            SetVoiceStatus(sb.ToString());
        }

        void SetStatus(string s)
        {
            if (_statusText != null) _statusText.text = s;
            Debug.Log($"[DevTestRoom] {s}");
        }

        void SetVoiceStatus(string s)
        {
            if (_voiceStatusText != null) _voiceStatusText.text = s;
            Debug.Log($"[DevTestRoom/Voice] {s}");
        }
    }
}
