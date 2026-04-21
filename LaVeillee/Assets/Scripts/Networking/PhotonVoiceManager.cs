using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Realtime;
using Photon.Voice.Fusion;
using Photon.Voice.Unity;
using UnityEngine;

namespace LaVeillee.Networking
{
    /// IVoiceManager implementation backed by Photon Voice 2 + Fusion 2 integration.
    ///
    /// Architecture: FusionVoiceClient auto-follows the NetworkRunner — when the runner
    /// joins/leaves a session, the voice client joins/leaves a parallel "<sessionName>_voice"
    /// room transparently. We just manage the local Recorder (mute) and observe remote
    /// Speakers spawned by FusionVoiceClient.
    [DisallowMultipleComponent]
    [RequireComponent(typeof(FusionVoiceClient))]
    [RequireComponent(typeof(Recorder))]
    public class PhotonVoiceManager : MonoBehaviour, IVoiceManager
    {
        FusionVoiceClient _voiceClient;
        Recorder _recorder;

        readonly Dictionary<Speaker, int> _trackedSpeakers = new();
        readonly HashSet<int> _activeSpeakers = new();

        bool _muted;
        MicPermissionStatus _micPermission = MicPermissionStatus.NotRequested;
        bool _wasConnected;

        public bool IsConnected => _voiceClient != null && _voiceClient.ClientState == ClientState.Joined;
        public bool IsMuted => _muted;
        public MicPermissionStatus MicPermission => _micPermission;
        public IReadOnlyCollection<int> CurrentSpeakers => _activeSpeakers;

        public event Action Connected;
        public event Action<VoiceLeaveReason> Disconnected;
        public event Action<int> SpeakerStartedTalking;
        public event Action<int> SpeakerStoppedTalking;
        public event Action<MicPermissionStatus> MicPermissionChanged;
        public event Action<VoiceError> Error;

        void Awake()
        {
            _voiceClient = GetComponent<FusionVoiceClient>();
            _recorder = GetComponent<Recorder>();

            // Wire FusionVoiceClient to use this Recorder as the primary mic source.
            _voiceClient.PrimaryRecorder = _recorder;

            // FusionVoiceClient ignores the primary recorder unless usePrimaryRecorder is true.
            // The field is [SerializeField] private with no public setter, so we reach in via reflection.
            // Without this, voice connects but no audio is captured/transmitted.
            var field = typeof(VoiceConnection).GetField("usePrimaryRecorder",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            field?.SetValue(_voiceClient, true);

            // Initial mic permission probe — granted/denied is cached by OS, so this is cheap.
            _micPermission = ProbeMicPermission();
        }

        void OnEnable()
        {
            _voiceClient.SpeakerLinked += OnSpeakerLinked;
        }

        void OnDisable()
        {
            _voiceClient.SpeakerLinked -= OnSpeakerLinked;
            foreach (var speaker in _trackedSpeakers.Keys)
            {
                if (speaker != null) speaker.OnRemoteVoiceRemoveAction = null;
            }
            _trackedSpeakers.Clear();
            _activeSpeakers.Clear();
        }

        void Update()
        {
            // Connect/disconnect transitions — VoiceConnection has no public connected/disconnected event,
            // so we observe ClientState changes here. Cheap (one bool comparison per frame).
            bool nowConnected = IsConnected;
            if (nowConnected && !_wasConnected) Connected?.Invoke();
            else if (!nowConnected && _wasConnected) Disconnected?.Invoke(VoiceLeaveReason.FollowedRoomLeft);
            _wasConnected = nowConnected;

            // Per-speaker IsPlaying poll. Speaker has no event — IsPlaying flips on/off as audio
            // frames arrive/stop. Polling here is the recommended pattern from Photon Voice docs.
            foreach (var (speaker, playerId) in _trackedSpeakers)
            {
                if (speaker == null) continue;
                bool isTalking = speaker.IsPlaying;
                bool wasTalking = _activeSpeakers.Contains(playerId);
                if (isTalking && !wasTalking)
                {
                    _activeSpeakers.Add(playerId);
                    SpeakerStartedTalking?.Invoke(playerId);
                }
                else if (!isTalking && wasTalking)
                {
                    _activeSpeakers.Remove(playerId);
                    SpeakerStoppedTalking?.Invoke(playerId);
                }
            }
        }

        public void RequestMicPermission(Action<MicPermissionStatus> onResult = null)
        {
            StartCoroutine(RequestMicPermissionCoroutine(onResult));
        }

        IEnumerator RequestMicPermissionCoroutine(Action<MicPermissionStatus> onResult)
        {
#if UNITY_IOS && !UNITY_EDITOR
            yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);
            var newStatus = Application.HasUserAuthorization(UserAuthorization.Microphone)
                ? MicPermissionStatus.Granted
                : MicPermissionStatus.Denied;
#else
            // macOS standalone / Editor: Microphone.devices.Length > 0 means OS-level access is granted
            // (macOS prompts on first Microphone.Start). We optimistically mark Granted if a device is enumerated.
            yield return null;
            var newStatus = Microphone.devices.Length > 0 ? MicPermissionStatus.Granted : MicPermissionStatus.Denied;
#endif
            if (newStatus != _micPermission)
            {
                _micPermission = newStatus;
                MicPermissionChanged?.Invoke(_micPermission);
            }
            if (newStatus == MicPermissionStatus.Denied)
            {
                Error?.Invoke(VoiceError.PermissionDenied);
                _recorder.TransmitEnabled = false;
            }
            onResult?.Invoke(newStatus);
        }

