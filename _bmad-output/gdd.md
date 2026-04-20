---
stepsCompleted: [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14]
workflow_completed: true
inputDocuments:
  - _bmad-output/game-brief.md
  - _bmad-output/brainstorming/brainstorming-session-2026-04-20-1553.md
documentCounts:
  briefs: 1
  research: 0
  brainstorming: 1
  projectDocs: 0
workflowType: 'gdd'
lastStep: 14
project_name: 'lg'
user_name: 'sayanth'
date: '2026-04-20'
game_type: 'party-game'
game_name: 'La Veillée'
---

# La Veillée - Game Design Document

**Author:** sayanth
**Game Type:** Party Game (party-game)
**Target Platform(s):** iOS (v1) puis Android (v2)

---

## Executive Summary

### Game Name
La Veillée

### Core Concept
Une adaptation mobile fidèle des Loups-Garous de Thiercelieux, où l'app devient le Maître du Jeu pour que tous tes potes jouent ensemble autour d'un feu de camp numérique. La Veillée recrée l'ambiance des veillées d'antan — ces soirées d'hiver où l'on se rassemblait autour du feu pour se raconter des histoires qui font peur, se méfier les uns des autres, rire ensemble. L'app prend en charge tout le travail du Maître du Jeu humain (distribution des rôles, narration, gestion des phases, timing), libérant l'ensemble du groupe pour enfin participer à la partie.

Deux modes d'utilisation possibles :
- **Campfire** — un seul téléphone posé au centre de la table, joueurs en présentiel
- **Remote** — chacun son tél, partie à distance avec vocal intégré

### Target Audience
Jeunes adultes francophones 18-30 ans qui organisent des soirées entre potes et connaissent déjà les Loups-Garous physiques, secondairement amis dispersés qui veulent jouer à distance.

### Unique Selling Points (USPs)
1. **Mode Campfire** — unique sur le marché (un seul téléphone pour toute la soirée)
2. **Voix MJ + ambiance cinématique + fidélité canon** réunies
3. **Écran noir = yeux fermés numérique** (innovation UX centrale)
4. **Vrai Loup-Garou français 100 % canonique** (Thiercelieux)

### Game Type
**Type :** Party Game
**Framework :** Ce GDD utilise le template `party-game` avec sections spécifiques pour le multijoueur local/online, les minigames cycliques (jour/nuit), le setup social rapide, et l'expérience casual accessible.

---

---

## Target Platform(s)

### Primary Platform
**iOS (v1 Launch)** — iPhone + iPad, iOS 15+, iPhone 11 et ultérieurs pour couvrir ~90% du parc iOS.

### Secondary Platform (v2, 6-12 mois post-launch)
**Android (Google Play)** — port avec adaptations UI mineures après stabilisation v1.

### Platform Considerations
**Features iOS à exploiter :**
- iOS CoreBluetooth pour la sync Totem + Compagnons en mode Campfire
- Haptic feedback (Taptic Engine) pour vibrations subtiles quand rôle actif
- Push notifications pour relancer les groupes ("Paul t'invite à une partie")
- iCloud pour la sauvegarde cosmétiques / stats
- App Store In-App Purchase pour la monétisation cosmétique

**Hors scope :** PC, console, web, VR.

### Control Scheme
- **Touch** : tap sur avatar (vote, cible nocturne), swipe horizontal (rotation vue 360° mode Remote)
- **Gyroscope** : rotation alternative pour regarder autour (mode Remote)
- **Micro** : vocal intégré constant
- **Haptics** : feedback subtil (vote comptabilisé, rôle actif, timer expiration)

---

## Target Audience

### Demographics
- **Âge** : 18-30 ans (cœur de cible 20-26)
- **Géographie** : France, Belgique, Suisse romande, Québec — marché francophone first
- **Groupes sociaux** : colloc, assos étudiantes, potes de lycée, collègues de bureau
- **Taille de groupe** : 5-15 personnes qui se voient régulièrement

### Gaming Experience
**Casual mobile gamers.** Pas hardcore. Habitués à Heads Up!, Kahoot, UNO mobile, Trivia Crack, Among Us. Téléchargent des party games sur recommandation, jouent quelques sessions puis désinstallent si pas excellent.

### Genre Familiarity
**Très élevée** — la cible connaît déjà le Loup-Garou physique. Pas besoin de tutoriel long sur les règles. Onboarding minimal possible pour l'utilisateur type.

### Session Length
**35-90 min par session**, en moyenne 1.5 parties enchaînées dès qu'un groupe se lance. Une session = une soirée, pas 5 min dans le métro.

### Secondary Audience
**Amis géographiquement dispersés** qui veulent jouer à distance via le vocal intégré + mode Remote (remplacer Discord + webcam + cartes partagées par une seule app).

### Player Motivations
1. Libérer le MJ — plus de pote qui sacrifie sa soirée
2. Pas devoir ramener les cartes
3. Envie d'une nouvelle expérience plus cinématique
4. Partager un moment mémorable — le jeu devient le centre de la soirée

