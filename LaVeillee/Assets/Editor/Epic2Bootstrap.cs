using System.IO;
using System.Linq;
using Fusion;
using LaVeillee.Core;
using LaVeillee.Networking;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LaVeillee.EditorTools
{
    /// Génère Main.unity — scène unique Épopée 2 contenant :
    ///   - AppBootstrap GameObject (init services + panel switching)
    ///   - LobbyState scene-placed (NetworkObject + LobbyState), visible par Fusion
    ///     dès que le Runner démarre en Shared mode.
    ///
    /// Idempotent : ré-exécute le script pour régénérer la scène après un pull ou
    /// un fix de prefab/NetworkObject GUID cassé.
    public static class Epic2Bootstrap
    {
        const string ScenePath = "Assets/Scenes/Main.unity";

        [MenuItem("LaVeillee/Épopée 2/Generate Main Scene")]
        public static void GenerateMainScene()
        {
            Directory.CreateDirectory(Path.Combine(Application.dataPath, "Scenes"));
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // Caméra : fond bleu nuit (en attendant que le canvas overlay couvre tout).
            var camGo = GameObject.Find("Main Camera");
            if (camGo != null)
            {
                var cam = camGo.GetComponent<Camera>();
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.043f, 0.070f, 0.141f); // Night900
            }

            // AppBootstrap — entry point
            var bootGo = new GameObject("[AppBootstrap]");
            bootGo.AddComponent<AppBootstrap>();

            // LobbyState scene-placed — Fusion le détecte au StartGame et le réplique
            // à tous les clients (Shared mode). Pas besoin de prefab dynamique.
            var lobbyGo = new GameObject("[LobbyState]");
            lobbyGo.AddComponent<NetworkObject>();
            lobbyGo.AddComponent<LobbyState>();

            EditorSceneManager.SaveScene(scene, ScenePath);

            // Ajoute Main.unity en tête des Build Settings (scène de démarrage).
            var existing = EditorBuildSettings.scenes ?? System.Array.Empty<EditorBuildSettingsScene>();
            var rebuilt = new System.Collections.Generic.List<EditorBuildSettingsScene>
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };
            foreach (var s in existing)
            {
                if (s.path == ScenePath) continue;
                rebuilt.Add(s);
            }
            EditorBuildSettings.scenes = rebuilt.ToArray();

            Debug.Log($"[Epic2Bootstrap] Scène {ScenePath} générée + mise en scène de démarrage.");
        }
    }
}
