# ADR-008 : Pivot Unity 2023 LTS → Unity 6 LTS

**Statut** : Accepté
**Date** : 2026-04-20
**Auteur** : sayanth
**Supersede partiellement** : choix engine de `_bmad-output/game-architecture.md` (Unity 2023.2.20f1 LTS)

---

## Contexte

L'architecture initiale (avril 2026) figeait le moteur sur **Unity 2023.2.20f1 LTS**. Au moment de bootstrapper la Story 1.1, l'environnement de dev avait **Unity 6.4.3f1 (= 6000.4.3f1) LTS** déjà installé via Unity Hub. Unity 6 est la nouvelle ligne LTS officielle, successeur direct de Unity 2023 LTS, en sortie depuis fin 2024 et désormais recommandée par Unity Technologies pour tout nouveau projet.

Forcer un retour à 2023.2.20f1 supposerait : install parallèle dans Unity Hub, divergence vs recommandation upstream, fenêtre de support qui se rétrécit (Unity 2023 LTS sort progressivement du support en 2026-2027 alors que Unity 6 LTS est supportée jusqu'en 2027+).

## Décision

**Adopter Unity 6.4 LTS (6000.4.3f1)** comme moteur cible v1.

## Conséquences

**✅ Positives**
- Aligné sur la recommandation upstream actuelle de Unity Technologies
- Fenêtre de support LTS plus longue
- URP / Render Graph plus mature qu'en 2023.2 — perf mobile améliorée
- Outillage déjà installé localement (zéro friction setup)
- Aucune réécriture nécessaire : C# cross-version, API URP / uGUI / Audio identiques pour ce qu'on consomme

**⚠️ À surveiller**
- **Photon Fusion 2** (ADR-001) : compatibilité Unity 6 confirmée par Photon Engine — pas de blocker connu, à valider concrètement à la Story 1.4
- **Photon Voice 2** (ADR-002) : idem — à valider à la Story 1.5
- **Firebase Unity SDK 11.x** : supporte Unity 6 — à valider à la Story 1.2
- **iOS Build Support** module installé et conforme

## Impact sur les artefacts existants

Les références "Unity 2023 LTS" / "Unity 2023.2.20f1" dans `_bmad-output/game-architecture.md`, `gdd.md`, et le `README.md` deviennent **historiquement correctes mais opérationnellement supersedées** par cet ADR. Pas de réécriture de ces docs : la traçabilité du pivot reste lisible via cet ADR-008.

## Alternatives écartées

- **Forcer Unity 2023.2.20f1 LTS** : install parallèle, alignement strict au plan, mais auto-imposition d'une version LTS en fin de cycle. Coût > bénéfice.
- **Unity 6.0 / 6.1 / 6.2 / 6.3 LTS antérieurs** : 6.4.3f1 est la dernière LTS patch disponible localement, pas de raison de downgrader.