---

---

## Goals and Context

### Project Goals

1. 🎨 **Créer la meilleure expérience mobile du Loup-Garou canonique francophone** — devenir LA référence par la qualité de l'immersion (voix MJ + ambiance + écran-noir)
2. 🔥 **Valider l'architecture Totem + Compagnons** comme nouveau standard pour les party games mobiles social-deduction
3. 📈 **Atteindre 100 000 téléchargements dans les marchés francophones à J+180** avec une rétention D7 ≥ 20%
4. 💰 **Atteindre le break-even 12 mois post-launch** avec un mix gratuit + IAP cosmétiques pour auto-financer la v2

### Background and Rationale

Le Loup-Garou de Thiercelieux est joué depuis 30 ans en France, avec plus de 10 millions d'exemplaires du jeu physique vendus. Il reste pourtant une tension fondamentale : un des joueurs doit sacrifier sa partie pour jouer le Maître du Jeu. Toutes les tentatives numériques existantes (Werewolf Online, etc.) ont résolu ce problème en perdant ce qui fait le charme du jeu : l'intimité, la voix, l'ambiance d'une soirée entre potes.

Le timing est maintenant idéal : la génération 18-30 est née avec les smartphones, a été habituée pendant le COVID aux party games mobiles (Among Us, Jackbox), et cherche des expériences premium qui valorisent le moment IRL au lieu de le remplacer. La Veillée arrive sur un marché mûr mais mal servi, avec une approche qu'aucun concurrent ne propose : le téléphone devient un objet au centre de la table, pas un écran entre chacun.

---

## Unique Selling Points (USPs)

### USP 1 — Mode Campfire : un seul téléphone pour toute la soirée
Aucune autre app ne le fait. Le tél posé au centre de la table devient le totem sonore — chacun ferme les yeux à l'appel du MJ sans installer l'app. Architecture unique (Bluetooth Totem-Speaker + Compagnons en mode Remote).

### USP 2 — Voix MJ cinématique + fidélité canon Thiercelieux + ambiance visuelle premium
Ce trio n'existe nulle part ailleurs sur mobile. Pas juste "une app avec les règles", mais une direction artistique audio-visuelle qui transforme une partie de 1h en expérience mémorable.

### USP 3 — L'écran noir = yeux fermés numérique
L'innovation UX centrale. Équivalent parfait du "fermez les yeux" physique, traduit sans trahir. Aucun concurrent ne l'a trouvé — opportunité de premier arrivé.

### USP 4 — Le vrai Loup-Garou français, 100 % canonique
10 rôles v1 fidèles à Thiercelieux. Pour la cible francophone, c'est LE critère de légitimité qui distingue La Veillée d'Among Us / Mafia Party / apps génériques.

### Competitive Positioning
La Veillée se positionne sur l'axe **"authentique × premium × francophone"** — un triangle que personne n'occupe aujourd'hui. Werewolf Online est générique et low-quality. L'app officielle Thiercelieux est juste un compagnon des cartes. Among Us est cousin mais pas Loup-Garou. La Veillée occupe le segment "j'aime le vrai Loup-Garou ET je veux une expérience premium sur mobile".

---

---

## Core Gameplay

### Game Pillars

**1. 🎭 Fidélité au canon**
Le jeu de Thiercelieux, respecté dans ses règles, ses rôles et son rythme. L'app transpose, n'invente pas.

**2. 🔥 L'intimité du cercle**
Ambiance d'une veillée autour d'un feu — sonore partagée, regards physiques, secret individuel. Pas de casque solo, pas de chat écrit, pas de viralité.

**3. 🎙️ L'app = Maître du Jeu, pas joueur**
Le téléphone remplace UNIQUEMENT le MJ. Tout ce qui peut rester entre humains reste entre humains.

**4. 🎬 Cinématique sans spectacle**
Transitions, morts, village, luminosité-sémantique. Le cinéma sert le gameplay, jamais l'inverse.

**Pillar Prioritization :** En cas de conflit — **Fidélité > App=MJ > Intimité > Cinématique**

### Core Gameplay Loop

```
┌─────────────────────────────────────────────┐
│  🌙 NUIT                                    │
│     → Écrans noirs sauf rôle actif          │
│     → MJ appelle rôle par rôle :            │
│       Cupidon (tour 1) → Voyante →          │
│       Loups → Sorcière → Salvateur          │
│     → Chacun agit via son tél perso         │
│                                             │
│  ☀️ RÉVEIL                                  │
│     → MJ annonce les morts                  │
│     → Animation socle s'effrite + tombe     │
│                                             │
│  💬 JOUR (timer ~5 min)                     │
│     → Débat vocal, accusations              │
│     → Clics de vote en temps réel           │
│     → Feed "X a voté pour Y" visible        │
│                                             │
│  🗳️ FIN DE VOTE                             │
│     → Joueur le plus voté éliminé           │
│     → Mort dramatique                       │
│                                             │
│  ⏪ Retour NUIT (sauf fin de partie)        │
└─────────────────────────────────────────────┘
```

