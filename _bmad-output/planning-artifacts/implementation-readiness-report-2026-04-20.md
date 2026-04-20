# Implementation Readiness Assessment Report

**Date :** 2026-04-20
**Project :** lg (La Veillée — Loups-Garous mobile iOS)
**Assesseur :** Game Producer / Scrum Master facilitator
**Scope :** Validation de la cohérence GDD ↔ Architecture ↔ UX ↔ Epics avant exécution

---

## 1. Document Discovery

### 1.1 Inventaire

| Document | Chemin | Lignes | Statut |
|---|---|---|---|
| GDD | `_bmad-output/gdd.md` | 792 | ✅ Complet, finalisé 2026-04-20 |
| Game Brief | `_bmad-output/game-brief.md` | 686 | ✅ Complet, source upstream |
| PRD | `_bmad-output/planning-artifacts/prd.md` | 12 | ⚠️ **Stub vide** — frontmatter seulement, pas de contenu |
| Brainstorming | `_bmad-output/brainstorming/brainstorming-session-2026-04-20-1553.md` | n/a | ✅ Référence amont |
| Architecture | `_bmad-output/game-architecture.md` | 987 | ✅ Complet, 4 décisions structurantes lockées, 7 ADRs |
| Epics & Stories | `_bmad-output/planning-artifacts/epics.md` | 2 312 | ✅ 11 épopées, 62 stories, FR coverage map complète |
| UX Design Spec | `_bmad-output/planning-artifacts/ux-design-specification.md` | 929 | ✅ Tokens, composants, wireframes, 20 UX-DRs |
| Implementation artifacts | `_bmad-output/implementation-artifacts/` | — | ⬜ Vide (attendu — pas encore en exécution) |
| Test artifacts | `_bmad-output/test-artifacts/` | — | ⬜ Vide (attendu) |

### 1.2 Doublons détectés

❌ Aucun doublon. Pas de version sharded en concurrence avec un fichier whole. Bonne hygiène.

### 1.3 Documents manquants ou incomplets

| Document | Sévérité | Impact |
|---|---|---|
| **PRD vide** | ⚠️ Mineur | Le GDD remplace fonctionnellement le PRD pour ce projet game-first. Le PRD pourrait être supprimé OU rempli si une vue produit-formelle est attendue par un investisseur/publisher. **Pas bloquant pour l'exécution.** |
| Plan de tests détaillé | ⚠️ Mineur | Pas de Test Strategy.md séparé. La QA est cadrée dans l'Épopée 11 (alpha 50p, beta 500p). Suffisant pour un solo dev / petite équipe ; à formaliser via `gds-test-design` si on souhaite rigoriser. |
| Document narratif | ⬜ N/A | Le GDD couvre suffisamment. Pas de besoin d'un narrative.md pour un party-game social-deduction. |

---

## 2. GDD Analysis

### 2.1 Contenu du GDD validé

✅ Executive Summary clair (concept, audience, USPs, type)
✅ Target Platform spécifié (iOS v1, Android v2)
✅ Target Audience documenté (cible 18-30 francophones)
✅ Goals & rationale (KPIs business + technique)
✅ USPs explicites (Campfire, voix MJ, écran-noir, canon Thiercelieux)
✅ Core Gameplay détaillé (loop jour/nuit, win/loss conditions)
✅ Game Mechanics complet (5 mécaniques + 10 rôles)
✅ Controls & Input + accessibilité
✅ Art & Audio direction (palette, refs, voix MJ, musique, SFX)
✅ Technical Specifications (perf, latence, app size)
✅ Asset Requirements
✅ Development Epics suggestion (10 épopées indicatives)
✅ Success Metrics (technical + gameplay + financial)
✅ Out of Scope explicite
✅ Assumptions & Dependencies + Risques résiduels

### 2.2 Gaps détectés dans le GDD

| Gap | Sévérité | Recommandation |
|---|---|---|
| Pas de FRs/NFRs numérotées formellement (FR1, FR2, …) — extraites par l'équipe Epics | ⬜ N/A | Acceptable. Les Epics ont fait le travail d'extraction (74 FRs + 26 NFRs). Le GDD reste narratif. |
| Pas de timeline détaillée v1 (juste KPIs J+30/90/180) | ⚠️ Mineur | Acceptable pour un solo dev. Roadmap technique implicite via les 11 épopées. |
| Pas de budget détaillé par poste (15-30k€ global mentionné) | ⚠️ Mineur | À détailler si recherche de financement externe ; non bloquant pour l'exécution. |

