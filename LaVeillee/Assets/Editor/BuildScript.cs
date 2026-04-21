using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace LaVeillee.EditorTools
{
    public static class BuildScript
    {
        public static void BuildMacOSDevTest()
        {
            var outputDir = Path.Combine(Path.GetTempPath(), "LaVeilleeDevTest");
            if (Directory.Exists(outputDir))
                Directory.Delete(outputDir, recursive: true);
            Directory.CreateDirectory(outputDir);
            var outputPath = Path.Combine(outputDir, "LaVeilleeDevTest.app");

            var options = new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Scenes/DevTestRoom.unity" },
                locationPathName = outputPath,
                target = BuildTarget.StandaloneOSX,
                targetGroup = BuildTargetGroup.Standalone,
                options = BuildOptions.None
            };

            Debug.Log($"[BuildScript] Starting macOS dev-test build to {outputPath}");
            var report = BuildPipeline.BuildPlayer(options);

            if (report.summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[BuildScript] macOS BUILD SUCCEEDED in {report.summary.totalTime.TotalSeconds:F1}s at {outputPath}");
                EditorApplication.Exit(0);
            }
            else
            {
                Debug.LogError($"[BuildScript] macOS BUILD FAILED: result={report.summary.result}, errors={report.summary.totalErrors}");
                EditorApplication.Exit(1);
            }
        }

        public static void BuildIOS()
        {
            var outputPath = Path.Combine(Path.GetTempPath(), "LaVeilleeIOSBuild");
            if (Directory.Exists(outputPath))
                Directory.Delete(outputPath, recursive: true);
            Directory.CreateDirectory(outputPath);

            var options = new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Scenes/HelloLaVeillee.unity" },
                locationPathName = outputPath,
                target = BuildTarget.iOS,
                targetGroup = BuildTargetGroup.iOS,
                options = BuildOptions.None
            };

            Debug.Log($"[BuildScript] Starting iOS build to {outputPath}");
            var report = BuildPipeline.BuildPlayer(options);

            if (report.summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[BuildScript] iOS BUILD SUCCEEDED in {report.summary.totalTime.TotalSeconds:F1}s, " +
                          $"size {report.summary.totalSize / 1024 / 1024} MiB at {outputPath}");
                EditorApplication.Exit(0);
            }
            else
            {
                Debug.LogError($"[BuildScript] iOS BUILD FAILED: result={report.summary.result}, " +
                               $"errors={report.summary.totalErrors}");
                EditorApplication.Exit(1);
            }
        }
    }
}