**Loop Timing :**
- 1 cycle complet : ~8-10 min (3 min nuit + 5 min jour + transitions)
- Partie complète : ~30 min (5 joueurs) à ~90 min (25 joueurs)
- Durée moyenne : 45-60 min pour 10-12 joueurs

**Loop Variation (ce qui rend chaque cycle différent) :**
1. Composition de rôles change chaque partie → dynamiques différentes
2. Morts successives modifient progressivement l'info disponible
3. Événements narratifs (amoureux, tir du Chasseur, potions) créent des rebondissements
4. Psychologie du groupe évolue (qui a été accusé, sauvé, parle peu)

### Win/Loss Conditions

#### Victory Conditions

**Camp des Villageois (Villageois, Voyante, Sorcière, Chasseur, Cupidon, Petite Fille, Salvateur, Maire) :**
- Tous les Loups-Garous sont éliminés

**Camp des Loups-Garous :**
- Les Loups-Garous égalent ou dépassent en nombre les autres joueurs

**Camp solo — Ange :**
- L'Ange est lynché dès le 1er tour de jour

**Camp solo — Amoureux (si Cupidon les a liés entre camps opposés) :**
- Les deux amoureux survivent ensemble jusqu'à la fin

#### Failure Conditions

Un joueur peut mourir par :
- 🐺 Attaque nocturne des Loups-Garous
- 🗳️ Vote de jour du village
- 🧪 Potion de mort de la Sorcière
- 💔 Chagrin d'amour (si le partenaire amoureux meurt)
- 🎯 Tir du Chasseur (si le chasseur meurt, il tire en réaction)

#### Failure Recovery
- Le joueur mort ne revient pas dans la partie en cours
- Il rejoint le salon des morts — chat vocal entre morts, sans info privilégiée
- À la partie suivante, distribution de rôles fresh

#### Partie Nulle (cas rare canon)
Si plus personne n'est vivant (ex : derniers joueurs amoureux l'un loup, l'autre villageois, s'entretuent) → match nul.

---

---

## Game Mechanics

### Primary Mechanics

**1. 🗣️ Parler (vocal intégré)**
- Quand : phase jour en permanence + loups la nuit entre eux + salon des morts
- Skill testé : conviction, bluff, lecture sociale, timing des révélations
- Feel : naturel, faible latence (<200ms), voix claire sans artefacts
- Interaction : nourrit le clic-vote (tu parles, tu convaincs, puis les gens cliquent)
- Sert pillar : Intimité + App=MJ

**2. 👆 Cliquer un joueur (vote / cible)**
- Quand : vote de jour + actions nocturnes ciblées (loups, voyante, sorcière, salvateur)
- Skill testé : décision, lecture du feed de votes, timing
- Feel : tap satisfaisant avec haptic feedback, compteur visible au-dessus tête
- Interaction : jour = public temps réel / nuit = privé silencieux
- Sert pillar : Fidélité + App=MJ

**3. 👀 Regarder / Pivoter la vue (360° — Remote only)**
- Quand : toute la partie en mode Remote
- Skill testé : attention aux réactions d'autrui, lecture comportementale
- Feel : swipe horizontal fluide OU gyroscope, caméra FPS immobile sur son socle
- Interaction : permet de sélectionner un joueur avec précision
- Sert pillar : Cinématique + Intimité

**4. 🎧 Écouter (MJ + ambiance + voix des autres)**
- Quand : toute la partie
- Skill testé : attention soutenue, mémoire auditive, lecture émotionnelle
- Feel : audio riche, spatialisé pour effets d'ambiance, voix MJ medium-cuit
- Interaction : donne le tempo (MJ appelle les rôles, annonce les phases)
- Sert pillar : Cinématique + Intimité + App=MJ

**5. 🎭 Agir selon son rôle**

| Rôle | Action | Timing |
|---|---|---|
| Villageois | Aucune action nocturne — seulement parler + voter | Jour |
| Loup-Garou | Cliquer ensemble sur une cible (vote silencieux) | Nuit |
| Voyante | Tap sur un joueur → révélation privée de son rôle | Nuit, chaque tour |
| Sorcière | 2 potions (vie + mort), usage 1 fois chacune | Nuit |
| Chasseur | Tap sur un joueur pour tirer à sa mort | Déclenché à sa propre mort |
| Cupidon | Tap sur 2 joueurs pour les lier | Nuit 1 UNIQUEMENT |
| Petite Fille | Peut ouvrir brièvement les yeux quand loups actifs | Nuit, optionnel |
| Salvateur | Tap sur un joueur à protéger (pas 2 nuits de suite sur le même) | Nuit |
| Maire | Voix de vote qui compte double | Jour (passif) |
| Ange | Pas d'action — gagne si lynché tour 1 | Jour 1 |

### Mechanic Interactions

