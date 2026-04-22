using System.Collections.Generic;
using Fusion;
using LaVeillee.Core;
using UnityEngine;

namespace LaVeillee.Networking
{
    /// État global de la partie Epic 3 — rôles, phases, votes, morts.
    /// StateAuthority = host (même joueur que LobbyState.HostPlayerRef).
    /// Scene-placed dans Main.unity à côté de LobbyState.
    ///
    /// Privacy note : pour MVP, tous les états networked sont visibles par tous les
    /// clients. L'UI masque ce qui ne doit pas être vu (rôle des autres, cible Loups).
    /// Un vrai anti-cheat requiert des RPC per-player (`[RpcTarget] PlayerRef`) —
    /// à durcir après playtests.
    public class GameState : NetworkBehaviour
    {
        public const int MaxPlayers = 32;
        public const int FeedCapacity = 20;

        // --- Phase & timer -----------------------------------------------
        [Networked] public GamePhase Phase { get; set; }
        [Networked] public TickTimer PhaseTimer { get; set; }
        [Networked] public int DayCount { get; set; }         // 0 avant Nuit 1, 1 après, etc.
        [Networked] public int WinnerCamp { get; set; }       // -1 en cours, 0 = Village, 1 = Loups, 2 = Nul

        // --- Rôles + vie ------------------------------------------------
        [Networked, Capacity(MaxPlayers)] public NetworkArray<int> Roles { get; }       // RoleId, -1 si slot vide
        [Networked, Capacity(MaxPlayers)] public NetworkArray<NetworkBool> Alive { get; }
        [Networked, Capacity(MaxPlayers)] public NetworkArray<NetworkBool> RoleAcked { get; } // Story 3.1

        // --- Voyante ----------------------------------------------------
        [Networked] public int VoyanteTargetThisNight { get; set; } // PlayerId ou -1
        [Networked] public NetworkBool VoyanteLockedThisNight { get; set; }

        // --- Loups ------------------------------------------------------
        /// Vote de chaque Loup pour une cible. -1 = pas encore voté.
        [Networked, Capacity(MaxPlayers)] public NetworkArray<int> LoupVoteTarget { get; }
        [Networked, Capacity(MaxPlayers)] public NetworkArray<NetworkBool> LoupVoteLocked { get; }
        /// Cible finale décidée par les Loups cette nuit (-1 si pas de consensus).
        [Networked] public int LoupsTargetResolved { get; set; }

        // --- Sorcière ---------------------------------------------------
        [Networked] public NetworkBool SorciereHasLifePotion { get; set; }
        [Networked] public NetworkBool SorciereHasDeathPotion { get; set; }
        [Networked] public NetworkBool SorciereUsedLifeThisNight { get; set; }
        [Networked] public int SorciereKillTargetThisNight { get; set; } // PlayerId ou -1
        [Networked] public NetworkBool SorciereLockedThisNight { get; set; }

        // --- Vote de jour ------------------------------------------------
        /// Vote public de chaque joueur vivant. -1 = pas de vote / abstention,
        /// -2 = abstention explicite (tapé son propre avatar).
        [Networked, Capacity(MaxPlayers)] public NetworkArray<int> DayVoteTarget { get; }

        // --- Chasseur ----------------------------------------------------
        /// PlayerId du Chasseur en attente de tir, -1 si aucun.
        [Networked] public int HunterPendingShooter { get; set; }

        // --- Feed (Story 3.7) -------------------------------------------
        [Networked, Capacity(FeedCapacity)] public NetworkArray<NetworkString<_64>> Feed { get; }
        [Networked] public int FeedHead { get; set; } // index circulaire

        // --- Révélation mort (affiché 5s après résolution) --------------
        [Networked] public int LastDeadPlayer { get; set; } // -1 si aucun
        [Networked] public int LastDeadRole { get; set; }   // RoleId du mort révélé
        [Networked] public NetworkString<_32> LastDeadReason { get; set; } // "vote", "loups", "potion", "chasseur"

