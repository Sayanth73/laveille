---
story: 1.1 — Bootstrap projet Unity iOS + CI
status: partial — phase dev complète, phase CI/TestFlight différée
date: 2026-04-20
epic: 1 — Plateforme & Authentification
---

# Story 1.1 — Bootstrap Unity iOS (phase dev)

## Périmètre livré

Couvre les AC de la **première phase** de la Story 1.1 (build local sur Mac + iPhone via cert perso). La phase CI/TestFlight est parkée jusqu'à acquisition de l'Apple Developer Program (99€/an).

### Faits

- ✅ Projet Unity créé à `/Users/sayanth/Desktop/laveille/LaVeillee/`
- ✅ Unity Editor `6000.4.3f1` (Unity 6.4 LTS) — pivot vs plan initial documenté dans [`docs/adr/ADR-008-unity-6-lts-pivot.md`](../../../docs/adr/ADR-008-unity-6-lts-pivot.md)
- ✅ Player Settings iOS configurés (`Assets/Editor/Bootstrap.cs`) :
  - Bundle ID iPhone : `com.laveillee.app`
  - Min iOS : 15.0
  - Target Device : iPhone + iPad
  - Auto signing : enabled
  - Company / Product Name : `La Veillee`
- ✅ Permissions Info.plist :
  - `NSMicrophoneUsageDescription` (via PlayerSettings)
  - `NSBluetoothAlwaysUsageDescription` + `NSBluetoothPeripheralUsageDescription` (via post-processor)
  - `NSLocalNetworkUsageDescription` (via post-processor)
- ✅ Scène `Assets/Scenes/HelloLaVeillee.unity` (Canvas + Text "Hello La Veillee", fond sombre)
- ✅ Scène ajoutée à Build Settings (index 0)
- ✅ Package `com.unity.ugui@2.0.0` ajouté au manifest

### AC validés (phase dev)

| AC | Statut | Note |
|---|---|---|
| Le projet ouvre sous Unity 2023 LTS | ✅ adapté | Unity 6.4 LTS (cf. ADR-008) |
| Build target iOS, iOS 15+ min, Bundle ID `com.laveillee.app` | ✅ | Visible dans `ProjectSettings/ProjectSettings.asset` |
| Permissions Mic / BT / LocalNetwork dans Info.plist | ✅ | Via PlayerSettings + post-processor |
| Pipeline CI build IPA signé → TestFlight | ⏸️ parké | Bloqué Apple Developer Program (cf. README §Prochaines étapes #1) |
| Échec explicite si signature/permissions incorrectes | ⏸️ parké | Idem CI |
| Écran "Hello La Veillée" < 5s + zéro crash sur iPhone 11 | ⏳ à valider | Déploiement device manuel (cert perso 7 jours) — non automatisé dans cette session |

## Reste à faire (suite Story 1.1)

1. **Acquérir Apple Developer Program** (99€/an) — bloquant pour la suite
2. **Provisioning profile** + Apple ID team configurés dans Xcode
3. **Setup Xcode Cloud** (workflow build + sign + upload TestFlight) — cf. ADR-007
4. **Validation device** (iPhone 11 réel) : ouvrir le `.xcodeproj` exporté, signer avec ton Apple ID, run, vérifier "Hello La Veillee" < 5s + console clean

## Prochaines stories

- Story 1.2 — Sign in with Apple (pré-requis : ADP pour SIWA capability)
- Story 1.3 — Profil minimal + déconnexion
- Story 1.4 — Stack multijoueur Photon Fusion 2 (cf. ADR-001 — épopée override : `epics.md` dit "Photon ou Mirror" mais Architecture a déjà tranché)
- Story 1.5 — Vocal Photon Voice 2 (cf. ADR-002 — idem override)

## Fichiers créés / modifiés

```
LaVeillee/                                                 [NEW]
├── Assets/
│   ├── Editor/
│   │   ├── Bootstrap.cs                                   [NEW]
│   │   └── iOSBuildPostProcessor.cs                       [NEW]
│   └── Scenes/
│       └── HelloLaVeillee.unity                           [NEW]
├── Packages/manifest.json                                 [+ com.unity.ugui]
└── ProjectSettings/                                       [auto par Unity]

docs/adr/ADR-008-unity-6-lts-pivot.md                      [NEW]
.planning/STATE.md                                         [maj]
.planning/todos/completed/2026-04-20-story-1-1-...md       [NEW — ce fichier]
```
