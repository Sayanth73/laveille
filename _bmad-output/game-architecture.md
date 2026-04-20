---
title: 'Game Architecture — La Veillée'
project: 'lg'
game_name: 'La Veillée'
date: '2026-04-20'
author: 'sayanth'
version: '1.0'
status: 'complete'
stepsCompleted: [1, 2, 3, 4, 5, 6, 7, 8, 9]
language: 'french'

# Source Documents
gdd: '_bmad-output/gdd.md'
epics: '_bmad-output/planning-artifacts/epics.md'
brief: '_bmad-output/game-brief.md'

# Locked Decisions (Étape 4)
networking_stack: 'Photon Fusion 2'
voice_chat: 'Photon Voice 2'
campfire_pattern: 'CoreBluetooth GATT custom (1 central + N peripherals)'
gm_voice_delivery: 'Bundled ElevenLabs assets (v1) — streaming roadmap v2'
---

# Game Architecture — La Veillée

## Executive Summary

**La Veillée** est un party game social-deduction iOS qui adapte les Loups-Garous de Thiercelieux. L'architecture cible **Unity 2023 LTS** sur iPhone 11+ / iPad / iOS 15+, avec **Photon Fusion 2** pour le multijoueur turn-based, **Photon Voice 2** pour le vocal intégré, **Firebase** pour l'auth/persistance/telemetrie, **CoreBluetooth GATT custom** pour le mode Campfire signature, et des assets vocaux **ElevenLabs bundled** pour le MJ.

L'architecture est volontairement **conservatrice et low-ops** : peu de serveurs custom à opérer, billing prévisible (Photon free tier 100 CCU couvre le MVP launch), services managés (Firebase) pour réduire le DevOps, et un seul stack Unity-C# pour limiter la fragmentation. Le seul investissement R&D fort est le **mode Campfire BT** qui matérialise l'USP marketing principale.

**Principe directeur** : 1 développeur Unity senior (+ freelances art/sound) doit pouvoir livrer la v1 en 4-6 mois sans bloqueur d'infra ni complexité accidentelle. Toute décision architecturale qui n'est pas exigée par le GDD est rejetée par défaut (YAGNI strict).

---

## Project Initialization

### Bootstrap (Story 1.1 du backlog Epics)

```bash
# Pré-requis machine dev (Mac obligatoire pour build iOS)
brew install --cask unity-hub
# Installer Unity 2023.2 LTS via Unity Hub avec module iOS Build Support
brew install --cask xcode
brew install fastlane cocoapods git-lfs

# Création du projet
unity-hub --headless install --version 2023.2.20f1 --module ios
git clone git@github.com:laveillee/laveillee-app.git
cd laveillee-app && git lfs install && git lfs pull

# Premier build (vérifie la chaîne)
fastlane ios build_dev
```

**Apple Developer Program (99€/an)** requis avant tout build sur device. Bundle ID réservé : `com.laveillee.app`.

### Stack initiale (lockée Story 1.1)

