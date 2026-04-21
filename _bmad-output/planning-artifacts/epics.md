---
stepsCompleted: [step-01-validate-prerequisites, step-02-design-epics, step-03-create-stories, step-04-final-validation]
inputDocuments:
  - _bmad-output/gdd.md
  - _bmad-output/game-brief.md
  - _bmad-output/planning-artifacts/prd.md
workflowType: 'epics-and-stories'
project_name: 'lg'
game_name: 'La Veillée'
user_name: 'sayanth'
date: '2026-04-20'
language: 'french'
notes:
  - "Architecture.md absent — exigences techniques dérivées du GDD (sections 'Technical Specifications' et 'Asset Requirements')."
  - "UX Design Specification absent — section UX Design Requirements vide ; les exigences UX sont intégrées aux stories pertinentes."
  - "Bootstrap Unity (Story 1.1) tient lieu de 'starter template' en l'absence d'Architecture explicite."
---

# La Veillée — Découpage Épopées & Stories

## Vue d'ensemble

Ce document décompose les exigences du GDD de **La Veillée** (adaptation mobile premium des Loups-Garous de Thiercelieux) en épopées orientées valeur joueur et en user stories implémentables. Chaque épopée est autonome et délivre une valeur joueur complète. Chaque story est dimensionnée pour être réalisée par un seul agent dev en une session.

**Périmètre v1 :** iOS uniquement (iPhone 11+, iOS 15+), 10 rôles canoniques Thiercelieux, modes Campfire (BT) + Remote (vocal intégré), voix MJ ElevenLabs, ambiance cinématique.

**Hors périmètre v1 :** Android, Web, rôles additionnels (Loup Blanc, etc.), thèmes alternatifs, voix MJ alternatives, mode enfants, matchmaking entre inconnus.

---

## Inventaire des Exigences

### Functional Requirements

**Authentification & Profil**
- **FR1** : L'utilisateur peut s'authentifier via Sign in with Apple
- **FR2** : Le système crée un profil minimal (pseudo + avatar par défaut) à la première connexion
- **FR3** : L'utilisateur peut se déconnecter et revenir à l'écran d'accueil

**Lobby & Création de Partie**
- **FR4** : Un host peut créer une partie privée générant un code de salle à 6 chiffres
- **FR5** : Un joueur peut rejoindre une partie en saisissant le code à 6 chiffres
- **FR6** : Un joueur peut rejoindre une partie via un deeplink (lien partagé)
- **FR7** : Le host peut configurer la composition des rôles (presets selon taille + custom)
- **FR8** : Le host peut configurer les timers nuit/jour (3 min / 5 min par défaut, ajustables)
- **FR9** : Le host choisit le mode Campfire ou Remote au setup de la partie
- **FR10** : Le host peut kicker un joueur du lobby pré-partie
- **FR11** : Le système recommande automatiquement une composition selon le nombre de joueurs

**Distribution des Rôles**
- **FR12** : Le système distribue aléatoirement les rôles de manière privée à chaque joueur
- **FR13** : Chaque joueur ne voit que son propre rôle, jamais celui des autres
- **FR14** : Le système supporte les 10 rôles canoniques v1 (Villageois, Loup-Garou, Voyante, Sorcière, Chasseur, Cupidon, Petite Fille, Salvateur, Maire, Ange)

**Boucle Jour/Nuit**
- **FR15** : Le système orchestre l'ordre nocturne canonique strict (Cupidon nuit 1 → Amoureux nuit 1 → Voyante → Loups → Petite Fille → Sorcière → Salvateur)
- **FR16** : Le système déclenche la phase jour avec débat vocal libre
- **FR17** : Le système gère un vote public temps réel pendant la phase jour
- **FR18** : Un joueur peut modifier son vote tant que le timer jour court
- **FR19** : Le système affiche un feed temps réel "X a voté pour Y"
- **FR20** : La phase jour a un timer paramétrable avec décompte audio + visuel
- **FR21** : Le système élimine le joueur le plus voté à la fin du timer jour
- **FR22** : Le système détecte la fin de partie pour tous les camps (Villageois, Loups, Ange, Amoureux)

**Actions de Rôles**
- **FR23** : Les Loups-Garous cliquent collectivement sur une cible nocturne (vote silencieux entre eux)
- **FR24** : La Voyante tape un joueur et reçoit la révélation privée de son rôle
- **FR25** : La Sorcière dispose de 2 potions (1 vie, 1 mort) utilisables 1 fois chacune
- **FR26** : Le Chasseur tire sur un joueur en réaction à sa propre mort
- **FR27** : Cupidon lie 2 joueurs comme amoureux pendant la nuit 1 uniquement
- **FR28** : Les deux amoureux se reconnaissent mutuellement pendant la nuit 1
- **FR29** : La Petite Fille peut ouvrir brièvement les yeux pendant la phase Loups (risque = être vue = mort)
- **FR30** : Le Salvateur protège un joueur chaque nuit, contrainte "pas 2 nuits de suite sur la même cible"
- **FR31** : Le vote du Maire compte double pendant la phase jour
- **FR32** : L'Ange gagne en solo s'il est lynché lors du tour 1 de jour
- **FR33** : Le système exige tap + hold 0.5s pour les actions irréversibles (cible mort, potion, tir)

**Communication Vocale**
- **FR34** : Vocal intégré actif pendant toute la phase jour pour les vivants
- **FR35** : Vocal intégré entre Loups pendant leur phase nocturne
- **FR36** : Vocal intégré entre joueurs morts dans le salon des morts
- **FR37** : Toggle mute/dé-mute disponible à tout moment
- **FR38** : Indicateur visuel "tu parles" affiché pendant l'émission vocale
- **FR39** : Push-to-talk optionnel (alternative au micro permanent)

**UX Écran-Noir (innovation centrale)**
- **FR40** : Pendant la nuit, l'écran d'un joueur dont le rôle n'est pas actif devient noir
- **FR41** : Seul le joueur dont le rôle est actif voit son écran allumé
- **FR42** : Haptic feedback subtil quand le rôle d'un joueur devient actif

**Mode Campfire (BT)**
- **FR43** : Le mode Campfire pair 1 téléphone totem + N téléphones compagnons via CoreBluetooth
- **FR44** : Le téléphone totem affiche une vue ambiante (sombre) + diffuse l'audio MJ et la musique
- **FR45** : Les téléphones compagnons affichent l'UI privée (rôle + action + vote)
- **FR46** : Le mode Remote affiche une vue 360° immersive (swipe horizontal OU gyroscope)
- **FR47** : Les joueurs Remote voient les autres avatars autour du feu sur leur écran

**Voix MJ & Ambiance**
- **FR48** : Le MJ vocal narre chaque transition de phase
- **FR49** : Le MJ vocal appelle chaque rôle pendant la phase nuit
- **FR50** : Le système joue de la musique d'ambiance nocturne pendant la nuit
- **FR51** : Le système joue de la musique d'ambiance diurne pendant le jour
- **FR52** : Un stinger sonore dramatique accompagne chaque mort
- **FR53** : Un thème final court (10-15s) joue à la victoire ou défaite

**Mort & Élimination**
- **FR54** : Une animation dramatique (socle qui craque, particules, tombe) joue à chaque mort
- **FR55** : Les joueurs morts rejoignent un salon des morts avec vocal séparé
- **FR56** : Un joueur mort ne peut plus rejoindre la partie en cours
- **FR57** : Un joueur déconnecté devient "endormi" (bot passif qui vote aléatoirement) — anti-griefing

**Pause / Reprise**
- **FR58** : N'importe quel joueur peut mettre la partie en pause via le menu (timer stoppé, vocal mute)
- **FR59** : N'importe quel joueur peut reprendre la partie depuis la pause

**Onboarding**
- **FR60** : Un joueur première-fois reçoit un tutoriel interactif au lancement de sa première partie
- **FR61** : Le tutoriel est skippable pour les joueurs vétérans

**Stats & Achievements**
- **FR62** : Le système enregistre les stats par joueur (parties jouées, taux de victoire par rôle)
- **FR63** : Le système détecte un "style de jeu" (bluffeur, investigateur, survivor, accusateur)
- **FR64** : Le système propose ~20 achievements en v1
- **FR65** : Leaderboards privés optionnels entre amis (désactivables)

**Cosmétiques**
- **FR66** : Le joueur peut customiser son avatar (coiffure, chapeau, peau, vêtements)
- **FR67** : Certains cosmétiques se déverrouillent via achievements (pas tout en IAP)
- **FR68** : Le système supporte des IAP cosmétiques (skins, thèmes, packs voix MJ alternatifs)
- **FR69** : Le skin "couronne d'or" se débloque quand le joueur est élu Maire

**Cloud & Sync**
- **FR70** : iCloud synchronise stats, cosmétiques et achievements entre appareils du même utilisateur
- **FR71** : Remote config (Firebase) pilote l'équilibrage et les feature flags sans nouvelle release

**Post-Partie**
- **FR72** : L'écran de fin de partie révèle le rôle de chaque joueur
- **FR73** : L'écran de fin de partie affiche un résumé des stats de la partie
- **FR74** : Les joueurs peuvent enchaîner immédiatement une nouvelle partie avec le même groupe ("Replay")

### NonFunctional Requirements

**Performance**
- **NFR1** : Cible 60 fps en cinématique, minimum 30 fps sur iPhone 11
- **NFR2** : Empreinte mémoire RAM en jeu < 500 MB
- **NFR3** : Taille app v1 launch < 500 MB ; < 1 GB avec tous les asset packs optionnels
- **NFR4** : Latence vocale moyenne < 200 ms ; p95 < 250 ms
- **NFR5** : Latence Bluetooth Totem-Compagnons < 100 ms ; p95 < 150 ms
- **NFR6** : Cold start < 5 s du tap icône à l'écran d'accueil

**Qualité & Fiabilité**
- **NFR7** : Crash-free rate ≥ 99.5%
- **NFR8** : Uptime backend ≥ 99.5%
- **NFR9** : Taux d'erreur matchmaking < 1%

**Plateforme**
- **NFR10** : Support iOS 15+ exclusivement en v1
- **NFR11** : iPhone 11 (A13 Bionic) minimum
- **NFR12** : iPad supporté dès v1 avec layout adaptatif
- **NFR13** : Résolution adaptive jusqu'à 1440p (Pro Max)

**Accessibilité**
- **NFR14** : Compatibilité VoiceOver complète sur tous les écrans
- **NFR15** : Sous-titres MJ optionnels activables dans les settings
- **NFR16** : UI daltonienne (information redondée par emojis + chiffres + couleurs)
- **NFR17** : Vibration alternative à l'écran noir pour joueurs malvoyants
- **NFR18** : Indication timer visuelle + audio (bip dernières 10s)
- **NFR19** : Micro off par défaut, push-to-talk optionnel

**Vie Privée & Sécurité**
- **NFR20** : Vocal non modéré v1 (gate âge 18+)
- **NFR21** : Pas de matchmaking entre inconnus en v1 — invitations uniquement par code/lien
- **NFR22** : Données utilisateur stockées avec chiffrement at-rest côté Firebase

**Internationalisation**
- **NFR23** : Langue v1 = français (UI + voix MJ)
- **NFR24** : Architecture i18n prête pour anglais en v2

**Conformité**
- **NFR25** : Conformité App Store (rating, IAP, vocal)
- **NFR26** : RGPD-compliant (consentement analytics, droit suppression compte)

### Additional Requirements

> ⚠️ **Note** : Aucun document `Architecture.md` n'existe à date. Les exigences techniques ci-dessous sont dérivées des sections "Technical Specifications", "Asset Requirements" et "Dependencies" du GDD. Story 1.1 fait office de bootstrap projet et tient lieu de "starter template".

**Stack Technique** _(superseded by `_bmad-output/game-architecture.md` + ADRs — voir notes ci-dessous)_
- Engine : ~~**Unity 2023 LTS**~~ → **Unity 6.4 LTS** + iOS build toolchain (Xcode) _(ADR-008)_
- Multijoueur : ~~Photon Quantum ou Mirror~~ → **Photon Fusion 2** _(ADR-001 — décision figée, plus à trancher en 1.4)_
- Vocal : ~~Agora ou LiveKit~~ → **Photon Voice 2** _(ADR-002 — stack unifiée, benchmark résiduel à la Story 1.5 pour valider latence)_
- Backend : **Firebase** (Auth, Firestore, Analytics, Cloud Save, Remote Config, Crashlytics)
- Voix MJ : **ElevenLabs API** (mode bundled v1, streaming si budget v2)
- IAP : App Store In-App Purchase
- Apple Sign In requis (NFR Apple)

**Infrastructure & Outils**
- CI/CD pour build iOS automatisé (Xcode Cloud ou GitHub Actions + Fastlane)
- Apple Developer Program (99€/an) configuré
- Asset Store packs Unity pour accélération art (avatars, environnement)

**Intégrations iOS Spécifiques**
- CoreBluetooth pour pairing Totem ↔ Compagnons (mode Campfire)
- Taptic Engine pour haptics
- iCloud sync (CloudKit) pour stats / cosmétiques
- Push notifications (APNs) pour relancer les groupes

**Données & Persistance**
- Profil utilisateur (Firebase Auth UID + pseudo + avatar)
- État de partie (Photon room state, éphémère)
- Stats cumulées (Firestore + iCloud sync)
- Cosmétiques possédés (Firestore + iCloud sync)
- Configuration distante (Remote Config)

**Pré-requis Voix MJ**
- Script complet ~150 lignes à écrire (toutes phases + variantes anti-répétition)
- Génération + validation collective des assets vocaux
- Mixage final pro

### UX Design Requirements

> ⚠️ **Note** : Aucun document `UX Design Specification` n'existe à date. Les contraintes UX énoncées dans le GDD sont intégrées directement dans les stories des Épopées 5 (cinématique), 8 (onboarding/accessibilité) et 2 (lobby). Si un UX-SPEC dédié est produit ultérieurement, les UX-DRs y seront extraites et croisées dans une révision de ce document.

### FR Coverage Map

