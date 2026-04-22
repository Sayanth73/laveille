using System.Collections.Generic;
using System.Linq;
using LaVeillee.Core;
using LaVeillee.Networking;
using UnityEngine;

namespace LaVeillee.UI
{
    /// Story 5.8 — Controlleur runtime qui mappe les joueurs réseau aux avatars 3D
    /// et transforme les clics 3D (via crosshair FPV) en RPC GameState.
    ///
    /// Architecture :
    ///   - [Socles] (créé par Epic5Bootstrap) contient 12 SceneAvatars pré-positionnés
    ///   - Mapping PlayerId → SceneAvatar : local → socle 0 (face au feu), autres triés
    ///     par PlayerId sur les socles suivants. Socles sans joueur → SetActive(false).
    ///   - Caméra locale : FPVCameraRig posé sur socle local, avatar local renderers OFF
    ///     (on voit ses yeux = rien à rendre).
    ///   - Click : tap/mouse-up sans drag → raycast depuis centre écran → SceneAvatar
    ///     pickable → dispatch au RPC GameState selon phase.
    [DefaultExecutionOrder(110)]
    public class GameSceneController : MonoBehaviour
    {
        GameState _gs;
        LobbyState _lobby;
        FusionRoomManager _room;
        FPVCameraRig _rig;
        SceneAvatar[] _avatars;
        readonly Dictionary<int, SceneAvatar> _byPlayerId = new();
        int _localPlayerId = -1;
        bool _initialLookDone;

        void Awake()
        {
            RebuildAvatarsCache();
        }

        void RebuildAvatarsCache()
        {
            _avatars = Object.FindObjectsByType<SceneAvatar>(FindObjectsSortMode.None)
                .OrderBy(a => a.SlotIndex).ToArray();
        }

        void EnsureRig()
        {
            if (_rig != null) return;
            var cam = Camera.main;
            if (cam == null) return;
            _rig = cam.GetComponent<FPVCameraRig>() ?? cam.gameObject.AddComponent<FPVCameraRig>();
        }

        void Update()
        {
            RefreshRefs();
            if (_gs == null || _gs.Object == null || !_gs.Object.IsValid) return;
            if (_lobby == null || _lobby.Object == null || !_lobby.Object.IsValid) return;
            if (_room == null || _room.Runner == null) return;
            if (_avatars == null || _avatars.Length == 0) RebuildAvatarsCache();
            if (_avatars.Length == 0) return;

            _localPlayerId = _room.Runner.LocalPlayer.PlayerId;

            RebuildMapping();
            ApplyPhaseVisuals();
            HandleClick();
        }

        void RefreshRefs()
        {
            if (_gs == null || _gs.Object == null || !_gs.Object.IsValid)
                _gs = FindFirstObjectByType<GameState>();
            if (_lobby == null || _lobby.Object == null || !_lobby.Object.IsValid)
                _lobby = FindFirstObjectByType<LobbyState>();
            if (_room == null)
                _room = FindFirstObjectByType<FusionRoomManager>();
            EnsureRig();
        }

        void RebuildMapping()
        {
            _byPlayerId.Clear();

            // Joueurs présents = ceux ayant un rôle assigné (Roles[i] >= 0).
            var activePids = new List<int>();
            for (int i = 0; i < GameState.MaxPlayers; i++)
                if (_gs.Roles.Get(i) >= 0) activePids.Add(i);
            activePids.Sort();

            // Local sur socle 0 (face au feu en sortie de BuildSocles), autres ensuite.
            int slot = 0;
            if (activePids.Contains(_localPlayerId))
            {
                AssignSlot(_localPlayerId, 0);
                slot = 1;
            }
            foreach (var pid in activePids)
            {
                if (pid == _localPlayerId) continue;
                if (slot >= _avatars.Length) break;
                AssignSlot(pid, slot++);
            }

            // Slots sans joueur → désactive l'avatar.
            var usedAvatars = new HashSet<SceneAvatar>(_byPlayerId.Values);
            for (int i = 0; i < _avatars.Length; i++)
            {
                var av = _avatars[i];
                if (av == null) continue;
                av.gameObject.SetActive(usedAvatars.Contains(av));
            }

            // Anchor caméra FPV sur socle local + cache l'avatar local.
            if (_byPlayerId.TryGetValue(_localPlayerId, out var localAvatar) && _rig != null)
            {
                _rig.Anchor = localAvatar.transform;
                HideAvatarMesh(localAvatar, true);
                if (!_initialLookDone)
                {
                    _rig.SetInitialLook(Vector3.zero); // regarde le feu
                    _initialLookDone = true;
                }
            }
        }

        void AssignSlot(int playerId, int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _avatars.Length) return;
            var av = _avatars[slotIndex];
            if (av == null) return;
            _byPlayerId[playerId] = av;

            int seed = _lobby.AvatarSeeds.Get(playerId);
            av.Assign(playerId, seed == 0 ? playerId * 37 + 13 : seed);
            av.SetAlive(_gs.IsAlive(playerId));

            if (playerId == _localPlayerId)
            {
                av.SetLabel(null); // pas de label au-dessus de sa propre tête
                return;
            }

            string pseudo = _lobby.Pseudos.Get(playerId).ToString();
            if (string.IsNullOrEmpty(pseudo)) pseudo = $"Joueur {playerId}";

            int votes = CountVotesAgainst(playerId);
            string label = votes > 0 ? $"{pseudo}\n{votes} vote{(votes > 1 ? "s" : "")}" : pseudo;

