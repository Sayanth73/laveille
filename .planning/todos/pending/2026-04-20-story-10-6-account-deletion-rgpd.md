---
created: 2026-04-20T20:30:00Z
title: Insert Story 10.6 — Suppression de compte RGPD-compliant
area: planning
files:
  - _bmad-output/planning-artifacts/epics.md (Épopée 10)
  - _bmad-output/game-architecture.md:Privacy
  - _bmad-output/planning-artifacts/implementation-readiness-report-2026-04-20.md:§5.6,§6.1
---

## Problem

L'architecture (`game-architecture.md` § Vie privée) détaille le workflow de suppression de compte avec une Cloud Function `deleteUser` qui purge `users/{uid}`, le mirror CloudKit, et anonymise dans `purchases/`.

**Aucune story Epics ne couvre ce workflow.** Sans cette story :
- Non-conformité **RGPD** (NFR26) — droit à l'effacement Article 17
- Risque légal en France/UE
- Risque de rejet App Store (conformité régulations locales)

Identifié dans `implementation-readiness-report-2026-04-20.md` § 5.6 (Epic Quality) et § 6.1 (Sécurité) comme **issue moyenne** à combler avant Épopée 10 (et impérativement avant launch Story 11.3).

## Solution

Insérer dans `epics.md` une nouvelle story numérotée **Story 10.6** dans l'Épopée 10 :

```
### Story 10.6 : Suppression de compte RGPD-compliant

As a joueur,
I want pouvoir supprimer mon compte et toutes mes données associées,
So que je suis conforme à mes droits RGPD (Article 17 — droit à l'effacement) et l'App Store accepte la soumission.

**Acceptance Criteria :**

**Given** je suis authentifié et j'ouvre les paramètres
**When** je vais dans Compte → "Supprimer mon compte"
**Then** un écran d'avertissement clair m'explique les conséquences :
  - Toutes les stats, achievements, cosmétiques perdus définitivement
  - Aucune restauration possible après 30 jours
  - Période de grâce 30 jours pendant laquelle je peux annuler
**And** une saisie explicite "SUPPRIMER" est requise pour confirmer

**Given** je confirme la suppression
**When** la requête est envoyée
**Then** une Cloud Function `requestAccountDeletion` est appelée
**And** un document `deletionRequests/{uid}` est créé avec `scheduledFor: now() + 30 days`
**And** mon compte est marqué `users/{uid}.deletionScheduled: true`
**And** je suis déconnecté immédiatement
**And** un email de confirmation m'est envoyé avec un lien d'annulation valide 30 jours

**Given** je veux annuler la suppression dans les 30 jours
**When** je tape le lien email OU me reconnecte avec Sign in with Apple
**Then** un écran "Souhaites-tu annuler la suppression de ton compte ?" s'affiche
**And** si je confirme, le flag est levé et je récupère intégralement mon compte

**Given** 30 jours sont écoulés sans annulation
**When** un cron Cloud Function `processDeletionRequests` s'exécute (daily)
**Then** la Cloud Function `executeDeletion` est appelée pour cet uid
**And** purge `users/{uid}` (Firestore)
**And** purge `friendGroups/` membres (rebuild sans uid)
**And** anonymise `purchases/` (remplace `uid` par `"deleted-user"`, garde reçu pour comptabilité)
**And** déclenche purge mirror CloudKit côté device au prochain sign-in (impossible à faire sans device)
**And** révoque le token Firebase Auth (force re-signin si jamais l'utilisateur recrée un compte)

**Given** un utilisateur supprimé tente de se reconnecter avec Sign in with Apple
**When** il atteint l'écran d'auth
**Then** un nouveau compte est créé from scratch (pas de récupération possible)

**Given** un audit RGPD interne ou externe
**When** le workflow complet est testé
**Then** aucune donnée personnelle ne subsiste dans Firestore après 30j+1
**And** les logs Crashlytics anciens sont anonymisés (UID remplacé par hash one-way)
```

À insérer impérativement avant **Story 11.3 (soumission App Store)**. Idéalement développée pendant Épopée 10.
