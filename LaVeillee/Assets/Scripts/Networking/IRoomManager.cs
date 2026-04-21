using System;
using System.Collections.Generic;

namespace LaVeillee.Networking
{
    /// Contract for any multiplayer room implementation.
    /// Decouples gameplay layer from the underlying SDK (Fusion 2 today, swappable tomorrow).
    public interface IRoomManager
    {
        bool IsInRoom { get; }
        string CurrentRoomId { get; }
        PlayerHandle LocalPlayer { get; }

        event Action<string> RoomCreated;
        event Action<string> RoomJoined;
        event Action<RoomLeaveReason> RoomLeft;
        event Action<PlayerHandle> PlayerJoined;
        event Action<PlayerHandle> PlayerLeft;
        event Action<RoomError> Error;

        void CreateRoom(string roomId = null, int maxPlayers = 25);
        void JoinRoom(string roomId);
        void LeaveRoom();
        IReadOnlyList<PlayerHandle> GetPlayersInRoom();
    }

    public readonly struct PlayerHandle : IEquatable<PlayerHandle>
    {
        public readonly int Id;
        public readonly string DisplayName;

        public PlayerHandle(int id, string displayName)
        {
            Id = id;
            DisplayName = displayName ?? $"Player{id}";
        }

        public bool Equals(PlayerHandle other) => Id == other.Id;
        public override bool Equals(object obj) => obj is PlayerHandle other && Equals(other);
        public override int GetHashCode() => Id;
        public override string ToString() => $"{DisplayName}#{Id}";
    }

    public enum RoomLeaveReason
    {
        LocalLeft,
        Disconnected,
        Kicked,
        RoomClosed
    }

    public enum RoomError
    {
        Unknown,
        RoomFull,
        RoomNotFound,
        ConnectionFailed,
        InvalidAppId,
        Timeout
    }
}