        public override void Spawned()
        {
            if (HasStateAuthority)
            {
                Phase = GamePhase.Idle;
                DayCount = 0;
                WinnerCamp = -1;
                LastDeadPlayer = -1;
                LoupsTargetResolved = -1;
                VoyanteTargetThisNight = -1;
                SorciereKillTargetThisNight = -1;
                HunterPendingShooter = -1;
                for (int i = 0; i < MaxPlayers; i++)
                {
                    Roles.Set(i, -1);
                    Alive.Set(i, false);
                    RoleAcked.Set(i, false);
                    LoupVoteTarget.Set(i, -1);
                    LoupVoteLocked.Set(i, false);
                    DayVoteTarget.Set(i, -1);
                }
            }
        }

        // =====================================================================
        // Host-only entry point — appelé par LobbyScreen.OnStartGame après
        // countdown + validation composition.
        // =====================================================================

        public void HostStartGame(LobbyState lobby)
        {
            if (!HasStateAuthority) return;
            if (Phase != GamePhase.Idle && Phase != GamePhase.Ended) return;

            // 1. Collecter les joueurs présents
            var activeIds = new List<int>();
            foreach (var p in Runner.ActivePlayers) activeIds.Add(p.PlayerId);
            if (activeIds.Count < 2) { Debug.LogWarning("[GameState] pas assez de joueurs"); return; }

            // 2. Déployer la composition validée en liste de rôles
            var pool = new List<int>();
            for (int i = 0; i < lobby.CompositionRoleCount; i++)
            {
                int roleId = lobby.CompositionRoles.Get(i);
                int count = lobby.CompositionCounts.Get(i);
                for (int c = 0; c < count; c++) pool.Add(roleId);
            }

            // Si le nombre de rôles dans la compo ne matche pas le nombre de joueurs,
            // on complète avec des Villageois (fallback robuste).
            while (pool.Count < activeIds.Count) pool.Add((int)RoleId.Villageois);
            if (pool.Count > activeIds.Count) pool.RemoveRange(activeIds.Count, pool.Count - activeIds.Count);

            // 3. Shuffle Fisher-Yates déterministe au host
            var rng = new System.Random();
            for (int i = pool.Count - 1; i > 0; i--)
            {
                int j = rng.Next(0, i + 1);
                (pool[i], pool[j]) = (pool[j], pool[i]);
            }

            // 4. Reset state + assign
            for (int i = 0; i < MaxPlayers; i++)
            {
                Roles.Set(i, -1);
                Alive.Set(i, false);
                RoleAcked.Set(i, false);
            }
            for (int k = 0; k < activeIds.Count; k++)
            {
                int pid = activeIds[k];
                if (pid < 0 || pid >= MaxPlayers) continue;
                Roles.Set(pid, pool[k]);
                Alive.Set(pid, true);
            }

            // 5. Sorcière : potions pleines pour la partie
            SorciereHasLifePotion = true;
            SorciereHasDeathPotion = true;

            DayCount = 0;
            WinnerCamp = -1;
            LastDeadPlayer = -1;
            ClearFeed();

            // 6. Phase Distribution (Story 3.1) — 30s pour que chacun ack son rôle.
            TransitionTo(GamePhase.Distribution, 30f);
            PushFeed("🎭 La veillée commence — découvrez votre rôle…");
        }

        // =====================================================================
        // Mode Solo Dev — contourne le min de 5 joueurs en peuplant des slots
        // "bots" (Alive=true, RoleAcked=true). Ils ne jouent jamais — le host
        // (joueur local) est le seul vrai participant, donc :
        //   - Leurs votes restent à -1 (non votants) → n'affectent pas les tallies
        //     sauf abstention/majorité relative.
        //   - Si le local choisit Loup : il peut voter seul → cible sera tuée.
        //   - Si le local choisit Voyante : son scan révèle un bot aléatoire.
        //   - Distribution → on skippe direct à la phase choisie.
        // Permet de tester chaque rôle sans ouvrir 5 instances.
        // =====================================================================