- **Unity 2023.2 LTS** + iOS Build Support module
- **Universal Render Pipeline (URP)** — pipeline mobile
- **C#** comme seul langage (pas de Lua / pas de scripting natif)
- **Git LFS** pour les assets binaires (textures, fbx, audio)
- **Xcode Cloud** pour CI/CD iOS (plus simple que GitHub Actions + Fastlane pour solo dev)
- **Unity Package Manager** pour toutes les dépendances (pas d'asset store importé en raw)

---

## Decision Summary

| Catégorie | Décision | Version vérifiée | Affecte | Rationale |
|---|---|---|---|---|
| Game Engine | Unity 2023 LTS | 2023.2.20f1 | Toutes épopées | Imposé par GDD ; meilleur écosystème mobile-iOS, asset store mature, langage C# pour solo dev productif |
| Render Pipeline | URP (Universal RP) | URP 14.x | Épopée 5 | Optimisé mobile (vs HDRP haut-de-gamme) ; supporte cycle jour/nuit + skybox dynamique GDD |
| Langage | C# | .NET Standard 2.1 | Toutes épopées | Standard Unity ; un seul langage à maîtriser pour solo dev |
| Multijoueur | Photon Fusion 2 | Fusion 2.0.x | Épopées 1-4, 7 | Host-based managed, latence FR/EU < 50 ms, free tier 100 CCU couvre MVP, pas de DevOps serveurs |
| Vocal | Photon Voice 2 | Voice 2.x | Épopées 1, 3 | Stack unifiée avec Fusion, billing groupé, qualité suffisante pour 5-25 joueurs (NFR4 < 250 ms p95) |
| Bluetooth (Campfire) | CoreBluetooth GATT custom | iOS 15+ | Épopée 7 | Seul pattern supportant 1 central + 14 peripherals (cible 25 joueurs) ; MultipeerConnectivity limite à 8 |
| Auth | Firebase Auth + Sign in with Apple | Firebase 11.x SDK iOS | Épopée 1 | Sign in with Apple imposé par App Store ; Firebase Auth abstrait la session multi-device |
| Persistance cloud | Cloud Firestore | Firebase 11.x | Épopées 9, 10 | Document-store flexible pour profils/stats/cosmétiques ; règles de sécurité serveur natives ; sync offline-first |
| Persistance device | iCloud (CloudKit) | iOS 15+ | Épopée 10 | Sync inter-device Apple sans backend custom ; complète Firestore (mirror) |
| Voix MJ delivery | Bundled ElevenLabs assets | ElevenLabs API (génération offline) | Épopée 6 | Coût zéro par partie, offline-friendly, latence nulle ; ~80 MB bundle accepté dans budget app size |
| TTS provider | ElevenLabs | API v1 | Épopée 6 (génération) | Qualité voix française premium ; assets pré-générés, jamais d'appel API runtime v1 |
| IAP | Unity IAP (wrapper StoreKit) | Unity IAP 4.x | Épopée 10 | Abstraction multi-store (utile si Android v2) ; intégration native StoreKit |
| Analytics | Firebase Analytics + Crashlytics | Firebase 11.x | Épopée 10 | Gratuit, illimité, dashboards prêts ; intégration Firebase native |
| Remote Config | Firebase Remote Config | Firebase 11.x | Épopée 10 | Cohérent avec stack Firebase ; pas de service tiers à gérer |
| UI Framework | Unity UI (uGUI) | Unity 2023 built-in | Toutes épopées | Plus mature et documentée que UI Toolkit pour mobile ; communauté large |
| Input | Unity Input System | 1.7.x | Toutes épopées | Standard moderne ; supporte multi-touch + gyroscope (FR46) + haptics |
| Audio | Unity Audio (built-in) | Unity 2023 built-in | Épopée 6 | FMOD/Wwise overkill pour ~6 musiques + ~80 SFX + 750 lignes vocales pré-générées |
| Localization | Unity Localization Package | 1.5.x | NFR23-24 | Stack i18n native, prête pour anglais v2 |
| Asset Loading | Addressables | 1.21.x | Toutes épopées | Permet packs DLC (cosmétiques, voix MJ alt v2) sans rebuild app |
| State Management | Photon NetworkBehaviour + State Machine | Fusion native | Épopées 3, 4 | Networked state via Fusion ; FSM côté client pour phases (Setup → Night → Day → Resolution → End) |
| Save System | Hybrid : RAM live + Firestore + CloudKit mirror | — | Épopée 9, 10 | Firestore = source de vérité ; CloudKit = offline-first device ; cache RAM = perf en jeu |
| CI/CD | Xcode Cloud | — | Bootstrap | Géré par Apple ; intégration TestFlight automatique ; moins d'ops qu'un setup Fastlane custom |
| Versioning Assets | Git LFS | Git 2.x + LFS 3.x | Toutes épopées | Audio/3D bundles dépassent vite la limite Git standard |
| Crash Reporting | Firebase Crashlytics | Firebase 11.x | Toutes épopées | Free tier illimité ; symboles iOS auto-uploadés via Xcode Cloud |
| Push Notifications | APNs via Firebase Cloud Messaging | Firebase 11.x | Épopée 9 (re-engagement) | Stack unifié Firebase ; FR engagement ("Paul t'invite à jouer") |

---

## Project Structure

```
laveillee-app/
├── Assets/
│   ├── _Project/                       # Tous les assets propres au projet (préfixe _ = trie en haut)
│   │   ├── Scripts/                    # Code C# applicatif
│   │   │   ├── Core/                   # Bootstrap, ServiceLocator, Constants
│   │   │   ├── Networking/             # Photon Fusion adapters, RPCs, NetworkBehaviours
│   │   │   │   ├── Fusion/             # Wrappers Fusion (RoomManager, NetworkRunner)
│   │   │   │   ├── Voice/              # Photon Voice integration
│   │   │   │   └── Bluetooth/          # CoreBluetooth bridge (mode Campfire)
│   │   │   ├── Game/                   # Logique de gameplay
│   │   │   │   ├── Phases/             # FSM phases (NightPhase, DayPhase, etc.)
│   │   │   │   ├── Roles/              # 1 fichier par rôle (Werewolf.cs, Seer.cs, etc.)
│   │   │   │   ├── Voting/             # Vote system (jour + nuit Loups)
│   │   │   │   ├── Composition/        # Composition validation + recommandations
│   │   │   │   └── WinConditions/      # Détection victoire par camp
│   │   │   ├── UI/                     # Vues uGUI (1 dossier par écran)
│   │   │   │   ├── Lobby/
│   │   │   │   ├── InGame/
│   │   │   │   ├── PostGame/
│   │   │   │   ├── Cosmetics/
│   │   │   │   ├── Settings/
│   │   │   │   └── Common/             # Composants partagés (PlayerAvatar, VoteCounter)
│   │   │   ├── Audio/                  # AudioManager, MJVoiceManager, MusicCue
│   │   │   ├── Cinematic/              # Transitions, ScreenBlackout, DeathAnimation
│   │   │   ├── Persistence/            # Firestore + CloudKit adapters, model serialization
│   │   │   ├── Auth/                   # Firebase Auth + Sign in with Apple bridge
│   │   │   ├── IAP/                    # Unity IAP wrappers + receipt validation
│   │   │   ├── Analytics/              # Firebase Analytics event helpers
│   │   │   ├── RemoteConfig/           # Firebase Remote Config wrappers
│   │   │   ├── Achievements/           # Détecteurs + tracking
│   │   │   ├── Stats/                  # Calcul style de jeu, agrégats
│   │   │   ├── Localization/           # i18n helpers, auto-binding
│   │   │   ├── Accessibility/          # VoiceOver, sous-titres, daltonisme helpers
│   │   │   └── Utils/                  # Helpers transverses (Result<T>, Logger, etc.)
│   │   ├── Scenes/
│   │   │   ├── 00_Boot.unity           # Initialisation services + routing
│   │   │   ├── 01_Auth.unity
│   │   │   ├── 02_Home.unity
│   │   │   ├── 03_Lobby.unity
│   │   │   ├── 04_Game.unity           # Scène principale (village 3D)
│   │   │   └── 99_Tutorial.unity
│   │   ├── Art/
│   │   │   ├── Avatars/                # Modulaires (heads, bodies, hair, hats)
│   │   │   ├── Environment/            # Village (chalets, feu, socles, props)
│   │   │   ├── VFX/                    # Particules (mort, feu, transitions)
│   │   │   ├── Materials/              # PBR materials
│   │   │   ├── Shaders/                # Shader Graph (skybox, dim, water)
│   │   │   └── UI/                     # Sprites, atlases UI
│   │   ├── Animations/                 # AnimatorControllers + clips
│   │   ├── Audio/
│   │   │   ├── Music/                  # 6+ pistes (nuit, jour, mort, victoire FR/LG, menu)
│   │   │   ├── SFX/                    # 80+ SFX courts
│   │   │   └── MJVoice/                # 750+ lignes ElevenLabs (organisées par catégorie)
│   │   │       ├── Intro/
│   │   │       ├── Night/
│   │   │       ├── Day/
│   │   │       ├── Death/
│   │   │       └── Victory/
│   │   ├── Prefabs/
│   │   │   ├── NetworkObjects/         # Player, GameSession, RoleHandler
│   │   │   ├── UI/                     # Vues réutilisables
│   │   │   └── Cinematic/              # Effets composables
│   │   ├── Resources/                  # Configs runtime (à minimiser, préférer Addressables)
│   │   └── Localization/
│   │       ├── fr.csv                  # v1 source
│   │       └── en.csv                  # placeholder v2
│   ├── Plugins/                        # Bridges natifs iOS
│   │   └── iOS/
│   │       ├── BluetoothBridge.swift   # CoreBluetooth GATT (Campfire)
│   │       ├── BluetoothBridge.mm      # Pont Swift ↔ C#
│   │       ├── AppleSignIn.swift
│   │       └── HapticFeedback.swift
│   └── ThirdParty/                     # SDKs externes (Photon, Firebase via UPM, ElevenLabs hooks)
├── Packages/
│   └── manifest.json                   # Toutes deps via UPM (pas d'import asset store raw)
├── ProjectSettings/                    # Versionné Git (Player Settings, Quality, Tags)
├── Tests/
│   ├── EditMode/                       # Tests unitaires (logique pure)
│   └── PlayMode/                       # Tests intégration (scènes)
├── BuildScripts/
│   └── fastlane/                       # Lanes locales fallback (xcode cloud principal)
├── Tools/
│   └── elevenlabs-generator/           # Script Python qui génère mj voice depuis script YAML
├── docs/                               # Doc dev (architecture, runbooks, ADRs)
├── .github/
│   └── workflows/                      # Lint, tests headless
├── .gitignore
├── .gitattributes                      # Règles Git LFS
├── README.md
└── CLAUDE.md                           # Pour les agents IA (conventions, contexte)
```

**Pourquoi ce layout ?**
- Préfixe `_Project/` : tri alphabétique met le code projet en haut, isole des assets tiers.
- 1 dossier = 1 responsabilité (Roles, UI, Audio, etc.) → AI agents trouvent vite leur fichier.
- Plugins natifs iOS isolés sous `Plugins/iOS/` (convention Unity).
- Tests séparés EditMode/PlayMode pour vitesse d'exécution.
- Pas de "Scripts dans Resources" anti-pattern.

---

## Epic to Architecture Mapping

| Épopée | Modules / fichiers principaux concernés |
|---|---|
| Épopée 1 — Plateforme & Auth | `Core/`, `Auth/`, `Networking/Fusion/`, `Networking/Voice/`, `Persistence/`, `00_Boot.unity`, `01_Auth.unity` |
| Épopée 2 — Setup Social | `UI/Lobby/`, `Game/Composition/`, `03_Lobby.unity`, RPC `RoomState` |
| Épopée 3 — Boucle MVP | `Game/Phases/`, `Game/Roles/{Villager, Werewolf, Seer, Witch, Hunter}.cs`, `Game/Voting/`, `Game/WinConditions/`, `04_Game.unity`, `UI/InGame/` |
| Épopée 4 — Catalogue complet | `Game/Roles/{Cupid, Lovers, LittleGirl, Bodyguard, Mayor, Angel}.cs`, extension `WinConditions/` |
| Épopée 5 — Cinématique | `Art/Environment/`, `Art/Avatars/`, `Animations/`, `Cinematic/`, Shader Graph dim/skybox, `UI/InGame/Remote360CameraController` |
| Épopée 6 — Voix MJ + Audio | `Audio/MJVoiceManager.cs`, `Audio/Music/`, `Audio/SFX/`, `Audio/MJVoice/`, `Tools/elevenlabs-generator/` |
| Épopée 7 — Campfire BT | `Networking/Bluetooth/`, `Plugins/iOS/BluetoothBridge.{swift, mm}`, `UI/InGame/TotemView`, `UI/InGame/CompanionView` |
| Épopée 8 — Onboarding & A11y | `99_Tutorial.unity`, `Accessibility/`, `Localization/`, `UI/Common/SubtitleOverlay` |
| Épopée 9 — Méta & Cosmétiques | `Stats/`, `Achievements/`, `UI/PostGame/`, `UI/Cosmetics/`, modèle `Avatar` modulable |
| Épopée 10 — Monétisation/Cloud/Tel | `IAP/`, `Persistence/CloudKitMirror.cs`, `RemoteConfig/`, `Analytics/`, `Auth/PrivacyConsent.cs` |
| Épopée 11 — Launch & QA | Pas de code applicatif (process), uses CI Xcode Cloud + Crashlytics + TestFlight |

---

## Technology Stack Details

### Core Technologies

**Unity 2023.2 LTS (LTS = support 2 ans)**
- Modules requis : iOS Build Support, Universal RP, Addressables, Localization, Input System, IAP
- Player Settings iOS : Target iOS 15.0, ARM64 only, IL2CPP backend, Apple Silicon Mac requis pour build
- Quality Settings : 1 profil "Mobile" actif (URP, MSAA off, soft shadows, 30/60 fps target)

**Photon Fusion 2 (multijoueur)**
- Mode : **Shared Authority** par défaut, escalade vers **Host Mode** pour les actions sensibles (résolution vote, élimination)
- Tick rate : 30 Hz (turn-based, pas besoin de plus)
- Compression : built-in delta compression
- Free tier : jusqu'à 100 CCU sur dashboard public — couvre les J+30 / J+60 du launch
- Plan payant ciblé : "Gaming Pro" $125/mo pour 500 CCU lors du scaling J+90+

**Photon Voice 2 (vocal)**
- Codec : Opus, 24 kbps stereo (qualité parole optimale)
- Push-to-talk : géré côté client via API `Recorder.TransmitEnabled`
- Channels : 1 channel principal "Day", 1 channel "Werewolves" (nuit), 1 channel "Dead" (salon des morts)
- Routing channel basé sur `PlayerState.Status` (Alive, Werewolf, Dead) muté par règles serveur

**Firebase 11.x**
- Modules : Auth, Firestore, Crashlytics, Analytics, Remote Config, Cloud Messaging
- Project ID : `la-veillee-prod` (+ `la-veillee-staging` séparé pour dev)
- Règles Firestore : tout par défaut deny ; whitelist explicite par collection (voir Security Architecture)

**ElevenLabs (voix MJ)**
- Mode : **génération offline batch** via script Python (`Tools/elevenlabs-generator/`)
- Voice ID : sélection unique pour v1 (validation collective interne, voix masculine grave-posée)
- Output format : `.ogg` 128 kbps mono
- Pas d'appel API runtime — assets bundlés dans Addressables `mj_voice_pack`

### Integration Points

**Sign in with Apple → Firebase Auth**
- iOS native `ASAuthorizationController` retourne un identity token
- Bridge Swift `AppleSignIn.swift` envoie le token à Unity (`SendMessage`)
- Unity appelle `Firebase.Auth.SignInWithCredentialAsync(OAuthProvider.GetCredential("apple.com", token))`
- Session Firebase persistée dans Keychain par le SDK

**Photon Fusion ↔ Firebase**
- Pas de couplage direct
- Le client Unity authentifie auprès de Firebase, récupère son `UID` Firebase, l'injecte comme `UserId` Photon
- Firestore stocke les profils ; Photon stocke les rooms (éphémères)

**Bluetooth Campfire ↔ Game Loop**
- Bridge natif iOS (`BluetoothBridge.swift`) implémente `CBCentralManager` (totem) et `CBPeripheralManager` (compagnons)
- Service GATT custom UUID : `A1B2C3D4-...-LAVEILLEE-V1`
- Caractéristiques :
  - `GameStateChar` (read + notify, write par central) — JSON sérialisé
  - `PlayerActionChar` (write par peripheral, notify central) — payloads votes/actions
  - `HeartbeatChar` (notify, 5s) — détection déconnexion
- Côté Unity : un singleton `BluetoothNetworkAdapter` route les messages BT à travers la même interface que Photon (transport pluggable)

**MJ Voice ↔ Phase FSM**
- `MJVoiceManager.PlayLineForEvent(eventType, context)` consulte `mj_voice_pack` via Addressables
- Sélection aléatoire d'une variante non répétée dans la session courante
- Callback `OnLineEnded` notifie la FSM pour démarrer la phase suivante

---

## Novel Pattern Designs

### Pattern 1 — Transport Pluggable (Photon ⊕ Bluetooth)

**Problème** : Le mode Remote utilise Photon ; le mode Campfire utilise Bluetooth. La logique de jeu doit être agnostique du transport.

**Solution** : Interface `INetworkTransport` avec deux implémentations :
- `FusionTransport` : utilise Photon RPCs et `NetworkBehaviour`
- `BluetoothTransport` : utilise GATT writes/notifies

```csharp
public interface INetworkTransport {
    Task SendToHost(NetworkMessage msg);
    Task BroadcastFromHost(NetworkMessage msg);
    event Action<NetworkMessage> OnMessageReceived;
    bool IsHost { get; }
    int PlayerCount { get; }
}
```

`GameSession` (singleton MonoBehaviour) injecte le transport au démarrage de partie selon `RoomConfig.Mode`. Toute la logique gameplay parle à `INetworkTransport`, jamais directement à Photon ou BT.

### Pattern 2 — Écran Noir UX (Single Source of Truth)

**Problème** : L'écran noir nuit doit être strictement synchronisé avec la phase active sur tous les clients (FR40, FR41).

**Solution** : Un component `ScreenBlackoutController` souscrit à `GameSession.OnPhaseChanged` et à `LocalPlayer.ActiveRole` :
- Si phase nuit + LocalPlayer.Role != PhaseActiveRole → blackout (alpha → 1.0 sur Canvas overlay)
- Si phase nuit + LocalPlayer.Role == PhaseActiveRole → unblackout (alpha → 0)
- Vibration alternative (NFR17) : si `Accessibility.UseVibrationInsteadOfBlackout`, déclenche `HapticFeedback.PlayPattern(2s)` à la place

L'overlay est un Canvas séparé en `ScreenSpace - Overlay` avec ordre de rendu max — impossible à louper par les rôles UI.

### Pattern 3 — Voix MJ Anti-Répétition

**Problème** : 5 parties enchaînées sans entendre 2 fois la même variante.

**Solution** : `MJVoiceManager` maintient un `Dictionary<EventType, Queue<int>>` des indices de variantes déjà jouées dans la session courante. À chaque appel `PlayLineForEvent`, exclut les indices encore dans la queue (taille = nb variantes - 2). Reset à `OnGameEnded`.

---

## Implementation Patterns

### Pattern : NetworkBehaviour pour entités synchronisées

Tous les objets dont l'état est synchronisé entre clients héritent de `NetworkBehaviour` Photon Fusion :

```csharp
public class PlayerState : NetworkBehaviour {
    [Networked] public NetworkString<_32> Pseudo { get; set; }
    [Networked] public PlayerRole Role { get; private set; }  // visible uniquement au propriétaire (custom)
    [Networked] public PlayerStatus Status { get; set; }      // Alive, Dead, Endormi
    [Networked] public NetworkBool IsMayor { get; set; }
    [Networked] public NetworkId VotedFor { get; set; }

    public override void Spawned() {
        // Hooks d'initialisation
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_RequestVote(NetworkId targetId) {
        // Validation côté Host avant d'écrire le state
    }
}
```

**Règle stricte** : Le rôle (`Role`) n'est JAMAIS broadcast. Il est révélé via RPC ciblée au seul propriétaire (`RpcTargets.InputAuthority`) lors de la phase de distribution. Sécurité = pas d'inspection client → impossible de hack le rôle des autres.

### Pattern : Phase FSM (côté Host)

Le Host est l'autorité unique sur la phase courante. Les clients réagissent à un `[Networked] CurrentPhase` mis à jour par le Host.

```csharp
public enum GamePhase {
    Setup, RoleDistribution, Cupid, Lovers, Seer, Werewolves,
    LittleGirl, Witch, Bodyguard, Wakeup, Day, Vote, Resolution, End
}

public class PhaseStateMachine : NetworkBehaviour {
    [Networked] public GamePhase CurrentPhase { get; private set; }
    [Networked] public TickTimer PhaseTimer { get; private set; }

    public override void FixedUpdateNetwork() {
        if (!HasStateAuthority) return;
        if (PhaseTimer.Expired(Runner)) AdvancePhase();
    }
}
```

### Pattern : Repository pour persistance

Toutes les écritures Firestore passent par un repository typé :

```csharp
public interface IPlayerProfileRepository {
    Task<PlayerProfile> GetAsync(string uid);
    Task UpdateAsync(PlayerProfile profile);
    Task<List<Achievement>> GetAchievementsAsync(string uid);
}
```

Implémentations :
- `FirestorePlayerProfileRepository` (prod)
- `InMemoryPlayerProfileRepository` (tests EditMode + tutoriel local)

### Pattern : ServiceLocator (light)

Pour éviter une DI framework lourd, on utilise un `ServiceLocator` minimaliste enregistré au boot :

```csharp
public static class Services {
    public static IPlayerProfileRepository Profiles { get; internal set; }
    public static IAnalytics Analytics { get; internal set; }
    public static INetworkTransport Transport { get; internal set; }
    // ...
}
```

Initialisé une fois dans `00_Boot.unity` → `BootController.Awake()`. Aucune logique métier n'instancie ses dépendances directement.

### Pattern : Result<T> pour appels asynchrones

Pas d'exceptions pour les flux normaux (auth refusé, room pleine, etc.) :

```csharp
public sealed class Result<T> {
    public bool IsSuccess { get; }
    public T Value { get; }
    public string ErrorCode { get; }
    public string ErrorMessage { get; }
}

// Usage
var result = await RoomManager.JoinRoomAsync(code);
if (result.IsSuccess) NavigateToLobby(result.Value);
else ShowError(result.ErrorMessage);  // localisé
```

---

## Consistency Rules

### Naming Conventions

| Élément | Convention | Exemple |
|---|---|---|
| Classes C# | `PascalCase` | `PlayerProfile`, `PhaseStateMachine` |
| Méthodes publiques | `PascalCase` | `JoinRoomAsync`, `RevealRole` |
| Méthodes privées | `PascalCase` | `ResolveVote` |
| Champs privés | `_camelCase` | `_currentRole`, `_phaseTimer` |
| Propriétés | `PascalCase` | `IsMayor`, `CurrentPhase` |
| Constantes | `UPPER_SNAKE` | `MAX_PLAYERS = 25` |
| Enums | `PascalCase`, valeurs `PascalCase` | `enum GamePhase { Setup, Day, ... }` |
| RPC Photon | préfixe `RPC_` | `RPC_RequestVote(...)` |
| Networked properties | `PascalCase` (norme Fusion) | `[Networked] CurrentPhase { get; set; }` |
| Async methods | suffixe `Async` | `LoadAvatarAsync` |
| Fichiers | 1 type public par fichier, nom = type | `PlayerProfile.cs` |
| Scènes Unity | `NN_PascalCase.unity` | `04_Game.unity` |
| Prefabs | `PascalCase.prefab` | `PlayerNetworkObject.prefab` |
| Ressources i18n | `screen.element.label` | `lobby.create.button` |

### Code Organization

- **1 type public par fichier**, nom du fichier = nom du type
- **Pas de `using namespace` global** dans les fichiers — toujours explicit imports
- **Méthodes privées sous les méthodes publiques** dans une classe
- **`#region` autorisé seulement pour grouper "RPCs", "Networked Properties"** (lisibilité Fusion)
- **Pas de logique dans les constructeurs** des `NetworkBehaviour` — tout dans `Spawned()`
- **Pas de `MonoBehaviour.Update()` pour la logique réseau** — utiliser `FixedUpdateNetwork()`
- **Coroutines interdites pour le code réseau** — `async/await` ou `TickTimer` Photon

### Error Handling

**Principe** : Pas d'exceptions pour les flux métier prévisibles. Exceptions réservées aux bugs.

| Type d'erreur | Pattern |
|---|---|
| Flux métier (room pleine, code invalide, vote refusé) | `Result<T>.Failure(code, message)` retourné, UI affiche le message localisé |
| Appel SDK tiers échoué (Firebase, Photon) | Catch ciblé → `Result.Failure` + log Crashlytics non-fatal |
| Bug logique (état impossible) | `throw new InvalidOperationException(...)` — sera capturé par Crashlytics |
| Réseau coupé | Retry exponentiel 3x via `RetryPolicy.WithExponentialBackoff(maxAttempts: 3)` puis `Result.Failure("network_lost")` |

Tous les messages d'erreur user-facing sont **localisés** (`fr.csv` clés `errors.*`) — jamais de string hardcodée à l'utilisateur.

### Logging Strategy

**Logger unifié** `Logger.Log(level, category, message, context?)` — wrapper sur `Debug.Log` + Crashlytics.

| Niveau | Quand | Destination |
|---|---|---|
| `Verbose` | Détails de boucle, fréquent | Console editor seulement, off en build |
| `Debug` | Étapes de flux importantes | Console editor, off en build release |
| `Info` | Événements business (room créée, partie démarrée) | Console + Crashlytics breadcrumb |
| `Warning` | Comportements anormaux non bloquants | Console + Crashlytics breadcrumb |
| `Error` | Échec d'une opération attendue | Console + Crashlytics non-fatal |
| `Critical` | État inattendu pouvant corrompre la session | Console + Crashlytics fatal + Analytics event |

**Catégories standards** : `Auth`, `Network`, `Voice`, `Bluetooth`, `Phase`, `Vote`, `Audio`, `UI`, `IAP`, `Persistence`, `Analytics`.

**Règle** : Aucun `Debug.Log` direct dans le code applicatif — toujours via `Logger`.

---

## Data Architecture

### Modèle de données Firestore

**Collection `users/`** (1 document par utilisateur Firebase)
```json
{
  "uid": "firebase-uid-xxx",
  "pseudo": "Sayanth",
  "createdAt": "2026-04-20T13:51:16.309Z",
  "lastSeenAt": "2026-04-20T20:11:00.000Z",
  "appleSignInIdentifier": "001234.xxx.yyy",
  "stats": {
    "gamesPlayed": 42,
    "gamesWon": 18,
    "winRateByRole": { "werewolf": 0.6, "seer": 0.4, ... },
    "mayorElectedCount": 3,
    "playStyle": "Bluffeur"
  },
  "achievements": ["first_win", "the_bluffer", ...],
  "cosmetics": {
    "owned": ["base_skin_01", "winter_pack_01", "mayor_crown"],
    "equipped": {
      "head": "head_03",
      "body": "body_02",
      "hat": "mayor_crown",
      "accessory": null
    }
  },
  "currency": { "embers": 250 },
  "settings": {
    "subtitlesEnabled": true,
    "pushToTalk": false,
    "vibrationInsteadOfBlackout": false,
    "leaderboardOptIn": true,
    "analyticsOptIn": true
  }
}
```

**Collection `purchases/`** (1 document par achat IAP, anti-fraude)
```json
{
  "uid": "firebase-uid-xxx",
  "productId": "winter_skin_pack",
  "transactionId": "apple-tx-xxx",
  "receiptValidated": true,
  "purchasedAt": "2026-05-01T14:22:00Z"
}
```

**Collection `friendGroups/`** (groupes amis détectés via parties partagées — leaderboards privés FR65)
```json
{
  "id": "group-hash-xxx",
  "members": ["uid1", "uid2", "uid3", ...],
  "lastPlayedAt": "...",
  "totalGamesTogether": 12
}
```

**État de partie** : NE PAS persister dans Firestore. État éphémère géré par Photon Fusion (room state). Seules les stats agrégées post-partie sont écrites dans `users/{uid}/stats`.

### Mirror iCloud (CloudKit)

Les mêmes données `users/{uid}` sont mirror-écrites dans CloudKit via le SDK CloudKit natif iOS, dans le container `iCloud.com.laveillee.app`. Sync transparente entre appareils Apple, fonctionne offline-first.

**Conflit de sync** : last-write-wins basé sur `updatedAt` (timestamp serveur). Implémenté dans `Persistence/CloudKitMirror.cs`.

### Modèle réseau (Photon Fusion)

**Network Objects** (1 instance par room)
- `GameSession` (singleton room) — phase courante, timer, composition, mode
- `PlayerState` (1 par joueur connecté) — pseudo, rôle (privé), status, vote
- `RoleHandlers` (1 par rôle actif) — état spécifique (potions Sorcière, lien amoureux Cupidon)

**RPCs** (toutes validées côté Host avant exécution)
- `RPC_RequestVote(NetworkId target)` → résolution dans `VoteSystem.OnFixedUpdate`
- `RPC_RequestRoleAction(RoleAction action, NetworkId target)`
- `RPC_RequestPause()` / `RPC_RequestResume()`
- `RPC_RevealRoleToPlayer(NetworkId player, PlayerRole role)` — ciblée, jamais broadcast

---

## API Contracts

### REST/HTTPS — Aucun

Pas d'API REST custom v1. Tout passe par les SDKs Firebase (auth, firestore, analytics) et Photon (multijoueur, voice).

### RPC — Photon Fusion

Toutes les RPCs sont déclarées avec `[Rpc]` Photon. Format strict :
```csharp
[Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
public void RPC_RequestVote(NetworkId targetPlayerId, RpcInfo info = default) { ... }
```

**Conventions** :
- Préfixe `RPC_` obligatoire
- Source = `InputAuthority` (joueur qui agit) sauf cas spécifique
- Target = `StateAuthority` (Host) qui valide puis modifie le state

### Bluetooth GATT — Service `LaveilleeV1Service`

**UUID Service** : `A1B2C3D4-1111-4444-9999-LAVEILLEE-V1` (à finaliser unique random à Story 7.1)

**Caractéristiques** :

| UUID (alias) | Permissions | Format payload | Direction |
|---|---|---|---|
| `GameStateChar` | Read + Notify | JSON sérialisé `GameStateSnapshot` (max 4 KB) | Central → Peripherals |
| `PlayerActionChar` | Write + Notify (Indication) | JSON `PlayerActionMessage` (max 1 KB) | Peripheral → Central, Central rebroadcast |
| `HeartbeatChar` | Notify (5s interval) | 1 byte (counter) | Bidirectionnel |
| `RoleAssignmentChar` | Write (chiffré, ciblé) | JSON `RoleAssignment` (max 256 B) | Central → 1 Peripheral |

**Encryption** : `RoleAssignmentChar` chiffré avec une clé partagée pendant le pairing initial (4-digit code → key derivation HKDF). Empêche un sniffer BT de récupérer les rôles.

**Reconnexion** : Heartbeat manqué 3x → marquer peripheral comme déconnecté. Tentative auto-reconnect 3x. Au-delà, joueur passe en mode "endormi" (FR57).

---

## Security Architecture

### Authentification

- **Sign in with Apple obligatoire** (App Store guideline 4.8). Pas d'autre méthode v1.
- Token Apple → Firebase Auth → session JWT 1h refresh 30j (config par défaut Firebase)
- Stockage token : Keychain iOS (sécurisé, biométrie-protégé)

### Autorisation Firestore

Règles publiées dans `firestore.rules` :

```javascript
rules_version = '2';
service cloud.firestore {
  match /databases/{database}/documents {

    // users/{uid} : seul le propriétaire lit/écrit, écriture limitée aux champs whitelist
    match /users/{uid} {
      allow read: if request.auth.uid == uid;
      allow create: if request.auth.uid == uid && validUserCreate(request.resource.data);
      allow update: if request.auth.uid == uid && validUserUpdate(request.resource.data, resource.data);
      allow delete: if false;  // suppression compte = workflow dédié
    }

    // purchases/{txId} : write uniquement par Cloud Function avec receipt validé
    match /purchases/{txId} {
      allow read: if request.auth.uid == resource.data.uid;
      allow create, update, delete: if false;  // Cloud Function only via Admin SDK
    }

    // friendGroups/{gid} : read par membres, write par Cloud Function (auto-detected)
    match /friendGroups/{gid} {
      allow read: if request.auth.uid in resource.data.members;
      allow write: if false;
    }
  }
}
```

### Validation IAP

- Côté client : `Unity IAP` reçoit le receipt
- Côté serveur : Cloud Function `validateAppleReceipt` (Node.js) appelle l'API verifyReceipt d'Apple, écrit dans `purchases/` si valide
- Le client ne peut PAS écrire directement dans `purchases/` (règle Firestore deny)

### Vie privée

- **Vocal non modéré v1, gate âge 17+** (App Store rating). Mécanisme de signalement basique : bouton "Signaler [pseudo]" → log Firestore + notification email équipe modération
- **Aucun chat texte global** (pas de risque modération)
- **Aucun matchmaking entre inconnus v1** (uniquement code/lien) — élimine 90% des risques toxicité
- **Analytics opt-in explicite** au premier launch (RGPD NFR26) → toggle dans Settings → Vie privée
- **Suppression compte** : workflow dédié dans Settings → Compte → "Supprimer mon compte" → Cloud Function `deleteUser` qui purge `users/{uid}`, mirror CloudKit, et anonymise dans `purchases/`

### Anti-cheat (turn-based, surface attaque limitée)

- **Rôles révélés via RPC ciblée Host → 1 joueur** : impossible de scraper les rôles des autres en sniffant le trafic Photon
- **Validation côté Host** de toutes les actions (vote, action de rôle) : un client malicieux ne peut pas injecter un vote pour un autre joueur ou se ressusciter
- **Rate limiting RPCs** côté Host : max 10 RPCs/seconde par joueur — anti-spam
- **Pas d'argent réel en jeu** (pas de PvP enjeux financiers) → pas de cible principale pour cheaters

---

## Performance Considerations

### Cibles (rappel NFRs)

- 60 fps cinématiques cible / **30 fps min iPhone 11** (NFR1)
- < 500 MB RAM (NFR2)
- < 500 MB app size launch (NFR3)
- < 250 ms p95 latence vocale (NFR4)
- < 150 ms p95 latence BT (NFR5)
- < 5 s cold start (NFR6)

### Optimisations Unity

- **URP avec MSAA off** sur iPhone 11/12 (texte uGUI déjà anti-aliasé), MSAA 2x sur iPhone 13+
- **Texture compression** : ASTC 6x6 pour avatars, ASTC 4x4 pour UI critique, ETC2 fallback
- **Mesh LOD** : 2 niveaux pour avatars (proche / lointain), 1 niveau pour environment (low-poly stylé)
- **Occlusion culling** : pré-baked sur la scène village (statique)
- **Lighting baked** sur l'environnement statique, lights dynamiques uniquement pour le feu central
- **Object pooling** pour les particules (mort, feu, transitions) via `ObjectPool<T>` Unity 2023
- **Async asset loading** via Addressables — pas de Resources.Load synchrone runtime

### Réseau

- **Tick rate Fusion : 30 Hz** (suffisant pour vote + actions discrètes)
- **Compression delta** : built-in Fusion, pas de tuning custom v1
- **Voice codec Opus 24 kbps** : balance qualité/bandwidth optimale pour parole
- **Pas de spatial audio voix** v1 (overkill pour usage Loup-Garou — la voix est claire stéréo plein écran)

### Bluetooth

- **MTU 185 bytes** négocié au pairing (max iOS) — payloads JSON optimisés en taille (champs courts, pas d'espaces, no-ID si dérivable)
- **Throttle des notifications GameState** : 1 notify par changement de phase ou par action (pas de notify par tick)
- **Heartbeat 5s** : compromis entre détection rapide et battery drain

### Cold start

- Scène `00_Boot.unity` ultra-légère : juste init Firebase + Photon connect → bascule vers `01_Auth.unity` ou `02_Home.unity` selon état session
- **Pas de scène intermédiaire splash** custom — utilise Unity Splash + iOS LaunchScreen storyboard
- Lazy load Addressables : seuls les assets de l'écran courant sont chargés au démarrage

---

## Deployment Architecture

### Environnements

| Env | Firebase Project | Photon AppId | iOS Build | Distribution |
|---|---|---|---|---|
| Local Dev | `la-veillee-staging` | Photon dev appid | Debug build sur device USB | Manuelle Xcode |
| Internal Testing | `la-veillee-staging` | Photon dev appid | TestFlight Internal | Auto via Xcode Cloud sur push `develop` |
| Beta | `la-veillee-prod` | Photon prod appid | TestFlight External | Manuelle (PR mergée + tag `beta-x.y.z`) |
| Production | `la-veillee-prod` | Photon prod appid | App Store | Manuelle (tag `vX.Y.Z`) |

### Pipeline CI/CD (Xcode Cloud)

**Workflow `internal-build`** (déclenché sur push `develop`)
1. Checkout + Git LFS pull
2. Unity batch mode build (compute heavy → Mac M2)
3. Xcode archive + signature (auto-managed)
4. Upload TestFlight Internal
5. Notification Slack + lien TestFlight

**Workflow `beta-release`** (déclenché sur tag `beta-*`)
1. Pareil + upload TestFlight External
2. Notes de version auto-générées depuis git log
3. Email aux 500 beta-testeurs

**Workflow `prod-release`** (déclenché sur tag `v*`)
1. Pareil + upload App Store Connect
2. **Manual gate** : promotion à "Pending Developer Release" requise dans App Store Connect
3. Release sur date marketing planifiée

### Backups & Disaster Recovery

- **Firestore** : exports automatiques quotidiens vers Cloud Storage (`gs://la-veillee-backups/firestore-YYYYMMDD/`), rétention 30 jours
- **Photon** : pas de persistance Photon-side (rooms éphémères) → rien à backup
- **Crashlytics** : rétention 90 jours par défaut Firebase, suffisant
- **Plan d'incident** : runbook dans `docs/runbooks/incident-response.md` (à créer Story 11.5)

### Rollback strategy

- **Code app** : seul Apple peut "rollback" via Phased Release pause → on push hotfix au lieu de rollback
- **Remote Config** : 1-tap rollback dans console Firebase → utile pour revertir un changement de balance qui casse l'équilibrage
- **Firestore schema** : changements toujours additifs (jamais de suppression de champ) → backward compat garantie

---

## Development Environment

### Prerequisites

**Hardware**
- Mac Apple Silicon (M1/M2/M3) — obligatoire pour build iOS et Xcode Cloud
- iPhone physique iOS 15+ pour test (iPhone 11 minimum recommandé)
- (Optionnel) iPad pour valider layout adaptatif

**Software**
- macOS Sonoma 14+ (compat Xcode 15+)
- Xcode 15+ (depuis App Store)
- Unity Hub + Unity 2023.2.20f1 LTS (module iOS)
- Git 2.x + Git LFS 3.x
- Apple Developer Program ($99/an)

**Comptes & accès**
- Firebase Console (Owner sur projet `la-veillee-prod`)
- Photon Dashboard (admin sur `LaVeilleeApp` AppId)
- ElevenLabs (API key pour génération offline voix MJ)
- Apple Developer Portal + App Store Connect (admin)

### AI Tooling (MCP Servers)

Recommandés pour les agents IA travaillant sur ce projet :

| MCP Server | Usage | Priorité |
|---|---|---|
| Filesystem MCP | Lecture/écriture fichiers projet | Critique |
| Git MCP | Historique commits, blame, diffs | Haute |
| Firebase MCP (community) | Lecture Firestore + Crashlytics depuis IDE | Moyenne |
| Photon MCP | (n'existe pas — fallback : doc fetch via WebFetch) | N/A |

### Setup Commands

```bash
# Première installation post-clone
git clone git@github.com:laveillee/laveillee-app.git
cd laveillee-app
git lfs install
git lfs pull

# Configuration Firebase locale
cp Assets/Plugins/iOS/GoogleService-Info.staging.plist Assets/Plugins/iOS/GoogleService-Info.plist

# Lancer Unity
open -a "Unity Hub" .

# Build iOS depuis CLI (équivalent Xcode Cloud)
fastlane ios build_dev

# Run tests
fastlane test
```

### Conventions de PR

- 1 story = 1 branche (`story/1.2-apple-signin`)
- 1 PR = 1 story complète (pas de commits "WIP" en master)
- Squash-merge obligatoire dans `develop`
- Code review : auto-merge si CI ✅ + 1 approval (cas solo dev = self-review puis merge)
- Tag `v1.0.0` sur `main` au launch

---

## Architecture Decision Records (ADRs)

### ADR-001 : Choix de Photon Fusion 2 plutôt que Photon Quantum ou Mirror

**Statut** : Accepté
**Date** : 2026-04-20

**Contexte** : Le jeu nécessite une couche multijoueur stable pour 5-25 joueurs en simultané, principalement turn-based avec des actions discrètes (votes, sélection cible). Pas de simulation physique synchronisée ni de mouvement temps réel.

**Décision** : Photon Fusion 2 en mode Shared Authority avec escalade Host pour actions sensibles.

**Alternatives écartées** :
- **Photon Quantum** : Deterministic ECS + rollback netcode — excellent pour fighting games / esport, mais courbe d'apprentissage massive et zéro ROI pour un turn-based social.
- **Mirror** : Open source, pas de coût, mais nécessite d'héberger ses propres serveurs (DevOps overhead inacceptable pour un solo dev / petite équipe). Pas de scaling auto.
- **Unity Netcode for GameObjects** : Plus jeune, écosystème plus pauvre que Photon, latence moins prouvée internationalement.

**Conséquences** :
- ✅ Free tier 100 CCU couvre tout le MVP launch
- ✅ Hosted, zéro DevOps
- ⚠️ Vendor lock-in (passage à Mirror plus tard = réécriture des `NetworkBehaviour`)
- ⚠️ Coût $125/mo dès 100+ CCU concurrent (prévisible et acceptable)

---

### ADR-002 : Choix de Photon Voice 2 plutôt qu'Agora ou LiveKit

**Statut** : Accepté
**Date** : 2026-04-20

**Contexte** : Vocal intégré requis (USP ; remplace Discord). Cible : 5-25 joueurs simultanés, latence < 250 ms p95, qualité parole.

**Décision** : Photon Voice 2 (intégré à la stack Fusion).

**Alternatives écartées** :
- **Agora** : Latence légèrement meilleure mondialement, mais stack séparée à gérer + billing MAU peut exploser sans visibilité.
- **LiveKit** : Open source moderne, mais nécessite de gérer infra TURN/STUN (DevOps inacceptable solo dev).

**Conséquences** :
- ✅ Stack unifiée, billing groupé Photon
- ✅ Channels par phase (Day, Werewolves, Dead) gérés nativement
- ⚠️ Plan B documenté : passage à Agora si retours beta indiquent qualité insuffisante

---

### ADR-003 : Mode Campfire via CoreBluetooth GATT custom

**Statut** : Accepté
**Date** : 2026-04-20

**Contexte** : Mode Campfire = USP marketing principale (1 tél au centre + N compagnons). Cible jusqu'à 25 joueurs autour d'une table, sans nécessairement de réseau internet (camping, soirée bouchée).

**Décision** : CoreBluetooth GATT custom — 1 central (totem) + N peripherals (compagnons jusqu'à 14, soit 15 joueurs). Au-delà, fallback Photon hybride à concevoir post-launch.

**Alternatives écartées** :
- **MultipeerConnectivity** : Framework Apple haut niveau, P2P automatique, mais limite stricte 8 pairs — bloquant pour la cible jusqu'à 25.
- **Server-Relay (réutiliser Photon)** : Tue l'USP "fonctionne sans data".

**Conséquences** :
- ✅ Tient la promesse marketing complète jusqu'à 15 joueurs
- ⚠️ Investissement R&D significatif (estimé 2 sprints : POC + pairing + sync state + reconnexion + sécurité chiffrement)
- ⚠️ Limite à 15 joueurs en Campfire BT pur — au-delà, mode Remote ou hybride à proposer

---

### ADR-004 : Voix MJ bundlée plutôt que streaming runtime

**Statut** : Accepté pour v1, réévaluable v2
**Date** : 2026-04-20

**Contexte** : ~150 lignes de script MJ avec 3-5 variantes par ligne = ~500-750 fichiers audio. ElevenLabs API permet génération à la demande mais coût par appel.

**Décision** : Génération offline batch via script Python local, bundling dans Addressables `mj_voice_pack`. Aucun appel API ElevenLabs runtime v1.

**Alternatives écartées** :
- **Streaming runtime** : Permettrait des phrases dynamiques ("Sayanth est mort") mais coût à scale (100k DAU × N phrases/partie = $$$) et latence 1-3s casserait le rythme.
- **Hybrid** : Complexité 2x pour un gain marginal v1.

**Conséquences** :
- ✅ Coût par partie zéro
- ✅ Offline-friendly (mode Campfire OK sans data)
- ✅ Latence nulle (préchargé en RAM ou Addressables cache)
- ⚠️ App size +80 MB (acceptable, < 500 MB total)
- ⚠️ Contenu figé entre updates (mitigation : Addressables remote pack pour ajouter du contenu sans rebuild)

---

### ADR-005 : Firestore + iCloud mirror plutôt qu'un seul backend

**Statut** : Accepté
**Date** : 2026-04-20

**Contexte** : Stats / cosmétiques / achievements doivent persister entre sessions et entre appareils (iPhone ↔ iPad).

**Décision** : Firestore = source de vérité (cross-platform-ready pour Android v2). CloudKit = mirror pour offline-first sur Apple devices. Sync `last-write-wins` basé sur `updatedAt` serveur.

**Alternatives écartées** :
- **CloudKit seul** : Lock complet Apple, bloquant pour Android v2.
- **Firestore seul** : Pas d'offline-first natif iOS (Firestore offline cache existe mais moins fluide que CloudKit native).

**Conséquences** :
- ✅ Path Android v2 préservé (réutilisera Firestore tel quel)
- ✅ Expérience offline-first sur Apple grâce à CloudKit
- ⚠️ Logique de sync à coder (`Persistence/CloudKitMirror.cs`) — estimé 1 sprint
- ⚠️ Cas conflit à gérer (last-write-wins acceptable car données non critiques en concurrence)

---

### ADR-006 : Pas de FMOD/Wwise — Unity Audio suffit

**Statut** : Accepté
**Date** : 2026-04-20

**Contexte** : ~6 musiques + ~80 SFX + ~750 lignes vocales pré-générées.

**Décision** : Unity Audio built-in.

**Alternatives écartées** :
- **FMOD** : Indispensable si adaptive music complexe ou +200 SFX dynamiques — pas le cas ici
- **Wwise** : AAA, overkill total pour ce périmètre

**Conséquences** :
- ✅ Zéro coût licence
- ✅ Zéro intégration tierce à maintenir
- ⚠️ Pas d'adaptive music sophistiqué — accepté (musique loop simple par phase)

---

### ADR-007 : Xcode Cloud plutôt que GitHub Actions + Fastlane custom

**Statut** : Accepté
**Date** : 2026-04-20

**Contexte** : CI/CD pour build iOS, nécessite runner macOS, signature, upload TestFlight.

**Décision** : Xcode Cloud (intégré App Store Connect).

**Alternatives écartées** :
- **GitHub Actions + Fastlane** : Très flexible mais setup et maintenance plus lourds, runners macOS coûteux.

**Conséquences** :
- ✅ Setup en 1h
- ✅ Intégration TestFlight automatique
- ✅ Signing managed par Apple
- ⚠️ Dépendance écosystème Apple (acceptable, on cible iOS only v1)
- ⚠️ Coût après free tier (25 build hours/mois gratuits, $50/mo pour 100h)

---

## Validation de cohérence (Étape 8)

✅ Toutes les FRs des Épopées 1-11 ont une trace dans cette architecture (modules, technologies, patterns).
✅ Toutes les NFRs (1-26) ont une stratégie technique adressée.
✅ Aucune décision n'est contradictoire (Fusion ↔ Voice unifiés, Firestore ↔ CloudKit complémentaires).
✅ Le périmètre v1 est tenable par 1 dev senior + freelances dans le budget cible 15-30k€ (pas de stack qui exige une équipe de 5).
✅ Les ADRs documentent les choix structurants pour les futurs onboardings.
✅ La path Android v2 est préservée (Firestore, IAP via Unity IAP wrapper, langage C# portable).

---

_Generated by GDS Architecture Workflow — La Veillée v1.0_
_Date : 2026-04-20_
_For : sayanth_
_Stack figée : Unity 2023.2 LTS · Photon Fusion 2 · Photon Voice 2 · CoreBluetooth GATT · Firebase 11.x · ElevenLabs (bundled) · iOS 15+_
