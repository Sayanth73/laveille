---
created: 2026-04-20T20:30:00Z
title: Insert Story 5.0 — Setup pipeline assets externes
area: planning
files:
  - _bmad-output/planning-artifacts/epics.md (Épopée 5)
  - _bmad-output/planning-artifacts/ux-design-specification.md:UX-DR12
  - _bmad-output/planning-artifacts/implementation-readiness-report-2026-04-20.md:§5.6
---

## Problem

L'Épopée 5 (Direction Artistique Cinématique) dépend lourdement de freelances 3D, illustrateur UI et compositeur sound. Aucune story n'orchestre :
- Le briefing initial (formats, conventions naming, palette à respecter)
- Le workflow de livraison (cadence, canaux, stockage Git LFS)
- Le processus de review/validation art (qui valide quoi et quand)
- L'intégration des assets dans le projet Unity

Sans cette story en amont, l'Épopée 5 risque un démarrage chaotique avec va-et-vient art-dev coûteux.

Identifié dans `implementation-readiness-report-2026-04-20.md` § 5.6 et § 8.1 comme **issue moyenne** à combler avant Épopée 5.

## Solution

Insérer dans `epics.md` une nouvelle story numérotée **Story 5.0** au début de l'Épopée 5 :

```
### Story 5.0 : Setup pipeline assets externes

As a chef de projet,
I want un workflow clair pour briefer, recevoir et valider les assets externes (3D, illustrations 2D UI, audio),
So que les freelances livrent en parallèle du dev sans bloqueur ni va-et-vient.

**Acceptance Criteria :**
- Brief écrit envoyé à chaque freelance (3D, illustrateur, compositeur) avec :
  - Direction artistique (refs GDD, palette UX-SPEC)
  - Conventions naming (`avatar_head_01.fbx`, `icon_role_werewolf.svg`, `music_night_loop.ogg`)
  - Format technique (FBX 30k poly max avatars, SVG vector icônes, OGG 128kbps audio)
  - Cadence livraison (weekly drops attendus)
- Conventions intégration Git LFS documentées dans `docs/asset-pipeline.md`
- Process de review art weekly (slot calendaire 30 min, validation/feedback documenté)
- Folder structure `Art/` initialisée dans le repo Unity (Story 1.1 pré-requis)
- Premier livrable test (1 avatar + 1 icône + 1 SFX) reçu et intégré sans friction
```

À insérer juste-à-temps avant que les Stories 5.1, 5.2 ou 6.3 commencent. Pas bloquant pour l'Épopée 1.