        public void DevStartSolo(LobbyState lobby, RoleId localRole, GamePhase startPhase)
        {
            if (!HasStateAuthority) return;
            if (Phase != GamePhase.Idle && Phase != GamePhase.Ended)
            {
                Debug.LogWarning($"[GameState] DevStartSolo ignoré : Phase={Phase}");
                return;
            }

            int localId = Runner.LocalPlayer.PlayerId;
            if (localId < 0 || localId >= MaxPlayers) return;

            // Reset complet
            for (int i = 0; i < MaxPlayers; i++)
            {
                Roles.Set(i, -1);
                Alive.Set(i, false);
                RoleAcked.Set(i, true); // évite de bloquer AllAliveAcked sur slot vide
                LoupVoteTarget.Set(i, -1);
                LoupVoteLocked.Set(i, false);
                DayVoteTarget.Set(i, -1);
            }

            // Compo 5 joueurs : 1 Loup + 1 Voyante + 1 Sorcière + 1 Chasseur + 1 Villageois
            // On retire un exemplaire du rôle choisi par le local pour le remplacer par lui.
            var botRoles = new List<RoleId>
            {
                RoleId.LoupGarou, RoleId.Voyante, RoleId.Sorciere, RoleId.Chasseur,
            };
            if (botRoles.Contains(localRole)) botRoles.Remove(localRole);
            while (botRoles.Count < 4) botRoles.Add(RoleId.Villageois);

            // Local
            Roles.Set(localId, (int)localRole);
            Alive.Set(localId, true);
            RoleAcked.Set(localId, true);

            // 4 bots — on choisit des slots contigus en évitant localId
            int botCursor = 0;
            for (int slot = 0; slot < MaxPlayers && botCursor < botRoles.Count; slot++)
            {
                if (slot == localId) continue;
                Roles.Set(slot, (int)botRoles[botCursor]);
                Alive.Set(slot, true);
                RoleAcked.Set(slot, true);
                // Vote de jour : les bots "votent pour le local" si on démarre en DayDebate
                // → ça garantit qu'au moins un vote existe → ResolveDayVote fonctionne.
                // Non — on laisse à -1 pour que le local contrôle le résultat.
                botCursor++;
            }

            // Potions pleines pour Sorcière
            SorciereHasLifePotion = true;
            SorciereHasDeathPotion = true;
            SorciereUsedLifeThisNight = false;
            SorciereKillTargetThisNight = -1;
            SorciereLockedThisNight = false;

            // Reset night-scoped
            VoyanteTargetThisNight = -1;
            VoyanteLockedThisNight = false;
            LoupsTargetResolved = -1;
            HunterPendingShooter = -1;
            LastDeadPlayer = -1;

            DayCount = startPhase switch
            {
                GamePhase.NightVoyante or GamePhase.NightLoups or GamePhase.NightSorciere
                    or GamePhase.NightResolve => 0,
                _ => 1,
            };
            WinnerCamp = -1;
            ClearFeed();

            // Pousse la composition dans le lobby pour cohérence UI
            var rolesArr = new[]
            {
                (int)RoleId.LoupGarou, (int)RoleId.Voyante, (int)RoleId.Sorciere,
                (int)RoleId.Chasseur, (int)RoleId.Villageois,
            };
            var countsArr = new[] { 1, 1, 1, 1, 1 };
            lobby.SetCompositionLocal(rolesArr, countsArr);

            PushFeed($"🛠 Mode solo — tu joues {RoleCatalog.EmojiFor(localRole)} {RoleCatalog.LabelFor(localRole)}");

            // Timer généreux pour laisser le temps de tester
            float seconds = startPhase switch
            {
                GamePhase.NightVoyante or GamePhase.NightLoups or GamePhase.NightSorciere => 120f,
                GamePhase.NightResolve => 1f,
                GamePhase.HunterShot => 60f,
                _ => 240f,
            };
            TransitionTo(startPhase, seconds);
        }

        // =====================================================================
        // State machine — tick uniquement côté host.
        // =====================================================================