1. **Parler × Cliquer** — le débat oral influence le vote visible. Changer son vote devient une arme sociale.
2. **Regarder × Parler** — en Remote, voir les hochements complète l'info vocale.
3. **Écouter × Action de rôle** — timing séquentiel sacré (pillar Fidélité).
4. **Cliquer privé × Cliquer public** — cible nocturne (privé) = mort ; vote jour (public) = lynchage.
5. **Vote modifiable × Feed temps réel** — diplomatie publique en direct.

### Mechanic Progression
Pas de mécanique qui évolue pendant la partie (fidélité au canon). La "progression" vient de :
- L'apprentissage des rôles partie après partie
- La maîtrise de la lecture sociale et du bluff
- Le déverrouillage cosmétique (skins, voix MJ alt, thèmes) via IAP hors-gameplay

---

## Controls and Input

### Control Scheme (iOS)

| Mechanic | Input | Feedback |
|---|---|---|
| Parler | Micro (toujours actif pendant phases vocales) | Indicateur visuel "tu parles" |
| Cliquer joueur | Tap sur avatar | Haptic léger + compteur vote |
| Retirer vote | Tap sur un autre joueur (auto-retrait) ou soi-même (abstention) | Haptic + retrait visible |
| Pivoter vue 360° | Swipe horizontal OU gyroscope | Rotation fluide 60fps |
| Scroller feed votes | Swipe vertical sur le feed | Déroulement naturel |
| Menu pause | Tap coin haut-gauche | Menu overlay semi-transparent |
| Confirmer action rôle | Tap + hold 0.5s sur cible | Haptic fort à confirmation |
| Muter/dé-muter | Tap icône micro | Icône change visuellement |

### Input Feel
- Tap instantané sur votes (< 100ms perçus)
- Haptic léger sur chaque vote/tap
- Haptic fort sur confirmations d'actions irréversibles
- Pas de swipe accidentel (swipe long > 30% écran requis)
- Tap + hold pour actions irréversibles (cible de mort, potion)

### Accessibility Controls
- VoiceOver compatible
- Timer visuel + audio (bip dernières 10s)
- Sous-titres du MJ (option)
- Daltonisme : emojis + couleurs + chiffres sur le feed
- Mode écran noir alternatif : vibration pour malvoyants
- Micro off par défaut + push-to-talk optionnel

---

## Party Game Specific Elements

### Minigame Variety

La Veillée **n'a pas plusieurs minigames différents** — c'est un party game à **round-based unique** : chaque "round" est un cycle jour/nuit de Loup-Garou. La variété vient de la **composition de rôles** et de la dynamique sociale émergente, pas d'un catalogue de minigames différents.

- **"Minigame" unique :** Loup-Garou canonique
- **Durée d'un round :** ~8-10 min (1 cycle nuit + jour)
- **Durée d'une partie complète :** 30-90 min selon nombre de joueurs
- **Skill vs luck :** 70% skill (bluff, lecture sociale, déduction) / 30% luck (distribution des rôles)
- **Team vs FFA :** équipes (Villageois vs Loups) + rôles solo (Ange, Amoureux)

### Turn Structure

- **Pas de plateau** — les "tours" sont des **cycles jour/nuit** séquentiels
- **Ordre nocturne canonique strict** (Fidélité pilar) :
  1. Cupidon (1ère nuit uniquement)
  2. Amoureux (se reconnaissent, 1ère nuit)
  3. Voyante
  4. Loups-Garous
  5. Petite Fille (peut espionner la phase loups)
  6. Sorcière
  7. Salvateur
- **Jour :** débat vocal libre + vote (ordre horizontal, pas de "tour de parole" forcé)
- **Durée de match :** ~30-90 min jusqu'à victoire d'un camp
- **Mécaniques spéciales :** potions Sorcière, tir Chasseur, lien amoureux, vote du Maire x2

### Player Elimination vs Points

- **Système : élimination pure** (last team standing)
- **Pas de points** — on ne marque pas, on survit ou on perd
- **Comeback mechanics :** tir du Chasseur, potion de vie de la Sorcière, lien amoureux qui change la donne stratégique
- **Handicap systems :** aucun (fidélité) — les timers paramétrables sont la seule adaptation
- **Victory conditions :** voir Win/Loss ci-dessus (élimination complète du camp adverse OU condition solo rôle spécial)

### Local Multiplayer UX (mode Campfire)

- **1 téléphone = totem central** (borrowed du host)
- **N téléphones compagnons** (chaque joueur, pour rôle + vote)
- **Pas de controller sharing** — chaque joueur a son propre tél en poche
- **Screen layout :** chaque tél a sa vue privée, le totem a l'ambiance partagée (audio uniquement, écran sombre/ambiant)
- **Turn clarity :** la voix du MJ annonce les phases + haptic feedback sur le tél du rôle actif
- **Spectator experience :** les joueurs morts rejoignent le salon des morts (chat vocal séparé, sans info)
- **Player join :** au setup uniquement, avant distribution des rôles — pas de drop-in en cours de partie
- **Player drop :** si un joueur se déconnecte, son avatar devient "endormi" (bot passif qui vote aléatoirement) — anti-griefing

