using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Game.Editor
{
    public static class BuildPlayerHelper
    {
        private static readonly string[] Scenes = new[]
        {
            "Assets/_Project/Scenes/BootstrapScene.unity",
            "Assets/_Project/Scenes/MainMenu.unity",
            "Assets/_Project/Scenes/SampleScene.unity"
        };

        public static void BuildWindows()
        {
            string outputPath = "c:/Projects/Own/SorceryStrife/build/windows/sorcery-strife.exe";
            string outputDir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            Debug.Log($"[BuildPlayerHelper] Starting Windows build to {outputPath}...");
            var options = new BuildPlayerOptions
            {
                scenes = Scenes,
                locationPathName = outputPath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[BuildPlayerHelper] Windows build succeeded: {summary.totalSize / (1024 * 1024)} MB");
            }
            else
            {
                Debug.LogError($"[BuildPlayerHelper] Windows build failed with result: {summary.result}");
                EditorApplication.Exit(1);
            }
        }

        public static void BuildAndroid()
        {
            string outputPath = "c:/Projects/Own/SorceryStrife/build/android/sorcery-strife.apk";
            string outputDir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            Debug.Log($"[BuildPlayerHelper] Starting Android build to {outputPath}...");
            var options = new BuildPlayerOptions
            {
                scenes = Scenes,
                locationPathName = outputPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[BuildPlayerHelper] Android build succeeded: {summary.totalSize / (1024 * 1024)} MB");
            }
            else
            {
                Debug.LogError($"[BuildPlayerHelper] Android build failed with result: {summary.result}");
                EditorApplication.Exit(1);
            }
        }
    }
}