        public override void FixedUpdateNetwork()
        {
            if (!HasStateAuthority) return;
            if (Phase == GamePhase.Idle || Phase == GamePhase.Ended) return;

            switch (Phase)
            {
                case GamePhase.Distribution:
                    if (AllAliveAcked() || PhaseTimer.Expired(Runner)) BeginNight();
                    break;

                case GamePhase.NightVoyante:
                    // On ne transitionne PAS immédiatement sur lock — Rpc_VoyanteScan réduit
                    // PhaseTimer à 6s pour laisser le temps de lire le résultat. La transition
                    // se fait naturellement via PhaseTimer.Expired.
                    if (!AnyAliveOfRole(RoleId.Voyante) || PhaseTimer.Expired(Runner))
                        NextNightStep(GamePhase.NightVoyante);
                    break;

                case GamePhase.NightLoups:
                    if (LoupVoteConsensusReached() || !AnyAliveOfRole(RoleId.LoupGarou) || PhaseTimer.Expired(Runner))
                    {
                        ResolveLoupsVote();
                        NextNightStep(GamePhase.NightLoups);
                    }
                    break;

                case GamePhase.NightSorciere:
                    bool outOfPotions = !SorciereHasLifePotion && !SorciereHasDeathPotion;
                    if (SorciereLockedThisNight || !AnyAliveOfRole(RoleId.Sorciere) || outOfPotions || PhaseTimer.Expired(Runner))
                        NextNightStep(GamePhase.NightSorciere);
                    break;

                case GamePhase.NightResolve:
                    ApplyNightDeaths();
                    break;

                case GamePhase.DayDebate:
                    // Vote + débat fusionnés : les joueurs votent en live pendant qu'ils
                    // discutent. ResolveDayVote quand le timer expire ou tous ont voté.
                    if (PhaseTimer.Expired(Runner) || AllAliveVoted()) ResolveDayVote();
                    break;

                case GamePhase.DayVote:
                    // Phase deprecated (merge dans DayDebate). Garde défensive au cas où
                    // un ancien état networked arriverait ici.
                    if (PhaseTimer.Expired(Runner) || AllAliveVoted()) ResolveDayVote();
                    break;

                case GamePhase.HunterShot:
                    if (PhaseTimer.Expired(Runner))
                    {
                        PushFeed("🎯 Le Chasseur n'a pas tiré — phase suivante.");
                        HunterPendingShooter = -1;
                        ResumeAfterHunter();
                    }
                    break;

                case GamePhase.RevealDeath:
                    if (PhaseTimer.Expired(Runner)) ResumeAfterReveal();
                    break;
            }
        }

        // =====================================================================
        // Transitions nuit / jour
        // =====================================================================

        void BeginNight()
        {
            DayCount++;
            // Reset night-scoped state
            VoyanteTargetThisNight = -1;
            VoyanteLockedThisNight = false;
            for (int i = 0; i < MaxPlayers; i++)
            {
                LoupVoteTarget.Set(i, -1);
                LoupVoteLocked.Set(i, false);
            }
            LoupsTargetResolved = -1;
            SorciereUsedLifeThisNight = false;
            SorciereKillTargetThisNight = -1;
            SorciereLockedThisNight = false;

            PushFeed($"🌙 Nuit {DayCount} — la forêt s'endort.");
            TransitionTo(GamePhase.NightVoyante, 90f);
        }

        void NextNightStep(GamePhase from)
        {
            switch (from)
            {
                case GamePhase.NightVoyante:
                    TransitionTo(GamePhase.NightLoups, 90f);
                    break;
                case GamePhase.NightLoups:
                    TransitionTo(GamePhase.NightSorciere, 90f);
                    break;
                case GamePhase.NightSorciere:
                    TransitionTo(GamePhase.NightResolve, 1f);
                    break;
            }
        }

        void ApplyNightDeaths()
        {
            // Morts nocturnes : cible Loups (sauf sauvetage), cible Sorcière (potion mort).
            int loupsVictim = LoupsTargetResolved;
            if (SorciereUsedLifeThisNight && SorciereHasLifePotion && loupsVictim >= 0)
            {
                // Sauvetage — on annule la mort
                SorciereHasLifePotion = false;
                loupsVictim = -1;
            }
            int sorciereVictim = SorciereKillTargetThisNight;

            if (loupsVictim >= 0 && Alive.Get(loupsVictim))
            {
                KillPlayer(loupsVictim, "loups");
                if (TryTriggerHunter(loupsVictim)) return;
                if (CheckWinCondition()) return;
            }

            if (sorciereVictim >= 0 && Alive.Get(sorciereVictim))
            {
                KillPlayer(sorciereVictim, "potion");
                if (TryTriggerHunter(sorciereVictim)) return;
                if (CheckWinCondition()) return;
            }

            if (loupsVictim < 0 && sorciereVictim < 0)
            {
                PushFeed("☀️ Aucune victime cette nuit.");
            }

            TransitionTo(GamePhase.DayDebate, 180f);
            PushFeed($"☀️ Jour {DayCount} — le village se réveille.");
        }

