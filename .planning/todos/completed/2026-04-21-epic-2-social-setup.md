# Épopée 2 — Setup Social de Partie

**Date de livraison :** 2026-04-21
**Branche :** main
**Scope :** 7 stories (2.1 → 2.7)

---

## Stories livrées

### Story 2.1 — HomeScreen
Écran d'accueil joueur : 2 CTA principaux (Créer / Rejoindre une partie), avatar + pseudo top-right (avec génération auto `Joueur-XXXX` persistée en PlayerPrefs), menu hamburger top-left (Paramètres / Mentions légales / Déconnexion — stubs post-MVP).

**Fichiers clés :**
- `Assets/Scripts/UI/HomeScreen.cs`
- `Assets/Scripts/Core/LocalPlayerIdentity.cs` (+ `IPlayerIdentity`, `PlayerIdentityService`)

### Story 2.2 — Création partie + code 6 chiffres
CreateRoomScreen transitoire (loading + retry/back sur échec), code numérique 6 chiffres généré côté `FusionRoomManager.GenerateRoomId()`, bouton Partager copie `laveillee://join/{code}` dans le presse-papier (share sheet iOS natif parked post-MVP).

**Fichiers clés :**
- `Assets/Scripts/UI/CreateRoomScreen.cs`
- `Assets/Scripts/Networking/FusionRoomManager.cs` (GenerateRoomId numérique)
- `Assets/Editor/iOSBuildPostProcessor.cs` (URL scheme CFBundleURLTypes)

### Story 2.3 — Rejoindre par code + deeplink
JoinRoomScreen avec input numérique 6 chars (NumberPad), bouton "Coller depuis le presse-papier" (extrait 6 chiffres), erreurs FR (RoomNotFound / RoomFull / Timeout). Deeplink `laveillee://join/XXXXXX` capturé par `DeeplinkHandler` (cold-start + runtime), pre-fill l'input via `PendingCode`.

**Fichiers clés :**
- `Assets/Scripts/UI/JoinRoomScreen.cs`
- `Assets/Scripts/Core/DeeplinkHandler.cs`

### Story 2.4 — Lobby
LobbyScreen panel affichant code partie + liste joueurs (avatar + pseudo + badge HOST + bouton Kick host-only). Infos joueurs répliquées via `LobbyState.NetworkArray<NetworkString<_32>> Pseudos` + `NetworkArray<int> AvatarSeeds` (indexé par PlayerId). Kick → `Rpc_Kick` + shutdown côté cible. Host migration auto : si l'host part, le `PlayerId` le plus bas est promu.

**Fichiers clés :**
- `Assets/Scripts/UI/LobbyScreen.cs`
- `Assets/Scripts/Networking/LobbyState.cs` (NetworkBehaviour scene-placed)

### Story 2.5 — Composition rôles
CompositionDialog modal avec preset recommandé (cf. `RoleCatalog.RecommendedFor(playerCount)`) + mode custom (+/- par rôle). Validation live (total = playerCount, min 1 Loup, min 1 non-Loup). Sur Valider, écrit dans `LobbyState.CompositionRoles/Counts` (networked arrays capacité 12).

**Fichiers clés :**
- `Assets/Scripts/UI/CompositionDialog.cs`
- `Assets/Scripts/Core/RoleCatalog.cs`

### Story 2.6 — Timers + mode + Démarrer
TimersDialog modal : slider nuit 60–300s (défaut 180), slider jour 120–600s (défaut 300), toggle group exclusif Campfire / Remote. "Démarrer la partie" déclenche `TickTimer` 3s networked, tous les clients transitionnent vers `GameScreen` à l'expiration.

**Fichiers clés :**
- `Assets/Scripts/UI/TimersDialog.cs`
- `Assets/Scripts/UI/LobbyScreen.cs` (`CheckCountdown`)
- `Assets/Scripts/UI/GameScreen.cs` (stub Épopée 3)

### Story 2.7 — Pause/Reprise synchro
Bouton Pause en header du lobby → `Rpc_RequestPauseToggle(pseudo)` → host écrit `Paused` + `PausedBy`. Overlay plein écran sur tous les clients affichant "⏸ Partie en pause par {pseudo}" + bouton Reprendre.

**Fichiers clés :**
- `Assets/Scripts/UI/LobbyScreen.cs` (`BuildPausedOverlay`, `OnPauseToggle`)
- `Assets/Scripts/Networking/LobbyState.cs` (`Rpc_RequestPauseToggle`, `Paused`, `PausedBy`)

