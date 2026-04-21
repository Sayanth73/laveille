# Story 1.4 — Stack multijoueur Photon Fusion 2 (livrée)

**Date livraison :** 2026-04-21
**Épopée :** 1 — Plateforme & Authentification

## Acceptance Criteria validés

- [x] SDK Photon Fusion 2 v2.0.12 importé (Assets/Photon/)
- [x] AppId Fusion injecté depuis `.photon.local` (gitignored) dans `Assets/Photon/Resources/PhotonAppSettings.asset`
- [x] `IRoomManager` interface (events RoomCreated/Joined/Left, PlayerJoined/Left, Error)
- [x] `FusionRoomManager` implémentation Shared mode (peer-to-peer, pas de serveur dédié)
- [x] DevTestRoom scène + UI (4 boutons, InputField room ID, status text)
- [x] Test bout-en-bout : 2 instances macOS standalone se voient dans la même room

## Artefacts

- `Assets/Scripts/Networking/IRoomManager.cs`, `FusionRoomManager.cs`
- `Assets/Scripts/UI/DevTestRoomController.cs`
- `Assets/Scenes/DevTestRoom.unity` (généré par PhotonBootstrap)
- `Assets/Editor/PhotonBootstrap.cs` (menu **LaVeillee → Story 1.4 — Run Photon Bootstrap**) — idempotent
- `Assets/Editor/BuildScript.cs::BuildMacOSDevTest()` — build standalone macOS
- `Assets/Photon/Fusion/Resources/NetworkProjectConfig.fusion` (config par défaut)

## Décisions techniques

- **Shared mode** retenu (vs Hosted/Client-Server) : pas de serveur dédié pour v1, host migration auto.
- **Réflexion** pour `Fusion.Photon.Realtime.PhotonAppSettings` — DLL exclu du Editor platform, on force-load via `Assembly.Load("Fusion.Realtime")`.
- **AppId hors-source** : stocké dans `LaVeillee/.photon.local` (gitignored), injecté à la build/setup.
- **Code de room 6 chars** alphabet sans confusion 0/O/1/I (`ABCDEFGHJKLMNPQRSTUVWXYZ23456789`).

## Notes

- Mon.Cecil 1.10.2 ajouté au manifest pour le IL weaver Fusion.
- `Fusion.Realtime.dll.meta` modifié : `Editor: enabled: 1` (était 0) pour rendre le DLL accessible aux scripts éditeur.
- Pour itérer plus rapidement à 2 instances, envisager d'installer `com.unity.multiplayer.playmode` (Multiplayer Play Mode package).
