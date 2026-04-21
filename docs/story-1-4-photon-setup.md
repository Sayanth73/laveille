# Story 1.4 — Setup Photon Fusion 2

Ce doc guide l'import du SDK Photon Fusion 2 et la config de l'AppId pour la Story 1.4.

---

## Pré-requis côté dashboard Photon (déjà fait)

Compte créé sur [dashboard.photonengine.com](https://dashboard.photonengine.com) + app de type **Fusion** créée. L'AppId est stocké localement (gitignored) dans `LaVeillee/.photon.local`.

Si tu perds l'AppId : dashboard → ton app Fusion → copie "App ID".

---

## Import du SDK Fusion 2

Photon ne distribue pas Fusion 2 via UPM public ni via git URL ouvert. Deux options :

### Option A — Asset Store (recommandée pour solo dev)

1. Ouvre Unity Hub → ouvre le projet `LaVeillee/`
2. Dans Unity : **Window → Package Manager**
3. Tout en haut : dropdown **Packages: Unity Registry** → **My Assets**
4. Connecte-toi à ton compte Unity si demandé
5. Cherche "Photon Fusion" → clique **Add to My Assets** (c'est gratuit)
6. Dans Package Manager : **Download** puis **Import**
7. Dans le dialog d'import : accepte tout (environ 80 MB)

### Option B — .unitypackage direct

1. [doc.photonengine.com/fusion/current/getting-started/sdk-download](https://doc.photonengine.com/fusion/current/getting-started/sdk-download)
2. Login → télécharge `photon-fusion-2.x.x.unitypackage`
3. Dans Unity : **Assets → Import Package → Custom Package...** → sélectionne le fichier
4. Accepte tout

---

## Configuration AppId post-import

Après l'import du SDK, Fusion 2 crée `Assets/Photon/Fusion/Resources/PhotonAppSettings.asset` (ou équivalent selon version).

**Deux options pour injecter l'AppId :**

### Option 1 — Menu Fusion Hub (manuel)

1. Menu Unity : **Fusion → Fusion Hub** (ou **Tools → Photon Fusion**)
2. Onglet Welcome → champ "App Id Fusion"
3. Copie l'AppId depuis `LaVeillee/.photon.local`
4. Save

### Option 2 — Automatisé via script (à lancer après SDK import)

Je fournirai un script `Assets/Editor/PhotonBootstrap.cs` qui lit `.photon.local` et configure `PhotonAppSettings.asset` en batchmode. Il sera exécutable via :

```bash
Unity -batchmode -nographics -quit -projectPath LaVeillee \
  -executeMethod LaVeillee.EditorTools.PhotonBootstrap.Configure \
  -logFile /tmp/photon-bootstrap.log
```

---

## Sécurité de l'AppId

L'AppId Fusion n'est techniquement **pas un secret** (il est embedded dans le build client et visible en traffic réseau). C'est un identifiant de routing, pas un credential. Mais par hygiène, on le garde hors de git :

- `LaVeillee/.photon.local` est gitignored
- `PhotonAppSettings.asset` est également gitignored (cf. `.gitignore`)
- Pour reproduire le setup sur une nouvelle machine : recopier `.photon.local` depuis un backup sécurisé (ou regénérer via dashboard Photon)

---

## AC Story 1.4 (epics.md) — mapping

| AC | Implémentation prévue |
|---|---|
| Décision Photon vs Mirror tranchée | Déjà fait — cf. [ADR-001](../_bmad-output/game-architecture.md) |
| SDK intégrée au projet Unity | Étape "Import SDK" ci-dessus |
| `RoomManager.cs` expose Create/Join/Leave/GetPlayers | `IRoomManager` + `FusionRoomManager` |
| Scène `DevTestRoom.unity` avec boutons UI | À créer (tâche #11) |
| 2 joueurs différents se voient en < 2s | Test bout-en-bout (tâche #13) |
| PlayerId unique persistant | `PlayerHandle.Id` (Fusion `PlayerRef.PlayerId`) |
| Disconnect brutal → `OnPlayerLeft` en 30s | Fusion timeout natif (configurable via `PhotonAppSettings`) |
| 26e joueur → `RoomFull` | Fusion rejette auto si `MaxPlayers=25` |

---

## Checklist

- [ ] SDK Fusion 2 importé (Option A ou B)
- [ ] AppId injecté dans PhotonAppSettings
- [ ] `DevTestRoom.unity` créée
- [ ] `FusionRoomManager` implémenté
- [ ] Test 2 instances : se voient dans la room
- [ ] Test disconnect brutal : `OnPlayerLeft` déclenché
