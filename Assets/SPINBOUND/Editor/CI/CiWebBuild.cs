#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Spinbound.Worlds.W01.DaisyMeadow;

namespace Spinbound.EditorTools.CI
{
    /// <summary>
    /// Single authoritative CI entry point for SPINBOUND Web builds.
    /// It generates and packages the playable World 1 diorama map plus the complete StageDefinition-driven scene catalog.
    /// No browser-only gameplay mirror is permitted.
    /// </summary>
    public static class CiWebBuild
    {
        private const string DefaultBuildPath = "build/WebGL/SPINBOUND";
        private const string GeneratedRenderingFolder = "Assets/SPINBOUND/Generated/Rendering";
        private const string PipelineAssetPath = GeneratedRenderingFolder + "/SPINBOUND_Web_URP.asset";
        private const string RendererAssetPath = GeneratedRenderingFolder + "/SPINBOUND_Web_Renderer.asset";
        private const string UrpBuiltinRendererTempPath = "Assets/UniversalRenderer.asset";
        private const string DiagnosticPath = "Logs/UnityDiagnostics/CiWebBuild-report.txt";
        private const string ProjectSettingsPath = "ProjectSettings/ProjectSettings.asset";

        public static void Build()
        {
            bool releaseBuild = HasFlag("-spinboundRelease");
            string customBuildPath = GetArgumentValue("-customBuildPath") ?? DefaultBuildPath;
            string buildPath = Path.GetFullPath(customBuildPath);

            RecordDiagnostic($"BEGIN flavor={(releaseBuild ? "release" : "preview")}, output={buildPath}, cwd={Directory.GetCurrentDirectory()}");

            try
            {
                Debug.Log($"SPINBOUND CI: flavor={(releaseBuild ? "release" : "preview")}, output={buildPath}");

                ConfigureWebSettings(releaseBuild);
                RecordDiagnostic("ConfigureWebSettings completed.");

                EnsureUrpPipeline();
                RecordDiagnostic("EnsureUrpPipeline completed.");

                ValidateRequiredShaders();
                RecordDiagnostic("ValidateRequiredShaders completed.");

                BuildWorld1Scenes.BuildAll();
                BuildWorld1MapScene.BuildAndSave();
                AssertWorld1SceneBatch();
                string[] world1ScenePaths = GetWorld1BuildScenePaths();
                RecordDiagnostic($"World 1 scene generation completed. WebGL scenes={world1ScenePaths.Length}, entry={world1ScenePaths[0]}");

                Directory.CreateDirectory(buildPath);

                var options = new BuildPlayerOptions
                {
                    scenes = world1ScenePaths,
                    locationPathName = buildPath,
                    target = BuildTarget.WebGL,
                    options = BuildOptions.None
                };

                RecordDiagnostic("BuildPipeline.BuildPlayer starting.");
                BuildReport report = BuildPipeline.BuildPlayer(options);
                RecordBuildReport(report);

                BuildSummary summary = report.summary;
                if (summary.result != BuildResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"SPINBOUND WebGL build failed: result={summary.result}, errors={summary.totalErrors}, warnings={summary.totalWarnings}");
                }

                RecordDiagnostic($"SUCCESS bytes={summary.totalSize}, duration={summary.totalTime}, output={buildPath}");
                Debug.Log(
                    $"SPINBOUND WebGL build succeeded: bytes={summary.totalSize}, duration={summary.totalTime}, output={buildPath}");
            }
            catch (Exception exception)
            {
                RecordDiagnostic($"EXCEPTION {exception.GetType().FullName}: {exception.Message}\n{exception.StackTrace}");
                throw;
            }
        }

        private static string[] GetWorld1BuildScenePaths()
        {
            var paths = new string[W01ReferenceRoutes.All.Count + 1];
            paths[0] = BuildWorld1MapScene.ScenePath;
            for (int i = 0; i < W01ReferenceRoutes.All.Count; i++)
                paths[i + 1] = BuildWorld1Scenes.GetScenePath(W01ReferenceRoutes.All[i].Stage);

            int expected = World1StageSequence.ExpectedStageCount + 1;
            if (paths.Length != expected)
            {
                throw new InvalidOperationException(
                    $"World 1 WebGL scene count mismatch: expected={expected}, actual={paths.Length}");
            }

            return paths;
        }