        public void SetMuted(bool muted)
        {
            _muted = muted;
            _recorder.TransmitEnabled = !muted && _micPermission == MicPermissionStatus.Granted;
        }

        public void JoinVoiceChannel(string roomId)
        {
            // No-op: FusionVoiceClient auto-follows the NetworkRunner's session.
            // We expose this method to keep the contract symmetric with IRoomManager and to
            // give callers a hook if we ever swap to a non-Fusion-coupled voice backend.
            _ = roomId;
        }

        public void LeaveVoiceChannel()
        {
            // No-op for the same reason — FusionVoiceClient auto-leaves when the runner shuts down.
        }

        void OnSpeakerLinked(Speaker speaker)
        {
            if (speaker == null) return;
            int playerId = ResolvePlayerId(speaker);
            _trackedSpeakers[speaker] = playerId;
            speaker.OnRemoteVoiceRemoveAction = OnSpeakerRemoved;
        }

        void OnSpeakerRemoved(Speaker speaker)
        {
            if (speaker == null) return;
            if (_trackedSpeakers.TryGetValue(speaker, out var playerId))
            {
                if (_activeSpeakers.Remove(playerId))
                    SpeakerStoppedTalking?.Invoke(playerId);
                _trackedSpeakers.Remove(speaker);
            }
        }

        static int ResolvePlayerId(Speaker speaker)
        {
            // Speaker is parented to the VoiceNetworkObject GameObject. The VoiceNetworkObject
            // owner (StateAuthority in Shared mode) maps to the player id from FusionRoomManager.
            var vno = speaker.GetComponentInParent<VoiceNetworkObject>();
            if (vno != null && vno.Object != null)
                return vno.Object.StateAuthority.PlayerId;
            // Fallback: speakers spawned for the FusionVoiceClient's own primary recorder
            // (no VoiceNetworkObject userData) — we can't resolve playerId from here, return -1.
            return -1;
        }

        static MicPermissionStatus ProbeMicPermission()
        {
#if UNITY_IOS && !UNITY_EDITOR
            return Application.HasUserAuthorization(UserAuthorization.Microphone)
                ? MicPermissionStatus.Granted
                : MicPermissionStatus.NotRequested;
#else
            return Microphone.devices.Length > 0
                ? MicPermissionStatus.Granted
                : MicPermissionStatus.NotRequested;
#endif
        }
    }
}