### Accessibility and Skill Range

- **Skill floor :** très bas — les règles sont oralement connues par la cible
- **Skill ceiling :** haut — le bluff et la lecture sociale se maîtrisent sur des dizaines de parties
- **Luck elements :** distribution des rôles (50% du fun vient de "oh mince je suis loup"), potions qui agissent sans que tu saches les contre-attaques
- **Assist modes :** sous-titres MJ, tutoriel interactif première partie, timer paramétrable (groupes lents peuvent doubler)
- **Child-friendly :** v1 18+ (vocal non modéré), v2+ mode enfants possible (pas de vocal, ambiance light, roadmap)
- **Colorblind :** palette validée avec daltoniens (chiffres + emojis partout, pas juste couleurs)

### Session Length

- **Quick play :** ~30 min (5 joueurs, composition simple)
- **Standard match :** 45-60 min (10-12 joueurs, composition classique)
- **Extended match :** 75-90 min (20-25 joueurs, composition riche avec tous les rôles)
- **Drop-in/out :** non supporté en cours de partie (social deduction casse si nouveau joueur arrive)
- **Pause/resume :** oui via menu pause (timer stop, vocal mute, tout le monde peut reprendre)
- **Party management :** host crée partie via code de salle (6 chiffres), invite via lien ou code, kick possible

---

## Progression and Balance

### Player Progression