---

## Architecture (choix de design)

### Scène unique Main.unity
Pivot architectural décidé en cours d'exécution : initialement multi-scène (Main → Lobby → Game) avec `Runner.LoadScene` pour sync. Trop de complexité avec `NetworkSceneManager` en Shared mode et les scene-placed NetworkObjects. Simplifié en **scène unique + panel switching**. Split de scènes reporté à Épopée 3 si la taille le justifie.

### LobbyState scene-placed
`LobbyState` (NetworkObject + NetworkBehaviour) baked dans Main.unity. Fusion détecte les scene-placed NetworkObjects au StartGame et les réplique (Shared mode). Pas besoin de prefab dynamique ni de registration Editor-time.

### Données joueurs par arrays
Plutôt que spawner un `PlayerData` prefab par joueur (qui requiert prefab baking), on embed tout dans `LobbyState` via `NetworkArray<NetworkString<_32>> Pseudos` + `NetworkArray<int> AvatarSeeds` indexés par `PlayerRef.PlayerId`. Chaque client pousse ses infos via RPC au host (`Rpc_RegisterPlayer`).

### Navigation hybride
- **Panel switching** (Home/Create/Join/Lobby/Game) via `NavigationService.Show<T>()`.
- **Scene load avec fade** disponible pour les transitions inter-contextes (utilisé uniquement en fallback maintenant).

### Design tokens en C# static
`DesignTokens.cs` = source de vérité pour couleurs/spacing/radius/text/motion. Pas de ScriptableObject pour éviter les sérialisations fragiles. Palette Direction A "Veillée intime" (§5.2 UX spec) : `Fire500 #E87B2F`, `Night900 #0B1224`, `Moon100 #F4F1E8`.

---

## Génération de la scène Main

Script Editor `Assets/Editor/Epic2Bootstrap.cs` accessible via menu :
**LaVeillee → Épopée 2 → Generate Main Scene**

Crée Main.unity avec :
- Caméra fond Night900
- `[AppBootstrap]` GameObject (entry point, init services + panel switching)
- `[LobbyState]` GameObject (NetworkObject + LobbyState) scene-placed
- Ajoute Main.unity en tête des Build Settings

Idempotent — safe à ré-exécuter après chaque refactor.

---

## Tests E2E à réaliser (avant de passer à Épopée 3)

Non automatisés (manual gate avec 2 instances macOS). Checklist :

- [ ] Launch 2 instances → HomeScreen avec avatars distincts
- [ ] Instance A clique Créer → room code 6 chiffres affiché dans lobby
- [ ] Instance B clique Rejoindre → entre le code → arrive dans le même lobby
- [ ] Les deux voient les 2 avatars + pseudos + badge HOST sur A
- [ ] A clique "Configurer les rôles" → preset auto-rempli pour 2 joueurs (KO, min 5) → composition custom possible
- [ ] A clique "Timers & mode" → sliders + toggle Campfire/Remote
- [ ] B clique Pause → overlay "⏸ Partie en pause par {pseudo B}" s'affiche sur les 2 instances → Reprendre → overlay disparaît
- [ ] A clique Kick sur B → B est éjecté vers Home
- [ ] A clique Démarrer (avec 5+ joueurs simulés via autres instances) → countdown 3s → transition vers GameScreen stub sur toutes les instances
- [ ] Deeplink `laveillee://join/XXXXXX` depuis terminal/Safari → pre-fill JoinRoomScreen

Tests complets parkés jusqu'à avoir facilement 5+ instances macOS ou ADP (TestFlight multi-device).

---

## Tech debt identifiée

- **Share sheet iOS natif** (Story 2.2 AC) : actuellement on copie dans le presse-papier. Upgrade v2 via `UnityEngine.iOS.NativeShare` ou plugin.
- **Pause button uniquement en lobby** : Épopée 3 déplacera le pause dans GameScreen (actuellement le pause en lobby n'a pas vraiment de sens mais fonctionne pour le demo Story 2.7).
- **Autorité tick host-driven vs state authority** : En Shared mode, toute opération "host-only" (kick, start game, set composition) passe par un check `HasStateAuthority` côté LobbyState. Si un autre peer devient host par migration, ça bascule proprement.
- **Collision check code 6 chiffres** : `GenerateRoomId` n'interroge pas Photon avant de créer. Collision rate ~10⁻⁴ pour 100 rooms actives, acceptable MVP.

---

## Commit

`feat: epic 2 — setup social de partie (stories 2.1 → 2.7)`