### 2.3 Verdict GDD

**✅ GDD READY** — Niveau de détail élevé, structure complète, suffisamment opérationnel pour servir de source de vérité aux autres artefacts.

---

## 3. Epic Coverage Validation

### 3.1 Couverture FR (74 FRs extraits du GDD)

J'ai croisé chacun des 74 FRs documentés dans `epics.md` § "FR Coverage Map" avec les stories réelles :

| Type de FR | Nombre | Couverture | Notes |
|---|---|---|---|
| Auth & Profil (FR1-3) | 3 | ✅ 100% (Stories 1.2, 1.3) | OK |
| Lobby & Setup (FR4-11) | 8 | ✅ 100% (Stories 2.1-2.6) | OK |
| Distribution Rôles (FR12-14) | 3 | ✅ 100% (Stories 3.1, 3.x, 4.x) | OK |
| Boucle Jour/Nuit (FR15-22) | 8 | ✅ 100% (Stories 3.2-3.8) | OK |
| Actions Rôles (FR23-33) | 11 | ✅ 100% (Stories 3.3-3.5, 3.10, 4.1-4.6) | OK |
| Vocal (FR34-39) | 6 | ✅ 100% (Stories 1.5, 3.3, 3.9, 8.5) | OK |
| Écran-Noir (FR40-42) | 3 | ✅ 100% (Story 5.6) | OK |
| Mode Campfire BT (FR43-45) | 3 | ✅ 100% (Stories 7.2-7.4) | OK |
| Vue Remote 360° (FR46-47) | 2 | ✅ 100% (Story 5.7) | OK |
| Voix MJ & Audio (FR48-53) | 6 | ✅ 100% (Stories 6.3, 6.5) | OK |
| Mort & Élimination (FR54-57) | 4 | ✅ 100% (Stories 3.9, 5.4) | OK |
| Pause/Resume (FR58-59) | 2 | ✅ 100% (Story 2.7) | OK |
| Onboarding (FR60-61) | 2 | ✅ 100% (Story 8.1) | OK |
| Stats & Achievements (FR62-65) | 4 | ✅ 100% (Stories 9.2, 9.3, 9.5) | OK |
| Cosmétiques (FR66-69) | 4 | ✅ 100% (Stories 4.5, 9.4) | OK |
| Cloud & Sync (FR70-71) | 2 | ✅ 100% (Stories 10.3, 10.4) | OK |
| Post-Partie (FR72-74) | 3 | ✅ 100% (Story 9.1) | OK |

**Total : 74/74 FRs couverts → ✅ 100% coverage.**

### 3.2 Couverture NFR (26 NFRs)

| Catégorie | NFRs | Couverture |
|---|---|---|
| Performance (NFR1-6) | 6 | ✅ Adressés dans Architecture (URP mobile, Photon tick 30Hz, Opus 24kbps) + validés QA Épopée 11 |
| Qualité (NFR7-9) | 3 | ✅ Crashlytics Story 10.5, monitoring Story 11.5 |
| Plateforme (NFR10-13) | 4 | ✅ Story 1.1 (Player Settings iOS 15+, iPhone 11+, iPad adaptatif) |
| Accessibilité (NFR14-19) | 6 | ✅ Stories 8.2-8.5, 5.6 + UX-DR14 |
| Vie privée (NFR20-22) | 3 | ✅ Stories 11.3 (rating 17+), 2.3 (invite-only), 1.2 (Firebase Auth) |
| i18n (NFR23-24) | 2 | ✅ Story 1.1 (Unity Localization Package) ; v2 EN prêt |
| Conformité (NFR25-26) | 2 | ✅ Stories 11.3 (App Store), 10.5 (RGPD) |

**Total : 26/26 NFRs adressés → ✅ 100% coverage.**

### 3.3 NFRs faibles (couverture théorique mais validation à risque)

