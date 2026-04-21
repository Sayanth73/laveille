# La Veillée

> Adaptation mobile premium des Loups-Garous de Thiercelieux. iOS first, francophone first.

**Statut :** 🟢 Épopée 1 amorcée — Story 1.1 phase dev livrée, phase CI/TestFlight parkée sur Apple Developer Program
**Auteur :** sayanth
**Plateforme cible v1 :** iOS 15+ (iPhone 11+, iPad)
**Stack technique :** Unity 6.4 LTS · Photon Fusion 2 · Firebase 11 · CoreBluetooth · ElevenLabs
_(Pivot Unity 2023 LTS → Unity 6 LTS documenté dans [`docs/adr/ADR-008-unity-6-lts-pivot.md`](docs/adr/ADR-008-unity-6-lts-pivot.md))_

---

## Concept

La Veillée recrée l'ambiance des veillées d'antan autour d'un feu de camp numérique. L'app prend en charge le travail du Maître du Jeu (distribution rôles, narration, gestion phases, timing), libérant tout le groupe pour participer.

**Deux modes :**
- **Campfire** — un seul téléphone posé au centre, joueurs en présentiel
- **Remote** — chacun son tél, partie à distance avec vocal intégré

---

## Structure du repo

```
lg/
├── LaVeillee/                  # Projet Unity 6 LTS (Story 1.1)
│   ├── Assets/
│   │   ├── Editor/             # Bootstrap.cs, iOSBuildPostProcessor.cs, BuildScript.cs
│   │   └── Scenes/HelloLaVeillee.unity
│   ├── Packages/manifest.json  # + com.unity.ugui
│   └── ProjectSettings/        # Bundle com.laveillee.app, iOS 15+, Mic/BT/LAN
├── _bmad/                      # Configuration BMAD/GDS modules
├── _bmad-output/               # Tous les artefacts de planning
│   ├── gdd.md                  # Game Design Document (792 lignes)
│   ├── game-architecture.md    # Architecture technique (987 lignes, 7 ADRs)
│   ├── game-brief.md           # Brief produit initial
│   ├── brainstorming/          # Sessions de brainstorming
│   ├── planning-artifacts/
│   │   ├── epics.md            # 11 épopées · 62 stories
│   │   ├── ux-design-specification.md  # Design system + wireframes
│   │   ├── prd.md              # (stub — remplacé fonctionnellement par GDD)
│   │   └── implementation-readiness-report-2026-04-20.md
│   ├── implementation-artifacts/  # Code & assets (à venir)
│   └── test-artifacts/            # Test plans & runs (à venir)
├── .planning/                  # Todos transverses (système GSD)
│   ├── STATE.md
│   └── todos/
│       ├── pending/
│       └── completed/
├── docs/                       # Documentation projet (runbooks, ADRs additionnels)
│   └── adr/ADR-008-unity-6-lts-pivot.md
├── README.md                   # Ce fichier
└── .gitignore                  # Ignores planning + Unity + iOS preventifs
```

---

## Avancement

| Phase | Statut | Doc |
|---|---|---|
| 1. Brief produit | ✅ | [`game-brief.md`](_bmad-output/game-brief.md) |
| 2. Brainstorming | ✅ | [`brainstorming/`](_bmad-output/brainstorming/) |
| 3. Game Design Document | ✅ | [`gdd.md`](_bmad-output/gdd.md) |
| 4. Epics & Stories | ✅ | [`epics.md`](_bmad-output/planning-artifacts/epics.md) |
| 5. Architecture technique | ✅ | [`game-architecture.md`](_bmad-output/game-architecture.md) |
| 6. UX Design Specification | ✅ | [`ux-design-specification.md`](_bmad-output/planning-artifacts/ux-design-specification.md) |
| 7. Implementation Readiness | ✅ 🟡 | [`implementation-readiness-report-2026-04-20.md`](_bmad-output/planning-artifacts/implementation-readiness-report-2026-04-20.md) |
| 8. Exécution Épopée 1 (Bootstrap Unity iOS) | 🟡 | Story 1.1 phase dev ✅ ([log](.planning/todos/completed/2026-04-20-story-1-1-bootstrap-unity-partial.md)) — CI/TestFlight ⏸️ |

**Verdict planning :** 🟡 Ready-with-caveats — 4 stories à insérer just-in-time (cf. `.planning/todos/pending/`), aucun blocker pour démarrer l'Épopée 1.

---

## Décisions techniques structurantes

| Décision | Choix | Rationale court | ADR |
|---|---|---|---|
| Engine | Unity 6.4 LTS + URP | Mobile, écosystème mature, solo-dev friendly | Architecture + [ADR-008](docs/adr/ADR-008-unity-6-lts-pivot.md) |
| Multijoueur | Photon Fusion 2 | Hosted, free tier 100 CCU, latence FR/EU < 50ms | ADR-001 |
| Vocal | Photon Voice 2 | Stack unifiée, billing groupé | ADR-002 |
| Mode Campfire | CoreBluetooth GATT custom | 1 central + 14 peripherals, vraiment offline | ADR-003 |
| Voix MJ | ElevenLabs assets bundled | Coût zéro per-play, offline-friendly | ADR-004 |
| Persistance | Firestore + iCloud (CloudKit) mirror | Multi-device + path Android v2 | ADR-005 |

---

## Prochaines étapes

1. **Acquérir Apple Developer Program** (99€/an) — bloquant pour CI/CD iOS + Sign in with Apple (Story 1.2) + TestFlight (reliquat Story 1.1)
2. **Valider déploiement device** du projet bootstrap sur iPhone 11 (cert perso 7 jours) — ouvrir `LaVeillee/` dans Unity, Build & Run
3. **Engager freelances en parallèle** : artiste 3D (avatars + environnement), illustrateur UI (40 icônes), compositeur sound
4. **Story 1.2** — Sign in with Apple (après ADP)
5. **Préempter benchmark Photon Voice** dès Story 1.5 — pivot Agora préempté si latence insuffisante
6. **Insérer juste-à-temps** les 4 stories manquantes :
   - Story 5.0 (asset pipeline) — avant Épopée 5
   - Story 6.0 (outillage voix MJ) — avant Épopée 6
   - Story 8.6 (signalement vocal) — avant Story 11.3 (App Store)
   - Story 10.6 (suppression compte RGPD) — avant Story 11.3 (App Store)

---

## Outillage

Le projet utilise deux systèmes de planning complémentaires :

- **BMAD/GDS** (`_bmad-output/`) — workflow principal de spec (GDD, Architecture, Epics, UX)
- **GSD** (`.planning/`) — tracker léger de todos transverses

Les modules BMAD installés : `core`, `bmm`, `bmb`, `cis`, `gds`, `tea` (cf. `_bmad/_config/manifest.yaml`).

---

_Repo initialisé 2026-04-20 — phase planning complète, prêt pour exécution._