**Dans une partie :** pas de progression interne (fidélité au canon) — le joueur reste à son niveau de départ (rôle fixe, pas d'upgrade pendant la game).

**Entre les parties (méta-progression) :**
- **Stats personnelles** : nb parties jouées, taux de victoire par rôle, "style de jeu" détecté (bluffeur, investigateur, survivor, accusateur)
- **Achievements** : "Première victoire en tant que Loup", "Survivre 5 parties d'affilée", "Gagner en tant qu'Ange", etc. (~20 achievements v1)
- **Cosmétiques déverrouillables** : quelques skins avatars / chapeaux gagnés par achievements (pas juste IAP)
- **Leaderboards privés entre amis** (optionnels, désactivables) — pas de classement global public

### Difficulty Curve

**Pas de courbe de difficulté IA** — la difficulté émerge de la composition humaine. Plus on joue avec le même groupe, plus on lit les autres, plus on devient difficile à bluffer.

**Équilibrage des compositions (trancher au playtest) :**
- **5 joueurs** : 1 loup + 1 voyante + 3 villageois (ratio 1:4)
- **8 joueurs** : 2 loups + voyante + sorcière + 4 villageois
- **10 joueurs** : 2-3 loups + voyante + sorcière + chasseur + cupidon + 3-4 villageois
- **15 joueurs** : 3-4 loups + voyante + sorcière + chasseur + cupidon + petite fille + salvateur + maire + ange + 4-5 villageois
- **25 joueurs** : 5-6 loups + tous les rôles spéciaux + ~10 villageois

**Recommendations automatiques au setup** (mais modifiables par le host).

### Economy and Resources

**Pas d'économie in-game** (fidélité au canon) — les "ressources" sont les rôles et leurs pouvoirs :
- **Sorcière** : 2 potions (vie + mort) = 2 usages par partie
- **Salvateur** : protection chaque nuit, contrainte "pas 2 nuits de suite même cible"
- **Chasseur** : 1 balle (usage auto à sa mort)
- **Petite Fille** : observation loups (risque = être vue = mort)

**Méta-économie (hors gameplay) :**
- **Monnaie virtuelle :** "Braises" (earned via play + achat IAP)
- **Achats possibles :** skins avatar, thèmes de village, packs voix MJ alternatifs
- **Pas de pay-to-win** — les achats sont 100% cosmétiques

---

## Level Design Framework

### Level Types

La Veillée **n'a pas de "levels" traditionnels** — le jeu est single-environnement (village) avec des **scénarios de jeu** variables.

**Scénarios (au sens compositions de parties) :**

1. **Classique Débutant** (5-8 joueurs) : compo minimaliste (1-2 loups + voyante + villageois)
2. **Classique Avancé** (8-12 joueurs) : +Sorcière, Chasseur, Cupidon
3. **Riche** (12-18 joueurs) : +Salvateur, Petite Fille, Maire
4. **Marathon XXL** (18-25 joueurs) : composition complète tous rôles v1 + Ange
5. **Héritage** (v2+) : compositions pré-faites typiques de Thiercelieux avec les extensions

### Level Progression

**Pas de niveaux à débloquer progressivement** — toutes les compositions sont disponibles dès la v1 pour les groupes qui savent jouer.

**Progression v1 → v2+ (contenu additionnel) :**
- v1 : 1 environnement (village d'hiver), 10 rôles
- v1.1 (post-launch J+30) : 1 rôle gratuit additionnel (ex : Loup Blanc)
- v2 (6 mois) : Thème "Halloween" (même village relooked) + 3 rôles additionnels
- v3 (12 mois) : Thème "Far West" + 5 rôles additionnels

---

## Art and Audio Direction

### Art Style

**Direction :** Stylisé 3D semi-réaliste, chaleureux et sombre — ambiance "cozy-horror" / "storybook gothique".

**Références visuelles :**
- 📚 Hades (palette contrastée, silhouettes lisibles, atmosphère dramatique)
- 🌲 Don't Starve (ambiance forêt sombre)
- 🎨 Tunic / Hollow Knight (économie de détail, charme)
- 🪵 Alba: A Wildlife Adventure (direction nature + maisons bois)
- 🎥 Squeezie Minecraft Loup-Garou (disposition cercle + socles)

**Palette :**
- Dominantes : orange chaud (feu), bleu nuit profond (ciel), noir (silhouettes loups, nuit absolue)
- Accents : blanc lune/neige, gris brume, vert-sombre forêt
- Mort : gris pierre (tombes), particules blanches (âmes)
- Maire : or (couronne — seule skin publique)

**Avatar design :**
- Neutres, lisibles, customisables sur traits non-révélateurs (pull, coiffure, chapeau, peau)
- Silhouettes distinctives pour reconnaissance rapide
- Animation : respirer, cligner, regarder autour

**Environnement 3D :**
- 1 scène principale (village nuit + feu + socles + chalets + forêt)
- ~10 chalets modulaires positionnés en arc derrière le cercle
- Socles de pierre autour du feu central (8-25 positions)
- Éléments dynamiques : flammes animées, particules, fumée cheminées, skybox jour/nuit

### Audio and Music

**Voix du MJ :**
- Ton : sobre, grave, posé, quelques inflexions dramatiques sur les morts
- Langue : français (v1), anglais (v2)
- Production : ElevenLabs haut de gamme (POC) → potentielle voix pro si budget
- Packs alternatifs (v2+) : drôle / flippant / chill / féminine

**Musique :**
- Ambiance nuit : drones atmosphériques, cordes graves, nappe subtile (Disasterpeace, Stranger Things, Dark Souls ambient)
- Ambiance jour : plus légère, percussions discrètes, accordéon lointain (auberge médiévale)
- Mort : silence ou stinger dramatique court
- Victoire/défaite : thèmes finaux courts (10-15s)
- ~30-60 min de musique totale v1

**SFX :**
- 🔥 Crépitement feu (continu, spatialisé sur le totem)
- 🐺 Hurlements lointains au lever de lune
- 🦉 Hibou, vent, bruissements forêt
- ⏱️ Tic-tac timer (subtil, dernières secondes)
- 💥 Mort : socle craque, particules, vent froid
- 🗳️ Clic de vote : "tap bois" satisfaisant
- ~50-100 SFX courts v1

---

## Technical Specifications

### Performance Requirements

- **Target FPS :** 30 minimum, 60 fps cible (animations cinématiques)
- **Resolution :** adaptive (iPhone 11 → iPhone 15+), max 1440p sur Pro Max
- **Memory footprint :** < 500 MB RAM en jeu
- **App size :** < 500 MB au lancement (v1 launch), < 1 GB avec tous les asset packs
- **Latence vocale :** < 200ms (Agora ou LiveKit)
- **Latence BT Totem-Compagnons :** < 100ms pour commandes (mode Campfire)
- **Cold start :** < 5s du tap icon à l'écran d'accueil

### Platform-Specific Details

**iOS (v1) :**
- iOS 15+
- iPhone 11 minimum (A13 Bionic ou +)
- iPad supporté dès v1 (layout adaptif)
- Apple Sign In, Sign in with Apple
- iCloud sync (cosmétiques, stats, achievements)
- CoreBluetooth pour pairing Totem + Compagnons (mode Campfire)
- Taptic Engine pour haptics
- App Store IAP pour monétisation

**Android (v2) :**
- Android 10+ (API 29+)
- Google Sign In
- Google Play Games Services (achievements, cloud save)
- Bluetooth LE pour pairing (potentielle adaptation vs iOS)
- Google Play Billing pour IAP

### Asset Requirements

**3D Art :**
- 1 environnement village (chalets ~10 modulaires, feu, arbres, terrain, skybox jour/nuit)
- ~25 combinaisons avatars modulaires (têtes + corps + chapeaux + accessoires)
- Animations : idle, parler, vote, mort dramatique, Chasseur tire (~15 animations totales)
- Effets VFX : flammes, particules mort, pluie, brume, flash nuit/jour, halo Maire

**Audio :**
- ~150 lignes de voix MJ (toutes les phases + variantes pour anti-répétition)
- ~5-8 morceaux musicaux (nuit, jour, mort, victoire, défaite, menu)
- ~60-100 SFX
- Mixage pro final

**UI :**
- 1 HUD principal (feed votes, timer, indicateur rôle)
- 1 écran menu / lobby / setup partie
- 1 écran fin de partie (révélation rôles, stats, replay possible)
- 1 écran store IAP
- 1 écran settings + accessibilité
- 1 onboarding tutoriel (skippable pour vétérans)

**Stack backend :**
- Photon Quantum ou Mirror (multijoueur)
- Agora ou LiveKit (vocal)
- Firebase (auth + analytics + cloud save + remote config)
- ElevenLabs API (voix MJ — mode streaming si budget le permet, sinon bundled assets)

---

## Development Epics

### Epic Structure (suggestion pour planning dev)

**Epic 1 — Core Architecture**
Build the foundation. Sprint sur :
- Unity setup iOS + CI/CD
- Auth Firebase + compte utilisateur minimal
- Backend multijoueur (Photon/Mirror) pour créer et rejoindre une room
- Stack vocal Agora/LiveKit intégré avec sync room
- Mode Remote bare-bones (sans UI fancy, juste pour valider la stack)

**Epic 2 — Gameplay Loop MVP**
Le jeu doit tourner, même feu de camp + 5 joueurs, même règles 5 rôles.
- Distribution aléatoire de rôles privés
- Loop jour/nuit avec timers
- Vote public temps réel + feed + modifiable
- Actions nocturnes (5 rôles basiques : Villageois, Loup, Voyante, Sorcière, Chasseur)
- Win/loss conditions + détection fin de partie
- Salon des morts basique

**Epic 3 — Art Direction Playable**
Le jeu devient cinématique.
- Environnement village + feu + socles + chalets (scène Unity finalisée)
- Avatars modulaires customisables + animations idle/parler/vote
- Mort dramatique (animation socle + tombe)
- Transitions jour/nuit cinématiques
- Écran noir = yeux fermés (dim screen par rôle non actif)

**Epic 4 — MJ Audio + Ambiance**
La voix prend vie.
- Script complet MJ (~150 lignes) écrit
- Génération voix ElevenLabs validée par écoute collective
- Musique nuit + jour + morts commandée et intégrée
- SFX ambiants intégrés (feu, forêt, hurlements)
- Sync audio avec phases de jeu

**Epic 5 — Mode Campfire**
Le mode signature.
- POC BT Totem-Speaker (iOS CoreBluetooth)
- Pairing entre 1 totem + N compagnons
- Totem-side : juste ambiance audio + écran sombre immobile
- Compagnons-side : UI privée (rôle + action + vote)
- Sync d'état multi-appareils via BT

**Epic 6 — Extensions Rôles v1 (5 rôles manquants)**
Compléter le catalogue canonique.
- Cupidon + amoureux
- Petite Fille
- Salvateur
- Maire (élection + skin couronne + vote x2)
- Ange

**Epic 7 — Setup Social + Onboarding**
L'entrée dans le jeu doit être friction-zéro.
- Lobby de création de partie (code 6 chiffres + lien partage)
- Écran setup composition de rôles (recommandations + custom)
- Écran setup timers (3/5 min par défaut + ajustable)
- Onboarding tutoriel interactif (skippable)
- Invitations via deeplink

**Epic 8 — Polish + Accessibility + Meta**
Finitions pour le launch.
- Stats post-partie (rôles joués, win rate, tendances)
- Achievements v1 (~20)
- Cosmétiques avatar (10 skins de base inclus)
- Settings + accessibilité (sous-titres MJ, daltonisme, timer audio)
- Sons de feedback (haptic + SFX votes)
- App Store listing + ASO FR

**Epic 9 — Monetization + Cloud**
Pour la pérennité.
- IAP cosmétiques (packs skins, thèmes, voix MJ)
- iCloud sync stats / cosmétiques
- Remote config (balance / feature flags)
- Analytics Firebase (retention, flow, crash)

**Epic 10 — QA + Beta + Launch**
Prêt à sortir.
- Playtest alpha fermée (50 personnes) : équilibrage + bugs
- Beta publique limitée (500 personnes) : polish + serveurs
- Soumission App Store + processus de validation
- Plan marketing : partenariats créateurs, presse, fiche ASO
- Launch day + monitoring

---

## Success Metrics

### Technical Metrics

- **Crash-free rate :** ≥ 99.5%
- **FPS moyen :** ≥ 55 fps sur iPhone 12+, ≥ 30 fps sur iPhone 11
- **App size final :** < 500 MB (v1), < 1 GB avec packs optionnels
- **Cold start :** < 5s
- **Latence vocale p95 :** < 250 ms
- **Latence BT Campfire p95 :** < 150 ms
- **Taux d'erreur matchmaking :** < 1%
- **Uptime backend :** ≥ 99.5%

### Gameplay Metrics

**Acquisition :**
- Launch J+30 : 5 000 téléchargements iOS France
- Launch J+90 : 25 000 téléchargements iOS France + DACH
- Launch J+180 : 100 000 téléchargements multi-marchés francophones

**Engagement :**
- D1 retention ≥ 40%
- D7 retention ≥ 20%
- D30 retention ≥ 10%
- Session moyenne ≥ 35 min (= 1 partie entière)
- Parties/session ≥ 1.5
- Taux d'achèvement de partie ≥ 85%

**Qualité perçue :**
- App Store rating ≥ 4.3/5
- Reviews mentionnant positivement "voix MJ" OU "ambiance" ≥ 70%
- NPS ≥ 40

**Communauté :**
- ≥ 50 vidéos TikTok/YouTube mentionnant le jeu au J+90
- 3-5 créateurs francophones qui y jouent régulièrement
- Featured Apple "New Games We Love" France (objectif cible)

**Financier :**
- Break-even 12 mois post-launch
- Marge nette ≥ 20% en year 1
- ARPPU ≥ 4€ (via IAP cosmétiques)
- Taux de conversion free → paying ≥ 3%

---

## Out of Scope

**Explicitement exclu de la v1 :**

- ❌ Plateformes : Android, PC, console, web, VR (Android en v2)
- ❌ Rôles additionnels : Loup Blanc, Infect Père, Joueur de Flûte, Ancien, Renard, Idiot, Voleur, Enfant Sauvage, Grand Méchant Loup, Servante Dévouée, etc. (roadmap rôles)
- ❌ Thèmes de village alternatifs (Halloween, Far West, médiéval-dark) — roadmap
- ❌ Voix MJ alternatives (drôle / flippant / chill / féminine) — roadmap IAP
- ❌ Mode enfants (rôles simplifiés, pas de vocal)
- ❌ Mode teambuilding corpo
- ❌ Missions collectives / mécaniques TV-show ajoutées (The Traitors)
- ❌ Features virales (highlight reels auto, streaming, leaderboards publics)
- ❌ Mode hybride Campfire + Remote mixte (exclusivité au setup)
- ❌ Matchmaking entre inconnus (pas en v1 — le jeu est "entre potes")
- ❌ IA qui apprend les joueurs / profilage comportemental
- ❌ Chat écrit global pendant la partie
- ❌ Plusieurs parties parallèles du même joueur (une partie à la fois)
- ❌ Mode spectateur pour non-joueurs externes
- ❌ Replay video de parties
- ❌ Friend system complexe (seulement invites par lien/code en v1)

---

## Assumptions and Dependencies

### Assumptions (à valider)

1. **Les utilisateurs iOS cible sont tolérants à une app 3D de 500 MB** pour un party game, si l'expérience le justifie.
2. **Les groupes de cible savent déjà jouer au Loup-Garou** — onboarding minimal possible.
3. **Le vocal intégré est assez bon** sans nécessiter Discord en parallèle (Agora/LiveKit à benchmarker).
4. **Le Bluetooth LE iOS est fiable** pour sync totem + compagnons dans une pièce (POC requis tôt).
5. **La voix ElevenLabs est "assez bonne"** pour passer pro-grade, au moins en v1 (sinon voix pro à budgeter).
6. **Le marché francophone prioritaire suffit** pour atteindre le break-even (pas besoin d'international en year 1).
7. **Les joueurs acceptent la friction "1 tél host qui prête son appareil"** en mode Campfire.
8. **Le respect strict du canon Thiercelieux est un différenciateur suffisant** (pas besoin d'innover mécaniquement).

### Dependencies (critiques)

**Techniques :**
- Unity 2023 LTS + iOS build toolchain
- Apple Developer Program (99€/an)
- Photon Quantum ou Mirror (licence si au-delà du free tier)
- Agora SDK ou LiveKit (coût au MAU)
- Firebase (auth + analytics + cloud save)
- ElevenLabs (voix — option principale)
- Unity Asset Store packs (accélération art)

**Humaines :**
- Développeur Unity mobile (senior recommandé)
- Artiste 3D freelance (avatars + animations)
- Artiste concept / UI designer freelance
- Compositeur sound design freelance
- (Optionnel) Comédien voix pro francophone

**Business :**
- Budget estimatif : 15-30 k€ pour v1 premium (fourchette indie)
- Accès créateurs francophones pour partenariats pré-launch
- Contact App Store relations si possible (featured)

### Risques résiduels post-launch

- **Coût vocal à scale** qui explose si 100k+ DAU (plan B : self-hosted WebRTC)
- **Backlash si composition déséquilibrée** au lancement (mitigation : playtest poussé + patch rapide)
- **Concurrent qui copie le mode Campfire** si succès visible (mitigation : first mover + ambiance/voix difficiles à répliquer)
- **App Store moderation** sur le vocal (mitigation : signalement + modération v1)

---

_Document finalisé — GDD La Veillée v1.0 — 2026-04-20_
_Prochaine étape recommandée : `/bmad-create-epics-and-stories` ou `/gds-create-epics-and-stories` pour détailler chaque Epic en user stories exécutables._