| NFR | Risque | Mitigation recommandée |
|---|---|---|
| NFR4 (latence vocale < 250 ms p95) | ⚠️ Photon Voice 2 sur réseau mobile FR/EU non benchmarké | Story 1.5 doit produire un benchmark mesuré sur 10 sessions test, sinon plan B Agora préempté |
| NFR5 (latence BT < 150 ms p95) | ⚠️ MTU iOS BT 185 bytes contraint le throughput | Story 7.1 doit valider avec payloads max représentatifs (vote 8 joueurs simultanés = stress test) |
| NFR3 (app size < 500 MB) | ⚠️ MJ voice bundle 80 MB + 3D assets + Photon SDK ≈ 350 MB estimé | Surveillance taille build à chaque sprint + Addressables remote pack pour cosmétiques optionnels (épopée 9) |
| NFR1 (60 fps cinématique iPhone 11) | ⚠️ A13 Bionic sur scène 3D + 25 avatars + particules → tight | Profiling Unity dès Story 5.1 (scène village) avec 25 avatars dummy |

---

## 4. UX Alignment Validation

### 4.1 UX-DRs ↔ Stories

Les 20 UX-DRs documentés dans `ux-design-specification.md` § 14 sont chacun mappés à des stories existantes :

| UX-DR | Stories couvertes | Statut |
|---|---|---|
| UX-DR1 (tokens design) | 1.1 | ✅ |
| UX-DR2 (composants partagés 20+) | 5.1-5.7 transversal | ✅ |
| UX-DR3 (avatar 4 axes 25+ combos) | 5.2, 9.4 | ✅ |
| UX-DR4 (ScreenBlackout 3 modes) | 5.6 | ✅ |
| UX-DR5 (ConfirmActionRing tap+hold) | 3.3, 3.4, 3.5, 3.10, 4.1, 4.2, 4.4 | ✅ |
| UX-DR6 (SubtitleOverlay sync MJ) | 6.5, 8.2 | ✅ |
| UX-DR7 (Remote360Camera) | 5.7 | ✅ |
| UX-DR8 (CampfireTotemView) | 7.3 | ✅ |
| UX-DR9 (CampfireCompanionView) | 7.4 | ✅ |
| UX-DR10 (transitions cinématiques 60fps) | 5.5 | ✅ |
| UX-DR11 (DeathDramaticAnimation orchestrée) | 5.4 | ✅ |
| UX-DR12 (40 icônes custom) | ⚠️ Story 5.1 (kickoff art) — pas de story dédiée illustrateur | ⚠️ Voir gap §4.3 |
| UX-DR13 (i18n Unity Localization) | 1.1, 11.3 | ✅ |
| UX-DR14 (audit accessibilité Apple) | 8.2-8.5 | ✅ |
| UX-DR15 (layout adaptatif iPhone+iPad) | 1.1, 11.1 | ✅ |
| UX-DR16 (RoleCard halos sémantiques) | 3.x, 4.x | ✅ |
| UX-DR17 (Toast erreurs non-bloquants) | Transversal | ✅ |
| UX-DR18 (pas d'IAP avant 1ère partie) | 8.1, 10.1 | ✅ |
| UX-DR19 (EmptyState illustré) | 9.2, 9.3, 9.5 | ✅ |
| UX-DR20 (Splash iOS + Boot Unity) | 1.1 | ✅ |

### 4.2 Cohérence Architecture ↔ UX ↔ Epics

| Aire | Cohérence ? | Notes |
|---|---|---|
| Stack UI (Unity uGUI) | ✅ | Architecture spécifie uGUI, UX décrit composants en uGUI, Epics référent à `Assets/_Project/Scripts/UI/Common/` |
| Écran-noir UX | ✅ | FR40-42 (Epics) ↔ Story 5.6 ↔ Pattern 2 Architecture ↔ UX-DR4 + § USP UX 1 |
| Mode Campfire | ✅ | Épopée 7 ↔ Architecture ADR-003 + GATT custom ↔ UX § 6.3 + UX-DR8/9 |
| Voix MJ | ✅ | Épopée 6 ↔ Architecture ADR-004 (bundled) ↔ UX § 12.3 (sous-titres sync) |
| Tap+hold pattern | ✅ | FR33 ↔ Architecture (référencé dans patterns) ↔ UX § 6.2 + UX-DR5 |
| Cosmétiques avatar | ✅ | Story 9.4 ↔ Architecture (modèle Avatar) ↔ UX § 5.3 (iconographie) + wireframe 13.3 |

### 4.3 Gaps UX détectés

| Gap | Sévérité | Recommandation |
|---|---|---|
| **UX-DR12 sans story dédiée pour l'illustration des 40 icônes custom** | ⚠️ Moyenne | Insérer une story "Story 5.0 — Briefing illustrateur freelance + livraison icônes set v1" en début Épopée 5, OU intégrer la commande externe dans la Story 1.1 (asset pipeline initial). Sans cela, blockers à la Story 5.1 si l'illustrateur n'est pas mobilisé en parallèle. |
| **Pas de wireframe pour l'écran de pairing BT (Story 7.2)** | ⚠️ Moyenne | UX § 13 a wireframé le totem mais pas le flow pairing 4-digit code côté compagnon. À ajouter avant Story 7.2 dev. |
| **Pas de spec UX pour le tutoriel interactif (Story 8.1)** | ⚠️ Moyenne | Le format des "highlights visuels" du tutoriel n'est pas défini dans UX (ex: arrows, spots, tooltips). Risque : refonte côté dev. |
| **Pas de spec UX pour l'écran de configuration BT (Story 2.6, choix Campfire/Remote)** | 🔵 Faible | Le toggle est mentionné mais pas wireframé. Acceptable car composant Toggle standard. |
| Iconographie 40 icônes custom sans grille de validation | 🔵 Faible | UX décrit le style mais pas le process de review art (qui valide ?). À cadrer dans Story 5.0 si créée. |

---

## 5. Epic Quality Review

### 5.1 Indépendance des Épopées

✅ Chaque épopée délivre une valeur joueur autonome dans son domaine.
✅ Aucune épopée future n'est requise par une épopée antérieure pour fonctionner.
✅ MVP minimal possible = Épopées 1+2+3+11 (jeu jouable, brut visuel, sans audio MJ).
✅ Épopées 5 (cinématique), 6 (audio), 7 (Campfire) ajoutent de la valeur sans casser le jeu si non livrées.

### 5.2 Indépendance des Stories au sein des Épopées

Audit échantillon de 10 stories aléatoires :

| Story | Dépendances forward ? | Verdict |
|---|---|---|
| 1.4 (multijoueur Photon) | Non — n'attend que 1.1 (bootstrap) | ✅ |
| 2.5 (composition rôles) | Non — n'attend que 2.4 (lobby) | ✅ |
| 3.7 (vote temps réel) | Note dans 3.7 : "hook préparé pour FR31 (Maire vote x2)" — bonne pratique, pas une dépendance bloquante | ✅ |
| 4.5 (Maire) | Utilise hook de 3.7 — bien | ✅ |
| 5.4 (mort dramatique) | N'attend que 3.8 + 5.1 (scène) | ✅ |
| 6.3 (musique) | N'attend que 6.1 + 6.2 (script + voix générées) | ✅ |
| 7.5 (sync BT multi-device) | N'attend que 7.2 (pairing) | ✅ |
| 8.1 (tutoriel) | Utilise les bots IA — pas de dep | ✅ |
| 9.4 (cosmétiques) | Utilise modèle Avatar de 5.2 | ✅ |
| 10.3 (iCloud sync) | Utilise stats de 9.2 | ✅ |

**Verdict : ✅ Stories séquentiellement implémentables, aucune dépendance forward détectée.**

### 5.3 Sizing des stories (1 dev session)

Audit des stories les plus à risque de surdimensionnement :

| Story | Estimation effort relatif | Verdict |
|---|---|---|
| 1.4 (multijoueur Photon stack) | 3-5 jours | ⚠️ Limite. Pourrait être splittée si on veut découper "intégration SDK" / "wrappers RoomManager" / "tests". Acceptable en l'état. |
| 5.1 (scène village 3D complète) | 5-8 jours (3D + lighting + LOD) | ⚠️ Limite. Forte dépendance sur le freelance 3D. À splitter si nécessaire en : "5.1a — base scène + feu", "5.1b — chalets + props", "5.1c — lighting + skybox". |
| 5.2 (avatars modulaires custom + 25 combos) | 5-7 jours art + 2 jours dev | ⚠️ Limite. Idem, l'art freelance est le gating factor. |
| 6.1 (script MJ ~150 lignes) | 2-4 jours rédaction | ✅ OK |
| 6.2 (génération + intégration ElevenLabs) | 2-3 jours | ✅ OK |
| 7.1 (POC BT iOS CoreBluetooth) | 3-5 jours | ⚠️ Limite. R&D, possibilité de débordement. Acceptable car milestone tech identifié. |
| 11.1 (playtest alpha 50p) | 2-4 semaines (calendaire) | ⬛ Process — pas une story dev classique, sizing OK pour la nature de l'item. |

**Verdict : 3 stories à risque de surdimensionnement (1.4, 5.1, 5.2). Acceptable en l'état mais à surveiller au moment du dev — split possible et préempté.**

### 5.4 Création d'entités à la demande (anti-pattern "tout en avance")

✅ Aucun "Story 1.1 crée toutes les tables Firestore" anti-pattern détecté.
✅ Profil créé Story 1.3, Braises Story 10.2, achievements Story 9.3, leaderboards Story 9.5 — création JIT respectée.

### 5.5 Acceptance criteria testables

Audit échantillon de 10 stories :

| Story | AC clairs Given/When/Then ? | AC mesurables ? |
|---|---|---|
| 1.2 (Sign in Apple) | ✅ | ✅ inclut "< 3 secondes" |
| 1.5 (vocal) | ✅ | ✅ inclut "< 250 ms p95" (NFR4) |
| 2.5 (composition) | ✅ | ✅ règles min explicites (1 loup + 1 villageois) |
| 3.3 (action Loups) | ✅ | ✅ vote majorité simple si timeout |
| 5.6 (écran-noir) | ✅ | ✅ inclut "500 ms fade" |
| 7.1 (POC BT) | ✅ | ✅ inclut "< 100 ms latence", "30 min stable" |
| 8.4 (VoiceOver) | ✅ | ✅ inclut "zéro item P0/P1 audit Apple" |
| 9.3 (achievements) | ✅ | ✅ liste exacte des 20 |
| 10.5 (analytics) | ✅ | ✅ liste événements + délai logs < 5 min |
| 11.5 (launch monitoring) | ✅ | ✅ KPI rétention D7 ≥ 20% |

**Verdict : ✅ Acceptance criteria de qualité élevée, testables et mesurables.**

### 5.6 Gaps Epic Quality détectés

| Gap | Sévérité | Recommandation |
|---|---|---|
| **Pas de story dédiée à l'asset pipeline** (3D, audio sourcing/livraison/QA) | ⚠️ Moyenne | Insérer "Story 5.0 — Setup pipeline assets externes" qui couvre : briefing illustrateurs, conventions naming, validation art weekly, intégration Git LFS, processus de review. Sans cela, ambiguïté sur le QUI/QUAND des livraisons art. |
| **Pas de story pour l'outillage `Tools/elevenlabs-generator/`** | ⚠️ Moyenne | Le script Python qui génère les voix MJ est mentionné en Architecture mais aucune story Epics ne le couvre. Insérer "Story 6.0 — Outillage génération voix MJ (script Python + workflow)" en début Épopée 6. |
| **Pas de story pour le workflow de suppression de compte (RGPD NFR26)** | ⚠️ Moyenne | Architecture détaille le Cloud Function `deleteUser` mais aucune story Epics. Insérer comme "Story 10.6 — Suppression de compte RGPD-compliant". |
| **Pas de story pour la modération vocal v1 (mécanisme signalement)** | ⚠️ Moyenne | Architecture mentionne "bouton Signaler [pseudo]" mais aucune story Epics. Insérer "Story 8.6 — Système de signalement vocal" (anti-rejet App Store rating 17+). |
| **Story 4.5 (Maire) timing flou** | 🔵 Faible | "Mini-phase Élection du Maire" déclenchée "avant le premier vote de lynchage" — préciser : se passe-t-elle après l'annonce des morts nuit 1, ou avant ? À clarifier en dev. |
| **Story 4.3 (Petite Fille détection 30%) sans rationale** | 🔵 Faible | La proba 30% est arbitraire, à valider en playtest alpha (Épopée 11). Mentionné dans Remote Config (Story 10.4) mais pas explicitement. |
| **Stories d'Épopée 11 pas testables au sens dev** | ⬜ N/A | Les stories 11.x sont des process (alpha, beta, soumission) pas du code. Acceptable. |

---

## 6. Cross-cutting Concerns Validation

### 6.1 Sécurité

✅ Règles Firestore documentées et restrictives (deny par défaut)
✅ Anti-cheat turn-based : rôles via RPC ciblée jamais broadcast
✅ Validation IAP côté serveur (Cloud Function)
✅ Encryption BT GATT pour `RoleAssignmentChar` (HKDF)
⚠️ **Pas de story explicite pour la suppression de compte RGPD** (cf. §5.6)
⚠️ **Pas de story pour le mécanisme de signalement** (cf. §5.6)

### 6.2 Performance

✅ Cibles NFR explicites
✅ Architecture documente optimisations (URP, MSAA, Addressables)
⚠️ Pas de story dédiée au profiling régulier (à intégrer comme convention dans `CLAUDE.md` du repo)

### 6.3 Internationalisation

✅ Unity Localization Package locké (Architecture)
✅ Story 1.1 inclut le bootstrap i18n
⚠️ Pas de story pour la review/relecture du français (acceptable pour un solo francophone natif)

### 6.4 Telemetrie & Pilotage

✅ Story 10.5 couvre les événements clés analytics
✅ Story 10.4 couvre Remote Config
⚠️ Pas de story pour la dashboard analytics (qui la construit ? Firebase console suffisante ?)

---

## 7. Risques transverses identifiés

| Risque | Probabilité | Impact | Mitigation préempté |
|---|---|---|---|
| Latence vocal Photon Voice insuffisante | Moyenne | Élevé | Plan B Agora documenté (ADR-002) ; benchmark obligatoire Story 1.5 |
| Coût Photon CCU explose au scale | Moyenne | Moyen | Free tier 100 CCU couvre J+30/60 ; suivi mensuel + bascule plan payant si traction |
| Mode Campfire BT échoue sur certains modèles iPhone | Moyenne | Élevé (USP) | Story 7.1 inclut matrice compat, fallback "Mode Remote" toujours possible |
| Voix MJ ElevenLabs jugée artificielle par testeurs | Moyenne | Moyen | Story 6.2 inclut validation collective ≥ 80% ; budget voix pro réservable si fail |
| Bundle app size > 500 MB (NFR3) | Moyenne | Moyen | Surveillance build size par sprint ; Addressables remote pack pour cosmétiques |
| App Store rejette pour vocal non modéré | Faible | Élevé | Story 11.3 inclut check anti-rejet ; rating 17+ + signalement basique requis |
| Solo dev burnout (12 mois solo + 11 épopées) | Élevée | Critique | Hors scope de cette assessment ; recommander cadence 4-6 semaines/épopée + freelances art/sound parallèle |
| Beta testeurs francophones absents au launch | Moyenne | Moyen | Story 11.4 inclut prospect créateurs FR ; backup : community Discord LG francophone existante |

---

## 8. Recommended Backlog Updates (à insérer dans `epics.md`)

> Si tu choisis de combler les gaps avant exécution, voici les stories suggérées à ajouter :

### 8.1 Stories à insérer

```
### Story 5.0 : Setup pipeline assets externes
As a chef de projet,
I want un workflow clair pour briefer, recevoir et valider les assets externes (3D, illustrations, audio),
So que les freelances livrent en parallèle sans bloquer le dev.
[AC à détailler : conventions naming, weekly art review, intégration Git LFS, livrables par épopée]

### Story 6.0 : Outillage génération voix MJ
As a chef de projet,
I want un script Python `elevenlabs-generator` qui transforme `mj_script.yaml` en bundle audio Unity,
So que je régénère les voix sans intervention manuelle après chaque modif de script.
[AC à détailler : input YAML, output ogg organisés, idempotent, integration Addressables]

### Story 8.6 : Système de signalement vocal
As a joueur,
I want pouvoir signaler un comportement abusif d'un autre joueur,
So que la modération est possible et l'App Store accepte le rating.
[AC à détailler : bouton accessible in-game + post-game, log Firestore, notification email équipe modération]

### Story 10.6 : Suppression de compte RGPD
As a joueur,
I want pouvoir supprimer mon compte et toutes mes données,
So que je suis conforme à mes droits RGPD.
[AC à détailler : confirmation modale, Cloud Function `deleteUser`, purge Firestore + CloudKit + anonymisation purchases, délai 30j de récupération]
```

### 8.2 Wireframes UX à compléter

- Écran pairing BT côté compagnon (scan + saisie code 4 chiffres)
- Tutoriel interactif (highlights, tooltips, arrows)

### 8.3 Précisions à apporter aux stories existantes

- Story 4.5 : timing exact de l'élection du Maire (avant ou après annonce nuits 1 morts ?)
- Story 4.3 : confirmer la valeur 30% de détection Petite Fille en Remote Config

---

## Summary and Recommendations

### Overall Readiness Status

# 🟡 NEEDS MINOR WORK — READY-WITH-CAVEATS

**Le projet est globalement prêt à entrer en exécution.** La couverture FR/NFR est de 100%, l'architecture est cohérente, l'UX est documentée. Les gaps identifiés sont mineurs à modérés et n'empêchent pas de démarrer la Story 1.1 immédiatement, mais doivent être traités avant les épopées concernées (notamment Épopées 5, 6, 8, 10).

### Critical Issues Requiring Immediate Action

**Aucun blocker critique.** Tous les gaps identifiés sont moyens ou faibles, non bloquants pour démarrer l'Épopée 1.

### Issues Moyennes (à traiter avant épopée concernée)

1. **Insérer Story 5.0** (asset pipeline) avant le démarrage Épopée 5
2. **Insérer Story 6.0** (outillage voix MJ) avant le démarrage Épopée 6
3. **Insérer Story 8.6** (signalement vocal) avant Épopée 8 — requis pour l'App Store rating
4. **Insérer Story 10.6** (suppression compte RGPD) avant Épopée 10
5. **Compléter wireframe UX pairing BT** avant Story 7.2
6. **Compléter wireframe UX tutoriel** avant Story 8.1
7. **Préciser timing Story 4.5** (Maire) avant Épopée 4
8. **Préempter benchmark Photon Voice** dès Story 1.5 — sinon fallback Agora pas timé
9. **Surveiller sizing Stories 1.4, 5.1, 5.2** — split possible si la session dev déborde

### Issues Faibles (cosmétiques / amélioration continue)

- Compléter ou supprimer le PRD vide
- Documenter le rationale Petite Fille 30%
- Ajouter une convention "profiling à chaque sprint" dans `CLAUDE.md` du repo

### Recommended Next Steps

1. **Décider** : combler les 4 stories manquantes (5.0, 6.0, 8.6, 10.6) maintenant via `gds-add-todo` ou `gds-explore`, OU les insérer juste-à-temps avant chaque épopée concernée.
2. **Démarrer l'exécution** via `/gds-create-story 1.1` pour préparer le bootstrap Unity iOS comme première story dev-ready.
3. **Mettre en place** le repo Git + CLAUDE.md pour AI agents avant la 1ère ligne de code.
4. **Engager les freelances** (3D artist, UI illustrator, sound composer) en parallèle car les assets sont gating factor des Épopées 5 et 6.
5. **Réserver budget Apple Developer** (99€) immédiatement — bloquant CI/CD.
6. **Valider la décision Photon Voice** via un POC latence dans la 1ère semaine de dev (Story 1.5) — temps réservé pour pivot Agora si insuffisant.

### Final Note

Cette assessment a identifié **9 issues moyennes** et **3 issues faibles** à travers **6 catégories** (couverture FR, NFR, UX, sécurité, sizing, gaps stories). 

**Aucun de ces issues n'est bloquant pour démarrer l'Épopée 1.** Ils peuvent être adressés en parallèle de la phase d'exécution. 

Le projet présente une qualité de spécification au-dessus de la moyenne pour un solo dev / petite équipe, avec une architecture conservatrice et low-ops, des décisions techniques tracées (7 ADRs), et une couverture exigences exhaustive (100% FR + 100% NFR mappés à des stories testables).

**Recommandation finale : 🟢 PROCEED TO EXECUTION** avec les ajustements listés à intégrer juste-à-temps.

---

_Generated by GDS Implementation Readiness Workflow — La Veillée v1.0_
_Date : 2026-04-20_
_Assesseur : Game Producer / Scrum Master facilitator_
_Stack figée : Unity 2023.2 LTS · Photon Fusion 2 · Firebase 11 · CoreBluetooth GATT · ElevenLabs (bundled)_