| FR | Épopée | Story | Description courte |
|---|---|---|---|
| FR1 | Épopée 1 | 1.2 | Sign in with Apple |
| FR2 | Épopée 1 | 1.3 | Profil minimal |
| FR3 | Épopée 1 | 1.3 | Déconnexion |
| FR4 | Épopée 2 | 2.2 | Code de salle 6 chiffres |
| FR5 | Épopée 2 | 2.3 | Rejoindre par code |
| FR6 | Épopée 2 | 2.3 | Rejoindre par deeplink |
| FR7 | Épopée 2 | 2.5 | Composition de rôles |
| FR8 | Épopée 2 | 2.6 | Timers paramétrables |
| FR9 | Épopée 2 | 2.6 | Choix mode Campfire/Remote |
| FR10 | Épopée 2 | 2.4 | Kicker un joueur |
| FR11 | Épopée 2 | 2.5 | Recommandation auto composition |
| FR12 | Épopée 3 | 3.1 | Distribution privée des rôles |
| FR13 | Épopée 3 | 3.1 | Visibilité limitée à son propre rôle |
| FR14 | Épopée 3 + 4 | 3.x + 4.x | 10 rôles canoniques |
| FR15 | Épopée 3 | 3.2 | Ordre nocturne canonique |
| FR16 | Épopée 3 | 3.6 | Phase jour avec débat vocal |
| FR17 | Épopée 3 | 3.7 | Vote public temps réel |
| FR18 | Épopée 3 | 3.7 | Modification du vote |
| FR19 | Épopée 3 | 3.7 | Feed temps réel |
| FR20 | Épopée 3 | 3.6 | Timer jour audio + visuel |
| FR21 | Épopée 3 | 3.8 | Élimination quotidienne |
| FR22 | Épopée 3 + 4 | 3.8 + 4.2 + 4.6 | Détection fin partie tous camps |
| FR23 | Épopée 3 | 3.3 | Action Loups-Garous |
| FR24 | Épopée 3 | 3.4 | Action Voyante |
| FR25 | Épopée 3 | 3.5 | Potions Sorcière |
| FR26 | Épopée 3 | 3.10 | Tir Chasseur |
| FR27 | Épopée 4 | 4.1 | Cupidon lie les amoureux |
| FR28 | Épopée 4 | 4.2 | Reconnaissance amoureux |
| FR29 | Épopée 4 | 4.3 | Petite Fille espionne |
| FR30 | Épopée 4 | 4.4 | Salvateur protège |
| FR31 | Épopée 4 | 4.5 | Vote double Maire |
| FR32 | Épopée 4 | 4.6 | Victoire solo Ange |
| FR33 | Épopée 3 | 3.3 (et autres) | Tap + hold actions irréversibles |
| FR34 | Épopée 1 | 1.5 | Vocal jour |
| FR35 | Épopée 3 | 3.3 | Vocal Loups |
| FR36 | Épopée 3 | 3.9 | Vocal salon des morts |
| FR37 | Épopée 1 | 1.5 | Mute / unmute |
| FR38 | Épopée 1 | 1.5 | Indicateur "tu parles" |
| FR39 | Épopée 8 | 8.5 | Push-to-talk |
| FR40 | Épopée 5 | 5.6 | Écran noir nuit |
| FR41 | Épopée 5 | 5.6 | Écran allumé rôle actif |
| FR42 | Épopée 5 | 5.6 | Haptic feedback rôle actif |
| FR43 | Épopée 7 | 7.2 | Pairing BT Totem + Compagnons |
| FR44 | Épopée 7 | 7.3 | Vue totem |
| FR45 | Épopée 7 | 7.4 | Vue compagnon |
| FR46 | Épopée 5 | 5.7 | Vue Remote 360° |
| FR47 | Épopée 5 | 5.7 | Avatars autour du feu |
| FR48 | Épopée 6 | 6.5 | MJ narre transitions |
| FR49 | Épopée 6 | 6.5 | MJ appelle rôles nuit |
| FR50 | Épopée 6 | 6.3 | Musique nuit |
| FR51 | Épopée 6 | 6.3 | Musique jour |
| FR52 | Épopée 6 | 6.3 | Stinger mort |
| FR53 | Épopée 6 | 6.3 | Thème victoire/défaite |
| FR54 | Épopée 5 | 5.4 | Animation mort dramatique |
| FR55 | Épopée 3 | 3.9 | Salon des morts |
| FR56 | Épopée 3 | 3.9 | Joueurs morts ne reviennent pas |
| FR57 | Épopée 3 | 3.9 | Bot endormi anti-griefing |
| FR58 | Épopée 2 | 2.7 | Pause |
| FR59 | Épopée 2 | 2.7 | Reprise |
| FR60 | Épopée 8 | 8.1 | Tutoriel interactif |
| FR61 | Épopée 8 | 8.1 | Tutoriel skippable |
| FR62 | Épopée 9 | 9.2 | Stats personnelles |
| FR63 | Épopée 9 | 9.2 | Détection style de jeu |
| FR64 | Épopée 9 | 9.3 | ~20 achievements |
| FR65 | Épopée 9 | 9.5 | Leaderboards privés |
| FR66 | Épopée 9 | 9.4 | Customisation avatar |
| FR67 | Épopée 9 | 9.4 | Déverrouillage via achievements |
| FR68 | Épopée 10 | 10.1 | IAP cosmétiques |
| FR69 | Épopée 4 | 4.5 | Skin couronne Maire |
| FR70 | Épopée 10 | 10.3 | iCloud sync |
| FR71 | Épopée 10 | 10.4 | Remote config |
| FR72 | Épopée 9 | 9.1 | Révélation rôles fin partie |
| FR73 | Épopée 9 | 9.1 | Stats fin partie |
| FR74 | Épopée 9 | 9.1 | Replay même groupe |

**NFR Coverage** : NFR1-NFR9 traitées transversalement et validées à l'Épopée 11 (QA) ; NFR10-NFR13 fixées à Story 1.1 ; NFR14-NFR19 traitées Épopée 8 ; NFR20-NFR22 Épopées 1, 2, 10 ; NFR23-NFR26 traitées Épopées 1 (i18n init), 10 (RGPD), 11 (App Store).

---

## Epic List

### Épopée 1 : Plateforme & Authentification
Bootstrap technique du projet et capacité pour un joueur de s'authentifier et créer une room basique. Pose les fondations sans laquelle aucune autre épopée ne fonctionne.
**FRs couvertes :** FR1, FR2, FR3, FR34, FR37, FR38

### Épopée 2 : Setup Social de Partie
Permet à un host de créer une partie configurée et à des amis de la rejoindre facilement. Délivre l'expérience pré-jeu friction-zéro qui est la promesse marketing.
**FRs couvertes :** FR4, FR5, FR6, FR7, FR8, FR9, FR10, FR11, FR58, FR59

### Épopée 3 : Boucle Jour/Nuit MVP (5 rôles essentiels)
Le jeu doit tourner — une partie complète jouable de bout en bout avec les 5 rôles fondamentaux (Villageois, Loup-Garou, Voyante, Sorcière, Chasseur). C'est le cœur de produit.
**FRs couvertes :** FR12, FR13, FR15, FR16, FR17, FR18, FR19, FR20, FR21, FR22 (partiel), FR23, FR24, FR25, FR26, FR33, FR35, FR36, FR55, FR56, FR57

### Épopée 4 : Catalogue Canonique Complet (5 rôles supplémentaires)
Compléter le catalogue v1 avec Cupidon+Amoureux, Petite Fille, Salvateur, Maire et Ange. Crédibilise la promesse "100% canonique Thiercelieux".
**FRs couvertes :** FR14 (complétée), FR22 (complétée), FR27, FR28, FR29, FR30, FR31, FR32, FR69

### Épopée 5 : Direction Artistique Cinématique & Mode Remote 360°
Le jeu devient cinématique : village 3D, avatars, animations, mort dramatique, écran noir, vue Remote 360°. Concrétise l'USP "ambiance premium".
**FRs couvertes :** FR40, FR41, FR42, FR46, FR47, FR54

### Épopée 6 : Voix MJ & Ambiance Sonore
La voix prend vie : script MJ complet, voix ElevenLabs, musique, SFX, sync audio. Concrétise l'USP "voix MJ cinématique".
**FRs couvertes :** FR48, FR49, FR50, FR51, FR52, FR53

### Épopée 7 : Mode Campfire (Bluetooth Totem + Compagnons)
Le mode signature unique sur le marché : un téléphone au centre = totem ambiance, les compagnons gardent l'UI privée. Différenciateur marketing.
**FRs couvertes :** FR43, FR44, FR45

### Épopée 8 : Onboarding & Accessibilité
Tutoriel friction-zéro, sous-titres, daltonisme, VoiceOver, push-to-talk. Conformité Apple + élargissement marché.
**FRs couvertes :** FR39, FR60, FR61

### Épopée 9 : Méta-Progression & Cosmétiques
Stats post-partie, achievements, skins déverrouillables, leaderboards privés, replay rapide. Pousse la rétention D7+.
**FRs couvertes :** FR62, FR63, FR64, FR65, FR66, FR67, FR72, FR73, FR74

### Épopée 10 : Monétisation, Cloud & Telemétrie
IAP cosmétiques, iCloud sync, Remote Config, Analytics Firebase. Fait tourner le business + permet pilotage post-launch.
**FRs couvertes :** FR68, FR70, FR71

### Épopée 11 : Lancement & QA
Playtest alpha (50), beta publique (500), soumission App Store, plan marketing créateurs, monitoring launch day.
**FRs couvertes :** Validation transversale de toutes les NFRs (1-26)

---

## Épopée 1 : Plateforme & Authentification

**Goal :** Poser les fondations techniques du projet (Unity, CI, Firebase) et permettre à un utilisateur de s'authentifier puis de créer/rejoindre une room minimaliste avec vocal opérationnel. À la fin de cette épopée, deux joueurs peuvent se retrouver dans la même room et se parler.

### Story 1.1 : Bootstrap projet Unity iOS + CI

As a développeur,
I want un projet Unity 2023 LTS configuré pour iOS avec CI/CD automatisé,
So that je peux build et déployer des versions sur des appareils de test sans manipulation manuelle.

**Acceptance Criteria :**

**Given** une machine de dev Mac configurée avec Xcode
**When** je clone le repo et lance `unity -projectPath .`
**Then** le projet ouvre sans erreur sous Unity 2023 LTS
**And** le build target est configuré sur iOS (Player Settings : iOS 15+ minimum, Bundle ID `com.laveillee.app`)
**And** les permissions Info.plist requises sont déclarées (Microphone, Bluetooth, Local Network)

**Given** un push sur la branche `main`
**When** la pipeline CI s'exécute
**Then** un build IPA de développement est produit et signé via le profil Apple Developer
**And** le build est uploadé vers TestFlight (canal interne)
**And** la pipeline échoue explicitement si la signature ou les permissions sont incorrectes

**Given** un développeur lance le projet sur un iPhone 11 connecté
**When** l'app démarre
**Then** un écran "Hello La Veillée" s'affiche en moins de 5 secondes
**And** aucun crash n'est rapporté dans Xcode console

### Story 1.2 : Authentification Sign in with Apple

As a joueur,
I want me connecter via mon Apple ID en un tap,
So that je n'ai pas à créer un nouveau mot de passe pour jouer.

**Acceptance Criteria :**

**Given** je lance l'app pour la première fois
**When** j'arrive sur l'écran d'accueil
**Then** un bouton "Continuer avec Apple" est visible et conforme aux Apple HIG
**And** aucune autre méthode d'auth concurrente n'est proposée en v1

**Given** je tape sur "Continuer avec Apple"
**When** je valide via Face ID / Touch ID dans la sheet système
**Then** un compte Firebase Auth est créé ou récupéré avec mon Apple ID UID
**And** je suis redirigé vers l'écran de profil minimal (Story 1.3)
**And** l'opération complète prend < 3 secondes

**Given** je suis déjà authentifié et je relance l'app
**When** l'app démarre
**Then** ma session Firebase est restaurée silencieusement
**And** je suis amené directement à l'écran d'accueil joueur (pas l'écran d'auth)

**Given** Apple retourne une erreur d'authentification
**When** la sheet système se ferme avec une erreur
**Then** un message clair en français s'affiche ("Connexion impossible — réessaie plus tard")
**And** le bouton reste actif pour retry

### Story 1.3 : Profil minimal et déconnexion

As a joueur authentifié,
I want choisir un pseudo et un avatar par défaut, et pouvoir me déconnecter,
So that mes amis me reconnaissent dans une partie et je peux passer le tél à quelqu'un d'autre.

**Acceptance Criteria :**

**Given** je viens d'être authentifié pour la première fois (Story 1.2)
**When** j'arrive sur l'écran de profil minimal
**Then** un champ "Pseudo" est pré-rempli avec mon prénom Apple (modifiable, 2-20 caractères, alphanumériques + espaces + accents)
**And** un avatar par défaut est sélectionné automatiquement parmi 6 modèles génériques
**And** un bouton "Valider" enregistre le profil dans Firestore et m'amène à l'écran d'accueil

**Given** je suis sur l'écran d'accueil et j'ouvre le menu paramètres
**When** je tape "Se déconnecter"
**Then** une confirmation modale apparaît ("Tu pourras te reconnecter à tout moment")
**And** si je confirme, ma session Firebase est invalidée et je retourne à l'écran d'auth

**Given** un pseudo entré contient une chaîne vide ou > 20 caractères
**When** je tape "Valider"
**Then** un message d'erreur en français explique la contrainte
**And** le bouton "Valider" reste désactivé tant que la contrainte n'est pas respectée

### Story 1.4 : Stack multijoueur Photon Fusion 2 et room minimaliste

_Note : le titre original "Photon ou Mirror" est superseded par ADR-001 (Photon Fusion 2 figé). L'AC "décision tranchée" reste valide — la décision est dans l'architecture._


As a joueur,
I want pouvoir rejoindre une room réseau avec d'autres joueurs,
So that la couche multijoueur est validée techniquement avant qu'on construise du gameplay dessus.

**Acceptance Criteria :**

**Given** la décision est figée : **Photon Fusion 2** (ADR-001)
**When** la SDK Fusion 2 est intégrée au projet Unity
**Then** un script `RoomManager.cs` expose `CreateRoom()`, `JoinRoom(roomId)`, `LeaveRoom()`, `GetPlayersInRoom()`
**And** une scène `DevTestRoom.unity` permet de tester ces méthodes via boutons UI placeholders

**Given** un joueur A appelle `CreateRoom()`
**When** un joueur B sur un autre appareil appelle `JoinRoom(roomId)` avec le même ID
**Then** les deux joueurs voient l'autre dans `GetPlayersInRoom()` en moins de 2 secondes
**And** chaque joueur a un PlayerId unique persistant pendant la session room