            // Mort : affiche le rôle révélé (Story 3.9 — salon des morts)
            if (!_gs.IsAlive(playerId))
            {
                var role = _gs.GetRoleOf(playerId);
                label = $"{pseudo}\n💀 {RoleCatalog.LabelFor(role)}";
            }
            av.SetLabel(label);
        }

        int CountVotesAgainst(int targetId)
        {
            if (_gs.Phase != GamePhase.DayDebate && _gs.Phase != GamePhase.DayVote) return 0;
            int n = 0;
            for (int i = 0; i < GameState.MaxPlayers; i++)
            {
                if (!_gs.Alive.Get(i)) continue;
                if (_gs.DayVoteTarget.Get(i) == targetId) n++;
            }
            return n;
        }

        void HideAvatarMesh(SceneAvatar av, bool hide)
        {
            foreach (var r in av.GetComponentsInChildren<Renderer>(true))
            {
                if (r.gameObject.name == "HighlightRing") continue; // garde le halo pickable visible
                r.enabled = !hide;
            }
        }

        void ApplyPhaseVisuals()
        {
            var localRole = _gs.GetRoleOf(_localPlayerId);
            var highlight = HighlightColorFor(_gs.Phase);
            foreach (var kv in _byPlayerId)
            {
                int pid = kv.Key;
                var av = kv.Value;
                bool pickable = CanPick(pid, localRole);
                av.IsPickable = pickable;
                if (pickable) av.Highlight(highlight);
                else av.ClearHighlight();
            }
        }

        bool CanPick(int targetPid, RoleId localRole)
        {
            if (targetPid == _localPlayerId) return false;

            // Chasseur : mort mais peut tirer une dernière fois
            if (_gs.Phase == GamePhase.HunterShot)
            {
                if (_gs.HunterPendingShooter != _localPlayerId) return false;
                return _gs.IsAlive(targetPid);
            }

            if (!_gs.IsAlive(_localPlayerId)) return false;
            if (!_gs.IsAlive(targetPid)) return false;

            return _gs.Phase switch
            {
                GamePhase.NightVoyante => localRole == RoleId.Voyante
                                          && !_gs.VoyanteLockedThisNight,
                GamePhase.NightLoups => localRole == RoleId.LoupGarou
                                        && _gs.GetRoleOf(targetPid) != RoleId.LoupGarou
                                        && !_gs.LoupVoteLocked.Get(_localPlayerId),
                GamePhase.NightSorciere => localRole == RoleId.Sorciere
                                           && _gs.SorciereHasDeathPotion
                                           && !_gs.SorciereLockedThisNight,
                GamePhase.DayDebate or GamePhase.DayVote => true,
                _ => false,
            };
        }

        Color HighlightColorFor(GamePhase phase)
        {
            return phase switch
            {
                GamePhase.NightVoyante  => DesignTokens.Colors.Crystal500,
                GamePhase.NightLoups    => DesignTokens.Colors.Blood500,
                GamePhase.NightSorciere => DesignTokens.Colors.Poison500,
                GamePhase.HunterShot    => DesignTokens.Colors.Fire500,
                GamePhase.DayDebate     => DesignTokens.Colors.Gold500,
                GamePhase.DayVote       => DesignTokens.Colors.Gold500,
                _                       => DesignTokens.Colors.Moon300,
            };
        }

        void HandleClick()
        {
            if (_rig == null) return;
            bool tapped = false;
#if UNITY_IOS || UNITY_ANDROID
            if (Input.touchCount > 0)
            {
                var t = Input.GetTouch(0);
                if (t.phase == UnityEngine.TouchPhase.Ended && !_rig.WasDrag())
                    tapped = true;
            }
#else
            if (Input.GetMouseButtonUp(0) && !_rig.WasDrag()) tapped = true;
#endif
            if (!tapped) return;

            // Bloquer si le pointer était sur un élément UI (bouton HUD, etc.)
            if (UnityEngine.EventSystems.EventSystem.current != null &&
                UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                return;

            var cam = Camera.main;
            if (cam == null) return;
            // Raycast depuis le centre de l'écran (crosshair)
            var ray = cam.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f));
            if (!Physics.Raycast(ray, out var hit, 80f)) return;
            var av = hit.collider.GetComponentInParent<SceneAvatar>();
            if (av == null || !av.IsPickable) return;
            DispatchVote(av.AssignedPlayerId);
        }

        void DispatchVote(int targetId)
        {
            var local = _room.Runner.LocalPlayer;
            switch (_gs.Phase)
            {
                case GamePhase.NightVoyante:
                    _gs.Rpc_VoyanteScan(local, targetId);
                    break;
                case GamePhase.NightLoups:
                    // Double-tap même cible = lock. Premier tap = vote sans lock.
                    bool sameAsCurrent = _gs.LoupVoteTarget.Get(_localPlayerId) == targetId;
                    _gs.Rpc_LoupVote(local, targetId, sameAsCurrent);
                    break;
                case GamePhase.NightSorciere:
                    _gs.Rpc_SorciereAction(local, false, targetId, true);
                    break;
                case GamePhase.DayDebate:
                case GamePhase.DayVote:
                    _gs.Rpc_DayVote(local, targetId);
                    break;
                case GamePhase.HunterShot:
                    _gs.Rpc_HunterShoot(local, targetId);
                    break;
            }
        }
    }
}
