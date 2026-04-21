using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

namespace LaVeillee.Networking
{
    /// IRoomManager implementation backed by Photon Fusion 2 in Shared mode.
    /// Shared mode means any peer can host transparently — no dedicated server needed for v1.
    [DisallowMultipleComponent]
    public class FusionRoomManager : MonoBehaviour, IRoomManager, INetworkRunnerCallbacks
    {
        const int DefaultMaxPlayers = 25;

        NetworkRunner _runner;
        bool _creatingNotJoining;

        public bool IsInRoom => _runner != null && _runner.IsRunning;
        public string CurrentRoomId => IsInRoom ? _runner.SessionInfo?.Name : null;
        public PlayerHandle LocalPlayer => IsInRoom
            ? new PlayerHandle(_runner.LocalPlayer.PlayerId, $"Player{_runner.LocalPlayer.PlayerId}")
            : default;

        /// Accès direct au Runner pour les écrans qui ont besoin de l'API Fusion
        /// bas-niveau (ActivePlayers, Spawn, RPCs) — ex. LobbyScreen.
        public NetworkRunner Runner => _runner;

        public event Action<string> RoomCreated;
        public event Action<string> RoomJoined;
        public event Action<RoomLeaveReason> RoomLeft;
        public event Action<PlayerHandle> PlayerJoined;
        public event Action<PlayerHandle> PlayerLeft;
        public event Action<RoomError> Error;

        public async void CreateRoom(string roomId = null, int maxPlayers = DefaultMaxPlayers)
        {
            roomId ??= GenerateRoomId();
            _creatingNotJoining = true;
            await StartRunner(roomId, maxPlayers);
        }

        public async void JoinRoom(string roomId)
        {
            if (string.IsNullOrWhiteSpace(roomId))
            {
                Error?.Invoke(RoomError.RoomNotFound);
                return;
            }
            _creatingNotJoining = false;
            await StartRunner(roomId, DefaultMaxPlayers);
        }

        public async void LeaveRoom()
        {
            if (_runner != null) await _runner.Shutdown();
        }

        public IReadOnlyList<PlayerHandle> GetPlayersInRoom()
        {
            var list = new List<PlayerHandle>();
            if (!IsInRoom) return list;
            foreach (var player in _runner.ActivePlayers)
                list.Add(new PlayerHandle(player.PlayerId, $"Player{player.PlayerId}"));
            return list;
        }

        async Task StartRunner(string sessionName, int maxPlayers)
        {
            EnsureRunner();
            var args = new StartGameArgs
            {
                GameMode = GameMode.Shared,
                SessionName = sessionName,
                PlayerCount = maxPlayers,
                SceneManager = _runner.gameObject.GetComponent<NetworkSceneManagerDefault>()
                    ?? _runner.gameObject.AddComponent<NetworkSceneManagerDefault>()
            };

            var result = await _runner.StartGame(args);
            if (result.Ok)
            {
                if (_creatingNotJoining) RoomCreated?.Invoke(sessionName);
                else RoomJoined?.Invoke(sessionName);
            }
            else
            {
                Error?.Invoke(MapShutdownReason(result.ShutdownReason));
            }
        }

        void EnsureRunner()
        {
            if (_runner != null) return;
            // FusionVoiceClient also RequireComponents NetworkRunner — pick up the shared instance if already present.
            _runner = GetComponent<NetworkRunner>() ?? gameObject.AddComponent<NetworkRunner>();
            _runner.ProvideInput = false;
            _runner.AddCallbacks(this);

            // Register any other INetworkRunnerCallbacks attached to the same GameObject (FusionVoiceClient
            // needs OnPlayerJoined to know when to fire up the voice connection).
            foreach (var cb in GetComponents<INetworkRunnerCallbacks>())
            {
                if (!ReferenceEquals(cb, this)) _runner.AddCallbacks(cb);
            }
        }

        static string GenerateRoomId()
        {
            // Story 2.2 — code NUMÉRIQUE 6 chiffres (AC : "code à 6 chiffres").
            // 10^6 combinaisons = 1M, collision rate ~10^-4 pour 100 rooms actives — OK pour MVP.
            // Upgrade : collision-check côté Photon lobby list avant de confirmer le code.
            var bytes = Guid.NewGuid().ToByteArray();
            Span<char> code = stackalloc char[6];
            for (int i = 0; i < 6; i++) code[i] = (char)('0' + (bytes[i] % 10));
            return new string(code);
        }

        static RoomError MapShutdownReason(ShutdownReason reason) => reason switch
        {
            ShutdownReason.GameNotFound        => RoomError.RoomNotFound,
            ShutdownReason.GameIsFull          => RoomError.RoomFull,
            ShutdownReason.ConnectionTimeout   => RoomError.Timeout,
            ShutdownReason.ConnectionRefused   => RoomError.ConnectionFailed,
            ShutdownReason.InvalidAuthentication => RoomError.InvalidAppId,
            _                                  => RoomError.Unknown
        };

        // ---- INetworkRunnerCallbacks ----

        void INetworkRunnerCallbacks.OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            PlayerJoined?.Invoke(new PlayerHandle(player.PlayerId, $"Player{player.PlayerId}"));
        }

        void INetworkRunnerCallbacks.OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            PlayerLeft?.Invoke(new PlayerHandle(player.PlayerId, $"Player{player.PlayerId}"));
        }

        void INetworkRunnerCallbacks.OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            var leaveReason = shutdownReason switch
            {
                ShutdownReason.Ok                       => RoomLeaveReason.LocalLeft,
                ShutdownReason.DisconnectedByPluginLogic => RoomLeaveReason.Kicked,
                ShutdownReason.GameClosed               => RoomLeaveReason.RoomClosed,
                _                                       => RoomLeaveReason.Disconnected
            };
            RoomLeft?.Invoke(leaveReason);
        }

        // Stubs required by INetworkRunnerCallbacks but unused for v1 dev test room.
        void INetworkRunnerCallbacks.OnInput(NetworkRunner runner, NetworkInput input) { }
        void INetworkRunnerCallbacks.OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
        void INetworkRunnerCallbacks.OnConnectedToServer(NetworkRunner runner) { }
        void INetworkRunnerCallbacks.OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
        void INetworkRunnerCallbacks.OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        void INetworkRunnerCallbacks.OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
        void INetworkRunnerCallbacks.OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
        void INetworkRunnerCallbacks.OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
        void INetworkRunnerCallbacks.OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
        void INetworkRunnerCallbacks.OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        void INetworkRunnerCallbacks.OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
        void INetworkRunnerCallbacks.OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
        void INetworkRunnerCallbacks.OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        void INetworkRunnerCallbacks.OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        void INetworkRunnerCallbacks.OnSceneLoadDone(NetworkRunner runner) { }
        void INetworkRunnerCallbacks.OnSceneLoadStart(NetworkRunner runner) { }
    }
}
