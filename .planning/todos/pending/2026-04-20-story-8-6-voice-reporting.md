---
created: 2026-04-20T20:30:00Z
title: Insert Story 8.6 — Système de signalement vocal abusif
area: planning
files:
  - _bmad-output/planning-artifacts/epics.md (Épopée 8)
  - _bmad-output/game-architecture.md:Privacy
  - _bmad-output/planning-artifacts/implementation-readiness-report-2026-04-20.md:§5.6,§6.1
---

## Problem

Le GDD et l'Architecture mentionnent que le vocal v1 est non-modéré (gate âge 17+) avec un mécanisme de signalement basique côté équipe modération.

**Aucune story Epics ne couvre l'implémentation de ce mécanisme.** Sans cette story :
- Risque de **rejet App Store** (rating 17+ avec contenu UGC vocal exige un moyen de signalement)
- Pas de capacité à modérer les abus post-launch
- Pas de protection juridique de la plateforme

Identifié dans `implementation-readiness-report-2026-04-20.md` § 5.6 (Epic Quality) et § 6.1 (Sécurité) comme **issue moyenne** à combler avant Épopée 8 (idéalement avant soumission App Store Story 11.3).

## Solution

Insérer dans `epics.md` une nouvelle story numérotée **Story 8.6** dans l'Épopée 8 (Onboarding & Accessibilité — appropriée car c'est aussi de l'UX safety) :

```
### Story 8.6 : Système de signalement vocal abusif

As a joueur,
I want pouvoir signaler le comportement vocal abusif d'un autre joueur en cours ou après partie,
So que la modération peut agir et l'App Store accepte le rating 17+.

**Acceptance Criteria :**

**Given** je suis en cours de partie ou sur l'écran de fin de partie
**When** j'ouvre la fiche d'un autre joueur (long press avatar)
**Then** un bouton "Signaler" est visible et accessible
**And** un haptic léger confirme l'action

**Given** je tape "Signaler"
**When** une sheet de motifs s'ouvre
**Then** je peux choisir parmi : Insultes / Harcèlement / Contenu inapproprié / Autre
**And** un champ texte optionnel permet de préciser (max 500 caractères)

**Given** j'envoie le signalement
**When** la requête s'effectue
**Then** un document est créé dans Firestore `reports/{reportId}` avec :
  - `reporterUid`, `reportedUid`, `roomId`, `gameId`
  - `reason`, `comment`, `timestamp`
  - `reviewed: false`, `actionTaken: null`
**And** une notification email automatique (Cloud Function `notifyModeration`) est envoyée à l'équipe modération
**And** un toast de confirmation "Signalement envoyé, merci" s'affiche

**Given** je signale un même joueur 2x dans la même session
**When** la 2ème requête arrive
**Then** elle est dédupliquée côté serveur (1 report par paire reporter↔reported↔roomId max)

**Given** un utilisateur signalé reçoit ≥ 3 signalements distincts en 7 jours glissants
**When** le seuil est dépassé
**Then** une alerte slack/email priorité haute est envoyée à l'équipe pour review

**Given** la review modération décide d'une action (warning, ban temporaire, ban permanent)
**When** le verdict est posté dans `users/{uid}/moderationStatus`
**Then** le client réagit en conséquence à la prochaine connexion (ex: bannissement vocal 7 jours)
```

À insérer impérativement avant **Story 11.3 (soumission App Store)**. Idéalement développée pendant Épopée 8.