        void ResolveDayVote()
        {
            // Compter les votes des vivants (hors abstentions)
            var tally = new Dictionary<int, int>();
            int maxVotes = 0;
            int winner = -1;
            bool tie = false;
            for (int voter = 0; voter < MaxPlayers; voter++)
            {
                if (!Alive.Get(voter)) continue;
                int tgt = DayVoteTarget.Get(voter);
                if (tgt < 0) continue;
                if (!tally.ContainsKey(tgt)) tally[tgt] = 0;
                tally[tgt]++;
                if (tally[tgt] > maxVotes) { maxVotes = tally[tgt]; winner = tgt; tie = false; }
                else if (tally[tgt] == maxVotes && tgt != winner) { tie = true; }
            }

            if (winner >= 0 && !tie && Alive.Get(winner))
            {
                KillPlayer(winner, "vote");
                if (TryTriggerHunter(winner)) return;
                if (CheckWinCondition()) return;
                // sinon on retourne à la nuit
                BeginNight();
            }
            else
            {
                PushFeed(tie ? "⚖️ Égalité au vote — personne n'est éliminé." : "🕊️ Aucun vote exprimé — personne n'est éliminé.");
                BeginNight();
            }
        }

        // =====================================================================
        // Kill + victoire
        // =====================================================================

        void KillPlayer(int playerId, string reason)
        {
            Alive.Set(playerId, false);
            var role = (RoleId)Roles.Get(playerId);
            LastDeadPlayer = playerId;
            LastDeadRole = (int)role;
            LastDeadReason = reason;
            PushFeed($"💀 Joueur {playerId} ({RoleCatalog.EmojiFor(role)} {RoleCatalog.LabelFor(role)}) — {reason}.");
        }

        bool TryTriggerHunter(int deadPlayer)
        {
            var role = (RoleId)Roles.Get(deadPlayer);
            if (role != RoleId.Chasseur) return false;
            HunterPendingShooter = deadPlayer;
            TransitionTo(GamePhase.HunterShot, 30f);
            return true;
        }

        public void ResumeAfterHunter()
        {
            if (CheckWinCondition()) return;
            // Reprend selon d'où on vient : après résolution nuit → jour ; après vote → nuit.
            // Heuristique simple : si DayCount == 0 ou on était dans ApplyNightDeaths,
            // on reprend au jour ; sinon (HunterShot déclenché depuis ResolveDayVote) on
            // passe à la nuit.
            if (Phase == GamePhase.HunterShot && DayCount >= 1 && LastDeadReason.ToString() == "vote")
                BeginNight();
            else
            {
                TransitionTo(GamePhase.DayDebate, 180f);
                PushFeed($"☀️ Jour {DayCount} — le village reprend.");
            }
        }

        void ResumeAfterReveal()
        {
            // Placeholder — pour l'instant unused ; utilisable pour animations mortes.
            TransitionTo(GamePhase.DayDebate, 180f);
        }

        bool CheckWinCondition()
        {
            int aliveWolves = 0, aliveVillage = 0;
            for (int i = 0; i < MaxPlayers; i++)
            {
                if (!Alive.Get(i)) continue;
                var r = (RoleId)Roles.Get(i);
                if (RoleCatalog.IsWolf(r)) aliveWolves++;
                else aliveVillage++;
            }
            if (aliveWolves == 0 && aliveVillage > 0)
            {
                WinnerCamp = 0;
                Phase = GamePhase.Ended;
                PushFeed("🏆 Le Village l'emporte !");
                return true;
            }
            if (aliveWolves > 0 && aliveWolves >= aliveVillage)
            {
                WinnerCamp = 1;
                Phase = GamePhase.Ended;
                PushFeed("🐺 Les Loups-Garous l'emportent !");
                return true;
            }
            if (aliveWolves == 0 && aliveVillage == 0)
            {
                WinnerCamp = 2;
                Phase = GamePhase.Ended;
                PushFeed("🪦 Match nul — plus aucun survivant.");
                return true;
            }
            return false;
        }

        // =====================================================================
        // Helpers d'état
        // =====================================================================

        bool AllAliveAcked()
        {
            for (int i = 0; i < MaxPlayers; i++)
            {
                if (!Alive.Get(i)) continue;
                if (!RoleAcked.Get(i)) return false;
            }
            return true;
        }

        bool AnyAliveOfRole(RoleId role)
        {
            for (int i = 0; i < MaxPlayers; i++)
            {
                if (!Alive.Get(i)) continue;
                if ((RoleId)Roles.Get(i) == role) return true;
            }
            return false;
        }

