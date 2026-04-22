using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace LaVeillee.EditorTools
{
    /// Story 5.1 — Les packs MegaKit Stylized Nature / Fantasy Village importent des
    /// FBX avec matériaux embarqués (`materialLocation: InPrefab`). Par défaut, les
    /// textures ne sont pas liées car Unity ne cherche pas dans un sibling folder
    /// `Textures/`. Résultat : tout est blanc/rose à l'écran.
    ///
    /// Cet outil :
    ///   1) Extrait les matériaux embarqués de chaque FBX en `.mat` externe.
    ///   2) Lie les textures par nom (le nom du matériau = le basename du BaseColor).
    ///   3) Shader Standard (Built-in RP) + _MainTex + _BumpMap.
    ///
    /// Idempotent : re-exécutable après un pull / ré-import.
    public static class MaterialLinker
    {
        const string BaseColorShaderProp = "_MainTex";
        const string NormalShaderProp = "_BumpMap";

        static readonly string[] ArtRoots = new[]
        {
            "Assets/Art/Environment",
            "Assets/Art/Props",
            "Assets/Art/Characters",
        };

        [MenuItem("LaVeillee/Épopée 5/Link Materials & Textures")]
        public static void LinkAll()
        {
            int fbxCount = 0, matCount = 0, texCount = 0;

            try
            {
                AssetDatabase.StartAssetEditing();

                var fbxGuids = AssetDatabase.FindAssets("t:Model", ArtRoots);
                foreach (var guid in fbxGuids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    if (!path.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase)) continue;
                    fbxCount++;

                    var (extracted, linked) = ExtractAndLink(path);
                    matCount += extracted;
                    texCount += linked;
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            Debug.Log($"[MaterialLinker] {fbxCount} FBX parcourus — {matCount} matériaux extraits, {texCount} textures liées.");
        }

        static (int extractedMats, int linkedTex) ExtractAndLink(string fbxPath)
        {
            var importer = AssetImporter.GetAtPath(fbxPath) as ModelImporter;
            if (importer == null) return (0, 0);

            var fbxDir = Path.GetDirectoryName(fbxPath).Replace('\\', '/');
            // Textures sont dans le dossier sibling "Textures/" (pattern MegaKit).
            var texDir = fbxDir.EndsWith("/Models")
                ? fbxDir.Substring(0, fbxDir.Length - "/Models".Length) + "/Textures"
                : fbxDir + "/Textures";
            var matDir = fbxDir + "/Materials";

            var embedded = AssetDatabase.LoadAllAssetsAtPath(fbxPath)
                .OfType<Material>()
                .Where(m => AssetDatabase.IsSubAsset(m))
                .ToArray();

            int extracted = 0, linked = 0;

            foreach (var embMat in embedded)
            {
                if (embMat == null || string.IsNullOrEmpty(embMat.name)) continue;

                if (!AssetDatabase.IsValidFolder(matDir))
                {
                    var parent = Path.GetDirectoryName(matDir).Replace('\\', '/');
                    AssetDatabase.CreateFolder(parent, "Materials");
                }

                var matPath = $"{matDir}/{embMat.name}.mat";
                Material mat;
                if (File.Exists(matPath))
                {
                    mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                }
                else
                {
                    var error = AssetDatabase.ExtractAsset(embMat, matPath);
                    if (!string.IsNullOrEmpty(error))
                    {
                        // Fallback : on crée un matériau standalone et on l'associe via remap.
                        mat = new Material(Shader.Find("Standard"));
                        mat.name = embMat.name;
                        AssetDatabase.CreateAsset(mat, matPath);
                    }
                    else
                    {
                        mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                    }
                    extracted++;
                }

                if (mat == null) continue;

                if (mat.shader == null || mat.shader.name != "Standard")
                    mat.shader = Shader.Find("Standard");

                if (LinkTextures(mat, embMat.name, texDir))
                    linked++;

                // Remap vers le .mat extrait — sinon le FBX continue d'utiliser l'embarqué.
                var id = new AssetImporter.SourceAssetIdentifier(embMat);
                importer.AddRemap(id, mat);
            }

            if (extracted > 0 || linked > 0)
            {
                importer.SaveAndReimport();
            }

            return (extracted, linked);
        }

        static bool LinkTextures(Material mat, string matName, string texDir)
        {
            if (!AssetDatabase.IsValidFolder(texDir)) return false;

            var baseCandidates = new[]
            {
                $"{texDir}/{matName}.png",
                $"{texDir}/{matName}_Diffuse.png",
                $"{texDir}/{matName}_BaseColor.png",
                $"{texDir}/{matName}_Albedo.png",
            };
            var normalCandidates = new[]
            {
                $"{texDir}/{matName}_Normal.png",
                $"{texDir}/{matName}_NormalMap.png",
                $"{texDir}/{matName}_N.png",
            };

            bool any = false;
            foreach (var c in baseCandidates)
            {
                if (File.Exists(c))
                {
                    var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(c);
                    if (tex != null)
                    {
                        mat.SetTexture(BaseColorShaderProp, tex);
                        any = true;
                        break;
                    }
                }
            }

            foreach (var c in normalCandidates)
            {
                if (File.Exists(c))
                {
                    // Unity a besoin que le TextureImporter soit en textureType=NormalMap.
                    var ti = AssetImporter.GetAtPath(c) as TextureImporter;
                    if (ti != null && ti.textureType != TextureImporterType.NormalMap)
                    {
                        ti.textureType = TextureImporterType.NormalMap;
                        ti.SaveAndReimport();
                    }
                    var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(c);
                    if (tex != null)
                    {
                        mat.SetTexture(NormalShaderProp, tex);
                        mat.EnableKeyword("_NORMALMAP");
                        any = true;
                        break;
                    }
                }
            }

            return any;
        }
    }
}