        private static void ConfigureWebSettings(bool releaseBuild)
        {
            AssertPersistedActiveInputHandlingBoth();

            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.runInBackground = true;
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
            PlayerSettings.WebGL.dataCaching = true;
            PlayerSettings.WebGL.decompressionFallback = !releaseBuild;
            PlayerSettings.WebGL.nameFilesAsHashes = releaseBuild;
            EditorUserBuildSettings.development = false;
        }

        private static void AssertPersistedActiveInputHandlingBoth()
        {
#if !ENABLE_INPUT_SYSTEM || !ENABLE_LEGACY_INPUT_MANAGER
            throw new InvalidOperationException(
                "SPINBOUND CI requires Unity to start with Active Input Handling=Both so editor and player compile with identical input symbols.");
#else
            if (!File.Exists(ProjectSettingsPath))
            {
                throw new InvalidOperationException(
                    $"SPINBOUND CI requires a version-controlled {ProjectSettingsPath} before Unity starts.");
            }

            string settings = File.ReadAllText(ProjectSettingsPath);
            if (!settings.Contains("activeInputHandler: 2"))
            {
                throw new InvalidOperationException(
                    "SPINBOUND CI expected persisted Active Input Handling=Both (activeInputHandler: 2).");
            }

            Debug.Log("SPINBOUND CI: editor compiled with Input System + legacy Input Manager from persisted project settings.");
#endif
        }

        private static void AssertWorld1SceneBatch()
        {
            var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var guids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            AssertScene(BuildWorld1MapScene.ScenePath, paths, guids);
            foreach (W01StageRouteContract contract in W01ReferenceRoutes.All)
                AssertScene(BuildWorld1Scenes.GetScenePath(contract.Stage), paths, guids);

            int expected = W01ReferenceRoutes.All.Count + 1;
            if (paths.Count != expected || guids.Count != expected)
            {
                throw new InvalidOperationException(
                    $"World 1 scene batch count mismatch: paths={paths.Count}, guids={guids.Count}, expected={expected}");
            }

            RecordDiagnostic($"World 1 scene batch verified: scenes={paths.Count}, uniqueGuids={guids.Count}");
        }

        private static void AssertScene(string path, HashSet<string> paths, HashSet<string> guids)
        {
            if (!File.Exists(path))
                throw new InvalidOperationException($"World 1 scene batch did not create: {path}");
            if (!paths.Add(path))
                throw new InvalidOperationException($"World 1 scene batch produced a duplicate path: {path}");

            string guid = AssetDatabase.AssetPathToGUID(path);
            if (string.IsNullOrWhiteSpace(guid))
                throw new InvalidOperationException($"World 1 scene is missing an AssetDatabase GUID: {path}");
            if (!guids.Add(guid))
                throw new InvalidOperationException($"World 1 scene batch produced a duplicate GUID: {guid} ({path})");
        }

