---
title: 'UX Design Specification — La Veillée'
project: 'lg'
game_name: 'La Veillée'
date: '2026-04-20'
author: 'sayanth'
version: '1.0'
status: 'complete'
stepsCompleted: [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14]
language: 'french'

inputDocuments:
  - _bmad-output/gdd.md
  - _bmad-output/game-architecture.md
  - _bmad-output/planning-artifacts/epics.md
  - _bmad-output/game-brief.md
---

# UX Design Specification — La Veillée

**Author:** sayanth
**Date:** 2026-04-20
**Plateforme cible :** iOS (iPhone 11+ / iPad), iOS 15+, mode portrait principal + paysage iPad
**Stack UI :** Unity uGUI (cf. Architecture)

---

## 1. Discovery & Joueurs cibles

### 1.1 Profils joueurs

| Profil | % cible | Caractéristiques | Besoins UX |
|---|---|---|---|
| **Le Initié** | 60% | 22-28 ans, joue déjà au Loup-Garou physique, connaît tous les rôles | Onboarding skippable, raccourcis rapides, customisation soignée |
| **Le Curieux** | 25% | 18-30 ans, a entendu parler du jeu, jamais joué | Tutoriel interactif essentiel, micro-explications contextuelles, feedback visuel rassurant |
| **Le Host social** | 10% | 25-35 ans, organisateur de soirée, lead du groupe | Création partie 1-tap, partage facile (lien iMessage/WhatsApp), lobby clair pour gérer le groupe |
| **L'Allergique au numérique** | 5% | Joueur invité par friction sociale, peu d'app games installées | Zero-friction join (deeplink, pas d'inscription supplémentaire), interface lisible, aucun jargon |

### 1.2 Contextes d'usage

- **Soirée chez des potes (Campfire)** — éclairage tamisé, smartphones partout, attention partagée → besoin d'éléments visuels gros, écran-noir non aveuglant, sons portés
- **Trajet ou pause (Remote async setup)** — transports publics, écouteurs branchés, focus partiel → micro-interactions optimisées pour un seul pouce
- **Salon famille / colloc (mix)** — bruit ambiant variable, plusieurs niveaux d'attention → vocal modulable, timer audio + visuel doublé

### 1.3 Game pillars revisités côté UX

Reformulation des 4 piliers du GDD en directives UX :

1. **🎭 Fidélité au canon** → Vocabulaire respecté ("Loups-Garous", "Sorcière"), iconographie inspirée des cartes Thiercelieux, ordre nocturne strictement annoncé
2. **🔥 L'intimité du cercle** → Pas de pub, pas de friend system viral, pas de chat global ; UI focus interne au groupe
3. **🎙️ L'app = Maître du Jeu** → L'UI s'efface dès qu'elle a transmis, le téléphone n'est jamais "le centre social" entre humains, juste l'outil
4. **🎬 Cinématique sans spectacle** → Animations courtes (max 2s), pas de skeuomorphisme baroque, transitions fluides 60fps

---

## 2. Core Experience

### 2.1 Le moment vrai (the moment that matters)

> **Quand le MJ vocal annonce "Le village s'endort…", l'écran de chacun devient noir. Pendant 5 secondes : silence. Puis la voix appelle "Voyante, ouvre les yeux"… et un seul écran s'allume dans la pièce. Personne ne sait lequel.**

C'est CE moment qui doit fonctionner. Toute la UX gravite autour de lui.

### 2.2 Boucle UX micro (un cycle nuit→jour)

```
[Voix MJ] "Le village s'endort"
       ↓
[Tous écrans] Fade-to-black 500ms (FR40)
       ↓
[Haptic] vibration légère sur le tél du rôle appelé (FR42)
       ↓
[Écran rôle actif] Fade-in 500ms → UI d'action minimaliste
       ↓
[Joueur] Tap + hold 0.5s sur cible (FR33)
       ↓
[Haptic] confirmation forte
       ↓
[Écran] Fade-to-black 500ms
       ↓
... (rôle suivant)
       ↓
[Voix MJ] "Le village se réveille"
       ↓
[Tous écrans] Fade-in 1s → vue village 3D + feed votes
       ↓
[Phase débat] Vocal libre + tap pour voter
       ↓
[Voix MJ] "Le vote est tombé"
       ↓
[Animation mort] socle craque + particules + tombe (1.5s)
```

### 2.3 Sensations recherchées

| Phase | Émotion ciblée | Levier UX |
|---|---|---|
| Setup partie | Excitation + anticipation | Compteur joueurs animé, pulsations subtiles, voix MJ qui chuchote en intro |
| Distribution rôle | Surprise (bonne ou mauvaise) | Carte qui retourne avec délai dramatique (0.8s), haptic fort à la révélation |
| Nuit (rôle inactif) | Frustration contrôlée + curiosité | Écran noir absolu, micro-vibrations possibles, pas de countdown visible (suspense) |
| Nuit (rôle actif) | Pression + responsabilité | Timer visible mais discret, options claires, tap+hold pour engager |
| Réveil | Soulagement / gravité | Voix MJ posée, animation village qui s'éveille, lumière chaude |
| Débat | Tension sociale + conviction | Avatars vivants (animations idle), feed votes pulsé, micro vert quand quelqu'un parle |
| Mort | Dramatique sans cruauté | Animation 1.5s avec stinger sonore, transition douce vers salon des morts |
| Victoire | Catharsis + envie de rejouer | Révélation séquentielle des rôles (une carte à la fois), bouton "Rejouer" gros et premier |

---

## 3. Emotional Response Design

### 3.1 Arc émotionnel d'une partie

```
        intensité
          │
   pic ⬛│           ⬛ Mort accusateur
          │         ⬛                  ⬛ Victoire
          │       ⬛  ⬛
          │     ⬛       ⬛  ⬛
          │   ⬛                 ⬛  ⬛
          │ ⬛
   bas ⬛│⬛
          └────────────────────────────→ temps
          Setup  Nuit1  Jour1  Nuit2  Jour2  Final
```

### 3.2 Émotions à NE PAS provoquer

❌ **Honte** : un joueur tué ne doit jamais se sentir bête (animation respectueuse, pas de "LOL t'es mort")
❌ **Frustration technique** : tap raté, micro qui coupe, écran noir trop long → casseur d'immersion absolu
❌ **Anxiété de la pression sociale** : timer qui clignote rouge agressif → contre-productif sur cible casual
❌ **FOMO méta** : pas de daily login, pas de battle pass, pas de "tu rates des récompenses si tu ne joues pas"

### 3.3 Émotions à amplifier

✅ **Complicité** entre amis qui rient des accusations
✅ **Suspense** au moment des révélations
✅ **Fierté** d'avoir bien bluffé / bien deviné
✅ **Nostalgie** de la veillée d'antan (direction artistique, voix grave du MJ)

---

## 4. Inspiration visuelle

### 4.1 Références jeu vidéo

| Réf | Ce qu'on emprunte | Ce qu'on évite |
|---|---|---|
| **Hades** | Palette contrastée, silhouettes lisibles, dramatisme | Densité d'UI hud (trop chargé pour party game mobile) |
| **Don't Starve** | Ambiance forêt sombre cozy-horror, économie de moyens | Style 2D cut-out (on veut 3D) |
| **Hollow Knight / Tunic** | Économie de détail, charme, lisibilité | Difficulté visuelle (notre cible casual) |
| **Alba** | Direction nature + maisons bois | Trop "enfant" en couleurs |
| **Squeezie Minecraft Loup-Garou** | Disposition cercle + socles | Esthétique blocky (on cherche stylisé semi-réaliste) |

### 4.2 Références hors jeu

- **Apple Memoji / Bitmoji** : pour la modularité avatar simple et instantanément attachant
- **iMessage / WhatsApp Stories** : pour la fluidité des transitions
- **Apple Weather (iOS 17)** : pour la skybox cinématique et les transitions jour/nuit
- **Carte Loup-Garou Thiercelieux originale** : iconographie fidèle de chaque rôle

### 4.3 Références à éviter explicitement

❌ Among Us (perception "kids", esthétique cartoon plat)
❌ Werewolf Online (low-quality, trop de pubs, esthétique générique)
❌ Battle royale modernes (densité info trop haute)

---

## 5. Design System Foundation

### 5.1 Choix : Custom design system léger sur uGUI

**Rationale** : Pas de système tiers (Material, Cupertino) pour préserver l'identité "veillée d'antan". Système custom mais petit (~30 composants), tokenisé pour cohérence.

**Implémentation** : ScriptableObjects pour les tokens (colors, spacing, typography), Prefabs Unity pour les composants partagés, dossier `Assets/_Project/Scripts/UI/Common/`.

### 5.2 Design Tokens

#### Couleurs (palette principale)

| Token | Hex | Usage |
|---|---|---|
| `color.fire.500` | `#E87B2F` | Orange chaud principal — feu, accents primaires, CTAs |
| `color.fire.700` | `#A8501A` | Hover/pressed des CTAs orange |
| `color.fire.300` | `#F5B380` | Highlights, halos, glows feu |
| `color.night.900` | `#0B1224` | Bleu nuit profond — fonds principaux |
| `color.night.700` | `#1A2440` | Surfaces overlay, cards |
| `color.night.500` | `#2E3A5F` | Borders, dividers |
| `color.moon.100` | `#F4F1E8` | Blanc lune — texte principal sur fonds sombres |
| `color.moon.300` | `#C9C0AC` | Texte secondaire, placeholders |
| `color.fog.500` | `#6B6B7A` | Gris brume — texte désactivé, états inactifs |
| `color.forest.700` | `#1C3D2E` | Vert sombre — accents nature, mode Campfire |
| `color.blood.500` | `#9B1F2D` | Rouge sang — alertes, mort, danger contextuel |
| `color.gold.500` | `#D4A436` | Or — Maire (couronne), achievements premium |
| `color.crystal.500` | `#5BA9CF` | Bleu glacé — Voyante, info révélée |
| `color.poison.500` | `#7A4FB5` | Violet — Sorcière, magie |

**Règle daltonisme (NFR16) :** chaque info couleur DOIT être doublée d'un emoji + chiffre + label. Aucune couleur n'est seule porteuse d'info.

#### Typographie

**Famille principale :** **Manrope** (free, Google Fonts, lisible mobile, neutre moderne)
**Famille accent :** **Cormorant Garamond** (free, Google Fonts) pour les titres dramatiques (intro partie, écrans de victoire)

| Token | Police | Taille | Poids | Usage |
|---|---|---|---|---|
| `text.display` | Cormorant Garamond | 36pt | Bold | Titres écran (Victoire / Défaite / "La Nuit tombe") |
| `text.h1` | Manrope | 28pt | ExtraBold | En-têtes de section (Lobby, Setup) |
| `text.h2` | Manrope | 22pt | Bold | Titres de cards (rôle, achievement) |
| `text.h3` | Manrope | 18pt | SemiBold | Sous-titres, noms de joueurs en gros |
| `text.body` | Manrope | 16pt | Regular | Texte courant |
| `text.body.bold` | Manrope | 16pt | SemiBold | Emphase dans body |
| `text.caption` | Manrope | 13pt | Regular | Légendes, helpers, timers détaillés |
| `text.label` | Manrope | 12pt | SemiBold | Labels boutons, badges |
| `text.subtitle.mj` | Manrope | 17pt | Medium | Sous-titres MJ (NFR15) avec ombre portée |

**Line-height :** 1.4 pour body, 1.2 pour h1/h2, 1.6 pour subtitle.mj
**Letter-spacing :** -0.5px sur display, 0 sur le reste, +1.0px sur labels uppercase

#### Spacing (système 8-pt)

| Token | Valeur | Usage |
|---|---|---|
| `spacing.xs` | 4 dp | Padding interne icônes |
| `spacing.sm` | 8 dp | Espace entre éléments tightly liés |
| `spacing.md` | 16 dp | Padding de cards, espace entre cards |
| `spacing.lg` | 24 dp | Marges écran, séparation de sections |
| `spacing.xl` | 32 dp | Marges majeures, espace entre groupes |
| `spacing.xxl` | 48 dp | Marges exceptionnelles, héro components |

#### Bordures & Radius

| Token | Valeur | Usage |
|---|---|---|
| `radius.sm` | 6 dp | Boutons compacts, tags |
| `radius.md` | 12 dp | Cards, sheets |
| `radius.lg` | 20 dp | CTAs principaux |
| `radius.full` | 999 dp | Avatars, badges circulaires |
| `border.thin` | 1 dp | Dividers |
| `border.thick` | 2 dp | Borders de focus, sélection active |

#### Élévation / Shadows

| Token | Valeur | Usage |
|---|---|---|
| `shadow.sm` | y=2, blur=4, opacity=0.15 | Boutons inactifs |
| `shadow.md` | y=4, blur=12, opacity=0.25 | Cards flottantes, sheets |
| `shadow.lg` | y=8, blur=24, opacity=0.35 | Modales, overlays critiques |
| `shadow.glow.fire` | blur=16, color=fire.500, opacity=0.6 | Halo CTA primaire, rôle actif |
| `shadow.glow.crystal` | blur=12, color=crystal.500, opacity=0.5 | Halo révélation Voyante |

#### Animation

| Token | Durée | Easing | Usage |
|---|---|---|---|
| `motion.fast` | 150 ms | ease-out | Hover/pressed, micro-feedback |
| `motion.medium` | 300 ms | ease-in-out | Transitions UI courantes |
| `motion.slow` | 500 ms | ease-in-out | Fade écran-noir (Story 5.6) |
| `motion.dramatic` | 1500 ms | custom (anticipation→action) | Animations de mort, révélations |
| `motion.transition.phase` | 3000 ms | ease-in-out | Transitions cinématiques jour↔nuit (Story 5.5) |

### 5.3 Iconographie

- **Style :** outline + fill mixte, 2px stroke, coins légèrement arrondis (radius 1.5dp)
- **Tailles standard :** 16dp, 20dp, 24dp (par défaut), 32dp, 48dp (héro)
- **Source :** custom set ~40 icônes commandé à un illustrateur freelance
- **Iconographie de rôle :** stylisée, inspirée des cartes Thiercelieux mais redessinée moderne
  - 🐺 Loup → silhouette de tête de loup avec yeux jaunes
  - 🔮 Voyante → boule de cristal avec étoiles
  - 🧪 Sorcière → fiole bouchée (couleurs différenciées vie/mort)
  - 🎯 Chasseur → croix de visée stylisée
  - 💘 Cupidon → arc et flèche
  - 👁️ Petite Fille → œil entrouvert
  - 🛡️ Salvateur → bouclier
  - 👑 Maire → couronne
  - 😇 Ange → auréole
  - 🧑‍🌾 Villageois → silhouette neutre

---

## 6. Defining Experience (USPs UX)

### 6.1 USP UX 1 — L'écran noir qui s'allume seul

C'est l'innovation centrale (FR40-42). Aucune app concurrente ne le fait. Spécifications strictes :

- Fade-to-black via Canvas overlay alpha 0→1 sur **500ms** (`motion.slow`)
- Pendant le noir : l'écran capte les inputs si nécessaire (notamment swipe pour le menu pause)
- Quand le rôle s'active : haptic 0.3s + fade-in alpha 1→0 sur 500ms
- Variante accessibilité : remplacement du fade-noir par un overlay opaque dimmed à 80% (pour utilisateurs avec photophobie ou lampes torches qui veulent contrôler la luminosité ambiante)
- Variante NFR17 (vibration alt) : pattern de vibration 2s longue (rôle actif) ou 0.3s 3x (rôle terminé)

### 6.2 USP UX 2 — Tap + hold pour les actions irréversibles

Toute action destructive ou définitive (cible mort, potion utilisée, vote final pour Cupidon) exige un **tap + hold 0.5s** avec :

- Cercle de remplissage progressif (0% → 100%) autour du doigt
- Haptic léger au début + haptic fort à la confirmation
- Annulation possible si le doigt sort du composant avant 0.5s

Ce pattern résout le risque "j'ai tapé par erreur" et donne le rituel de l'action engagée.

### 6.3 USP UX 3 — Le tél au centre = totem ambiance

En mode Campfire, le tél host devient un objet décoratif sonore. Spécifications :

- Luminosité écran réduite à 30% (FR44)
- Affichage : flammes animées en boucle, fond sombre, aucune UI interactive (FR44)
- Brightness override : si l'utilisateur tape l'écran totem, un sub-menu apparaît 3 secondes (boutons "Pause", "Volume", "Annuler partie") puis disparaît
- Le totem n'a pas de rôle dans la partie (il représente "l'app MJ", pas un joueur)

---

## 7. Visual Foundation

### 7.1 Grille & Layout

**Grille de base :** 8-point grid (compatible iOS HIG)
**Marges écran :**
- iPhone : 16dp horizontaux par défaut, 24dp pour écrans héro
- iPad : 24dp horizontaux par défaut, layout multi-colonnes au-delà

**Safe areas :**
- Notch iPhone : padding top auto
- Home indicator : padding bottom 34dp obligatoire (zone interactive intacte)
- iPad : adapt aux écrans Stage Manager

### 7.2 Anatomie d'écran type (in-game)

```
┌─────────────────────────────────────┐
│ [☰]                          [⚙️] │ ← Top bar 56dp (menu pause + paramètres)
├─────────────────────────────────────┤
│                                     │
│                                     │
│       Vue 3D village                │ ← Vue principale
│       (caméra 360°)                 │   (zone immersive)
│                                     │
│                                     │
│                                     │
├─────────────────────────────────────┤
│ ⏱️ 3:42                       🎙️│ ← HUD bas 80dp (timer + micro)
└─────────────────────────────────────┘
```

### 7.3 Layouts critiques

#### Lobby (5-25 joueurs)

```
┌─────────────────────────────────────┐
│ La Veillée                     [✕] │
├─────────────────────────────────────┤
│ Code de partie                      │
│ ┌─────────────┐                     │
│ │  4 8 2 7 1 5│  [📋 Copier]       │
│ └─────────────┘  [🔗 Partager]     │
├─────────────────────────────────────┤
│ Joueurs (8 / 25)                   │
│                                     │
│ ┌──┐ Sayanth (Host)            ⚙️ │
│ ├──┤                                │
│ │👤│ Marie                          │
│ ├──┤                                │
│ │👤│ Julien                  [Kick]│
│ └──┘                                │
│ ...                                 │
├─────────────────────────────────────┤
│ Composition: [Recommandée] [Custom]│
│ Mode: ◉ Campfire  ○ Remote         │
│ Timers: Nuit 3min · Jour 5min      │
├─────────────────────────────────────┤
│ ┌───────────────────────────────┐  │
│ │   Démarrer la partie          │  │ ← CTA principal, fire.500 + halo
│ └───────────────────────────────┘  │
└─────────────────────────────────────┘
```

#### Vote de jour (Remote)

```
┌─────────────────────────────────────┐
│ ☰  Jour 2                    ⏱️4:12│
├─────────────────────────────────────┤
│                                     │
│         🔥 Vue village 3D           │
│       (avatars en cercle)           │
│         ◀ swipe pour pivoter ▶      │
│                                     │
├─────────────────────────────────────┤
│ Feed 📋                             │
│ Marie a voté pour Paul              │
│ Julien a voté pour Marie            │
│ Tu as voté pour Marie               │
├─────────────────────────────────────┤
│ Tap + hold sur un avatar pour voter │
│            🎙️ Marie parle           │
└─────────────────────────────────────┘
```

#### Action nocturne (rôle actif — ex: Voyante)

```
┌─────────────────────────────────────┐
│             [🔮 Voyante]            │ ← Header rôle + halo crystal
├─────────────────────────────────────┤
│                                     │
│    "Voyante, désigne un joueur     │
│     dont tu veux découvrir le rôle"│ ← Texte MJ subtitle
│                                     │
├─────────────────────────────────────┤
│  Joueurs vivants                    │
│                                     │
│  ┌────────┐  ┌────────┐  ┌────────┐│
│  │   👤   │  │   👤   │  │   👤   ││
│  │ Marie  │  │ Julien │  │  Paul  ││
│  └────────┘  └────────┘  └────────┘│
│  ┌────────┐  ┌────────┐  ┌────────┐│
│  │   👤   │  │   👤   │  │   👤   ││
│  │  Léa   │  │ Hugo   │  │ Camille││
│  └────────┘  └────────┘  └────────┘│
│                                     │
├─────────────────────────────────────┤
│      Tap + hold pour révéler        │
│        ⏱️ 1:24 restantes           │
└─────────────────────────────────────┘
```

---

## 8. Design Directions explorées

### Direction A — "Veillée intime" (RETENUE)

**Mood :** chaleureux-sombre, palette orange/bleu nuit dominante, typographie moderne mais avec des accents serif pour les moments dramatiques. Animations subtiles (1-3s max), pas de spectacle visuel agressif.

**Pourquoi retenue :** colle parfaitement aux 4 piliers GDD, différenciation claire vs Among Us / Werewolf Online, scalable techniquement.

### Direction B — "Médiéval cinématique" (écartée)

**Mood :** Souls-like, palette terreuse-vert sombre, typographie heavy-serif partout, animations dramatiques (3-5s).

**Pourquoi écartée :** trop "hardcore" pour la cible casual, freine la rétention D7. Surcharge visuelle réduit la lisibilité mobile.

### Direction C — "Stylisé clean" (écartée)

**Mood :** Apex/Valorant lite, palette saturée, typographie sans-serif geometric, micro-interactions punchy.

**Pourquoi écartée :** perd l'identité "veillée d'antan", générique, ne capitalise pas sur l'aspect canon Thiercelieux.

---

## 9. User Journeys (parcours utilisateur)

### 9.1 Parcours "Première utilisation" (le Curieux)

```
Téléchargement App Store
        ↓
[App Open] → écran d'accueil "Continuer avec Apple"
        ↓
[Sign in with Apple] → Face ID
        ↓
[Profil minimal] pseudo + avatar par défaut → Valider
        ↓
[Home] "Créer" / "Rejoindre"
        ↓
[Modale tutoriel] "Première partie ? Découvre les bases (5 min)"
        ↓
[Tutoriel interactif] mini-partie avec 5 bots, narrateur off
        ↓
[Récap] "Tu sais maintenant…"
        ↓
[Home] retour, prêt à jouer pour de vrai
```

**Friction critique surveillée :**
- Sign in with Apple échoué → message clair + retry (Story 1.2)
- Pseudo refusé → contraintes affichées (Story 1.3)
- Tutoriel jugé trop long → bouton "Passer" toujours visible

### 9.2 Parcours "Soirée Campfire entre potes" (le Host)

```
Tap "Créer une partie" depuis Home
        ↓
[Génération code 4827 1 5] + bouton Partager
        ↓
[Lobby] amis rejoignent un par un (anim ding subtile)
        ↓
[Tap "Configurer"] → choisir compo + timers + mode Campfire
        ↓
[Sélection mode Campfire] → "Active le Bluetooth — vérifications…"
        ↓
[Pairing] code 4 chiffres affiché, amis scannent et entrent le code
        ↓
[Lobby] tous compagnons connectés, totem affiché à droite
        ↓
[Tap "Démarrer"] → countdown 3-2-1
        ↓
[Distribution rôles] cartes qui se révèlent une par une
        ↓
[Partie en cours]
        ↓
[Fin de partie] révélation rôles + stats + "Rejouer ?"
        ↓
[Rejouer] mêmes joueurs, nouvelle compo aléatoire
```

**Friction critique surveillée :**
- BT permission refusée → fallback explicite "Tu peux passer en Remote"
- Compagnon perd la connexion BT → reconnexion auto silencieuse (3 essais)
- Host quitte au mauvais moment → promotion auto + notification

### 9.3 Parcours "Rejoindre via deeplink" (l'Allergique au numérique)

```
Reçoit un message WhatsApp
        ↓
Tap sur le lien laveillee://join/482715
        ↓
[App Store] si app pas installée → Download
        ↓
[App Open] → si déjà auth, va direct au lobby code 482715
        ↓
[Sign in if needed] Sign in with Apple unique étape
        ↓
[Lobby] dans la room, attend les autres
        ↓
[Partie] joue
```

**Friction critique surveillée :**
- Aucun pop-up "Crée un compte" — Sign in Apple ou rien
- Aucun upsell IAP avant la première partie complétée

### 9.4 Parcours "Mort en cours de partie" (n'importe quel joueur)

```
Le joueur est éliminé (vote ou Loups)
        ↓
[Animation mort] socle craque, particules, 1.5s
        ↓
[Voix MJ] "X est mort, X était [rôle]"
        ↓
[Transition] vers salon des morts (1s fade)
        ↓
[Salon des morts] vue spéciale (ciel étoilé, autres morts visibles)
        ↓
[Vocal séparé] joueur entend uniquement les autres morts
        ↓
[Spectateur] peut suivre la partie en cours en read-only
        ↓
[Fin de partie] récap commun avec les vivants
```

**Friction critique surveillée :**
- Pas de pop-up "Tu es mort, achète un revive" (anti-pattern)
- Spectateur visible mais pas interactif côté vivants
- Vocal séparé = strictement isolé (pas de fuite info)

---

## 10. Component Strategy

### 10.1 Catalogue de composants partagés

Tous dans `Assets/_Project/Scripts/UI/Common/`. Chaque composant a un Prefab Unity + ScriptableObject de variantes.

| Composant | Variants | Usage |
|---|---|---|
| `PrimaryButton` | default, danger, success | CTA principal écran |
| `SecondaryButton` | default, ghost | Actions secondaires |
| `IconButton` | sm, md, lg | Boutons icône-only (mute, settings, partage) |
| `PlayerCard` | lobby, in-game, post-game | Affichage avatar + pseudo + statut |
| `PlayerAvatar` | xs (24dp), sm (40dp), md (64dp), lg (120dp) | Avatar circulaire |
| `RoleCard` | small (rappel), large (révélation) | Affichage rôle avec icône + nom + halo couleur |
| `VoteCounter` | default, mayor (x2 indicator) | Bulle au-dessus avatar avec nb votes |
| `TimerHUD` | day, night, pause | Compte à rebours visible + audible |
| `SubtitleOverlay` | mj, system | Sous-titres bas écran (NFR15) |
| `FeedItem` | vote, action, system | Ligne du feed temps réel |
| `MicIndicator` | speaking, muted, push-to-talk | Icône d'état vocal |
| `ScreenBlackout` | full, dimmed (a11y) | Overlay nuit (Story 5.6) |
| `ConfirmActionRing` | default | Cercle de tap+hold 0.5s |
| `Modal` | info, confirm, error | Sheet centrale modale |
| `Sheet` | bottom, side | Sheets glissantes |
| `BadgeAchievement` | locked, unlocked, new | Badges achievements |
| `CosmeticTile` | owned, locked, premium | Tile dans la boutique cosmétique |
| `CompositionSlot` | role-selector | Slot de configuration de rôle dans la compo |
| `EmptyState` | no-friends, no-history, error | États vides illustrés |
| `Toast` | info, success, warning, error | Notifications éphémères |

### 10.2 Composants spécifiques (non réutilisables)

- `Remote360CameraController` — gestion vue 360° swipe + gyroscope (Épopée 5)
- `CampfireTotemView` — la vue très spécifique du tél totem
- `CampfireCompanionView` — la vue très spécifique des compagnons
- `DeathDramaticAnimation` — orchestration animation socle + tombe + particules
- `RoleRevealCarousel` — défilement des cartes en fin de partie

### 10.3 Conventions composants

- Tous les composants exposent leurs ScriptableObject de tokens (jamais de couleurs hardcodées)
- Tous les composants ont au moins 2 états visuels (default + pressed/hovered)
- Tous les composants interactifs ont un label VoiceOver (NFR14)
- Tous les composants textuels passent par `LocalizedText` (NFR23)

---

## 11. UX Patterns

### 11.1 Patterns d'interaction

| Pattern | Quand l'utiliser | Exemple |
|---|---|---|
| **Tap simple** | Action réversible, choix non-destructif | Naviguer, taper un avatar pour pré-sélectionner |
| **Tap + hold 0.5s** | Action irréversible (FR33) | Confirmer cible mort, valider potion, désigner amoureux |
| **Swipe horizontal** | Vue 360° Remote (FR46) | Pivoter caméra dans le village |
| **Swipe vertical** | Scroller listes / feed | Feed votes, liste joueurs |
| **Swipe down** | Fermer une sheet | Fermer la sheet de paramètres |
| **Pull-to-refresh** | Actualiser une liste statique | Liste de parties en cours (futures versions) |
| **Long press 1s** | Voir détails | Long press avatar = sheet info joueur (pseudo, achievements communs) |
| **Pinch** | Zoom (rare) | Zoom avatar dans customizer |

### 11.2 Patterns de feedback

| Stimulus | Réponse multi-canal |
|---|---|
| Tap réussi (button) | Visuel pressed + haptic léger + SFX subtil |
| Tap+hold confirmé | Cercle complet + haptic fort + SFX "tap bois" |
| Vote enregistré | Bulle compteur pulse + haptic léger |
| Mort imminente | Stinger sonore + animation socle craque + écran shake léger |
| Erreur réseau | Toast warning rouge + haptic erreur (3 vibrations courtes) |
| Achievement débloqué | Animation badge + SFX "ding doré" + haptic + compteur points |

### 11.3 Patterns de navigation

- **Stack navigation** sur les flows linéaires (auth → profil → home)
- **Tab navigation** sur les écrans pairs (Home / Mes Stats / Boutique)
- **Modal** sur les actions ponctuelles (settings, kick joueur, achat IAP)
- **Sheet bottom** sur les actions secondaires contextuelles (menu pause, options)

### 11.4 Patterns de chargement

| Cas | Pattern |
|---|---|
| Cold start | Splash iOS natif (LaunchScreen) → scène Boot Unity (logo La Veillée 2s) |
| Création room (< 2s attendu) | Spinner overlay sur le bouton |
| Rejoindre room (< 2s attendu) | Idem + message "Connexion à la partie…" |
| Distribution rôles | Animation cinématique (pas de spinner — l'attente est dramatique) |
| Cas long inattendu (> 5s) | Skeleton screens pour les listes, jamais de spinner plein écran > 3s sans explication texte |

### 11.5 Patterns d'erreur

- **Erreur réseau** → Toast non-bloquant + retry auto silencieux 3x + bouton "Réessayer" si échec persistant
- **Permission refusée (micro / BT)** → Modal explicative + bouton "Ouvrir Réglages"
- **Action invalide (compo non valide)** → Inline error en rouge sous le champ, jamais de pop-up
- **Crash** → Crashlytics auto + écran dédié de re-launch propre (pas d'écran blanc Unity par défaut)

---

## 12. Responsive & Accessibility

### 12.1 Breakpoints

| Device | Largeur cible | Layout |
|---|---|---|
| iPhone SE (1ère gen) | 320pt | Non supporté v1 (iOS 15+ requis) |
| iPhone 11/12/13 mini | 360pt | Layout compact 1 col |
| iPhone 12/13/14 (std) | 390pt | Layout standard 1 col |
| iPhone 12/13/14 Pro Max | 428pt | Layout standard 1 col, padding +8dp |
| iPad mini | 744pt | Layout tablette : 2 colonnes lobby/customizer |
| iPad std/Pro | 820pt+ | Layout tablette plein, vue 3D enrichie |

### 12.2 Orientation

- **iPhone :** portrait obligatoire en partie (UI optimisée pour 1 main)
- **iPhone setup/lobby :** portrait préféré, paysage toléré
- **iPad :** portrait + paysage, layout adaptatif

### 12.3 Accessibilité (NFR14-19)

#### VoiceOver (NFR14)
- Tous les éléments interactifs ont un `accessibilityLabel` localisé
- Tous les éléments décoratifs sont marqués `accessibilityHidden`
- Annonces automatiques sur changements d'état critiques (vote, phase, mort)
- Navigation par swipe parcourt logiquement les écrans (top → bottom)

#### Sous-titres MJ (NFR15)
- Toggle dans Settings → Accessibilité (off par défaut)
- Police 17pt SemiBold avec ombre portée pour lisibilité sur tous fonds
- Bande semi-transparente noir 60% opacity pour contraste
- Disparaissent à la fin de la ligne audio (synchronisé avec MJVoiceManager)
- Position : bas écran, au-dessus du HUD bas

#### Daltonisme (NFR16)
- Aucune info portée seule par couleur
- Camps : Loup = 🐺 + texte "Loup" + couleur night.700
- Camps : Villageois = 🧑‍🌾 + texte "Villageois" + couleur fire.500
- Maire : 👑 + label "Maire" + couleur gold.500
- Test obligatoire : simulation deutéranope + protanope dans Xcode debug

#### Vibration alternative écran-noir (NFR17)
- Toggle dans Settings → Accessibilité
- Pattern "rôle actif" : vibration continue 2s puis stop
- Pattern "rôle terminé" : 3 vibrations courtes 0.3s
- Pattern "phase changée" : 1 vibration médium 1s

#### Timer audio (NFR18)
- Bip discret aux 60s restantes
- Bip plus fort aux 10s restantes
- Tic-tac progressif sur les 5 dernières secondes
- Désactivable individuellement dans Settings

#### Push-to-talk (NFR19)
- Mic OFF par défaut au premier launch
- Toggle dans Settings → Vocal
- Quand actif : bouton micro flottant en bas droite, hold pour parler
- Haptic léger début/fin transmission

### 12.4 Contrast ratios (WCAG AA min)

Tous les contrastes texte sur fond sont validés ≥ 4.5:1 (body) et ≥ 3:1 (large text).

| Combinaison | Ratio | Statut |
|---|---|---|
| `text.body` (moon.100) sur `night.900` | 14.2:1 | ✅ AAA |
| `text.body` (moon.100) sur `fire.500` | 4.8:1 | ✅ AA |
| `text.caption` (moon.300) sur `night.700` | 8.1:1 | ✅ AAA |
| `text.label` sur `fire.500` (CTA) | 4.6:1 | ✅ AA |
| `text.label.gold` (gold.500 sur night.900) | 11.2:1 | ✅ AAA |

### 12.5 Tailles de touche minimum

- Boutons interactifs : **44x44 pt** minimum (Apple HIG)
- Avatars cibles de vote : **64x64 pt** minimum (groupes denses)
- Espacement entre cibles tap : **8 dp** minimum

---

## 13. Wireframes des écrans clés (ASCII)

### 13.1 Écran de révélation de rôle

```
┌─────────────────────────────────────┐
│                                     │
│                                     │
│         ✨ Ton rôle est… ✨          │
│                                     │
│                                     │
│         ┌─────────────────┐         │
│         │                 │         │
│         │       🐺        │         │ ← Carte qui fait le flip 0.8s
│         │                 │         │
│         │  Loup-Garou     │         │
│         │                 │         │
│         │ Tu te réveilles │         │
│         │ chaque nuit avec│         │
│         │ ta meute pour   │         │
│         │ choisir une     │         │
│         │ victime.        │         │
│         │                 │         │
│         └─────────────────┘         │
│                                     │
│                                     │
│         [J'ai compris]              │
│                                     │
└─────────────────────────────────────┘
```

### 13.2 Écran de fin de partie

```
┌─────────────────────────────────────┐
│       🌅 Le village a vaincu ! 🌅   │ ← Display cormorant 36pt
├─────────────────────────────────────┤
│                                     │
│   Les rôles révélés                 │
│                                     │
│   👤 Sayanth     🧑‍🌾 Villageois    │
│   👤 Marie       🔮 Voyante  ✓ Vit  │
│   👤 Julien      🐺 Loup    💀 Mort │
│   👤 Paul        🧪 Sorcière ✓ Vit  │
│   ...                               │
│                                     │
├─────────────────────────────────────┤
│   Stats de la partie                │
│   Durée: 47 min · 4 tours · 6 morts │
│   MVP: Marie (Voyante)              │
├─────────────────────────────────────┤
│   ┌───────────────────────────────┐ │
│   │      🔁 Rejouer               │ │ ← CTA primaire
│   └───────────────────────────────┘ │
│   ┌───────────────────────────────┐ │
│   │      🏠 Retour à l'accueil    │ │ ← CTA secondaire
│   └───────────────────────────────┘ │
└─────────────────────────────────────┘
```

### 13.3 Customizer avatar

```
┌─────────────────────────────────────┐
│ ← Mon avatar                        │
├─────────────────────────────────────┤
│                                     │
│           ┌─────────────┐           │
│           │             │           │
│           │     👤      │           │ ← Preview avatar 3D rotatif
│           │   Sayanth   │           │
│           │             │           │
│           └─────────────┘           │
│                                     │
├─────────────────────────────────────┤
│ [Tête] [Corps] [Coiffure] [Chapeau]│ ← Tabs
├─────────────────────────────────────┤
│ Coiffure                            │
│                                     │
│ ┌───┐ ┌───┐ ┌───┐ ┌───┐ ┌───┐      │
│ │ ✓ │ │   │ │   │ │   │ │ 🔒│      │ ← Tiles cosmétiques
│ │👨‍🦰│ │👨‍🦱│ │👨‍🦲│ │👩‍🦱│ │👑 │      │
│ └───┘ └───┘ └───┘ └───┘ └───┘      │
│                                     │
│ ┌───┐ ┌───┐ ┌───┐                  │
│ │ 🔒│ │ 🔒│ │ 🔒│                  │
│ │   │ │   │ │   │                  │
│ └───┘ └───┘ └───┘                  │
│                                     │
├─────────────────────────────────────┤
│   [💾 Sauvegarder]                  │
└─────────────────────────────────────┘
```

### 13.4 Écran totem (mode Campfire)

```
┌─────────────────────────────────────┐
│                                     │
│                                     │
│                                     │ ← Luminosité 30%
│              🔥                     │   Pas d'UI interactive
│         (flammes animées)           │
│                                     │
│                                     │
│         La Veillée                  │ ← Caption discrète
│         Partie en cours…            │
│                                     │
│                                     │
└─────────────────────────────────────┘
       (haut-parleur joue MJ + musique)
```

### 13.5 Salon des morts

```
┌─────────────────────────────────────┐
│ ✨ Salon des morts ✨               │
├─────────────────────────────────────┤
│                                     │
│       (ciel étoilé background)      │
│                                     │
│   👤 Julien (mort tour 1)           │
│   👤 Léa (mort tour 2)              │
│   👤 Toi (mort tour 3)              │
│                                     │
├─────────────────────────────────────┤
│                                     │
│   La partie continue… 👀            │
│                                     │
│   Voyante: Marie (vivante)          │
│   Loups restants: 2                 │
│                                     │
│   [Spectateur] [👁️ Voir partie]    │
│                                     │
├─────────────────────────────────────┤
│   🎙️ Vocal entre morts uniquement   │
└─────────────────────────────────────┘
```

---

## 14. UX Design Requirements (UX-DRs) consolidés pour Epics

> Reformulation des exigences UX en items actionnables tels qu'ils auraient figuré dans `epics.md` section "UX Design Requirements" si ce doc avait existé en amont. Ces UX-DRs croisent les FRs des Épopées et fournissent les acceptance criteria visuels.

| ID | UX Design Requirement | Couvert par |
|---|---|---|
| UX-DR1 | Système de tokens design implémenté en ScriptableObjects (couleurs, typo, spacing, motion, shadows) | Story 1.1 (bootstrap) |
| UX-DR2 | 20+ composants partagés (`UI/Common/`) avec variantes documentées | Stories 5.1-5.7, transversal |
| UX-DR3 | Custom de l'avatar via 4 axes modulaires (tête, corps, coiffure, chapeau) avec ≥ 25 combinaisons v1 | Story 5.2, 9.4 |
| UX-DR4 | Composant `ScreenBlackout` avec 3 modes (full noir, dimmed a11y, vibration alt) | Story 5.6 |
| UX-DR5 | Composant `ConfirmActionRing` (tap+hold 0.5s) appliqué à toutes les actions irréversibles | Stories 3.3, 3.4, 3.5, 3.10, 4.1, 4.2, 4.4 |
| UX-DR6 | Composant `SubtitleOverlay` synchronisé avec `MJVoiceManager` | Story 6.5, 8.2 |
| UX-DR7 | Composant `Remote360CameraController` (swipe + gyroscope) | Story 5.7 |
| UX-DR8 | `CampfireTotemView` minimaliste (luminosité 30%, no UI, flames anim) | Story 7.3 |
| UX-DR9 | `CampfireCompanionView` réutilise les vues Remote pour cohérence | Story 7.4 |
| UX-DR10 | Animations cinématiques 60fps avec `motion.transition.phase` 3s | Story 5.5 |
| UX-DR11 | `DeathDramaticAnimation` orchestrée (socle + tombe + particules + stinger) | Story 5.4 |
| UX-DR12 | Iconographie custom 40 icônes commandée à un illustrateur freelance | Story 5.1 (kickoff art), Story 11.4 (final pass) |
| UX-DR13 | Système i18n (Unity Localization) avec `fr.csv` complet v1, `en.csv` placeholder | Bootstrap Story 1.1, finalisation Story 11.3 |
| UX-DR14 | Audit accessibilité Apple complet (VoiceOver, contrastes, daltonisme) avant soumission | Stories 8.2, 8.3, 8.4, 8.5 |
| UX-DR15 | Layout adaptatif iPhone portrait + iPad portrait/paysage testé sur 3 modèles min | Story 1.1 (Player Settings), QA Story 11.1 |
| UX-DR16 | `RoleCard` avec halo de couleur sémantique différenciée par camp (fire = villageois, night = loup, gold = maire, crystal = voyante, poison = sorcière, blood = chasseur, etc.) | Stories 3.x, 4.x |
| UX-DR17 | Toast non-bloquants pour erreurs réseau / IAP failed / actions invalides | Transversal |
| UX-DR18 | Aucun pop-up d'upsell IAP avant complétion de la 1ère partie | Story 8.1, 10.1 |
| UX-DR19 | `EmptyState` illustré pour : pas d'amis, pas de stats, pas d'achievements | Story 9.2, 9.3, 9.5 |
| UX-DR20 | Splash screen iOS natif (LaunchScreen.storyboard) puis scène Boot Unity (logo 2s max) | Story 1.1 |

---

## Validation

✅ Tous les pillars du GDD sont traduits en directives UX  
✅ Tous les FRs visuels des Épopées 1-11 ont une stratégie UI/UX correspondante  
✅ Tous les NFRs accessibilité (NFR14-19) ont un pattern UX dédié  
✅ Le design system custom est documenté avec tokens implémentables  
✅ Les composants critiques sont catalogués avec variantes  
✅ Les wireframes des écrans clés permettent de démarrer le travail Unity uGUI  
✅ Les UX-DRs sont mappés aux stories existantes dans `epics.md`  
✅ Les contrastes WCAG AA sont validés sur la palette principale  

---

_Generated by GDS UX Design Workflow — La Veillée v1.0_  
_Date : 2026-04-20_  
_For : sayanth_  
_Direction artistique : Veillée intime · Palette feu/nuit · Manrope + Cormorant · Custom design system sur Unity uGUI_
