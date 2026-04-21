using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LaVeillee.EditorTools
{
    public static class Bootstrap
    {
        const string CompanyName = "La Veillee";
        const string ProductName = "La Veillee";
        const string BundleId    = "com.laveillee.app";
        const string MinIOS      = "15.0";

        const string MicDescription =
            "La Veillee utilise le micro pour le vocal entre joueurs pendant la partie.";

        const string SceneFolder = "Assets/Scenes";
        const string ScenePath   = "Assets/Scenes/HelloLaVeillee.unity";

        [MenuItem("LaVeillee/Bootstrap/Run All")]
        public static void RunAll()
        {
            ConfigureProjectIOS();
            CreateHelloScene();
            AssetDatabase.SaveAssets();
            Debug.Log("[Bootstrap] All steps complete.");
        }

        [MenuItem("LaVeillee/Bootstrap/Configure iOS Player Settings")]
        public static void ConfigureProjectIOS()
        {
            PlayerSettings.companyName = CompanyName;
            PlayerSettings.productName = ProductName;

            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, BundleId);

            PlayerSettings.iOS.targetOSVersionString = MinIOS;
            PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPhoneAndiPad;
            PlayerSettings.iOS.appleEnableAutomaticSigning = true;

            PlayerSettings.iOS.microphoneUsageDescription = MicDescription;
            // Bluetooth + LocalNetwork descriptions injected via iOSBuildPostProcessor (Unity 6 no longer
            // exposes bluetoothUsageDescription on PlayerSettings.iOS).

            // Story 2.1 — Loups-Garous mobile se joue en portrait uniquement
            // (évite les bascules au milieu d'une partie longue).
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;
            PlayerSettings.useAnimatedAutorotation = false;

            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);

            Debug.Log($"[Bootstrap] iOS configured: bundleId={BundleId}, minIOS={MinIOS}, orientation=Portrait");
        }

        [MenuItem("LaVeillee/Bootstrap/Create Hello Scene")]
        public static void CreateHelloScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            var canvasGO = new GameObject("Canvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1170, 2532);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGO.AddComponent<GraphicRaycaster>();

            var textGO = new GameObject("HelloText");
            textGO.transform.SetParent(canvasGO.transform, false);
            var text = textGO.AddComponent<Text>();
            text.text = "Hello La Veillee";
            text.alignment = TextAnchor.MiddleCenter;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 64;
            text.color = Color.white;

            var rt = textGO.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var camGO = GameObject.Find("Main Camera");
            if (camGO != null)
            {
                var cam = camGO.GetComponent<Camera>();
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.07f, 0.05f, 0.10f);
            }

            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<EventSystem>();
            esGO.AddComponent<StandaloneInputModule>();

            if (!AssetDatabase.IsValidFolder(SceneFolder))
                AssetDatabase.CreateFolder("Assets", "Scenes");

            EditorSceneManager.SaveScene(scene, ScenePath);

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };

            Debug.Log($"[Bootstrap] Scene created at {ScenePath} and added to Build Settings.");
        }
    }
}
