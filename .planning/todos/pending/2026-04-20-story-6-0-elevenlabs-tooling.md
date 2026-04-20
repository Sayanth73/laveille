---
created: 2026-04-20T20:30:00Z
title: Insert Story 6.0 — Outillage génération voix MJ ElevenLabs
area: planning
files:
  - _bmad-output/planning-artifacts/epics.md (Épopée 6)
  - _bmad-output/game-architecture.md:Tools/elevenlabs-generator
  - _bmad-output/planning-artifacts/implementation-readiness-report-2026-04-20.md:§5.6
---

## Problem

L'architecture (`game-architecture.md`) mentionne un script Python `Tools/elevenlabs-generator/` qui transforme `mj_script.yaml` en bundle audio Unity (assets `.ogg` organisés par catégorie). Cet outillage est critique pour :
- Régénérer toutes les voix après chaque modif de script (Story 6.1)
- Permettre l'itération sans intervention manuelle ElevenLabs UI
- Maintenir la cohérence du bundle entre versions

**Aucune story Epics ne couvre la création de cet outil.** Story 6.2 suppose les assets déjà générés, mais ne dit pas par qui ni comment.

Identifié dans `implementation-readiness-report-2026-04-20.md` § 5.6 et § 8.1 comme **issue moyenne** à combler avant Épopée 6.

## Solution

Insérer dans `epics.md` une nouvelle story numérotée **Story 6.0** au début de l'Épopée 6 :

```
### Story 6.0 : Outillage génération voix MJ (script Python)

As a chef de projet,
I want un script Python `elevenlabs-generator` qui transforme `mj_script.yaml` en bundle audio Unity,
So que je régénère toutes les voix MJ sans intervention manuelle après chaque modif de script.

**Acceptance Criteria :**

**Given** un fichier `mj_script.yaml` (format Story 6.1) avec catégories (intro, night, day, death, victory) et variantes par ligne
**When** je lance `python tools/elevenlabs-generator/generate.py --voice-id=XXXX --output=Assets/_Project/Audio/MJVoice/`
**Then** chaque ligne est générée via API ElevenLabs et sauvegardée en `.ogg` 128 kbps mono
**And** la structure de fichiers respecte `Audio/MJVoice/{Intro,Night,Day,Death,Victory}/[lineId]_[variantIndex].ogg`
**And** un manifest `mj_voice_manifest.json` est généré listant tous les assets pour `MJVoiceManager`

**Given** le script est ré-exécuté avec un YAML modifié
**When** une ligne existe déjà dans le manifest avec le même hash texte+voiceId
**Then** elle n'est pas régénérée (idempotent — économie crédits ElevenLabs)
**And** seules les lignes nouvelles ou modifiées appellent l'API

**Given** la génération est terminée
**When** Unity rafraîchit le dossier
**Then** Addressables détecte le pack `mj_voice_pack` et l'inclut au build

**Given** une erreur API (rate limit, key invalide, réseau)
**When** un appel échoue
**Then** le script log clairement, garde les fichiers déjà générés, et propose retry
```

À insérer juste-à-temps avant Story 6.2 démarre. Pas bloquant pour l'Épopée 1.
