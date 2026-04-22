using System.IO;
using System.Linq;
using LaVeillee.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LaVeillee.EditorTools
{
    /// Génère l'environnement 3D de la scène Main.unity — Story 5.1 :
    /// clairière nocturne avec feu de camp central, socles en cercle pour
    /// les avatars, arbres scattered en périphérie, skybox HDRI + brouillard.
    /// Idempotent : supprime [Environment] existant avant reconstruction.
    public static class Epic5Bootstrap
    {
        const string ScenePath       = "Assets/Scenes/Main.unity";
        const string EnvRootName     = "[Environment]";
        const string SkyboxMatPath   = "Assets/Art/Skyboxes/NightSkybox.mat";
        const string SataraPath      = "Assets/Art/Skyboxes/satara_night_4k.exr";
        const string GroundMatPath   = "Assets/Art/Environment/GroundMat.mat";
        const string StoneMatPath    = "Assets/Art/Environment/StoneMat.mat";
        const string LogMatPath      = "Assets/Art/Environment/LogMat.mat";

        const int MaxSocles = 12;
        const float SocleRingRadius = 4.5f;
        const int TreeCount = 18;
        const float TreeMinRadius = 10f;
        const float TreeMaxRadius = 20f;

        [MenuItem("LaVeillee/Épopée 5/Generate Village Scene")]
        public static void GenerateVillageScene()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            // Idempotence : purge l'ancienne env avant reconstruction
            var existing = GameObject.Find(EnvRootName);
            if (existing != null) Object.DestroyImmediate(existing);

            var envRoot = new GameObject(EnvRootName);

            ConfigureRenderSettings();
            ConfigureMainCamera();
            BuildLighting(envRoot.transform);
            BuildGround(envRoot.transform);
            BuildCampfire(envRoot.transform);
            BuildSocles(envRoot.transform);
            BuildAvatars(envRoot.transform);
            ScatterTrees(envRoot.transform);
            ScatterRocks(envRoot.transform);
            BuildGameSceneController(envRoot.transform);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log($"[Epic5Bootstrap] Scène village générée dans {ScenePath}");
        }

        static void ConfigureRenderSettings()
        {
            RenderSettings.skybox = EnsureSkyboxMaterial();
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.12f, 0.15f, 0.25f);
            RenderSettings.ambientEquatorColor = new Color(0.08f, 0.10f, 0.18f);
            RenderSettings.ambientGroundColor = new Color(0.04f, 0.05f, 0.08f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.08f, 0.10f, 0.18f);
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.018f;
            DynamicGI.UpdateEnvironment();
        }

        static Material EnsureSkyboxMaterial()
        {
            var existing = AssetDatabase.LoadAssetAtPath<Material>(SkyboxMatPath);
            if (existing != null) return existing;

            var shader = Shader.Find("Skybox/Panoramic") ?? Shader.Find("Skybox/Cubemap");
            var mat = new Material(shader);
            var hdri = AssetDatabase.LoadAssetAtPath<Texture>(SataraPath);
            if (hdri != null)
            {
                mat.SetTexture("_MainTex", hdri);
                mat.SetFloat("_Exposure", 1.0f);
            }
            Directory.CreateDirectory("Assets/Art/Skyboxes");
            AssetDatabase.CreateAsset(mat, SkyboxMatPath);
            return mat;
        }

        static void ConfigureMainCamera()
        {
            var camGo = GameObject.Find("Main Camera");
            if (camGo == null) return;
            camGo.transform.position = new Vector3(0f, 2.4f, -7.5f);
            camGo.transform.rotation = Quaternion.Euler(12f, 0f, 0f);
            var cam = camGo.GetComponent<Camera>();
            cam.clearFlags = CameraClearFlags.Skybox;
            cam.fieldOfView = 62f;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 200f;
        }

        static void BuildLighting(Transform parent)
        {
            var group = new GameObject("[Lighting]");
            group.transform.SetParent(parent, false);

            var moon = new GameObject("Moonlight");
            moon.transform.SetParent(group.transform, false);
            moon.transform.rotation = Quaternion.Euler(55f, 25f, 0f);
            var moonLight = moon.AddComponent<Light>();
            moonLight.type = LightType.Directional;
            moonLight.color = new Color(0.55f, 0.63f, 0.95f);
            moonLight.intensity = 0.35f;
            moonLight.shadows = LightShadows.Soft;
            moonLight.shadowStrength = 0.6f;
        }

        static void BuildGround(Transform parent)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Plane);
            go.name = "Ground";
            go.transform.SetParent(parent, false);
            go.transform.localScale = new Vector3(8f, 1f, 8f); // Plane = 10u base → 80x80u

            var mat = LoadOrCreateMat(GroundMatPath, () =>
            {
                var m = new Material(Shader.Find("Standard"));
                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(
                    "Assets/Art/Environment/Nature/Textures/Grass.png");
                if (tex != null)
                {
                    m.mainTexture = tex;
                    m.mainTextureScale = new Vector2(16f, 16f);
                }
                m.color = new Color(0.40f, 0.45f, 0.35f);
                m.SetFloat("_Glossiness", 0.1f);
                return m;
            });
            go.GetComponent<MeshRenderer>().sharedMaterial = mat;
        }

        static void BuildCampfire(Transform parent)
        {
            var fire = new GameObject("Campfire");
            fire.transform.SetParent(parent, false);
            fire.transform.position = new Vector3(0f, 0f, 0f);

            // Anneau de pierres
            var stoneMat = LoadOrCreateMat(StoneMatPath, () =>
            {
                var m = new Material(Shader.Find("Standard"));
                m.color = new Color(0.32f, 0.32f, 0.34f);
                m.SetFloat("_Glossiness", 0.15f);
                return m;
            });
            var rockFbx = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Art/Environment/Nature/Models/Rock_Medium_1.fbx");
            for (int i = 0; i < 8; i++)
            {
                var angle = i * Mathf.PI * 2f / 8f;
                Vector3 pos = new Vector3(Mathf.Cos(angle) * 1.1f, 0.05f, Mathf.Sin(angle) * 1.1f);
                GameObject stone;
                if (rockFbx != null)
                {
                    stone = (GameObject)PrefabUtility.InstantiatePrefab(rockFbx, fire.transform);
                    stone.transform.localScale = Vector3.one * 0.5f;
                }
                else
                {
                    stone = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    stone.transform.SetParent(fire.transform, false);
                    stone.transform.localScale = new Vector3(0.45f, 0.3f, 0.45f);
                }
                stone.name = $"Stone_{i}";
                stone.transform.localPosition = pos;
                stone.transform.localRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
                ApplyMat(stone, stoneMat);
            }

            // Bûches empilées en tipi
            var logMat = LoadOrCreateMat(LogMatPath, () =>
            {
                var m = new Material(Shader.Find("Standard"));
                m.color = new Color(0.32f, 0.20f, 0.10f);
                m.SetFloat("_Glossiness", 0.25f);
                return m;
            });
            for (int i = 0; i < 4; i++)
            {
                var log = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                log.name = $"Log_{i}";
                log.transform.SetParent(fire.transform, false);
                log.transform.localScale = new Vector3(0.12f, 0.8f, 0.12f);
                log.transform.localPosition = new Vector3(0f, 0.6f, 0f);
                log.transform.localRotation = Quaternion.Euler(
                    Random.Range(-65f, -55f),
                    i * 90f + Random.Range(-10f, 10f),
                    Random.Range(-15f, 15f));
                ApplyMat(log, logMat);
            }

            // Particules feu
            var fxGo = new GameObject("Flames");
            fxGo.transform.SetParent(fire.transform, false);
            fxGo.transform.localPosition = new Vector3(0f, 0.4f, 0f);
            var ps = fxGo.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 1.4f;
            main.startSpeed = 1.2f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.25f, 0.55f);
            main.startColor = new ParticleSystem.MinMaxGradient(
                new Color(1f, 0.7f, 0.25f),
                new Color(1f, 0.35f, 0.05f));
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = ps.emission;
            emission.rateOverTime = 30f;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 8f;
            shape.radius = 0.15f;
            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[] {
                    new GradientColorKey(new Color(1f, 0.8f, 0.3f), 0f),
                    new GradientColorKey(new Color(1f, 0.3f, 0f), 0.6f),
                    new GradientColorKey(new Color(0.15f, 0.15f, 0.15f), 1f),
                },
                new[] { new GradientAlphaKey(0.8f, 0f), new GradientAlphaKey(0f, 1f) });
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(grad);
            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            var curve = new AnimationCurve();
            curve.AddKey(0f, 1f);
            curve.AddKey(1f, 0.2f);
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, curve);

            // Renderer — shader additif pour l'effet lumineux
            var psr = fxGo.GetComponent<ParticleSystemRenderer>();
            var fireMat = new Material(Shader.Find("Mobile/Particles/Additive"));
            // fallback si shader introuvable
            if (fireMat.shader == null || fireMat.shader.name == "Hidden/InternalErrorShader")
                fireMat = new Material(Shader.Find("Particles/Standard Unlit"));
            psr.material = fireMat;

            // Lumière point feu (chaude, flicker géré à runtime si besoin)
            var fireLightGo = new GameObject("FireLight");
            fireLightGo.transform.SetParent(fire.transform, false);
            fireLightGo.transform.localPosition = new Vector3(0f, 0.6f, 0f);
            var fireLight = fireLightGo.AddComponent<Light>();
            fireLight.type = LightType.Point;
            fireLight.color = new Color(1f, 0.55f, 0.15f);
            fireLight.intensity = 4.5f;
            fireLight.range = 8f;
            fireLight.shadows = LightShadows.Soft;
        }

        static void BuildSocles(Transform parent)
        {
            var group = new GameObject("[Socles]");
            group.transform.SetParent(parent, false);

            var stoneMat = AssetDatabase.LoadAssetAtPath<Material>(StoneMatPath);
            for (int i = 0; i < MaxSocles; i++)
            {
                var angle = i * Mathf.PI * 2f / MaxSocles - Mathf.PI * 0.5f;
                var pos = new Vector3(
                    Mathf.Cos(angle) * SocleRingRadius,
                    0f,
                    Mathf.Sin(angle) * SocleRingRadius);
                var socle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                socle.name = $"Socle_{i}";
                socle.transform.SetParent(group.transform, false);
                socle.transform.localPosition = pos + Vector3.up * 0.15f;
                socle.transform.localScale = new Vector3(0.7f, 0.15f, 0.7f);
                if (stoneMat != null) socle.GetComponent<MeshRenderer>().sharedMaterial = stoneMat;

                // Rotation pour que l'avatar posé dessus regarde le feu
                socle.transform.LookAt(new Vector3(0, socle.transform.position.y, 0));
            }
        }

        static void BuildAvatars(Transform parent)
        {
            var group = new GameObject("[Avatars]");
            group.transform.SetParent(parent, false);

            var male = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Art/Characters/Models/Superhero_Male_FullBody.fbx");
            var female = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Art/Characters/Models/Superhero_Female_FullBody.fbx");

            for (int i = 0; i < MaxSocles; i++)
            {
                var angle = i * Mathf.PI * 2f / MaxSocles - Mathf.PI * 0.5f;
                var pos = new Vector3(
                    Mathf.Cos(angle) * SocleRingRadius,
                    0.3f,
                    Mathf.Sin(angle) * SocleRingRadius);

                GameObject go;
                if ((male != null || female != null) && i < 12)
                {
                    var prefab = (i % 2 == 0 && male != null) ? male : (female ?? male);
                    go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, group.transform);
                    // Les FBX MegaKit ont un scale à 0.01 (cm → m), on ajuste.
                    go.transform.localScale = Vector3.one;
                }
                else
                {
                    go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                    go.transform.SetParent(group.transform, false);
                    go.transform.localScale = new Vector3(0.5f, 0.9f, 0.5f);
                }
                go.name = $"Avatar_{i}";
                go.transform.position = pos;
                // Regarde le feu (orienté vers le centre)
                go.transform.LookAt(new Vector3(0f, pos.y, 0f));

                // BoxCollider pour raycast input (1.9u de haut, centré sur l'avatar)
                var col = go.GetComponent<Collider>();
                if (col == null) col = go.AddComponent<BoxCollider>();
                if (col is BoxCollider bc)
                {
                    bc.center = new Vector3(0f, 0.9f, 0f);
                    bc.size = new Vector3(0.9f, 1.9f, 0.6f);
                }

                var avatar = go.AddComponent<SceneAvatar>();
                avatar.SlotIndex = i;

                // Au démarrage les 5 premiers sont actifs, les 7 suivants cachés (spawnés on-demand)
                go.SetActive(i < 5);
            }
        }

        static void BuildGameSceneController(Transform parent)
        {
            var go = new GameObject("[GameSceneController]");
            go.transform.SetParent(parent, false);
            go.AddComponent<GameSceneController>();
        }

        static void ScatterTrees(Transform parent)
        {
            var group = new GameObject("[Trees]");
            group.transform.SetParent(parent, false);

            var trees = new[]
            {
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Environment/Nature/Models/CommonTree_1.fbx"),
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Environment/Nature/Models/CommonTree_2.fbx"),
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Environment/Nature/Models/CommonTree_3.fbx"),
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Environment/Nature/Models/CommonTree_4.fbx"),
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Environment/Nature/Models/CommonTree_5.fbx"),
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Environment/Nature/Models/DeadTree_1.fbx"),
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Environment/Nature/Models/DeadTree_2.fbx"),
            }.Where(t => t != null).ToArray();

            if (trees.Length == 0)
            {
                Debug.LogWarning("[Epic5Bootstrap] Aucun tree FBX trouvé — skip scatter.");
                return;
            }

            var rand = new System.Random(42); // seed reproductible
            for (int i = 0; i < TreeCount; i++)
            {
                var angle = (float)(rand.NextDouble() * Mathf.PI * 2f);
                var radius = TreeMinRadius + (float)rand.NextDouble() * (TreeMaxRadius - TreeMinRadius);
                var pos = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
                var prefab = trees[rand.Next(trees.Length)];
                var tree = (GameObject)PrefabUtility.InstantiatePrefab(prefab, group.transform);
                tree.transform.position = pos;
                tree.transform.rotation = Quaternion.Euler(0f, (float)(rand.NextDouble() * 360f), 0f);
                tree.transform.localScale = Vector3.one * (0.85f + (float)rand.NextDouble() * 0.4f);
            }
        }

        static void ScatterRocks(Transform parent)
        {
            var group = new GameObject("[Rocks]");
            group.transform.SetParent(parent, false);

            var rocks = new[]
            {
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Environment/Nature/Models/Rock_Medium_1.fbx"),
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Environment/Nature/Models/Rock_Medium_2.fbx"),
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Environment/Nature/Models/Rock_Medium_3.fbx"),
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Environment/Nature/Models/Pebble_Round_1.fbx"),
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Environment/Nature/Models/Pebble_Round_2.fbx"),
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Environment/Nature/Models/Pebble_Round_3.fbx"),
            }.Where(r => r != null).ToArray();

            if (rocks.Length == 0) return;

            var rand = new System.Random(17);
            for (int i = 0; i < 24; i++)
            {
                var angle = (float)(rand.NextDouble() * Mathf.PI * 2f);
                var radius = 2.5f + (float)rand.NextDouble() * 14f;
                var pos = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
                var prefab = rocks[rand.Next(rocks.Length)];
                var rock = (GameObject)PrefabUtility.InstantiatePrefab(prefab, group.transform);
                rock.transform.position = pos;
                rock.transform.rotation = Quaternion.Euler(0f, (float)(rand.NextDouble() * 360f), 0f);
                rock.transform.localScale = Vector3.one * (0.4f + (float)rand.NextDouble() * 0.6f);
            }
        }

        // ---- Helpers ----
        static Material LoadOrCreateMat(string path, System.Func<Material> factory)
        {
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null) return existing;
            var mat = factory();
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }

        static void ApplyMat(GameObject go, Material m)
        {
            foreach (var r in go.GetComponentsInChildren<MeshRenderer>())
                r.sharedMaterial = m;
        }
    }
}
