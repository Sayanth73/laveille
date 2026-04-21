using System;
using System.IO;
using LaVeillee.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LaVeillee.EditorTools
{
    /// One-shot editor bootstrap for Photon networking + voice:
    ///   1) inject Fusion / Voice / Chat AppIds from .photon.local into PhotonAppSettings asset
    ///   2) generate Assets/Scenes/DevTestRoom.unity with room + voice UI wired to DevTestRoomController
    /// Idempotent — safe to re-run after each story increment.
    public static class PhotonBootstrap
    {
        const string PhotonLocalRelativePath = "../.photon.local";
        const string DevTestScenePath = "Assets/Scenes/DevTestRoom.unity";

        // Maps .photon.local keys → field names on Fusion.Photon.Realtime.AppSettings.
        // Order matters only for log clarity; injection is independent per key.
        static readonly (string EnvKey, string AssetField)[] AppIdMappings =
        {
            ("FUSION_APP_ID", "AppIdFusion"),
            ("VOICE_APP_ID",  "AppIdVoice"),
            ("CHAT_APP_ID",   "AppIdChat"),
        };

        [MenuItem("LaVeillee/Run Photon Bootstrap")]
        public static void RunAll()
        {
            ConfigureAppId();
            CreateDevTestRoomScene();
        }

        public static void ConfigureAppId()
        {
            var localPath = Path.GetFullPath(Path.Combine(Application.dataPath, PhotonLocalRelativePath));
            if (!File.Exists(localPath))
            {
                Debug.LogError($"[PhotonBootstrap] .photon.local introuvable : {localPath}");
                return;
            }

            var env = new System.Collections.Generic.Dictionary<string, string>();
            foreach (var line in File.ReadAllLines(localPath))
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("#") || string.IsNullOrEmpty(trimmed)) continue;
                var idx = trimmed.IndexOf('=');
                if (idx < 0) continue;
                env[trimmed.Substring(0, idx).Trim()] = trimmed.Substring(idx + 1).Trim();
            }

            // Fusion.Photon.Realtime.PhotonAppSettings lives in Fusion.Realtime.dll. Force-load the
            // assembly via reflection — Fusion.Realtime is excluded from the Editor platform's
            // auto-reference set, so it isn't in our compile chain.
            Type photonAppSettingsType = null;
            try
            {
                var asm = System.Reflection.Assembly.Load("Fusion.Realtime");
                photonAppSettingsType = asm?.GetType("Fusion.Photon.Realtime.PhotonAppSettings");
            }
            catch (Exception e)
            {
                Debug.LogError($"[PhotonBootstrap] Échec du chargement de Fusion.Realtime : {e.Message}");
                return;
            }

            if (photonAppSettingsType == null)
            {
                Debug.LogError("[PhotonBootstrap] Type Fusion.Photon.Realtime.PhotonAppSettings introuvable.");
                return;
            }

            ScriptableObject settings = null;
            string assetPath = null;
            var guids = AssetDatabase.FindAssets($"t:{photonAppSettingsType.Name}");
            if (guids.Length > 0)
            {
                assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                settings = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);
            }
            else
            {
                // Auto-create the asset under Assets/Photon/Resources so Resources.Load picks it up at runtime.
                Directory.CreateDirectory(Path.Combine(Application.dataPath, "Photon/Resources"));
                assetPath = "Assets/Photon/Resources/PhotonAppSettings.asset";
                settings = ScriptableObject.CreateInstance(photonAppSettingsType);
                AssetDatabase.CreateAsset(settings, assetPath);
                Debug.Log($"[PhotonBootstrap] Asset PhotonAppSettings créé : {assetPath}");
            }

            if (settings == null)
            {
                Debug.LogError($"[PhotonBootstrap] Impossible de charger/créer l'asset {assetPath}.");
                return;
            }

            var so = new SerializedObject(settings);
            var appSettingsProp = so.FindProperty("AppSettings");
            if (appSettingsProp == null)
            {
                Debug.LogError("[PhotonBootstrap] Propriété 'AppSettings' introuvable sur PhotonAppSettings.");
                return;
            }

            int injected = 0;
            foreach (var (envKey, assetField) in AppIdMappings)
            {
                if (!env.TryGetValue(envKey, out var appId) || string.IsNullOrEmpty(appId)) continue;
                var prop = appSettingsProp.FindPropertyRelative(assetField);
                if (prop == null)
                {
                    Debug.LogWarning($"[PhotonBootstrap] Champ '{assetField}' introuvable — ignoré.");
                    continue;
                }
                prop.stringValue = appId;
                Debug.Log($"[PhotonBootstrap] {assetField} ← {envKey} ({appId.Substring(0, 8)}…)");
                injected++;
            }

            if (injected == 0)
            {
                Debug.LogError("[PhotonBootstrap] Aucun AppId injecté — vérifie .photon.local.");
                return;
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
            Debug.Log($"[PhotonBootstrap] {injected} AppId(s) sauvegardés dans {assetPath}.");
        }

        public static void CreateDevTestRoomScene()
        {
            Directory.CreateDirectory(Path.Combine(Application.dataPath, "Scenes"));
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // EventSystem (required for UGUI input)
            var eventSystem = new GameObject("EventSystem",
                typeof(UnityEngine.EventSystems.EventSystem),
                typeof(UnityEngine.EventSystems.StandaloneInputModule));

            // Canvas root (Screen Space Overlay so it works without a camera setup)
            var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            // Compact center-anchored layout. Spans ~600px to fit room + voice controls in iPhone portrait.
            CreateText(canvasGo.transform, "Title",
                new Vector2(0, 280), new Vector2(700, 40),
                "La Veillée — Dev Test Room", 22, TextAnchor.MiddleCenter);

            var roomIdInput = CreateInputField(canvasGo.transform, "RoomIdInput",
                new Vector2(0, 225), new Vector2(560, 45),
                "Room ID (vide = auto)…");

            var createBtn = CreateButton(canvasGo.transform, "CreateButton",
                new Vector2(0, 170), new Vector2(560, 50), "Créer une room");
            var joinBtn = CreateButton(canvasGo.transform, "JoinButton",
                new Vector2(0, 115), new Vector2(560, 50), "Rejoindre");
            var leaveBtn = CreateButton(canvasGo.transform, "LeaveButton",
                new Vector2(0, 60), new Vector2(560, 50), "Quitter la room");

            var muteBtn = CreateButton(canvasGo.transform, "MuteButton",
                new Vector2(0, 0), new Vector2(560, 50), "🎙 Couper micro");
            var muteLabel = muteBtn.GetComponentInChildren<Text>();

            var quitBtn = CreateButton(canvasGo.transform, "QuitButton",
                new Vector2(0, -55), new Vector2(560, 50), "Quitter l'appli");

            var statusText = CreateText(canvasGo.transform, "StatusText",
                new Vector2(0, -135), new Vector2(700, 100),
                "Initialisation…", 16, TextAnchor.UpperLeft);

            var voiceStatusText = CreateText(canvasGo.transform, "VoiceStatusText",
                new Vector2(0, -230), new Vector2(700, 60),
                "Vocal : —", 16, TextAnchor.UpperLeft);

            // Controller
            var controllerGo = new GameObject("DevTestRoomController");
            var controller = controllerGo.AddComponent<DevTestRoomController>();
            var so = new SerializedObject(controller);
            so.FindProperty("_roomIdInput").objectReferenceValue = roomIdInput;
            so.FindProperty("_createButton").objectReferenceValue = createBtn;
            so.FindProperty("_joinButton").objectReferenceValue = joinBtn;
            so.FindProperty("_leaveButton").objectReferenceValue = leaveBtn;
            so.FindProperty("_quitButton").objectReferenceValue = quitBtn;
            so.FindProperty("_muteButton").objectReferenceValue = muteBtn;
            so.FindProperty("_muteLabel").objectReferenceValue = muteLabel;
            so.FindProperty("_statusText").objectReferenceValue = statusText;
            so.FindProperty("_voiceStatusText").objectReferenceValue = voiceStatusText;
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, DevTestScenePath);

            // Add scene to Build Settings (after HelloLaVeillee if present)
            var hello = "Assets/Scenes/HelloLaVeillee.unity";
            var newScenes = new System.Collections.Generic.List<EditorBuildSettingsScene>();
            if (File.Exists(Path.Combine(Application.dataPath, "../" + hello)))
                newScenes.Add(new EditorBuildSettingsScene(hello, true));
            newScenes.Add(new EditorBuildSettingsScene(DevTestScenePath, true));
            EditorBuildSettings.scenes = newScenes.ToArray();

            Debug.Log($"[PhotonBootstrap] Scène {DevTestScenePath} créée + ajoutée aux Build Settings.");
        }

        // ---- UI helpers ----

        static Text CreateText(Transform parent, string name, Vector2 pos, Vector2 size,
                               string content, int fontSize, TextAnchor align)
        {
            var go = new GameObject(name, typeof(Text));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var t = go.GetComponent<Text>();
            t.text = content;
            t.fontSize = fontSize;
            t.alignment = align;
            t.color = Color.white;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            return t;
        }

        static InputField CreateInputField(Transform parent, string name, Vector2 pos, Vector2 size, string placeholder)
        {
            var go = new GameObject(name, typeof(Image), typeof(InputField));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            go.GetComponent<Image>().color = new Color(0.15f, 0.15f, 0.18f, 1f);

            var input = go.GetComponent<InputField>();

            // Placeholder
            var phGo = new GameObject("Placeholder", typeof(Text));
            phGo.transform.SetParent(go.transform, false);
            var phRt = phGo.GetComponent<RectTransform>();
            phRt.anchorMin = Vector2.zero; phRt.anchorMax = Vector2.one;
            phRt.offsetMin = new Vector2(20, 5); phRt.offsetMax = new Vector2(-20, -5);
            var phText = phGo.GetComponent<Text>();
            phText.text = placeholder;
            phText.fontSize = 28;
            phText.alignment = TextAnchor.MiddleLeft;
            phText.color = new Color(1, 1, 1, 0.4f);
            phText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            phText.fontStyle = FontStyle.Italic;

            // Live text
            var txtGo = new GameObject("Text", typeof(Text));
            txtGo.transform.SetParent(go.transform, false);
            var txtRt = txtGo.GetComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero; txtRt.anchorMax = Vector2.one;
            txtRt.offsetMin = new Vector2(20, 5); txtRt.offsetMax = new Vector2(-20, -5);
            var txt = txtGo.GetComponent<Text>();
            txt.fontSize = 28;
            txt.alignment = TextAnchor.MiddleLeft;
            txt.color = Color.white;
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.supportRichText = false;

            input.textComponent = txt;
            input.placeholder = phText;
            input.characterLimit = 12;
            input.contentType = InputField.ContentType.Alphanumeric;

            return input;
        }

        static Button CreateButton(Transform parent, string name, Vector2 pos, Vector2 size, string label)
        {
            var go = new GameObject(name, typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            go.GetComponent<Image>().color = new Color(0.2f, 0.4f, 0.7f, 1f);

            var labelGo = new GameObject("Label", typeof(Text));
            labelGo.transform.SetParent(go.transform, false);
            var lblRt = labelGo.GetComponent<RectTransform>();
            lblRt.anchorMin = Vector2.zero; lblRt.anchorMax = Vector2.one;
            lblRt.offsetMin = Vector2.zero; lblRt.offsetMax = Vector2.zero;
            var lbl = labelGo.GetComponent<Text>();
            lbl.text = label;
            lbl.fontSize = 32;
            lbl.alignment = TextAnchor.MiddleCenter;
            lbl.color = Color.white;
            lbl.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            return go.GetComponent<Button>();
        }
    }
}