        bool LoupVoteConsensusReached()
        {
            int locked = 0, target = -1;
            for (int i = 0; i < MaxPlayers; i++)
            {
                if (!Alive.Get(i)) continue;
                if ((RoleId)Roles.Get(i) != RoleId.LoupGarou) continue;
                if (!LoupVoteLocked.Get(i)) return false; // pas encore tous verrouillés
                int t = LoupVoteTarget.Get(i);
                if (target == -1) target = t;
                else if (t != target) return false; // désaccord
                locked++;
            }
            return locked > 0;
        }

        void ResolveLoupsVote()
        {
            // Si consensus atteint, target final = cible verrouillée commune.
            // Sinon, majorité simple sur les votes verrouillés ; si personne verrouillé, on somme les votes non verrouillés.
            var tally = new Dictionary<int, int>();
            int maxVotes = 0, winner = -1;
            bool tie = false;
            bool anyLocked = false;
            for (int i = 0; i < MaxPlayers; i++)
            {
                if (!Alive.Get(i)) continue;
                if ((RoleId)Roles.Get(i) != RoleId.LoupGarou) continue;
                int t = LoupVoteTarget.Get(i);
                if (LoupVoteLocked.Get(i)) anyLocked = true;
                if (!LoupVoteLocked.Get(i) && anyLocked) continue; // prioriser les votes locked
                if (t < 0) continue;
                if (!tally.ContainsKey(t)) tally[t] = 0;
                tally[t]++;
                if (tally[t] > maxVotes) { maxVotes = tally[t]; winner = t; tie = false; }
                else if (tally[t] == maxVotes && t != winner) { tie = true; }
            }
            LoupsTargetResolved = (winner >= 0 && !tie) ? winner : -1;
        }

        bool AllAliveVoted()
        {
            for (int i = 0; i < MaxPlayers; i++)
            {
                if (!Alive.Get(i)) continue;
                if (DayVoteTarget.Get(i) == -1) return false; // pas encore voté (abstention explicite = -2)
            }
            return true;
        }

        void TransitionTo(GamePhase target, float seconds)
        {
            Phase = target;
            PhaseTimer = TickTimer.CreateFromSeconds(Runner, seconds);
        }

        // =====================================================================
        // Feed
        // =====================================================================

        void ClearFeed()
        {
            for (int i = 0; i < FeedCapacity; i++) Feed.Set(i, "");
            FeedHead = 0;
        }

        void PushFeed(string msg)
        {
            if (!HasStateAuthority) return;
            Feed.Set(FeedHead, msg.Length > 63 ? msg.Substring(0, 63) : msg);
            FeedHead = (FeedHead + 1) % FeedCapacity;
        }

        public IEnumerable<string> ReadFeedChronological()
        {
            // Retourne les FeedCapacity derniers messages dans l'ordre chrono.
            var list = new List<string>(FeedCapacity);
            for (int i = 0; i < FeedCapacity; i++)
            {
                int idx = (FeedHead + i) % FeedCapacity;
                var s = Feed.Get(idx).ToString();
                if (!string.IsNullOrEmpty(s)) list.Add(s);
            }
            return list;
        }