**Given** un joueur quitte la room (ferme l'app brutalement)
**When** 30 secondes passent
**Then** l'autre joueur reçoit un événement `OnPlayerLeft(playerId)` et la liste se met à jour

**Given** la room atteint 25 joueurs (capacité max v1)
**When** un 26ème joueur tente `JoinRoom()`
**Then** la SDK retourne une erreur `RoomFull` et l'UI affiche un message clair

### Story 1.5 : Vocal intégré Photon Voice 2 avec sync room

_Note : le titre original "Agora ou LiveKit" est superseded par ADR-002 (Photon Voice 2 figé). Benchmark de latence reste à faire pour valider NFR4 p95 < 250ms._


As a joueur,
I want parler aux autres joueurs présents dans la même room,
So that la promesse "vocal intégré sans Discord" est validée.

**Acceptance Criteria :**

**Given** la décision est figée : **Photon Voice 2** (ADR-002) ; benchmark latence à exécuter à cette story (pivot Agora préempté si p95 > 250ms)
**When** la SDK Photon Voice 2 est intégrée et configurée
**Then** un script `VoiceManager.cs` expose `JoinVoiceChannel(roomId)`, `LeaveVoiceChannel()`, `MuteSelf(bool)`, `OnSomeoneSpeaking(playerId, isSpeaking)`

**Given** deux joueurs sont dans la même room (Story 1.4) et `JoinVoiceChannel` a été appelé
**When** le joueur A parle dans son micro
**Then** le joueur B entend le joueur A en moins de 250 ms (NFR4 p95)
**And** un indicateur "tu parles" / "X parle" est affiché sur l'UI placeholder

**Given** un joueur tape l'icône "mute"
**When** l'icône change d'état
**Then** son micro est coupé immédiatement et les autres ne l'entendent plus
**And** son indicateur visuel passe à "muté"

**Given** la première fois que `JoinVoiceChannel` est appelé
**When** iOS demande la permission micro
**Then** un message en français explique pourquoi (NSMicrophoneUsageDescription)
**And** si refusé, l'app reste fonctionnelle mais affiche "Vocal indisponible — active le micro dans Réglages"

---

## Épopée 2 : Setup Social de Partie

**Goal :** Permettre à un host d'orchestrer une partie complète depuis un écran d'accueil propre : créer la room, inviter les amis (code/lien), configurer la composition de rôles, fixer les timers, choisir Campfire ou Remote, et démarrer la partie. Inclut la pause/reprise. À la fin de cette épopée, l'expérience pré-jeu est friction-zéro et le jeu est prêt à recevoir la boucle gameplay.

### Story 2.1 : Écran d'accueil joueur

As a joueur authentifié,
I want un écran d'accueil clair avec deux actions principales,
So that je sais immédiatement comment lancer ou rejoindre une soirée.

**Acceptance Criteria :**

**Given** je suis authentifié et arrive sur l'écran d'accueil
**When** l'écran charge
**Then** deux gros boutons sont visibles : "Créer une partie" (primaire) et "Rejoindre une partie" (secondaire)
**And** mon avatar et mon pseudo sont affichés en haut à droite
**And** un bouton menu hamburger ouvre les paramètres / déconnexion / mentions légales

**Given** je tape "Créer une partie"
**When** la transition s'effectue
**Then** je suis redirigé vers l'écran de création (Story 2.2) en < 500 ms

**Given** je tape "Rejoindre une partie"
**When** la transition s'effectue
**Then** je suis redirigé vers l'écran de saisie de code (Story 2.3) en < 500 ms

### Story 2.2 : Création de partie avec code à 6 chiffres

As a host,
I want créer une partie privée avec un code unique à partager,
So that mes amis peuvent me rejoindre sans matchmaking public.

**Acceptance Criteria :**

**Given** je tape "Créer une partie" depuis l'accueil
**When** l'action s'exécute
**Then** une room Photon est créée via `RoomManager.CreateRoom()` (Story 1.4)
**And** un code à 6 chiffres unique est généré (collision-free vis-à-vis des rooms actives)
**And** je suis automatiquement ajouté comme host dans le lobby (Story 2.4)

**Given** je suis dans le lobby comme host
**When** l'écran s'affiche
**Then** le code à 6 chiffres est affiché en gros, copiable d'un tap
**And** un bouton "Partager le lien" génère un deeplink (`laveillee://join/[code]`) et ouvre la sheet de partage iOS

**Given** la génération du code échoue (réseau coupé)
**When** l'erreur survient
**Then** un message en français s'affiche ("Connexion réseau perdue — réessaie")
**And** un bouton "Réessayer" relance la création

### Story 2.3 : Rejoindre une partie par code ou deeplink

As a joueur invité,
I want saisir un code ou taper sur un lien partagé pour rejoindre la partie de mon pote,
So que la friction d'entrée est nulle.

**Acceptance Criteria :**

**Given** je tape "Rejoindre une partie" depuis l'accueil
**When** l'écran de saisie s'affiche
**Then** un champ numérique 6 cases avec clavier numérique focus auto est visible
**And** je peux coller un code depuis le presse-papier d'un tap

**Given** je saisis un code valide à 6 chiffres
**When** je tape "Rejoindre"
**Then** `RoomManager.JoinRoom(code)` est appelé
**And** en cas de succès, je suis ajouté au lobby de la partie (Story 2.4) en < 2 secondes

**Given** je tape sur un deeplink `laveillee://join/[code]` reçu par message
**When** l'app s'ouvre
**Then** si je suis authentifié, la room est rejointe directement avec ce code
**And** si je ne suis pas authentifié, je passe par le flow d'auth (Story 1.2) puis suis redirigé vers la room après validation

**Given** le code saisi est invalide ou la room n'existe pas
**When** la SDK retourne `RoomNotFound`
**Then** un message clair en français s'affiche ("Cette partie n'existe pas ou est terminée")
**And** je reste sur l'écran de saisie pour réessayer

**Given** la room est pleine (25 joueurs)
**When** je tente de rejoindre
**Then** l'erreur `RoomFull` est traduite en "Partie complète, désolé !" et je reste sur l'écran de saisie

### Story 2.4 : Lobby pré-partie (liste joueurs + kick)

As a host ou joueur dans le lobby,
I want voir qui est présent et le host peut virer un indésirable,
So que le groupe est sous contrôle avant le démarrage.

**Acceptance Criteria :**

**Given** je suis dans le lobby (host ou invité)
**When** la liste des joueurs charge
**Then** chaque joueur connecté apparaît avec son avatar et son pseudo
**And** un indicateur visuel distingue le host des autres
**And** un compteur "X / 25 joueurs" est visible

**Given** un nouveau joueur rejoint la room
**When** l'événement `OnPlayerJoined` se déclenche
**Then** sa fiche apparaît dans la liste en < 1 seconde avec une animation subtile
**And** un SFX léger joue ("ding" discret)

**Given** je suis le host et je tape sur l'avatar d'un joueur autre que moi
**When** la sheet d'options s'ouvre
**Then** une option "Kicker du lobby" est visible
**And** si je confirme, le joueur est expulsé et reçoit une notification "Tu as été retiré de la partie"
**And** un joueur non-host voit la sheet d'options sans l'option kick

**Given** le host quitte le lobby
**When** la déconnexion est détectée
**Then** un autre joueur est promu host automatiquement (le plus ancien dans la room)
**And** tous les joueurs voient l'indicateur de host changer

### Story 2.5 : Configuration de la composition des rôles

As a host,
I want choisir une composition de rôles adaptée à la taille du groupe,
So que la partie est équilibrée avant de démarrer.

**Acceptance Criteria :**

**Given** je suis host dans le lobby et il y a au moins 5 joueurs
**When** je tape "Configurer la partie"
**Then** un écran de composition s'ouvre
**And** une composition recommandée est pré-sélectionnée selon le nombre de joueurs présents (FR11 — voir tableau d'équilibrage GDD)

**Given** la composition est pré-sélectionnée pour 8 joueurs
**When** je consulte l'écran
**Then** je vois "2 Loups + 1 Voyante + 1 Sorcière + 4 Villageois" listés visuellement (icônes + chiffres)
**And** un bouton "Personnaliser" permet d'ajouter/retirer des rôles

**Given** je tape "Personnaliser"
**When** l'écran custom s'ouvre
**Then** je peux ajuster le nombre de chaque rôle disponible
**And** le compteur total doit toujours = nombre de joueurs présents (sinon bouton "Valider" désactivé avec explication)
**And** au minimum 1 Loup-Garou et au minimum 1 Villageois (ou rôle non-loup) sont obligatoires

**Given** la composition est valide
**When** je tape "Valider"
**Then** la composition est enregistrée comme état de la room et visible des autres joueurs en lecture seule

**Given** moins de 5 joueurs sont dans le lobby
**When** j'essaie de configurer
**Then** un message s'affiche "5 joueurs minimum requis pour démarrer"
**And** le bouton "Configurer" reste désactivé

### Story 2.6 : Configuration timers et choix mode Campfire/Remote

As a host,
I want régler les timers nuit/jour et choisir entre Campfire (1 tél) ou Remote (chacun son tél),
So que la partie correspond au contexte physique du groupe.

**Acceptance Criteria :**

**Given** je suis dans l'écran de config de partie après avoir validé la composition (Story 2.5)
**When** la section "Timers" s'affiche
**Then** deux sliders apparaissent : "Nuit (par rôle)" 1-5 min (défaut 3) et "Jour (débat)" 2-10 min (défaut 5)
**And** les valeurs sélectionnées sont affichées en clair

**Given** la section "Mode" s'affiche dans le même écran
**When** je consulte les options
**Then** deux toggles mutuellement exclusifs sont visibles : "Campfire (1 tél au centre)" et "Remote (chacun son tél)"
**And** une description courte explique chaque mode (1-2 phrases)

**Given** je sélectionne "Campfire"
**When** la sélection est validée
**Then** un message d'info précise "Le mode Campfire nécessite que les joueurs soient physiquement dans la même pièce avec Bluetooth activé"
**And** un check de prérequis (BT activé sur le tél host) est lancé en arrière-plan

**Given** je tape "Démarrer la partie"
**When** la composition + timers + mode sont validés
**Then** tous les joueurs sont notifiés "La partie démarre dans 3, 2, 1..."
**And** la transition vers la phase de distribution des rôles s'enclenche (Épopée 3)

### Story 2.7 : Pause et reprise de partie

As a joueur en cours de partie,
I want pouvoir mettre la partie en pause si quelqu'un doit aller aux toilettes,
So que personne n'est lésé par une interruption.

**Acceptance Criteria :**

**Given** une partie est en cours (n'importe quelle phase)
**When** un joueur ouvre le menu pause
**Then** un bouton "Pause" est visible et taupable

**Given** un joueur tape "Pause"
**When** l'action s'exécute
**Then** tous les timers sont stoppés sur tous les appareils en < 1 seconde
**And** le vocal est muté pour tous les joueurs
**And** un overlay "Partie en pause par [pseudo]" s'affiche sur tous les écrans
**And** un bouton "Reprendre" est visible pour tous

**Given** la partie est en pause
**When** n'importe quel joueur tape "Reprendre"
**Then** une notification "Partie reprise par [pseudo]" s'affiche brièvement
**And** les timers reprennent là où ils étaient
**And** le vocal est ré-activé selon les règles de la phase courante

**Given** une partie est en pause depuis plus de 10 minutes
**When** le seuil est atteint
**Then** un host (ou joueur le plus ancien si host parti) reçoit une option "Annuler la partie"
**And** annuler ramène tout le monde au lobby

---

## Épopée 3 : Boucle Jour/Nuit MVP (5 rôles essentiels)

**Goal :** Implémenter la boucle de gameplay complète avec 5 rôles canoniques (Villageois, Loup-Garou, Voyante, Sorcière, Chasseur). Une partie peut être jouée du début à la fin, avec votes, éliminations, salon des morts, et détection de victoire. C'est le cœur de produit ; à la fin de cette épopée, le jeu est jouable, même nu visuellement.

### Story 3.1 : Distribution privée des rôles

As a joueur,
I want recevoir mon rôle secret au démarrage de la partie,
So que le bluff puisse commencer.

**Acceptance Criteria :**

**Given** une partie démarre depuis le lobby (transition Story 2.6)
**When** la phase de distribution s'enclenche
**Then** le système assigne aléatoirement les rôles selon la composition validée (Story 2.5)
**And** chaque joueur reçoit son rôle via un canal privé sécurisé (pas broadcast)
**And** un écran "Ton rôle est…" s'affiche avec icône, nom et 2-3 lignes de règles

**Given** je suis joueur lambda
**When** je tape sur le bandeau "Ton rôle"
**Then** je vois UNIQUEMENT mon propre rôle
**And** aucune requête côté client ne peut révéler le rôle d'un autre joueur (validation côté serveur Photon)

**Given** la distribution est terminée pour tous les joueurs
**When** le dernier joueur a confirmé "J'ai compris"
**Then** la phase nuit 1 démarre automatiquement (Story 3.2)
**And** un délai max de 30 secondes force le démarrage si certains joueurs ne confirment pas

### Story 3.2 : Phase nuit avec ordre canonique des appels

As a joueur,
I want que le système appelle chaque rôle dans l'ordre canonique strict,
So que l'expérience respecte la fidélité Thiercelieux (Pillar 1).

**Acceptance Criteria :**

**Given** la phase nuit démarre
**When** le système initialise la séquence
**Then** l'ordre canonique est respecté : Voyante → Loups → Sorcière (rôles MVP) — l'ordre complet (Cupidon, Amoureux, Petite Fille, Salvateur) sera étendu en Épopée 4
**And** les rôles non présents dans la composition sont sautés silencieusement

**Given** un rôle est appelé (ex : Voyante)
**When** son tour s'enclenche
**Then** UNIQUEMENT le joueur Voyante a son écran/UI activée pour son action
**And** les autres joueurs voient un écran d'attente neutre (l'écran-noir cinématique sera ajouté en Épopée 5)
**And** un timer max par rôle (paramétré nuit, défaut 3 min) limite la durée d'action

**Given** un joueur ayant un rôle actif n'agit pas dans le timer
**When** le timer expire
**Then** son action est sautée (pas de défaut "automatique" qui pourrait modifier la game)
**And** le rôle suivant est appelé immédiatement

**Given** tous les rôles nocturnes ont été appelés
**When** la dernière action est résolue
**Then** la phase passe au "Réveil" (annonce des morts, Story 3.8)
**And** un événement `OnNightEnded` est broadcasté à tous les clients

### Story 3.3 : Action nocturne des Loups-Garous (vote silencieux collectif)

As a Loup-Garou,
I want voter avec mes complices sur la cible à dévorer,
So que nous éliminons un villageois en accord.

**Acceptance Criteria :**

**Given** la phase Loups démarre
**When** mon écran s'active (je suis Loup)
**Then** je vois la liste des joueurs vivants non-Loups avec leur avatar
**And** je vois aussi les pseudos des autres Loups marqués (je connais mes complices)
**And** le vocal entre Loups est ouvert exclusivement entre nous (FR35)

**Given** un Loup tape sur la cible souhaitée
**When** le tap est enregistré
**Then** un compteur "X loups votent pour Y" s'affiche en temps réel pour les Loups
**And** un Loup peut changer son vote en tapant un autre joueur

**Given** un Loup veut confirmer le vote (cible irréversible)
**When** il tape + maintient 0.5s sur la cible (FR33)
**Then** un haptic fort confirme et le vote est verrouillé pour ce Loup
**And** quand TOUS les Loups ont verrouillé sur la même cible, l'action est résolue
**And** la cible est marquée "morte cette nuit" (visible uniquement à la résolution finale, Story 3.8)

**Given** la phase Loups dure plus que le timer max
**When** le timer expire
**Then** si une majorité simple de Loups est verrouillée sur une cible, cette cible est tuée
**And** sinon, aucune mort par les Loups cette nuit

**Given** je suis non-Loup pendant la phase Loups
**When** mon écran est en attente
**Then** je n'entends pas le vocal Loups
**And** je ne vois aucune information sur la cible visée

### Story 3.4 : Action nocturne de la Voyante

As a Voyante,
I want espionner le rôle d'un joueur chaque nuit,
So que je découvre les loups et oriente le village.

**Acceptance Criteria :**

**Given** la phase Voyante démarre (avant les Loups, ordre canonique)
**When** mon écran s'active
**Then** je vois la liste des joueurs vivants (autres que moi)
**And** un message MJ texte/audio "Voyante, sonde un joueur" est affiché

**Given** je tape un joueur cible
**When** je confirme via tap + hold 0.5s
**Then** je vois immédiatement le rôle réel de ce joueur (icône + nom) en révélation privée
**And** la révélation reste visible 5 secondes minimum
**And** mon action est verrouillée pour cette nuit (pas de second sondage)

**Given** je n'agis pas avant la fin du timer
**When** le timer expire
**Then** aucun rôle n'est révélé cette nuit
**And** la phase suivante démarre

**Given** je suis Voyante et déjà verrouillée
**When** la phase Loups démarre
**Then** mon écran reflète "Tu attends que la nuit s'achève"
**And** aucun nouveau sondage n'est possible

### Story 3.5 : Action nocturne de la Sorcière (potions)

As a Sorcière,
I want utiliser mes 2 potions (vie + mort) au bon moment,
So que je sauve un allié ou j'élimine un suspect.

**Acceptance Criteria :**

**Given** la phase Sorcière démarre (après les Loups)
**When** mon écran s'active
**Then** je vois la cible visée par les Loups cette nuit (révélation privée à la Sorcière, conformément au canon)
**And** je vois mes potions disponibles (vie : 1, mort : 1, dégrisées si déjà utilisées)

**Given** la Sorcière a sa potion de vie disponible
**When** elle tape "Sauver [pseudo de la cible Loups]" + hold 0.5s
**Then** la mort de cette cible est annulée pour cette nuit
**And** la potion de vie est consommée et grisée définitivement

**Given** la Sorcière a sa potion de mort disponible
**When** elle tape un autre joueur vivant + "Tuer" + hold 0.5s
**Then** ce joueur sera annoncé mort au réveil
**And** la potion de mort est consommée et grisée définitivement

**Given** la Sorcière n'utilise aucune potion
**When** elle tape "Passer" ou laisse expirer le timer
**Then** aucune potion n'est consommée
**And** la phase suivante démarre

**Given** les deux potions sont déjà consommées (nuits précédentes)
**When** la phase Sorcière démarre
**Then** son écran affiche "Tu n'as plus de potions" et la phase passe immédiatement à la suivante (auto-skip)

### Story 3.6 : Phase jour avec timer + débat vocal libre

As a joueur vivant,
I want débattre vocalement avec les autres survivants pendant la phase jour,
So que je peux convaincre, accuser et lire les réactions.

**Acceptance Criteria :**

**Given** le réveil est annoncé (Story 3.8) et au moins 2 camps sont encore en jeu
**When** la phase jour démarre
**Then** le vocal est ouvert pour TOUS les joueurs vivants (FR34)
**And** un timer visuel + audio s'affiche en haut d'écran (par défaut 5 min, configurable Story 2.6)
**And** un bip discret retentit aux 60 dernières secondes, puis aux 10 dernières (NFR18)

**Given** la phase jour est en cours
**When** un joueur parle
**Then** son indicateur "il parle" apparaît au-dessus de son avatar (FR38)
**And** la latence vocale est < 250 ms p95 (NFR4)

**Given** un joueur veut se mettre en mute
**When** il tape l'icône micro
**Then** son micro est immédiatement coupé et un indicateur "muté" remplace l'icône
**And** le toggle est instantanément réversible

**Given** le timer jour expire
**When** la dernière seconde s'écoule
**Then** le vocal est coupé pour tous
**And** le système passe à la résolution du vote (Story 3.8)

### Story 3.7 : Vote public temps réel + feed + modifiable

As a joueur,
I want voir qui vote pour qui en temps réel et pouvoir changer mon vote,
So que la pression sociale et le retournement de coalitions sont possibles.

**Acceptance Criteria :**

**Given** la phase jour est active (Story 3.6)
**When** je tape un avatar de joueur vivant
**Then** mon vote est enregistré pour cette cible
**And** un haptic léger confirme
**And** un compteur "X votes" apparaît au-dessus de la cible

**Given** j'ai déjà voté pour le joueur A
**When** je tape sur le joueur B
**Then** mon vote est retiré de A et ajouté à B atomiquement
**And** les compteurs des deux cibles sont mis à jour en < 500 ms pour tous

**Given** je veux m'abstenir
**When** je tape sur mon propre avatar
**Then** mon vote est retiré (s'il existait) sans être réassigné
**And** je suis marqué "abstention" dans le feed

**Given** un vote est enregistré, modifié ou retiré
**When** l'événement se propage
**Then** un feed temps réel scrollable affiche "[Pseudo A] a voté pour [Pseudo B]" (FR19)
**And** le feed conserve les 20 dernières actions

**Given** la phase jour est active et le vote du Maire compte (Épopée 4 — sera à valider plus tard, ici on prépare l'extension)
**When** le système calcule les compteurs
**Then** la fonction `GetVoteCount(targetId)` est extensible pour pondérer un votant (placeholder pour FR31)

### Story 3.8 : Résolution du vote, élimination et détection win/loss MVP

As a joueur,
I want savoir clairement qui meurt à la fin du jour et si la partie est terminée,
So que la tension narrative culmine.

**Acceptance Criteria :**

**Given** le timer jour expire (Story 3.6) ou n'importe quelle nuit se termine
**When** le système calcule les morts du tour
**Then** le joueur le plus voté est éliminé (FR21)
**And** en cas d'égalité, aucun n'est éliminé (canon : lynchage évité — choix de design)
**And** une animation placeholder de mort joue (l'animation cinématique arrive Épopée 5)

**Given** un joueur est éliminé
**When** sa mort est résolue
**Then** son rôle est révélé publiquement à tous (annonce MJ texte placeholder)
**And** il est retiré de la liste des votants/cibles
**And** il rejoint le salon des morts (Story 3.9)

**Given** après chaque mort, le système vérifie les conditions de victoire
**When** tous les Loups-Garous sont éliminés
**Then** le camp Villageois est déclaré vainqueur et la partie se termine
**And** un écran de fin placeholder s'affiche (UI complète arrive Story 9.1)

**Given** après chaque mort, le système vérifie les conditions de victoire
**When** le nombre de Loups vivants ≥ nombre de non-Loups vivants
**Then** le camp Loups est déclaré vainqueur et la partie se termine

**Given** la partie est en cours
**When** plus aucune mort ne peut faire basculer le jeu (cas limite : 0 vivant)
**Then** la partie se termine en match nul

### Story 3.9 : Salon des morts avec vocal séparé + bot anti-griefing

As a joueur mort,
I want continuer à discuter avec les autres morts sans pourrir le jeu des vivants,
So que je reste engagé socialement même éliminé.

**Acceptance Criteria :**

**Given** je viens d'être éliminé (Story 3.8)
**When** ma mort est résolue
**Then** je suis automatiquement déplacé vers le salon des morts
**And** mon vocal vivants est coupé (FR56 — pas de spoil)
**And** un nouveau channel vocal "Morts" est ouvert pour moi avec les autres morts (FR36 / FR55)

**Given** je suis dans le salon des morts
**When** la partie continue côté vivants
**Then** je vois l'évolution de la partie (qui meurt, qui vote) en mode spectateur
**And** je ne peux émettre AUCUNE info vers les vivants (vocal séparé strictement)

**Given** un joueur perd la connexion en pleine partie (avant d'être mort)
**When** sa déconnexion est détectée pendant > 30 secondes
**Then** son avatar passe en mode "endormi" : un bot le remplace et vote aléatoirement (FR57)
**And** un indicateur visuel "endormi" est visible des autres
**And** s'il revient en jeu (réseau rétabli) avant la fin de partie, il reprend le contrôle de son personnage

### Story 3.10 : Action du Chasseur (tir réactif à sa mort)

As a Chasseur,
I want tirer sur un joueur quand je meurs,
So que je punis mon assassin (réel ou suspecté) ou je sauve le village.

**Acceptance Criteria :**

**Given** le Chasseur est tué (par Loups, Sorcière, ou vote)
**When** sa mort est annoncée (Story 3.8)
**Then** son écran s'active avec un overlay "Tu meurs — choisis ta cible"
**And** il voit la liste des joueurs vivants (autres que lui)
**And** un timer de 30 secondes s'enclenche

**Given** le Chasseur tape sur une cible + hold 0.5s
**When** la cible est confirmée
**Then** ce joueur meurt immédiatement après le Chasseur
**And** son rôle est révélé publiquement
**And** le système re-vérifie les conditions de victoire (Story 3.8)

**Given** le Chasseur ne choisit pas avant le timer
**When** les 30 secondes expirent
**Then** son tir est perdu (canon : pas de défaut auto)
**And** la partie continue normalement

**Given** la cible du Chasseur est elle-même Chasseuse
**When** sa mort déclenche son propre tir
**Then** la chaîne de tirs en cascade est gérée correctement (récursion contrôlée)

---

## Épopée 4 : Catalogue Canonique Complet (5 rôles supplémentaires)

**Goal :** Compléter le catalogue v1 avec les 5 rôles canoniques manquants (Cupidon + Amoureux, Petite Fille, Salvateur, Maire, Ange). À la fin de cette épopée, le jeu offre les 10 rôles promis dans l'USP "100% canonique Thiercelieux".

### Story 4.1 : Cupidon — Lien d'amoureux nuit 1

As a Cupidon,
I want lier deux joueurs comme amoureux à la première nuit,
So que je crée un dilemme stratégique majeur dans la partie.

**Acceptance Criteria :**

**Given** la partie démarre et la composition contient un Cupidon
**When** la nuit 1 commence
**Then** Cupidon est appelé EN PREMIER (avant Voyante), conforme à l'ordre canonique (FR15)
**And** son écran montre tous les joueurs vivants (lui inclus, peut s'auto-lier)

**Given** Cupidon tape un premier joueur + hold 0.5s
**When** la sélection est confirmée
**Then** ce joueur est marqué visuellement comme "amoureux 1"
**And** Cupidon doit choisir un deuxième joueur (différent ou identique en cas d'auto-lien — règle canon Thiercelieux à confirmer en playtest)

**Given** Cupidon tape un second joueur + hold 0.5s
**When** la sélection est confirmée
**Then** les deux joueurs sont liés en amoureux
**And** l'action de Cupidon est terminée pour la partie (action unique)

**Given** Cupidon n'agit pas avant la fin du timer
**When** le timer expire
**Then** aucun couple n'est formé
**And** la phase suivante démarre

**Given** la composition ne contient pas de Cupidon
**When** la nuit 1 commence
**Then** la phase Cupidon est sautée silencieusement et l'ordre canonique passe au rôle suivant

### Story 4.2 : Reconnaissance des Amoureux + condition de victoire solo

As an Amoureux,
I want apprendre qui est mon partenaire à la nuit 1,
So que nous savons que nos destins sont liés.

**Acceptance Criteria :**

**Given** Cupidon a lié deux amoureux (Story 4.1)
**When** la phase "Amoureux" démarre (juste après Cupidon, avant Voyante)
**Then** chaque amoureux voit privément l'identité et le rôle de l'autre
**And** un message court "Tu es amoureux/amoureuse de [Pseudo]" s'affiche 5 secondes minimum

**Given** un amoureux meurt en cours de partie
**When** sa mort est résolue
**Then** son partenaire meurt immédiatement après de "chagrin d'amour"
**And** un message MJ "Mort de chagrin" annonce sa mort en révélant le lien amoureux
**And** le système re-vérifie les conditions de victoire (Story 3.8)

**Given** les deux amoureux sont issus de camps opposés (un Loup + un Villageois)
**When** ils survivent ensemble jusqu'à ce qu'eux seuls restent en vie
**Then** le camp solo "Amoureux" est déclaré vainqueur
**And** la partie se termine avec un écran de victoire spécial (placeholder ; cinématique Épopée 5)

**Given** les deux amoureux sont du même camp
**When** la partie progresse
**Then** la condition de victoire normale de leur camp s'applique
**And** la mort de l'un entraîne toujours celle de l'autre

### Story 4.3 : Petite Fille — Espionnage des Loups

As a Petite Fille,
I want espionner brièvement la phase des Loups,
So que je peux apprendre qui ils sont, au risque d'être démasquée.

**Acceptance Criteria :**

**Given** la phase Loups démarre (Story 3.3) et la composition contient une Petite Fille
**When** mon rôle Petite Fille s'active en parallèle
**Then** un bouton "Ouvrir un œil" est disponible pendant la phase Loups
**And** un avertissement "Risque : être vue par les Loups = mort" est visible

**Given** je tape "Ouvrir un œil"
**When** l'action s'exécute
**Then** je vois pendant 3 secondes max l'écran des Loups (qui sont les Loups + leur cible en cours)
**And** une probabilité paramétrable (par défaut 30%) déclenche un événement "détectée" qui me signale aux Loups
**And** si détectée, les Loups voient un message "Une Petite Fille espionne — votre cible peut être ajustée"

**Given** je suis détectée
**When** la phase Loups se termine
**Then** les Loups peuvent choisir de me cibler à la place de leur victime initiale (option supplémentaire)

**Given** je n'ouvre pas l'œil
**When** la phase Loups s'achève
**Then** rien ne se passe et je reste sécurisée

**Given** la composition ne contient pas de Petite Fille
**When** la phase Loups démarre
**Then** aucun bouton "Ouvrir un œil" n'apparaît côté autres joueurs

### Story 4.4 : Salvateur — Protection nocturne avec contrainte

As a Salvateur,
I want protéger un joueur chaque nuit, sans pouvoir protéger le même 2 nuits de suite,
So que je sauve discrètement le village de la mort des Loups.

**Acceptance Criteria :**

**Given** la phase Salvateur démarre (après Sorcière, ordre canonique)
**When** mon écran s'active
**Then** je vois la liste des joueurs vivants
**And** la cible que j'ai protégée la nuit précédente est grisée et non sélectionnable (FR30)

**Given** je tape un joueur + hold 0.5s
**When** ma sélection est confirmée
**Then** ce joueur est protégé pour cette nuit
**And** si les Loups le ciblent, sa mort est annulée (silencieusement, le joueur ne saura pas qu'il a été protégé)

**Given** je ne sélectionne personne
**When** le timer expire
**Then** aucun joueur n'est protégé cette nuit

**Given** je veux me protéger moi-même
**When** je tape mon propre avatar (sauf si je l'ai protégé la nuit précédente)
**Then** l'auto-protection est autorisée
**And** la même contrainte "pas 2 nuits de suite" s'applique à moi

### Story 4.5 : Maire — Élection + vote double + skin couronne

As a joueur,
I want élire un Maire au début de la partie qui aura un vote pondéré,
So que la dynamique de pouvoir social est enrichie.

**Acceptance Criteria :**

**Given** la partie démarre et la composition contient le rôle "Maire" (en réalité un statut, pas un rôle exclusif)
**When** le tout premier jour démarre (avant le premier vote de lynchage)
**Then** une mini-phase "Élection du Maire" s'enclenche
**And** chaque joueur vivant peut voter pour un Maire (vote rapide, 90s max)

**Given** l'élection se termine
**When** le candidat avec le plus de votes est désigné
**Then** ce joueur reçoit le statut "Maire" (en plus de son rôle secret)
**And** le skin "couronne d'or" s'applique visuellement à son avatar pour tous (FR69 — déverrouillage cosmétique persistant)
**And** sa stat personnelle "Maire élu" est incrémentée (Story 9.2)

**Given** un vote de jour normal est en cours
**When** le Maire vote pour une cible
**Then** son vote compte comme +2 dans `GetVoteCount(targetId)` (FR31, hook préparé Story 3.7)
**And** le feed précise "[Maire X] vote pour Y (compte double)"

**Given** le Maire meurt
**When** sa mort est résolue
**Then** avant l'animation de mort, il peut désigner son successeur parmi les vivants (timer 30s)
**And** s'il ne désigne personne, aucun successeur n'est nommé et la partie continue sans Maire

### Story 4.6 : Ange — Condition victoire solo (lynché tour 1)

As an Ange,
I want gagner solo si je suis lynché au premier tour de jour,
So que je peux tenter une stratégie ultra-risquée et mémorable.

**Acceptance Criteria :**

**Given** la partie démarre et la composition contient un Ange
**When** la distribution des rôles s'effectue (Story 3.1)
**Then** l'Ange reçoit un écran de rôle expliquant clairement sa condition de victoire ("Sois lynché au tour 1 de jour")

**Given** la phase jour 1 se résout (Story 3.8)
**When** l'Ange est le joueur lynché
**Then** l'Ange est déclaré vainqueur solo
**And** la partie se termine immédiatement (pas de continuation)
**And** un écran de victoire spécial s'affiche (placeholder)

**Given** la phase jour 1 se résout et l'Ange n'est PAS le lynché
**When** le tour 1 se termine
**Then** l'Ange perd sa condition de victoire spéciale
**And** il devient un Villageois standard pour la suite (camp Villageois)
**And** un message privé l'informe "Tu n'as pas été lynché — tu rejoins les Villageois"

**Given** l'Ange meurt par une autre cause au tour 1 (Loups, Sorcière)
**When** sa mort est résolue
**Then** il perd sa condition spéciale (canon : seul le lynchage compte)
**And** il rejoint le salon des morts comme un joueur normal

---

## Épopée 5 : Direction Artistique Cinématique & Mode Remote 360°

**Goal :** Habiller le jeu : scène village 3D, avatars modulaires, animations, mort dramatique, transitions cinématiques jour/nuit, écran-noir UX, et vue Remote 360° (swipe + gyroscope). À la fin de cette épopée, le jeu est visuellement à la hauteur de la promesse "premium cinématique".

### Story 5.1 : Scène village 3D (chalets, feu, socles, skybox)

As a joueur Remote,
I want voir un beau village autour de moi avec un feu central et les autres joueurs disposés en cercle,
So que l'immersion fonctionne dès l'écran de jeu.

**Acceptance Criteria :**

**Given** une partie démarre en mode Remote
**When** la scène village charge
**Then** un environnement 3D unifié s'affiche : feu central animé, ~10 chalets modulaires en arc de cercle, sol enneigé/forêt, skybox jour ou nuit selon la phase
**And** le rendu tient les 30 fps minimum sur iPhone 11 (NFR1)

**Given** la scène est chargée
**When** je regarde le feu central
**Then** des flammes animées avec particules (étincelles, fumée) sont visibles
**And** le feu est spatialisé audio (crépitement présent quand on regarde dans sa direction)

**Given** N joueurs sont dans la partie (5 ≤ N ≤ 25)
**When** la scène se génère
**Then** N socles de pierre sont positionnés en cercle autour du feu, équidistants
**And** chaque socle accueille un avatar joueur (Story 5.2)
**And** les chalets en arrière-plan se réduisent visuellement si N > 12 pour garder le cadre lisible

**Given** la phase passe de jour à nuit
**When** la transition s'enclenche (Story 5.5)
**Then** la skybox transitionne (cycle solaire en 3 secondes)
**And** la lumière ambiante change (orange chaud nuit / bleu lumineux jour)

### Story 5.2 : Avatars modulaires customisables

As a joueur,
I want que mon avatar soit visuellement distinct et reflète ma customisation,
So que mes potes me reconnaissent autour du feu.

**Acceptance Criteria :**

**Given** je suis sur l'écran de profil ou cosmétiques
**When** j'ouvre le customizer d'avatar
**Then** je peux modifier 4 axes : tête (5+ options), corps/pull (5+ options), coiffure (5+ options), chapeau/accessoire (5+ options)
**And** les modifications sont appliquées en preview temps réel
**And** au moins 25 combinaisons distinctes existent dès la v1

**Given** je sauvegarde ma customisation
**When** la modification est validée
**Then** l'avatar est persisté dans Firestore (sync via Story 10.3)
**And** dans la prochaine partie, mon avatar reflète cette customisation pour tous

**Given** une partie est en cours
**When** la scène village charge (Story 5.1)
**Then** chaque avatar joueur est rendu sur son socle avec sa customisation
**And** l'avatar est lisible à distance (silhouette distinctive, NFR daltonisme)

### Story 5.3 : Animations idle / parler / vote

As a joueur,
I want voir les autres avatars bouger naturellement,
So que la scène vit et n'est pas figée.

**Acceptance Criteria :**

**Given** un avatar est sur son socle (Story 5.2)
**When** aucune action particulière n'est déclenchée
**Then** une animation idle joue en boucle (respiration, clignement, regard autour)
**And** les animations sont décalées entre avatars pour éviter l'effet "armée"

**Given** un joueur parle (vocal détecté, Story 1.5)
**When** son indicateur "il parle" est actif
**Then** son avatar joue une animation "parle" (mouvement labial subtil + tête animée)
**And** un effet visuel léger (halo, particules) accompagne pour visibilité daltonienne

**Given** un joueur vote pour quelqu'un (Story 3.7)
**When** le vote est enregistré
**Then** son avatar joue une animation courte "désigne du doigt" (1 seconde)
**And** un trait visuel relie temporairement son avatar à sa cible

### Story 5.4 : Mort dramatique (animation socle + tombe + particules)

As a joueur,
I want voir les morts mises en scène cinématiquement,
So que chaque mort est un événement mémorable.

**Acceptance Criteria :**

**Given** un joueur est éliminé (Story 3.8 ou autre cause)
**When** sa mort est résolue
**Then** le socle du joueur se craquelle visuellement (shader)
**And** l'avatar tombe en avant avec une animation de chute (1.5s)
**And** des particules blanches (âme) s'élèvent depuis le corps
**And** une tombe en pierre apparaît à la place du socle (asset persistant pour le reste de la partie)

**Given** la mort est annoncée
**When** l'animation joue
**Then** un stinger sonore court (Story 6.3) se déclenche en sync
**And** la caméra (mode Remote) se centre brièvement sur le mort si dans le champ de vision

**Given** plusieurs morts surviennent dans le même tour (ex : Loups + Sorcière + Chasseur)
**When** la résolution s'enchaîne
**Then** les animations de mort se déclenchent séquentiellement (avec ~1s d'écart)
**And** l'enchaînement reste lisible et pas chaotique

### Story 5.5 : Transitions cinématiques jour ↔ nuit

As a joueur,
I want que le passage de phase soit visuel et marquant,
So que j'identifie immédiatement le changement d'ambiance.

**Acceptance Criteria :**

**Given** une phase nuit se termine et le réveil est annoncé (Story 3.8)
**When** la transition vers jour s'enclenche
**Then** la skybox transite de "nuit étoilée bleue" à "aube orange" en 3 secondes
**And** la lumière ambiante s'éclaircit progressivement
**And** des SFX matin (oiseaux distants, vent doux) s'enclenchent

**Given** la phase jour se termine
**When** la transition vers nuit s'enclenche
**Then** la skybox passe de "jour pâle" à "nuit profonde" en 3 secondes
**And** le feu central s'intensifie visuellement
**And** des SFX nocturnes (hibou, hurlements distants) s'enclenchent

**Given** une transition est en cours
**When** je tape pour une action
**Then** les inputs sont bufferisés mais inopérants jusqu'à fin de transition
**And** un feedback visuel (overlay opacité 50%) indique "transition en cours"

### Story 5.6 : Écran noir UX = yeux fermés numérique (USP centrale)

As a joueur dont le rôle n'est pas actif pendant la nuit,
I want que mon écran s'éteigne complètement,
So que l'équivalent UX du "fermez les yeux" physique fonctionne.

**Acceptance Criteria :**

**Given** la phase nuit démarre (Story 3.2)
**When** un rôle est appelé et il n'est pas le mien
**Then** mon écran devient noir (luminosité à 0, contenu masqué) en 500 ms (FR40)
**And** le vocal des autres rôles ne m'est pas audible (sauf vocal Loups si je suis Loup)

**Given** mon rôle devient actif (ex : c'est mon tour de Voyante)
**When** le système détecte mon tour
**Then** mon écran s'allume progressivement (fade in 500 ms) (FR41)
**And** un haptic léger se déclenche pour me signaler subtilement (FR42)
**And** mon UI d'action est immédiatement utilisable

**Given** mon rôle est fini pour cette nuit
**When** mon action est résolue
**Then** mon écran retourne au noir (fade out 500 ms)
**And** je dois attendre soit le réveil, soit ma prochaine activation

**Given** je suis joueur malvoyant et l'option "vibration alternative" est activée (NFR17)
**When** mon rôle devient actif
**Then** une vibration prolongée (2s) remplace l'effet visuel d'allumage
**And** le contenu de l'UI est annoncé via VoiceOver

### Story 5.7 : Vue Remote 360° (swipe + gyroscope)

As a joueur Remote,
I want regarder autour de moi en swipant ou en bougeant le tél,
So que je peux observer les réactions de tous les autres joueurs comme si j'étais dans le cercle.

**Acceptance Criteria :**

**Given** je suis en mode Remote (Story 2.6)
**When** la scène village charge
**Then** je vois la scène 3D avec une caméra FPS positionnée sur mon socle (vue à hauteur d'avatar)
**And** je vois les autres avatars autour du feu en cercle (FR47)
**And** la caméra est immobile par défaut (regarde le feu)

**Given** je swipe horizontalement sur l'écran
**When** mon doigt se déplace
**Then** la caméra pivote en yaw (rotation horizontale) à vitesse proportionnelle
**And** la rotation est fluide à 60 fps (NFR1) sans saccades
**And** un swipe complet (gauche-droite) couvre 180° (FR46)

**Given** j'active l'option "gyroscope" dans les paramètres
**When** je tourne physiquement le tél
**Then** la caméra suit les mouvements du tél en temps réel
**And** la sensibilité est paramétrable (lent / normal / rapide)

**Given** je veux sélectionner un joueur pour voter ou cibler
**When** je tape directement sur son avatar à l'écran
**Then** le système identifie correctement la cible via raycast
**And** un feedback visuel (highlight + nom) confirme la cible avant le tap+hold de validation

**Given** je suis en mode Campfire (Épopée 7), pas Remote
**When** la scène charge
**Then** la vue 360° n'est pas active (l'expérience visuelle Campfire est différente — voir Stories 7.3 et 7.4)

---

## Épopée 6 : Voix MJ & Ambiance Sonore

**Goal :** Donner sa voix au jeu : script MJ complet, voix ElevenLabs intégrée, musique nuit/jour/morts, SFX ambiants, sync parfaite audio ↔ phases. À la fin de cette épopée, l'USP "voix MJ cinématique + ambiance premium" est opérationnelle.

### Story 6.1 : Script complet MJ (~150 lignes)

As a joueur,
I want que le MJ ait un texte riche et varié pour chaque phase,
So que les répétitions soient inaudibles sur 5 parties.

**Acceptance Criteria :**

**Given** le besoin de couvrir toutes les phases du jeu
**When** le script complet est rédigé
**Then** un fichier `mj_script.yaml` (ou équivalent) contient minimum 150 lignes catégorisées :
- Introduction de partie (5+ variantes)
- Annonce de chaque phase nuit (rôle x 10 rôles, 3+ variantes par rôle)
- Annonce du réveil (5+ variantes)
- Annonce des morts (par cause : Loups, Sorcière, vote, Chasseur, chagrin d'amour, etc., 3+ variantes par cause)
- Annonce d'élection du Maire (3+ variantes)
- Annonce de victoire/défaite par camp (Villageois, Loups, Ange, Amoureux, match nul, 3+ variantes par camp)

**Given** le script est validé
**When** un game state demande une ligne (ex : "annoncer mort par Loups")
**Then** le système choisit aléatoirement parmi les variantes disponibles
**And** évite de jouer la même variante 2 fois de suite dans une partie

**Given** une revue éditoriale du script
**When** le contenu est lu à voix haute
**Then** le ton est sobre, grave, posé, conforme à la direction artistique GDD ("medium-cuit, dramatique sans cabotinage")
**And** le français est canonique Thiercelieux (vocabulaire respecté : "Loups-Garous", "Sorcière", "Voyante", etc.)

### Story 6.2 : Génération voix ElevenLabs + intégration assets

As a joueur,
I want entendre une voix MJ professionnelle et immersive,
So que l'ambiance "veillée d'antan" est crédible.

**Acceptance Criteria :**

**Given** le script complet (Story 6.1) est validé
**When** la génération ElevenLabs s'effectue (mode bundled v1)
**Then** chaque ligne est générée avec une voix sélectionnée (validation collective interne)
**And** les fichiers audio sont au format `.ogg` ou `.mp3` 128 kbps mono
**And** le bundle audio total tient < 80 MB (objectif app size NFR3)

**Given** les assets audio sont générés
**When** ils sont intégrés dans le projet Unity
**Then** un système `MJVoiceManager.cs` expose `Play(lineId)` et `PlayWithCallback(lineId, onComplete)`
**And** les fichiers sont organisés par catégorie (intro/nuit/jour/mort/victoire)
**And** un test playback de 10 lignes au hasard est validé sans glitch

**Given** un test d'écoute collective (équipe + 5 testeurs externes)
**When** la qualité est évaluée
**Then** ≥ 80% des testeurs jugent la voix "convaincante" et "immersive" (qualitatif)
**And** les lignes jugées "robotiques" sont régénérées avec d'autres paramètres ElevenLabs

### Story 6.3 : Musique nuit / jour / morts / victoire

As a joueur,
I want une bande son atmosphérique adaptée à chaque phase,
So que l'ambiance sonore renforce la tension narrative.

**Acceptance Criteria :**

**Given** la commande sonore est définie (compositeur freelance)
**When** les pistes sont livrées
**Then** au minimum 6 pistes existent : ambiance nuit (3-5 min loopable), ambiance jour (3-5 min loopable), stinger mort (5-10s), thème victoire Villageois (10-15s), thème victoire Loups (10-15s), menu d'accueil (1-2 min loopable)
**And** chaque piste respecte la palette sonore GDD (drones, cordes graves nuit / accordéon, percussions discrètes jour)

**Given** les pistes sont intégrées
**When** une phase démarre
**Then** la piste correspondante joue automatiquement avec un crossfade 1s
**And** le volume musique est paramétrable indépendamment du volume voix MJ et SFX (NFR accessibilité)

**Given** une mort survient
**When** l'animation de mort se déclenche (Story 5.4)
**Then** le stinger mort joue en sync, 0.2s avant le pic visuel
**And** la musique d'ambiance baisse de -6 dB pendant 3 secondes (ducking)

**Given** la victoire d'un camp est déclarée
**When** l'écran de fin charge
**Then** le thème victoire correspondant joue en boucle calme
**And** la musique précédente s'arrête immédiatement

### Story 6.4 : SFX ambiants (feu, forêt, hurlements, hibou)

As a joueur,
I want entendre la nature autour de moi pour me sentir vraiment au village,
So que l'immersion auditive est totale.

**Acceptance Criteria :**

**Given** la nuit démarre
**When** la scène village charge
**Then** les SFX d'ambiance nocturne se déclenchent en boucle : crépitement du feu (continu, spatialisé sur le centre), vent (subtil), hibou (occurrence aléatoire toutes les 30-90s), hurlements lointains (occurrence rare, dramatique)

**Given** le jour démarre
**When** la phase passe
**Then** les SFX nocturnes s'arrêtent en fade
**And** des SFX diurnes prennent le relais : oiseaux, vent plus léger, cloche de chapelle distante (occurrence rare)

**Given** un joueur vote (Story 3.7)
**When** le tap est enregistré
**Then** un SFX "tap bois" satisfaisant joue (FR feedback)
**And** le SFX est court (< 200 ms) et n'interfère pas avec la voix MJ

**Given** au moins 50 SFX courts uniques sont catalogués (FR ~50-100 SFX v1)
**When** le test audio complet est joué
**Then** aucun SFX n'est répétitif au point de devenir gênant
**And** le mix global est équilibré (voix MJ ≥ musique > SFX > vocal des joueurs sont les niveaux par défaut, ajustables en settings)

### Story 6.5 : Sync audio MJ ↔ phases de jeu

As a joueur,
I want que la voix MJ enchaîne parfaitement avec les transitions visuelles,
So que rien ne casse l'illusion narrative.

**Acceptance Criteria :**

**Given** une transition de phase s'enclenche (ex : nuit → jour)
**When** le système orchestre la séquence
**Then** la voix MJ "Le village s'éveille…" démarre 500 ms après le début de la transition visuelle (Story 5.5)
**And** la musique de jour démarre 1s avant la fin de la voix MJ (overlap doux)

**Given** la voix MJ annonce un rôle pendant la nuit
**When** elle joue
**Then** l'écran-noir des autres joueurs est synchronisé (Story 5.6 fade out commence en même temps)
**And** la phase active du rôle ne démarre QU'APRÈS la fin de la ligne MJ

**Given** plusieurs morts à annoncer dans le même tour
**When** la séquence enchaîne
**Then** chaque ligne MJ "X est mort de [cause]" joue séquentiellement
**And** chaque animation de mort (Story 5.4) se déclenche sur le mot pivot ("mort", "tué", etc.)
**And** l'enchaînement reste lisible et dramatiquement rythmé

**Given** un joueur a activé "sous-titres MJ" (NFR15)
**When** une ligne MJ joue
**Then** un sous-titre apparaît en bas d'écran avec le texte exact
**And** le sous-titre disparaît automatiquement à la fin de la ligne audio

---

## Épopée 7 : Mode Campfire (Bluetooth Totem + Compagnons)

**Goal :** Implémenter le mode signature "1 téléphone au centre = totem ambiance, N compagnons = UI privée". Inclut le pairing BT, la sync d'état multi-appareils, et les vues différenciées totem vs compagnon. À la fin de cette épopée, le mode Campfire est jouable et constitue un différenciateur marketing concret.

### Story 7.1 : POC Bluetooth iOS CoreBluetooth

As a développeur,
I want valider techniquement la communication BT entre 2 iPhones,
So que la fondation Campfire est solide avant de construire l'UX.

**Acceptance Criteria :**

**Given** deux iPhones avec l'app installée
**When** un script `BluetoothManager.cs` expose `StartAsTotem()`, `DiscoverTotems()`, `ConnectToTotem(id)`, `Send(payload)`, `OnReceive(payload)`
**Then** un iPhone A en mode "Totem" est découvrable par les iPhones environnants en < 5 secondes
**And** un iPhone B peut se connecter à A et échanger un payload texte aller-retour

**Given** la connexion BT est établie
**When** A envoie un payload de 1 KB à B
**Then** B le reçoit en < 100 ms (NFR5)
**And** la connexion reste stable pendant 30 minutes minimum sans déconnexion

**Given** la permission Bluetooth iOS n'est pas accordée
**When** l'utilisateur tente le mode Campfire
**Then** un message clair en français explique le besoin (NSBluetoothAlwaysUsageDescription)
**And** un bouton ouvre directement les Réglages iOS de l'app

**Given** le test passe sur iPhone 11 + iPhone 14 Pro
**When** la matrice de compatibilité est validée
**Then** un document interne liste les modèles testés OK
**And** les modèles non testés affichent un avertissement "Mode Campfire non garanti" si détectés

### Story 7.2 : Pairing 1 totem + N compagnons (jusqu'à 14)

As a host (Campfire mode),
I want pairer mon tél comme totem et que mes amis se connectent en compagnons,
So que la session Campfire démarre.

**Acceptance Criteria :**

**Given** je suis le host et j'ai choisi mode Campfire (Story 2.6)
**When** je tape "Devenir le totem"
**Then** mon tél bascule en mode totem et émet un signal BT découvrable
**And** un code de pairing (4 chiffres) s'affiche en gros à l'écran de mon tél
**And** un compteur "0 / N compagnons connectés" est visible

**Given** mon ami a installé l'app, est dans le même lobby, et tape "Rejoindre comme compagnon"
**When** il scanne les totems disponibles
**Then** il voit mon totem dans la liste
**And** après saisie du code 4 chiffres, sa connexion BT est établie en < 10s

**Given** N compagnons (jusqu'à 14, soit 15 joueurs total) se connectent
**When** la connexion est établie
**Then** le totem affiche le compteur mis à jour en temps réel
**And** chaque compagnon connecté apparaît dans la liste lobby (Story 2.4)

**Given** un compagnon perd la connexion BT en cours de partie
**When** la déconnexion dépasse 15 secondes
**Then** le système tente une reconnexion automatique 3 fois
**And** si échec, le joueur passe en mode "endormi" (FR57, Story 3.9) côté gameplay
**And** une notification au totem informe les autres

### Story 7.3 : Vue totem (audio ambiance + écran sombre immobile)

As a joueur,
I want que le tél au centre joue sa voix MJ et son ambiance pour le groupe,
So que l'expérience sonore est partagée physiquement.

**Acceptance Criteria :**

**Given** je suis le tél totem (Story 7.2)
**When** la partie démarre
**Then** mon écran affiche une scène ambiante minimale : feu animé en boucle, fond sombre, sans UI interactive (FR44)
**And** la luminosité est réduite à 30% pour ne pas éclairer la table
**And** les inputs sur mon écran sont désactivés (le totem n'est pas un joueur actif via cet écran)

**Given** une phase nuit ou jour s'enclenche
**When** la voix MJ doit jouer
**Then** la voix MJ joue à pleine puissance via le haut-parleur du totem (Story 6.2)
**And** la musique d'ambiance joue également via le totem (Story 6.3)

**Given** je suis le host et je dois être joueur aussi
**When** la partie démarre en Campfire
**Then** mon vrai rôle de joueur joue depuis un autre tél compagnon que je dois pairer en parallèle (mon iPhone perso ≠ totem)
**And** le système distingue clairement "tél totem (sans rôle)" de "tél host-joueur (avec rôle)"

**Given** la partie se termine
**When** l'écran de fin s'affiche
**Then** le totem affiche un visuel de fin partagé (qui a gagné, en grand)
**And** les détails (rôles révélés, stats) restent sur les compagnons individuels

### Story 7.4 : Vue compagnon (UI privée rôle + vote)

As a joueur compagnon en Campfire,
I want que mon tél en main affiche mon rôle et mes actions privées,
So que mes voisins ne voient pas mon rôle.

**Acceptance Criteria :**

**Given** je suis connecté en compagnon (Story 7.2)
**When** la partie démarre
**Then** mon écran affiche mon rôle privé (Story 3.1) sans aucune information visible des autres
**And** les actions de rôle (Voyante, Loups, Sorcière, etc.) se font sur mon écran personnel (FR45)
**And** ma luminosité reste normale (je dois lire mon écran)

**Given** la nuit démarre et mon rôle n'est pas actif
**When** un autre rôle est appelé
**Then** mon écran devient noir (Story 5.6) — l'innovation UX écran-noir reste valide en Campfire
**And** je ne dois pas révéler que mon écran est noir aux autres (équivalent "ferme les yeux")

**Given** mon rôle devient actif
**When** ma phase commence
**Then** mon écran s'allume avec mon UI d'action (haptic léger, FR42)
**And** je dois cacher mon écran avec ma main si je veux pas que mon voisin voie

**Given** la phase jour démarre
**When** je dois voter
**Then** je vote en tapant sur l'écran de mon compagnon
**And** mon vote est synchronisé via BT au totem et aux autres compagnons en < 100 ms (NFR5)

### Story 7.5 : Sync d'état multi-appareils via BT

As a player en Campfire,
I want que tous les téléphones soient parfaitement synchronisés sur l'état de la partie,
So que personne ne se retrouve désynchro.

**Acceptance Criteria :**

**Given** une action change l'état de la partie (vote, action de rôle, mort)
**When** elle est validée sur un compagnon
**Then** un message d'état est envoyé au totem en < 50 ms
**And** le totem rebroadcaste l'état à tous les autres compagnons en < 50 ms additionnels
**And** la latence totale joueur-A → joueur-B est < 150 ms (NFR5 p95)

**Given** un compagnon vient de reconnecter après une déconnexion brève
**When** il se rejoint
**Then** un message "snapshot d'état" lui est envoyé par le totem
**And** il rattrape l'état complet de la partie en < 2 secondes
**And** son écran reflète la phase actuelle correctement

**Given** un conflit d'état survient (ex : 2 compagnons votent simultanément)
**When** les messages arrivent au totem
**Then** le totem applique une logique d'ordre déterministe (timestamp + ID joueur tiebreaker)
**And** tous les compagnons reçoivent le même état final résolu

**Given** une perte totale de connexion BT survient (ex : interférence majeure)
**When** la déconnexion dure > 60 secondes
**Then** le totem affiche un message "Connexion perdue — vérifiez le Bluetooth"
**And** les compagnons affichent un message équivalent
**And** un bouton "Annuler la partie et retourner au lobby" est offert

---

## Épopée 8 : Onboarding & Accessibilité

**Goal :** Tutoriel friction-zéro pour les nouveaux joueurs, et conformité accessibilité complète (sous-titres, daltonisme, VoiceOver, push-to-talk, vibrations alternatives). À la fin de cette épopée, La Veillée passe la review accessibilité Apple sans souci et accueille la cible casual sans friction.

### Story 8.1 : Tutoriel interactif première partie (skippable)

As a joueur première-fois,
I want apprendre les bases en jouant un mini-cycle guidé,
So que je suis autonome dès ma vraie première partie.

**Acceptance Criteria :**

**Given** je suis un nouvel utilisateur (jamais joué de partie)
**When** je tape "Jouer" pour la première fois
**Then** une modale "Première partie ? Découvre les bases (~5 min)" m'est proposée
**And** un bouton "Passer (je connais le Loup-Garou)" est clairement visible (FR61)

**Given** je tape "Découvrir"
**When** le tutoriel démarre
**Then** une mini-partie scriptée se lance avec 5 bots IA + moi
**And** un narrateur off explique chaque phase (rôle, vote, mort, victoire) avec des highlights visuels
**And** je joue concrètement (vote, action si rôle actif) — pas une vidéo passive

**Given** le tutoriel est terminé
**When** la mini-partie se conclut
**Then** un résumé "Tu sais maintenant…" recap les 5 mécaniques apprises
**And** je suis invité à créer ou rejoindre une vraie partie
**And** le statut "tutoriel complété" est marqué dans mon profil (pas re-proposé)

**Given** je tape "Passer"
**When** le tutoriel est sauté
**Then** je vais directement à l'écran d'accueil
**And** un onboarding tooltip ultra-light (3 bulles max) m'aide à trouver "Créer / Rejoindre"

**Given** un joueur veut revoir le tutoriel
**When** il ouvre les paramètres
**Then** une option "Refaire le tutoriel" est disponible

### Story 8.2 : Sous-titres MJ optionnels

As a joueur malentendant ou en environnement bruyant,
I want voir le texte du MJ en direct,
So que je ne rate aucune information narrative.

**Acceptance Criteria :**

**Given** j'ouvre les paramètres
**When** je vais dans la section Accessibilité
**Then** un toggle "Sous-titres MJ" est disponible (off par défaut, NFR15)

**Given** j'active les sous-titres
**When** la voix MJ joue (Story 6.5)
**Then** le texte exact apparaît en bas d'écran dans une bande semi-transparente
**And** la police est lisible à distance (taille équivalente body text +20%)
**And** les sous-titres disparaissent automatiquement à la fin de la ligne audio

**Given** plusieurs lignes s'enchaînent
**When** la suivante démarre
**Then** la précédente s'efface et la nouvelle apparaît sans flash
**And** un délai minimum de lisibilité (1s par ligne) est garanti

**Given** je joue en mode Campfire et le totem affiche les sous-titres ?
**When** la voix MJ joue depuis le totem
**Then** par défaut les sous-titres apparaissent sur le totem ET sur mon compagnon (option configurable)

### Story 8.3 : Mode daltonisme (info redondée par chiffres + emojis)

As a joueur daltonien,
I want que toutes les informations couleur aient un équivalent visuel non-couleur,
So que je peux jouer sans handicap.

**Acceptance Criteria :**

**Given** je joue n'importe quel rôle
**When** un compteur de votes apparaît au-dessus d'une cible (Story 3.7)
**Then** le nombre est affiché en chiffres clairs (pas seulement par taille de bulle ou couleur)
**And** un emoji distinctif (ex : 🗳️) accompagne

**Given** une mort est annoncée
**When** la victime est désignée
**Then** son icône de rôle est affichée en plus d'une couleur (ex : 🐺 Loup, 🔮 Voyante, 🧪 Sorcière)
**And** le texte précise le rôle exactement

**Given** un test daltonien (validation avec utilisateur deutéranope ET protanope) est mené
**When** une partie complète est jouée
**Then** ≥ 95% des informations clés (vote, rôle, mort, camp) sont identifiables sans dépendance à la couleur (NFR16)

### Story 8.4 : Compatibilité VoiceOver complète

As a joueur aveugle ou très malvoyant,
I want naviguer toute l'app via VoiceOver,
So que je peux jouer comme tout le monde.

**Acceptance Criteria :**

**Given** VoiceOver est activé sur iOS
**When** j'ouvre l'app
**Then** chaque élément interactif a un label VoiceOver descriptif (NFR14)
**And** la navigation via swipe à gauche/droite parcourt logiquement les écrans

**Given** je suis dans une phase de jeu et un événement survient (ex : vote)
**When** un changement d'état important se produit
**Then** VoiceOver annonce automatiquement l'événement ("X a voté pour Y")
**And** les annonces ne sont pas spammées (cooldown 2s minimum entre 2 annonces du même type)

**Given** mon écran devient noir pendant la nuit (Story 5.6)
**When** la phase nuit s'enclenche
**Then** VoiceOver annonce "Nuit — ferme les yeux"
**And** quand mon rôle s'active, VoiceOver annonce "C'est ton tour, [rôle]"

**Given** un audit accessibilité Apple est réalisé
**When** le rapport est généré
**Then** zéro item critique (P0/P1) n'est listé
**And** au moins 3 testeurs malvoyants ont validé le flow complet

### Story 8.5 : Push-to-talk + mic off par défaut + vibration alternative

As a joueur sensible à la vie privée ou en environnement bruyant,
I want pouvoir n'activer mon micro qu'à la demande,
So que je contrôle ma communication.

**Acceptance Criteria :**

**Given** je crée mon profil pour la première fois
**When** la première partie démarre
**Then** mon micro est COUPÉ par défaut (NFR19)
**And** un message d'accueil m'invite à activer le micro pour parler

**Given** j'ouvre les paramètres
**When** je vais dans la section Vocal
**Then** un toggle "Push-to-talk" est disponible (off par défaut, FR39)

**Given** j'active push-to-talk
**When** je veux parler en partie
**Then** un bouton micro flottant apparaît à l'écran
**And** je dois maintenir le bouton pour parler (relâcher coupe)
**And** un haptic léger confirme le début/fin de la transmission

**Given** je suis joueur malvoyant et l'option "vibration alternative" est activée (NFR17)
**When** mon rôle devient actif (Story 5.6)
**Then** une vibration prolongée (2s) remplace le fade-in visuel d'écran
**And** VoiceOver annonce mon rôle simultanément

---

## Épopée 9 : Méta-Progression & Cosmétiques

**Goal :** Construire la couche méta qui pousse la rétention D7+ : écran fin de partie complet, stats personnelles cumulées, ~20 achievements, customisation avatar avec 10 skins de base + déverrouillables, leaderboards privés optionnels, et option replay rapide. À la fin de cette épopée, le joueur a une raison de revenir au-delà du fun pur.

### Story 9.1 : Écran fin de partie (révélation + stats partie + replay)

As a joueur,
I want voir tous les rôles révélés et un debrief de la partie,
So que je comprends ce qui s'est passé et que je peux relancer immédiatement.

**Acceptance Criteria :**

**Given** une partie se termine (toutes conditions de victoire, Story 3.8 / 4.2 / 4.6)
**When** l'écran de fin charge
**Then** le camp vainqueur est annoncé en grand au centre (FR72)
**And** chaque joueur de la partie est listé avec son rôle révélé et son statut (vivant/mort, qui l'a tué)

**Given** l'écran de fin est affiché
**When** je consulte les stats de la partie (FR73)
**Then** je vois : durée totale de la partie, nombre de tours, nombre de morts, qui a voté pour qui (recap), MVP (joueur le plus voté contre / le plus utile à son camp selon heuristique simple)

**Given** je veux relancer une partie immédiatement avec le même groupe
**When** je tape "Rejouer" (FR74)
**Then** un nouveau lobby est créé automatiquement avec les mêmes joueurs pré-invités
**And** la dernière configuration (composition, timers, mode) est pré-sélectionnée
**And** le host peut lancer en un tap supplémentaire

**Given** je veux quitter
**When** je tape "Retour à l'accueil"
**Then** je retourne à l'écran d'accueil joueur (Story 2.1)
**And** mes stats personnelles sont mises à jour (Story 9.2)

### Story 9.2 : Stats personnelles cumulées + détection style de jeu

As a joueur régulier,
I want voir mon évolution et mon style de jeu,
So que je m'identifie à un profil de joueur.

**Acceptance Criteria :**

**Given** au moins une partie a été jouée
**When** j'ouvre l'écran "Mes stats" depuis le menu
**Then** je vois : nombre total de parties, taux de victoire global, taux de victoire par rôle (FR62), nombre de fois élu Maire (Story 4.5)

**Given** j'ai joué ≥ 10 parties
**When** mes stats sont calculées
**Then** un "style de jeu" est détecté heuristiquement (FR63) parmi : Bluffeur (gagne souvent en Loup), Investigateur (gagne souvent en Voyante), Survivor (vit longtemps), Accusateur (vote souvent en premier), Connecteur (élu Maire souvent)
**And** le style est affiché avec un badge visuel sur le profil

**Given** j'ai un compte multi-appareils (iPad + iPhone par ex)
**When** mes stats sont consultées
**Then** elles sont synchronisées via iCloud (Story 10.3)
**And** la cohérence entre appareils est garantie (last-write-wins avec timestamp)

### Story 9.3 : Achievements v1 (~20)

As a joueur,
I want débloquer des accomplissements visuels,
So que j'ai des objectifs courts terme et collectionne des badges.

**Acceptance Criteria :**

**Given** un set de 20 achievements est défini (FR64)
**When** la liste est validée, elle inclut au minimum :
- "Première victoire" (gagner sa 1ère partie)
- "Premier sang" (être le premier à voter une élimination réussie)
- "Le Bluffeur" (gagner 5 parties en tant que Loup)
- "L'Œil qui voit tout" (gagner 5 parties en tant que Voyante)
- "Sorcellerie" (utiliser les 2 potions de la Sorcière dans la même partie)
- "Vengeur" (en tant que Chasseur, tuer un Loup au moment de mourir)
- "L'Élu" (être élu Maire pour la première fois)
- "Cupidon Stratège" (lier 2 amoureux qui gagnent en duo)
- "Petite Souris" (espionner les Loups sans être détectée 3 fois dans la même partie)
- "Salvateur Salvifique" (sauver la cible Loups 3 fois dans la même partie)
- "L'Ange Déchu" (gagner en tant qu'Ange)
- "Survivor" (survivre 5 parties d'affilée)
- "Sociable" (jouer 10 parties)
- "Vétéran" (jouer 50 parties)
- "Doyen" (jouer 200 parties)
- "Cosmopolite" (jouer avec 20 amis différents)
- "Maître de Soirée" (host 10 parties)
- "Veillée Marathon" (jouer 3 parties de suite avec le même groupe)
- "Tricheur Repéré" (être lynché 5 fois en tant que Loup au tour 1)
- "Légendaire" (gagner 100 parties)

**Given** je débloque un achievement
**When** la condition est remplie
**Then** une notification animée s'affiche en fin de partie ("Achievement débloqué : X")
**And** un badge persistant est ajouté à mon profil
**And** un cosmétique associé est éventuellement déverrouillé (Story 9.4, FR67)

**Given** je consulte la liste d'achievements
**When** j'ouvre l'écran dédié
**Then** les 20 sont listés avec : nom, description, statut (débloqué/grisé), date de déblocage si fait, % de joueurs qui l'ont (statistique sociale)

### Story 9.4 : Cosmétiques avatar (10 skins inclus + déverrouillables)

As a joueur,
I want personnaliser mon avatar avec des skins variés,
So que mon identité visuelle est unique.

**Acceptance Criteria :**

**Given** la v1 lance avec 10 skins de base (variations têtes, corps, coiffures, chapeaux, accessoires)
**When** je consulte le customizer (Story 5.2)
**Then** les 10 skins de base sont gratuits et utilisables immédiatement
**And** ils respectent la palette art GDD (chaleureux, lisibles, distinctifs)

**Given** j'ai débloqué un achievement qui donne un cosmétique (FR67)
**When** je retourne au customizer
**Then** le nouveau cosmétique apparaît avec un badge "Nouveau"
**And** je peux l'équiper sans coût supplémentaire
**And** une popup explique l'origine ("Débloqué via : Première victoire")

**Given** je suis élu Maire pour la première fois
**When** la victoire post-Maire arrive
**Then** la "Couronne d'or" est ajoutée à ma collection (FR69)
**And** je peux l'équiper indépendamment du fait d'être Maire dans une future partie

**Given** un cosmétique IAP existe (Story 10.1)
**When** je le consulte sans l'avoir
**Then** un cadenas + bouton "Acheter X €" est visible
**And** un preview "essai 5s" est disponible avant achat

### Story 9.5 : Leaderboards privés entre amis (optionnels)

As a joueur,
I want voir un classement entre mes amis sans compétition publique stressante,
So que la rivalité reste fun et locale.

**Acceptance Criteria :**

**Given** j'ai joué ≥ 5 parties avec un même groupe d'amis (détecté via Photon room IDs)
**When** j'ouvre l'onglet "Mes potes"
**Then** un leaderboard privé est proposé (FR65)
**And** il classe les amis selon : nb victoires, taux de victoire, nb parties (3 onglets)

**Given** je veux désactiver les leaderboards
**When** je vais dans paramètres → vie privée
**Then** un toggle "Apparaître dans les leaderboards de mes amis" est disponible (on par défaut, désactivable)
**And** si désactivé, je n'apparais dans aucun leaderboard ami
**And** je ne peux plus voir les leaderboards non plus (réciprocité)

**Given** un ami a désactivé les leaderboards
**When** je consulte mon leaderboard
**Then** il n'apparaît pas dans ma liste
**And** aucun message ne révèle qu'il a désactivé (vie privée)

---

## Épopée 10 : Monétisation, Cloud & Telemétrie

**Goal :** Brancher l'IAP cosmétiques (App Store), la monnaie virtuelle "Braises", iCloud sync pour stats/cosmétiques, Remote Config Firebase pour piloter l'équilibrage post-launch sans nouvelle release, et Analytics + Crashlytics pour mesurer la rétention et corriger les bugs. Cette épopée fait tourner le business et permet le pilotage produit.

### Story 10.1 : IAP cosmétiques (App Store In-App Purchase)

As a joueur engagé,
I want acheter des skins ou packs de voix MJ alternatifs,
So que je supporte le projet et personnalise davantage.

**Acceptance Criteria :**

**Given** la console App Store Connect est configurée
**When** les produits IAP sont créés
**Then** au minimum 3 produits initiaux existent v1 :
- "Pack Skins Hiver" (5 skins exclusifs, ~3.99 €)
- "Pack Voix MJ Drôle" (variante voix MJ comique pour toute la partie, ~4.99 €) — ⚠️ activable seulement si Story 6.2 voix alternative est techniquement supportée v1, sinon roadmap v2
- "Pack Soutien Studio" (skin doré + badge supporter, ~9.99 €)

**Given** je suis sur l'écran Boutique
**When** j'ouvre la liste des produits
**Then** chaque produit affiche : visuel, nom, description, prix localisé
**And** un bouton "Acheter" lance la sheet IAP iOS native

**Given** je confirme un achat via Face ID
**When** Apple valide la transaction
**Then** le produit est immédiatement déverrouillé dans ma collection (FR68)
**And** un événement Analytics "iap_purchase" est tracké (Story 10.5)
**And** un reçu est stocké côté Firestore pour vérification anti-fraude

**Given** je restaure mes achats sur un nouvel appareil
**When** je tape "Restaurer mes achats"
**Then** Apple StoreKit retourne mes anciens achats
**And** ils sont re-déverrouillés dans ma collection sans frais

**Given** un achat échoue (Apple rejette, réseau coupé)
**When** l'erreur survient
**Then** un message clair en français explique
**And** aucun produit n'est crédité

### Story 10.2 : Monnaie virtuelle "Braises"

As a joueur,
I want gagner des Braises en jouant et pouvoir les utiliser pour acheter des cosmétiques,
So que la progression débloque du contenu sans payer.

**Acceptance Criteria :**

**Given** je joue une partie
**When** la partie se termine
**Then** je gagne X Braises (formule paramétrable Remote Config Story 10.4 — par défaut : 10 si je perds, 25 si je gagne, +5 par achievement débloqué dans la partie)
**And** un compteur "Braises gagnées : +X" s'affiche dans l'écran fin de partie

**Given** je consulte mon solde de Braises
**When** j'ouvre la Boutique
**Then** mon total est visible en haut
**And** chaque produit Braises affiche son prix en Braises

**Given** je veux acheter un cosmétique en Braises
**When** je tape "Acheter avec Braises"
**Then** une confirmation modale apparaît
**And** si j'ai assez de Braises, le cosmétique est déverrouillé et mon solde décrémenté
**And** sinon, un bouton "Acheter des Braises avec IAP" propose des packs de Braises à l'achat

**Given** un pack de Braises IAP existe
**When** je consulte la Boutique
**Then** des packs de différentes tailles sont proposés (ex : 100 / 500 / 2000 Braises avec prix dégressif)

### Story 10.3 : iCloud sync stats / cosmétiques / achievements

As a joueur multi-appareils,
I want que mes stats, achievements et cosmétiques soient les mêmes partout,
So que je n'ai pas l'impression de repartir de zéro sur mon iPad.

**Acceptance Criteria :**

**Given** je suis authentifié avec mon Apple ID
**When** mes données changent (nouvelle stat, nouveau cosmétique, achievement débloqué)
**Then** elles sont écrites dans Firestore (source de vérité)
**And** un mirror iCloud (CloudKit) est mis à jour pour offline-first sur les autres appareils Apple (FR70)

**Given** je me connecte sur un nouvel appareil
**When** mon profil est restauré
**Then** mes stats, cosmétiques équipés/possédés, achievements et solde Braises sont identiques en < 5 secondes

**Given** un conflit survient (modification offline sur 2 appareils en parallèle)
**When** la sync s'effectue
**Then** la dernière modification (timestamp serveur) gagne
**And** une notification informe "Tes données ont été synchronisées"

### Story 10.4 : Remote Config Firebase

As a chef de projet,
I want pouvoir ajuster certains paramètres de gameplay sans nouvelle release,
So que je peux équilibrer rapidement post-launch.

**Acceptance Criteria :**

**Given** Firebase Remote Config est intégré
**When** la liste des paramètres exposés est définie
**Then** au minimum les paramètres suivants sont remote-configurables (FR71) :
- Multiplicateur de Braises gagnées par partie
- Probabilité de détection de la Petite Fille (Story 4.3, défaut 30%)
- Durée min/max des timers nuit et jour
- Activation/désactivation de feature flags v2 (mode enfants, etc.)
- Composition de rôles recommandée par taille de groupe (Story 2.5)

**Given** je modifie un paramètre dans la console Firebase
**When** je publie le changement
**Then** les nouveaux clients fetchent la valeur au prochain cold start
**And** les sessions actives ne sont pas impactées (cohérence en cours de partie)

**Given** Firebase est inaccessible
**When** un client lance une partie
**Then** il utilise les valeurs par défaut bundled
**And** aucun crash ou blocage ne survient

### Story 10.5 : Analytics Firebase + Crashlytics

As a chef de projet,
I want mesurer la rétention, le funnel d'engagement et corriger les crashs rapidement,
So que je pilote le produit avec données.

**Acceptance Criteria :**

**Given** Firebase Analytics + Crashlytics sont intégrés
**When** un événement clé survient
**Then** il est tracké automatiquement avec les paramètres pertinents :
- `app_open`, `signup_complete`, `tutorial_complete`, `tutorial_skip`
- `room_create`, `room_join`, `room_leave`
- `game_start`, `game_end` (avec : durée, nb joueurs, mode Campfire/Remote, camp vainqueur)
- `iap_view`, `iap_purchase`, `iap_fail`
- `achievement_unlock`
- `session_pause`, `session_resume`

**Given** un crash survient sur un appareil
**When** Crashlytics capture l'erreur
**Then** le rapport est envoyé avec : stack trace, version OS, modèle d'appareil, dernière action utilisateur
**And** les rapports sont consultables dans la console Firebase en < 5 minutes
**And** un alerting Slack/email est configuré pour crashs critiques (P0)

**Given** une dashboard Analytics est configurée
**When** un product manager consulte
**Then** les KPIs cibles GDD sont visibles : D1/D7/D30 retention, session length moyenne, parties/session, conversion free→paying
**And** les données respectent RGPD (anonymisation des UIDs côté analytics, opt-out possible)

**Given** un utilisateur ouvre les paramètres
**When** il va dans "Vie privée"
**Then** un toggle "Partager données analytics anonymes" est disponible (on par défaut avec consentement explicite à la 1ère ouverture, désactivable, NFR26)

---

## Épopée 11 : Lancement & QA

**Goal :** Préparer le launch : playtest alpha 50 personnes, beta publique 500 personnes, soumission App Store, plan marketing créateurs/presse, et monitoring temps réel le jour J. À la fin de cette épopée, La Veillée est sur l'App Store et la rétention D7 ≥ 20% est mesurable.

### Story 11.1 : Playtest alpha fermée (50 personnes)

As a chef de projet,
I want un playtest fermé pour valider l'équilibrage et chasser les bugs critiques,
So que la beta publique se déroule sans drame.

**Acceptance Criteria :**

**Given** la build alpha est stabilisée (toutes Épopées 1-10 ✅)
**When** un programme TestFlight alpha est créé
**Then** 50 testeurs cibles sont invités (mix : potes proches + influenceurs amateurs francophones)
**And** un canal Discord/Slack dédié recueille les feedbacks
**And** un formulaire structuré (Notion / Airtable) capture : bugs, équilibrage rôles, expérience UX, qualité audio

**Given** la phase alpha dure 2-4 semaines
**When** au moins 30 sessions de jeu sont enregistrées
**Then** un rapport synthétique est produit : top 10 bugs, top 5 frustrations UX, suggestions d'équilibrage
**And** un sprint de fix est planifié (durée 1-2 semaines)

**Given** les bugs critiques sont corrigés
**When** une nouvelle build est poussée
**Then** elle est validée à nouveau par 10 testeurs alpha (sous-set)
**And** la build est promue en candidate beta

### Story 11.2 : Beta publique limitée (500 personnes)

As a chef de projet,
I want une beta plus large pour tester la scalabilité serveurs et capter des retours plus diversifiés,
So que les surprises du launch soient minimes.

**Acceptance Criteria :**

**Given** la build candidate beta est validée (Story 11.1)
**When** un programme TestFlight public est créé
**Then** 500 spots sont ouverts via inscription (formulaire Typeform ou waitlist landing page)
**And** la beta dure 4-6 semaines
**And** un onboarding minimal explique aux beta-testeurs comment remonter les feedbacks

**Given** la beta est en cours
**When** les serveurs Firebase / Photon / Agora reçoivent du traffic
**Then** les NFRs de scalabilité sont validés sous charge réelle (NFR8 uptime, NFR9 matchmaking error rate)
**And** des alertes monitoring sont configurées (Datadog / Grafana sur les métriques Firebase)

**Given** la beta détecte des bugs critiques en production
**When** un correctif est déployé
**Then** une nouvelle build TestFlight est poussée en < 48h
**And** les beta-testeurs sont notifiés

### Story 11.3 : Soumission App Store + ASO FR

As a chef de projet,
I want soumettre l'app à Apple et qu'elle passe la review au premier essai,
So que le launch peut être daté précisément.

**Acceptance Criteria :**

**Given** la build beta finale est validée
**When** la fiche App Store est préparée
**Then** elle contient : titre "La Veillée — Loups-Garous", sous-titre, description optimisée FR (400 mots), 8 screenshots iPhone + 5 iPad, vidéo preview 30s, mots-clés ASO (Loup-Garou, Mafia, Soirée entre amis, Party Game, Thiercelieux, Bluff, etc.), catégorie Games > Party
**And** un classement âge 17+ est appliqué (vocal non modéré, NFR20)

**Given** la soumission Apple est faite
**When** la review s'effectue
**Then** la check-list anti-rejet est validée : permissions justifiées, IAP fonctionnels, pas de placeholder, vocal modération assumée par signalement (mécanisme à implémenter en bonus si possible)
**And** si Apple rejette, le motif est traité en < 5 jours et resoumis

**Given** la build est approuvée
**When** la date de launch est fixée
**Then** la "Release Date" est planifiée 7-14 jours après approbation pour aligner com et marketing
**And** la build reste "Pending Developer Release" jusqu'au jour J

### Story 11.4 : Plan marketing pré-launch (créateurs francophones, presse)

As a chef de projet,
I want une vague de visibilité orchestrée au launch,
So que les premières dizaines de milliers de téléchargements sont au rendez-vous.

**Acceptance Criteria :**

**Given** une liste de cibles est définie
**When** la phase pré-launch démarre (J-30)
**Then** au minimum sont contactés :
- 10 créateurs YouTube/Twitch/TikTok francophones spécialisés party-games (Squeezie, McFly & Carlito, Domingo, etc. cibles potentielles + tier 2)
- 5 médias presse jeux vidéo / lifestyle FR (Numerama, Frandroid, Le Figaro Tech, Konbini, etc.)
- 3 communautés Reddit/Discord LG francophones

**Given** des kits presse sont préparés
**When** un journaliste demande
**Then** un kit complet est fourni : pitch 1-page, fiche tech, screenshots HD, vidéo 60s, accès anticipé build (TestFlight)

**Given** une stratégie d'embargo est en place
**When** les créateurs reçoivent l'app en avance
**Then** une date d'embargo levé = date launch est convenue
**And** les vidéos/articles sont publiés en vague coordonnée le J = launch day

### Story 11.5 : Launch day + monitoring temps réel

As a chef de projet,
I want surveiller activement les métriques le jour du launch et réagir aux incidents,
So que la première impression utilisateur est positive.

**Acceptance Criteria :**

**Given** le launch est imminent
**When** la build est libérée sur App Store
**Then** un war-room est en place (équipe dispo 12h+ jour J)
**And** des dashboards live tournent : nb téléchargements (sandbox + AppFigures), crash rate, latence vocale moyenne, erreurs matchmaking, rapports d'accessibilité Apple

**Given** un incident critique survient (crash global, serveur down)
**When** l'alerte se déclenche
**Then** un runbook est consulté (procédures pré-écrites)
**And** un hotfix est préparable et déployable en < 4h (TestFlight → submit ou Remote Config rollback)

**Given** le launch day se termine (J+1)
**When** un debrief est organisé
**Then** les KPIs J+1 sont analysés vs objectifs (ex : J+30 → 5000 téléchargements, donc J+1 → ~150-300 attendus)
**And** un post-mortem express identifie ce qui a marché / pas marché
**And** les ajustements sont planifiés pour le J+7 sprint

**Given** la rétention D7 atteint au moins le seuil cible (≥ 20%, NFR business)
**When** la mesure est faite à J+8
**Then** le launch est déclaré "succès soft"
**And** la roadmap v1.1 (1 rôle additionnel gratuit + ajustements) est priorisée

---

## Validation Finale

### ✅ Couverture des FRs

Toutes les **74 FRs** listées dans l'Inventaire des Exigences sont couvertes par au moins une story (cf. **FR Coverage Map** ci-dessus).

### ✅ Couverture des NFRs

| Catégorie | NFRs | Couverture |
|---|---|---|
| Performance (NFR1-6) | Tenue 30/60 fps, RAM, app size, latence vocale, latence BT, cold start | Validation transversale + Épopée 11 (QA) |
| Qualité (NFR7-9) | Crash-free, uptime, matchmaking | Crashlytics Story 10.5 + monitoring Story 11.5 |
| Plateforme (NFR10-13) | iOS 15+, iPhone 11+, iPad, multi-résolution | Bootstrap Story 1.1 |
| Accessibilité (NFR14-19) | VoiceOver, sous-titres, daltonisme, vibration alt, timer audio, push-to-talk | Stories 8.2, 8.3, 8.4, 8.5, 5.6 |
| Vie privée (NFR20-22) | Vocal non modéré 18+, pas matchmaking inconnus, chiffrement | Stories 11.3 (App Store rating), 2.3 (invite-only), 1.2 (Firebase Auth) |
| i18n (NFR23-24) | FR v1, prêt EN v2 | Bootstrap Story 1.1 (i18n stack) |
| Conformité (NFR25-26) | App Store, RGPD | Stories 11.3, 10.5 |

### ✅ Indépendance des Épopées

Chaque épopée est autonome dans son domaine de valeur :
- Épopée 1 = fondation (auth + multijoueur + vocal) → utilisée par 2-11
- Épopée 2 = lobby complet → utilise Épopée 1, utilisée par 3-11
- Épopée 3 = boucle MVP jouable → utilise Épopées 1 + 2
- Épopée 4 = catalogue complet → utilise Épopée 3 (ne casse pas si non livrée)
- Épopée 5 = habillage cinématique → utilise Épopée 3 (le jeu reste jouable visuellement minimal sans elle)
- Épopée 6 = audio MJ → utilise Épopée 3 (le jeu reste jouable muet sans elle)
- Épopée 7 = mode Campfire → ajoute un mode, le mode Remote reste fonctionnel sans
- Épopée 8 = onboarding/accessibilité → améliore mais ne bloque pas le gameplay core
- Épopée 9 = méta → ajoute la rétention, le jeu se joue sans
- Épopée 10 = monétisation/cloud/telemetrie → indépendante du gameplay
- Épopée 11 = launch → étape finale

**Aucune épopée future n'est requise par une épopée antérieure pour fonctionner.** Une release MVP minimaliste pourrait cibler Épopées 1+2+3+11 (jeu jouable, brut visuellement, sans audio MJ) — l'architecture est respectée.

### ✅ Indépendance des Stories au sein des Épopées

Chaque story est implémentable séquentiellement (Story N.M ne dépend que de N.1...N.M-1 et des épopées antérieures).

### ✅ Création d'entités à la demande

Aucune story ne crée d'avance des structures de données qui ne lui servent pas. Exemples :
- Le profil utilisateur est créé Story 1.3 (et pas avant).
- Les Braises sont créées Story 10.2 (pas dans 1.3).
- Les achievements sont définis Story 9.3 (pas dans Épopée 1).

### ✅ Total

- **11 épopées**
- **62 stories** détaillées
- **74 FRs** couvertes 1:1 ou multi-stories
- **26 NFRs** couvertes transversalement

---

**Document finalisé — Epics & Stories La Veillée v1.0 — 2026-04-20**

**Prochaines étapes recommandées :**
1. `/gds-create-architecture` (ou `/bmad-create-architecture`) pour produire l'`Architecture.md` qui finalisera les choix techniques (Photon vs Mirror, Agora vs LiveKit, structure data Firestore)
2. `/gds-create-ux-design` pour produire l'`UX Design Specification` qui détaillera les wireframes, design tokens et composants UI
3. `/gds-check-implementation-readiness` pour valider que GDD + Architecture + UX + Epics & Stories sont alignés avant de créer les sprints
4. `/gds-create-story [story-id]` pour passer la première story (Story 1.1) en mode dev-ready avec tout le contexte agent
