using Fusion;
using UnityEngine;

namespace LaVeillee.Networking
{
    /// État global partagé du lobby + partie en cours.
    /// StateAuthority = host (le plus ancien joueur dans la room, géré via
    /// FusionRoomManager / la promotion auto Shared mode).
    ///
    /// Porte :
    ///   - Slot public par PlayerId (Story 2.4 : pseudo + avatar synchronisés)
    ///   - Composition des rôles (Story 2.5)
    ///   - Timers + mode (Story 2.6)
    ///   - État pause (Story 2.7)
    ///
    /// Scene-placed dans Lobby.unity (voir Epic2SceneBootstrap éditeur) — Fusion
    /// détecte les NetworkObject présents dans la scène au StartGame et les réplique
    /// à tous les clients.
    public class LobbyState : NetworkBehaviour
    {
        public const int MaxPlayers = 32; // headroom sur les 25 joueurs réels
        public const int MaxRoles = 12;

        // --- Host --------------------------------------------------------
        [Networked] public PlayerRef HostPlayerRef { get; set; }

        // --- Story 2.4 : Info par joueur --------------------------------
        // Indexé par PlayerRef.PlayerId. Slot vide = Pseudos[i] == "".
        [Networked, Capacity(MaxPlayers)] public NetworkArray<NetworkString<_32>> Pseudos { get; }
        [Networked, Capacity(MaxPlayers)] public NetworkArray<int> AvatarSeeds { get; }

        // --- Story 2.5 : Composition ------------------------------------
        [Networked, Capacity(MaxRoles)] public NetworkArray<int> CompositionRoles { get; }
        [Networked, Capacity(MaxRoles)] public NetworkArray<int> CompositionCounts { get; }
        [Networked] public int CompositionRoleCount { get; set; }
        [Networked] public NetworkBool CompositionValidated { get; set; }

        // --- Story 2.6 : Timers + mode ----------------------------------
        [Networked] public int NightDurationSeconds { get; set; } = 180; // 3 min défaut
        [Networked] public int DayDurationSeconds { get; set; } = 300;   // 5 min défaut
        [Networked] public NetworkBool ModeCampfire { get; set; }        // false = Remote

        // --- Story 2.6 : Countdown démarrage ----------------------------
        [Networked] public NetworkBool GameStarting { get; set; }
        [Networked] public TickTimer StartCountdown { get; set; }

        // --- Story 2.7 : Pause ------------------------------------------
        [Networked] public NetworkBool Paused { get; set; }
        [Networked] public NetworkString<_32> PausedBy { get; set; }
        [Networked] public float PausedAtSimulationTime { get; set; }

        public override void Spawned()
        {
            if (HasStateAuthority)
            {
                HostPlayerRef = Runner.LocalPlayer;
                CompositionValidated = false;
                Paused = false;
                GameStarting = false;
            }
        }

        // --- RPCs : identité --------------------------------------------

        /// Chaque joueur pousse son pseudo/avatar au host à l'entrée en room.
        /// Host valide l'index et écrit dans les networked arrays.
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void Rpc_RegisterPlayer(PlayerRef sender, string pseudo, int avatarSeed, RpcInfo info = default)
        {
            int idx = sender.PlayerId;
            if (idx < 0 || idx >= MaxPlayers) return;
            Pseudos.Set(idx, pseudo ?? "Joueur");
            AvatarSeeds.Set(idx, avatarSeed);
        }

        /// Libère le slot quand un joueur quitte (host-driven via poll ActivePlayers).
        public void ClearPlayerSlot(int playerId)
        {
            if (!HasStateAuthority) return;
            if (playerId < 0 || playerId >= MaxPlayers) return;
            Pseudos.Set(playerId, "");
            AvatarSeeds.Set(playerId, 0);
        }

        /// Promote a new host (lowest PlayerId still present). Host migration Shared mode.
        public void PromoteHost(PlayerRef newHost)
        {
            if (!HasStateAuthority) return;
            HostPlayerRef = newHost;
        }

        // --- RPCs : kick (Story 2.4) ------------------------------------

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void Rpc_Kick(PlayerRef target)
        {
            if (Runner.LocalPlayer == target)
            {
                Debug.Log("[LobbyState] Kické par l'host — déconnexion.");
                Runner.Shutdown();
            }
        }

        // --- Composition (Story 2.5) ------------------------------------

        public void SetCompositionLocal(int[] roles, int[] counts)
        {
            if (!HasStateAuthority) return;
            var n = Mathf.Min(roles?.Length ?? 0, MaxRoles);
            n = Mathf.Min(n, counts?.Length ?? 0);
            for (int i = 0; i < n; i++)
            {
                CompositionRoles.Set(i, roles[i]);
                CompositionCounts.Set(i, counts[i]);
            }
            CompositionRoleCount = n;
            CompositionValidated = true;
        }

        // --- Timers + mode (Story 2.6) ----------------------------------

        public void SetTimersAndMode(int nightSec, int daySec, bool campfire)
        {
            if (!HasStateAuthority) return;
            NightDurationSeconds = Mathf.Clamp(nightSec, 60, 300);
            DayDurationSeconds   = Mathf.Clamp(daySec, 120, 600);
            ModeCampfire = campfire;
        }

        // --- Démarrer partie (Story 2.6) --------------------------------

        public void StartGame()
        {
            if (!HasStateAuthority) return;
            GameStarting = true;
            StartCountdown = TickTimer.CreateFromSeconds(Runner, 3f);
        }

        // --- Pause (Story 2.7) ------------------------------------------

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void Rpc_RequestPauseToggle(string pausingPseudo, RpcInfo info = default)
        {
            Paused = !Paused;
            PausedBy = Paused ? (pausingPseudo ?? "Inconnu") : "";
            PausedAtSimulationTime = Paused ? (float)Runner.SimulationTime : 0f;
        }

        /// Annuler la partie (Story 2.7 : option après 10min de pause).
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void Rpc_CancelGame()
        {
            Debug.Log("[LobbyState] Partie annulée par l'host — retour home.");
            // Chaque client réagit localement via un listener dans LobbyScreen/GameScreen.
        }
    }
}