        private static void EnsureUrpPipeline()
        {
            Directory.CreateDirectory(GeneratedRenderingFolder);
            AssetDatabase.Refresh();

            DeleteAssetIfPresent(PipelineAssetPath);
            DeleteAssetIfPresent(RendererAssetPath);
            DeleteAssetIfPresent(UrpBuiltinRendererTempPath);

            var pipeline = UniversalRenderPipelineAsset.Create();
            pipeline.name = "SPINBOUND Web URP";

            ScriptableRendererData rendererData = pipeline.LoadBuiltinRendererData(RendererType.UniversalRenderer);
            if (rendererData == null)
            {
                UnityEngine.Object.DestroyImmediate(pipeline);
                throw new InvalidOperationException("SPINBOUND CI could not create the URP Universal Renderer data.");
            }

            string createdRendererPath = AssetDatabase.GetAssetPath(rendererData);
            if (string.IsNullOrEmpty(createdRendererPath))
            {
                UnityEngine.Object.DestroyImmediate(pipeline);
                throw new InvalidOperationException("SPINBOUND CI created URP renderer data without a persistent asset path.");
            }

            string moveError = AssetDatabase.MoveAsset(createdRendererPath, RendererAssetPath);
            if (!string.IsNullOrEmpty(moveError))
            {
                UnityEngine.Object.DestroyImmediate(pipeline);
                throw new InvalidOperationException($"SPINBOUND CI could not move URP renderer data: {moveError}");
            }

            pipeline.renderScale = 1f;
            pipeline.msaaSampleCount = 4;
            pipeline.supportsHDR = true;
            pipeline.useSRPBatcher = true;
            pipeline.shadowDistance = 55f;
            pipeline.shadowCascadeCount = 2;
            pipeline.mainLightShadowmapResolution = 2048;

            AssetDatabase.CreateAsset(pipeline, PipelineAssetPath);
            EditorUtility.SetDirty(pipeline);
            AssetDatabase.SaveAssets();

            GraphicsSettings.defaultRenderPipeline = pipeline;
            QualitySettings.renderPipeline = pipeline;
            AssetDatabase.SaveAssets();

            if (GraphicsSettings.currentRenderPipeline is not UniversalRenderPipelineAsset)
            {
                throw new InvalidOperationException(
                    "SPINBOUND CI expected URP to be active, but Unity is still using the Built-in Render Pipeline.");
            }

            Debug.Log($"SPINBOUND CI: active render pipeline={GraphicsSettings.currentRenderPipeline.name}");
        }

        private static void ValidateRequiredShaders()
        {
            string[] requiredShaders =
            {
                "SPINBOUND/Stylized PBR",
                "SPINBOUND/Stylized Foliage",
                "SPINBOUND/Highland Sky",
                "Universal Render Pipeline/Lit"
            };

            foreach (string shaderName in requiredShaders)
            {
                Shader shader = Shader.Find(shaderName);
                if (shader == null)
                    throw new InvalidOperationException($"SPINBOUND CI required shader was not found: {shaderName}");
                if (!shader.isSupported)
                    throw new InvalidOperationException($"SPINBOUND CI required shader is unsupported for this build: {shaderName}");
            }
        }

        private static void RecordBuildReport(BuildReport report)
        {
            if (report == null)
            {
                RecordDiagnostic("BuildPipeline.BuildPlayer returned a null BuildReport.");
                return;
            }

            BuildSummary summary = report.summary;
            RecordDiagnostic(
                $"BUILD SUMMARY result={summary.result}, errors={summary.totalErrors}, warnings={summary.totalWarnings}, bytes={summary.totalSize}, duration={summary.totalTime}");

            foreach (BuildStep step in report.steps)
            {
                foreach (BuildStepMessage message in step.messages)
                {
                    if (message.type == LogType.Error || message.type == LogType.Exception || message.type == LogType.Assert)
                        RecordDiagnostic($"BUILD MESSAGE step={step.name}, type={message.type}: {message.content}");
                }
            }
        }

        private static void RecordDiagnostic(string message)
        {
            try
            {
                string fullPath = Path.GetFullPath(DiagnosticPath);
                string directory = Path.GetDirectoryName(fullPath);
                if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
                File.AppendAllText(fullPath, $"{DateTime.UtcNow:O} {message}{Environment.NewLine}");
            }
            catch (Exception diagnosticException)
            {
                Debug.LogWarning($"SPINBOUND CI could not persist diagnostics: {diagnosticException.Message}");
            }
        }

        private static void DeleteAssetIfPresent(string path)
        {
            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path) != null && !AssetDatabase.DeleteAsset(path))
                throw new InvalidOperationException($"SPINBOUND CI could not delete stale generated asset: {path}");
        }

        private static bool HasFlag(string flag)
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (string.Equals(args[i], flag, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }

        private static string GetArgumentValue(string key)
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (string.Equals(args[i], key, StringComparison.OrdinalIgnoreCase)) return args[i + 1];
            }
            return null;
        }
    }
}
#endif
