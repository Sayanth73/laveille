# Story 1.5 — Vocal intégré Photon Voice 2 (livrée)

**Date livraison :** 2026-04-21
**Épopée :** 1 — Plateforme & Authentification

## Acceptance Criteria validés

- [x] SDK Photon Voice 2 importé (Assets/Photon/PhotonVoice/)
- [x] Voice AppId injecté depuis `.photon.local` via PhotonBootstrap (idempotent)
- [x] `IVoiceManager` interface (Connected/Disconnected, SpeakerStartedTalking/Stopped, MicPermissionChanged, Error)
- [x] `PhotonVoiceManager` implémentation (FusionVoiceClient + Recorder + Speaker template, Shared mode via auto-follow du NetworkRunner)
- [x] Bouton Mute (toggle TransmitEnabled) dans DevTestRoom
- [x] Status text "qui parle" via polling `Speaker.IsPlaying`
- [x] Demande permission micro avec texte FR (NSMicrophoneUsageDescription iOS + macOS Info.plist)
- [x] Fallback si permission refusée (TransmitEnabled forcé à false, erreur remontée)
- [x] Test bout-en-bout : 2 instances macOS standalone s'entendent mutuellement

## Artefacts

- `Assets/Scripts/Networking/IVoiceManager.cs` (interface + enums MicPermissionStatus, VoiceLeaveReason, VoiceError)
- `Assets/Scripts/Networking/PhotonVoiceManager.cs`
- `Assets/Scripts/UI/DevTestRoomController.cs` (étendu : mute button, voice status, permission flow)
- `Assets/Editor/PhotonBootstrap.cs` (étendu : injection multi-AppId via `AppIdMappings`, UI mute+voice)
- `Assets/Editor/iOSBuildPostProcessor.cs` (+ NSMicrophoneUsageDescription)

## Décisions techniques

- **Auto-follow Fusion** : `FusionVoiceClient` suit automatiquement le `NetworkRunner` et rejoint un voice room `<sessionName>_voice` séparé. `JoinVoiceChannel/LeaveVoiceChannel` sont des no-ops — on garde l'API symétrique avec `IRoomManager` pour un futur pivot Agora (NFR4 : latence p95 < 250ms).
- **`usePrimaryRecorder` forcé via réflexion** : le flag `[SerializeField] private bool` n'a pas de setter public, mais sans lui `FusionVoiceClient.AddRecorder(PrimaryRecorder)` n'est jamais appelé → pas de capture micro. Fix : `FieldInfo.SetValue` en runtime dans `PhotonVoiceManager.Awake`.
- **SpeakerTemplate actif** : Unity `Instantiate(SpeakerPrefab)` hérite du flag `activeSelf` — un template inactif donne des clones muets ("Can not play a disabled audio source"). Template laissé actif ; pas de nuisance car aucune remote voice n'y est liée.
- **Callbacks Fusion explicites** : `NetworkRunner.AddCallbacks` ne fait pas de scan auto — on itère sur `GetComponents<INetworkRunnerCallbacks>` dans `FusionRoomManager.EnsureRunner` pour enregistrer FusionVoiceClient en parallèle.
- **Playerid resolution** : `Speaker.GetComponentInParent<VoiceNetworkObject>().Object.StateAuthority.PlayerId` pour relier le speaker au joueur Fusion. Fallback -1 si pas de VoiceNetworkObject (cas du primary recorder).

## Notes

- **Latence observée** : subjective (pas de métrique automatisée pour l'instant). Test oral 2 instances même Mac : délai perçu < 300ms, acceptable. Benchmark automatisé à faire avant soumission App Store (NFR4).
- **Avertissement PUN** "not yet run the Photon setup wizard" : bruit sans effet, PUN importé par erreur avec Voice 2. Nettoyage optionnel (dossiers `PhotonUnityNetworking`, `PhotonChat`, `PhotonVoice/Code/PUN`, demos non-Fusion).
- **Permission micro iOS** : NSMicrophoneUsageDescription FR injecté par `iOSBuildPostProcessor`. Texte : "La Veillee a besoin du micro pour faire entendre ta voix aux autres joueurs (Maitre du Jeu et discussions de village)."
- **Fréquence** : mic natif macOS = 44100-96000Hz, on utilise 24000Hz en encodage → resampling côté recorder (warning normal, pas de perte audible).
