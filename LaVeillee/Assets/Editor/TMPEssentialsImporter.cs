using System.IO;
using UnityEditor;
using UnityEngine;

namespace LaVeillee.EditorTools
{
    /// TMP_FontAsset.CreateFontAsset et TMP_Settings.defaultFontAsset dépendent tous
    /// les deux d'un TMP_Settings.asset présent dans Resources/. Sans ça, tout rendu
    /// TextMeshPro NPE. On importe automatiquement le package "TMP Essential Resources"
    /// livré avec com.unity.ugui au premier chargement de l'éditeur.
    [InitializeOnLoad]
    public static class TMPEssentialsImporter
    {
        const string InstalledMarker = "Assets/TextMesh Pro/Resources/TMP Settings.asset";

        static TMPEssentialsImporter()
        {
            EditorApplication.delayCall += TryAutoImport;
        }

        static void TryAutoImport()
        {
            if (File.Exists(InstalledMarker)) return;
            ImportNow();
        }

        [MenuItem("LaVeillee/Bootstrap/Import TMP Essentials")]
        public static void ImportNow()
        {
            var pkg = FindEssentialsPackage();
            if (pkg == null)
            {
                Debug.LogError("[LaVeillee] TMP Essentials .unitypackage introuvable dans les Packages Unity.");
                return;
            }

            Debug.Log($"[LaVeillee] Importing TMP Essentials from {pkg}");
            AssetDatabase.ImportPackage(pkg, false);
        }

        static string FindEssentialsPackage()
        {
            string[] roots =
            {
                "Packages/com.unity.ugui",
                "Packages/com.unity.textmeshpro",
            };

            foreach (var root in roots)
            {
                string full;
                try { full = Path.GetFullPath(root); }
                catch { continue; }

                var pkg = Path.Combine(full, "Package Resources", "TMP Essential Resources.unitypackage");
                if (File.Exists(pkg)) return pkg;
            }

            return null;
        }
    }
}
