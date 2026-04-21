# Project State — La Veillée

**Project :** lg (La Veillée — Loups-Garous mobile iOS)
**Last updated :** 2026-04-21 (Story 1.5 livrée — voice E2E à 2 instances ✅)

---

## Current Execution State

- **Épopée active :** 1 — Plateforme & Authentification
- **Stories livrées :**
  - 1.1 — Bootstrap Unity iOS (phase dev ✅ ; phase CI/TestFlight ⏸️ bloquée ADP)
  - 1.4 — Stack multijoueur Photon Fusion 2 (✅ complet — DevTestRoom valide la création/rejoindre room avec 2 instances)
  - 1.5 — Vocal intégré Photon Voice 2 (✅ complet — 2 instances macOS s'entendent mutuellement, mute fonctionne)
- **Stories bloquées (ADP) :** 1.2 (Sign in with Apple), 1.3 (TestFlight pipeline)
- **Prochaine option :** Story 1.6 (si elle existe) ou Story 2.x (lobby/UX) — au choix
- **Moteur :** Unity 6.4.3f1 LTS (pivot vs plan initial — cf. [`docs/adr/ADR-008-unity-6-lts-pivot.md`](../docs/adr/ADR-008-unity-6-lts-pivot.md))
- **Stack réseau :** Photon Fusion 2 v2.0.12 — Shared mode (peer-to-peer, pas de serveur dédié pour v1)
- **Stack vocal :** Photon Voice 2 — auto-follow Fusion (voice room `<sessionName>_voice`)
- **Projet Unity :** `LaVeillee/` (sibling à `_bmad-output/`, `.planning/`, `docs/`)

---

## Planning Artifacts (BMAD/GDS)

Stockés dans `_bmad-output/` (système BMAD/GDS, indépendant de cette structure `.planning/` qui est GSD).

| Artefact | Chemin | Statut |
|---|---|---|
| GDD | `_bmad-output/gdd.md` | ✅ Complet |
| Architecture | `_bmad-output/game-architecture.md` | ✅ Complet |
| Epics & Stories | `_bmad-output/planning-artifacts/epics.md` | ✅ 11 épopées · 62 stories |
| UX Design Spec | `_bmad-output/planning-artifacts/ux-design-specification.md` | ✅ Complet |
| Implementation Readiness Report | `_bmad-output/planning-artifacts/implementation-readiness-report-2026-04-20.md` | ✅ 🟡 Ready-with-caveats |

---

## Accumulated Context

### Pending Todos (4)

Stories à insérer dans `epics.md` avant l'exécution des épopées concernées (issues moyennes identifiées par le readiness report) :

- [Insert Story 5.0 — Setup pipeline assets externes](todos/pending/2026-04-20-story-5-0-asset-pipeline.md) — avant Épopée 5
- [Insert Story 6.0 — Outillage génération voix MJ ElevenLabs](todos/pending/2026-04-20-story-6-0-elevenlabs-tooling.md) — avant Épopée 6
- [Insert Story 8.6 — Système de signalement vocal abusif](todos/pending/2026-04-20-story-8-6-voice-reporting.md) — avant Story 11.3 (App Store soumission)
- [Insert Story 10.6 — Suppression de compte RGPD-compliant](todos/pending/2026-04-20-story-10-6-account-deletion-rgpd.md) — avant Story 11.3 (App Store soumission)

### Completed Todos (3)

- [Story 1.1 — Bootstrap Unity iOS (phase dev)](todos/completed/2026-04-20-story-1-1-bootstrap-unity-partial.md) — 2026-04-20. Projet Unity 6 LTS, Player Settings iOS, scène Hello, Info.plist permissions, build Xcode project export OK. Phase CI/TestFlight parkée sur acquisition ADP.
- [Story 1.4 — Stack multijoueur Photon Fusion 2](todos/completed/2026-04-21-story-1-4-photon-fusion-stack.md) — 2026-04-21. DevTestRoom scène + FusionRoomManager (Shared mode), AppId injection via bootstrap idempotent, E2E 2 instances macOS validé.
- [Story 1.5 — Vocal intégré Photon Voice 2](todos/completed/2026-04-21-story-1-5-photon-voice.md) — 2026-04-21. IVoiceManager + PhotonVoiceManager (auto-follow Fusion), mute button, permission micro FR, E2E 2 instances macOS s'entendent mutuellement.

---

## Notes

- Le projet utilise le système **BMAD/GDS** (game-dev-studio) pour le planning principal (`_bmad-output/`)
- Cette structure **`.planning/`** (GSD — get-shit-done) sert uniquement de tracker léger pour les todos transverses
- Pas de commit git automatique pour ces todos (le repo git est à `C:/Users/sayan/`, hors-projet — risque trop large)
