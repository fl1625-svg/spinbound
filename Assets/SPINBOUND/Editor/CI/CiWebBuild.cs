#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Spinbound.EditorTools.CI
{
    /// <summary>
    /// Single authoritative CI entry point for SPINBOUND Web builds.
    /// It generates the same Unity scene used by the project and then builds that scene.
    /// No browser-only gameplay mirror is permitted.
    /// </summary>
    public static class CiWebBuild
    {
        private const string GeneratedScenePath = "Assets/SPINBOUND/Worlds/W01/DaisyHighlands/Scenes/W01_01_VerticalSlice.unity";
        private const string DefaultBuildPath = "build/WebGL/SPINBOUND";

        public static void Build()
        {
            bool releaseBuild = HasFlag("-spinboundRelease");
            string customBuildPath = GetArgumentValue("-customBuildPath") ?? DefaultBuildPath;
            string buildPath = Path.GetFullPath(customBuildPath);

            Debug.Log($"SPINBOUND CI: flavor={(releaseBuild ? "release" : "preview")}, output={buildPath}");

            ConfigureWebSettings(releaseBuild);
            BuildW01_01VerticalSlice.Build();

            if (!File.Exists(GeneratedScenePath))
            {
                throw new InvalidOperationException($"Generated scene was not created: {GeneratedScenePath}");
            }

            Directory.CreateDirectory(buildPath);

            var options = new BuildPlayerOptions
            {
                scenes = new[] { GeneratedScenePath },
                locationPathName = buildPath,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;
            if (summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"SPINBOUND WebGL build failed: result={summary.result}, errors={summary.totalErrors}, warnings={summary.totalWarnings}");
            }

            Debug.Log(
                $"SPINBOUND WebGL build succeeded: bytes={summary.totalSize}, duration={summary.totalTime}, output={buildPath}");
        }

        private static void ConfigureWebSettings(bool releaseBuild)
        {
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.runInBackground = true;
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
            PlayerSettings.WebGL.dataCaching = true;
            // GitHub Pages cannot guarantee Unity's required Content-Encoding: br header.
            // Preview builds therefore use Unity's JS fallback; release builds disable it
            // for hosts such as CrazyGames that are configured for native decompression.
            PlayerSettings.WebGL.decompressionFallback = !releaseBuild;
            PlayerSettings.WebGL.nameFilesAsHashes = releaseBuild;
            EditorUserBuildSettings.development = false;
        }

        private static bool HasFlag(string flag)
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (string.Equals(args[i], flag, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        private static string GetArgumentValue(string key)
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (string.Equals(args[i], key, StringComparison.OrdinalIgnoreCase))
                {
                    return args[i + 1];
                }
            }
            return null;
        }
    }
}
#endif
