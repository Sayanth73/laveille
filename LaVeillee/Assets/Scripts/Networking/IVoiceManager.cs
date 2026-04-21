using System;
using System.Collections.Generic;

namespace LaVeillee.Networking
{
    /// Contract for any in-room voice chat implementation.
    /// Decouples gameplay from the underlying voice SDK (Photon Voice 2 today, Agora possible if NFR4 latency target missed).
    public interface IVoiceManager
    {
        bool IsConnected { get; }
        bool IsMuted { get; }
        MicPermissionStatus MicPermission { get; }
        IReadOnlyCollection<int> CurrentSpeakers { get; }

        event Action Connected;
        event Action<VoiceLeaveReason> Disconnected;
        event Action<int> SpeakerStartedTalking;
        event Action<int> SpeakerStoppedTalking;
        event Action<MicPermissionStatus> MicPermissionChanged;
        event Action<VoiceError> Error;

        void RequestMicPermission(Action<MicPermissionStatus> onResult = null);
        void SetMuted(bool muted);
        void JoinVoiceChannel(string roomId);
        void LeaveVoiceChannel();
    }

    public enum MicPermissionStatus
    {
        Unknown,
        Granted,
        Denied,
        NotRequested
    }

    public enum VoiceLeaveReason
    {
        LocalLeft,
        Disconnected,
        FollowedRoomLeft
    }

    public enum VoiceError
    {
        Unknown,
        InvalidAppId,
        ConnectionFailed,
        MicrophoneUnavailable,
        PermissionDenied
    }
}