        // =====================================================================
        // RPCs — actions des joueurs
        // =====================================================================

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void Rpc_AckRole(PlayerRef player)
        {
            int pid = player.PlayerId;
            if (pid < 0 || pid >= MaxPlayers) return;
            RoleAcked.Set(pid, true);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void Rpc_VoyanteScan(PlayerRef voyante, int targetId)
        {
            int pid = voyante.PlayerId;
            if (Phase != GamePhase.NightVoyante) return;
            if (pid < 0 || pid >= MaxPlayers) return;
            if ((RoleId)Roles.Get(pid) != RoleId.Voyante) return;
            if (!Alive.Get(pid)) return;
            if (VoyanteLockedThisNight) return;
            if (targetId < 0 || targetId >= MaxPlayers || !Alive.Get(targetId)) return;
            VoyanteTargetThisNight = targetId;
            VoyanteLockedThisNight = true;
            // Fenêtre de lecture du résultat — si remaining > 6s on raccourcit,
            // si remaining < 6s on prolonge (6s garantis après le lock).
            PhaseTimer = TickTimer.CreateFromSeconds(Runner, 6f);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void Rpc_LoupVote(PlayerRef loup, int targetId, NetworkBool locked)
        {
            int pid = loup.PlayerId;
            if (Phase != GamePhase.NightLoups) return;
            if (pid < 0 || pid >= MaxPlayers) return;
            if ((RoleId)Roles.Get(pid) != RoleId.LoupGarou) return;
            if (!Alive.Get(pid)) return;
            if (LoupVoteLocked.Get(pid)) return; // déjà verrouillé — plus de changement
            LoupVoteTarget.Set(pid, targetId);
            LoupVoteLocked.Set(pid, locked);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void Rpc_SorciereAction(PlayerRef sorciere, NetworkBool saveLoupsTarget, int killTarget, NetworkBool lockAction)
        {
            int pid = sorciere.PlayerId;
            if (Phase != GamePhase.NightSorciere) return;
            if (pid < 0 || pid >= MaxPlayers) return;
            if ((RoleId)Roles.Get(pid) != RoleId.Sorciere) return;
            if (!Alive.Get(pid)) return;
            if (SorciereLockedThisNight) return;
            if (saveLoupsTarget && SorciereHasLifePotion) SorciereUsedLifeThisNight = true;
            if (killTarget >= 0 && killTarget < MaxPlayers && Alive.Get(killTarget) && SorciereHasDeathPotion)
            {
                SorciereKillTargetThisNight = killTarget;
                SorciereHasDeathPotion = false; // consumée même si timeout
            }
            if (lockAction) SorciereLockedThisNight = true;
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void Rpc_DayVote(PlayerRef voter, int targetId)
        {
            int pid = voter.PlayerId;
            if (Phase != GamePhase.DayVote && Phase != GamePhase.DayDebate) return;
            if (pid < 0 || pid >= MaxPlayers) return;
            if (!Alive.Get(pid)) return;
            // -2 = abstention explicite, -1 = pas de vote, autre = cible
            DayVoteTarget.Set(pid, targetId);
            if (targetId == -2)
                PushFeed($"🕊️ Joueur {pid} s'abstient.");
            else if (targetId >= 0 && targetId < MaxPlayers)
                PushFeed($"🗳️ Joueur {pid} → joueur {targetId}.");
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void Rpc_HunterShoot(PlayerRef hunter, int targetId)
        {
            int pid = hunter.PlayerId;
            if (Phase != GamePhase.HunterShot) return;
            if (pid != HunterPendingShooter) return;
            if (targetId < 0 || targetId >= MaxPlayers) return;
            if (!Alive.Get(targetId)) return;
            KillPlayer(targetId, "chasseur");
            HunterPendingShooter = -1;
            if (TryTriggerHunter(targetId)) return; // chaîne possible
            if (CheckWinCondition()) return;
            ResumeAfterHunter();
        }

        // =====================================================================
        // Helpers d'accès pour UI
        // =====================================================================

        public RoleId GetRoleOf(int playerId)
        {
            if (playerId < 0 || playerId >= MaxPlayers) return RoleId.Villageois;
            int r = Roles.Get(playerId);
            return r < 0 ? RoleId.Villageois : (RoleId)r;
        }

        public bool IsAlive(int playerId)
        {
            if (playerId < 0 || playerId >= MaxPlayers) return false;
            return Alive.Get(playerId);
        }

        public IEnumerable<int> AlivePlayerIds()
        {
            for (int i = 0; i < MaxPlayers; i++)
            {
                if (Alive.Get(i)) yield return i;
            }
        }

        public IEnumerable<int> AliveWolfIds()
        {
            for (int i = 0; i < MaxPlayers; i++)
            {
                if (!Alive.Get(i)) continue;
                if ((RoleId)Roles.Get(i) == RoleId.LoupGarou) yield return i;
            }
        }
    }

    /// Phases du cycle de partie. Ordre canonique Thiercelieux pour la nuit MVP :
    /// Voyante → Loups → Sorcière (Cupidon arrive Épopée 4).
    public enum GamePhase
    {
        Idle = 0,
        Distribution = 1,    // Story 3.1 — écran "Ton rôle est…"
        NightVoyante = 2,
        NightLoups = 3,
        NightSorciere = 4,
        NightResolve = 5,    // résolution des morts nocturnes
        DayDebate = 6,
        DayVote = 7,
        HunterShot = 8,      // overlay si Chasseur meurt
        RevealDeath = 9,     // courte phase d'annonce de mort
        Ended = 10
    }
}
