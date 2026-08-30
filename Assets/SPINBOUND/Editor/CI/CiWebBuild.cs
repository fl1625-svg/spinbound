#if UNITY_EDITOR
using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build.Profile;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

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
        private const string GeneratedRenderingFolder = "Assets/SPINBOUND/Generated/Rendering";
        private const string PipelineAssetPath = GeneratedRenderingFolder + "/SPINBOUND_Web_URP.asset";
        private const string RendererAssetPath = GeneratedRenderingFolder + "/SPINBOUND_Web_Renderer.asset";
        private const string UrpBuiltinRendererTempPath = "Assets/UniversalRenderer.asset";
        private const string DiagnosticPath = "Logs/UnityDiagnostics/CiWebBuild-report.txt";

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

                BuildW01_01VerticalSlice.Build();
                RecordDiagnostic("BuildW01_01VerticalSlice completed.");

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

        private static void ConfigureWebSettings(bool releaseBuild)
        {
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.runInBackground = true;

            // Unity 6's obsolete named PlayerSettings API can return a native sentinel for
            // activeInputHandler in source-first projects. The Input System package itself
            // resolves the effective PlayerSettings object through BuildProfile and edits the
            // serialized property directly, so CI mirrors that Unity-supported workaround.
            int activeInputHandler = SetActiveInputHandlingBoth();
            if (activeInputHandler != 2)
            {
                throw new InvalidOperationException(
                    $"SPINBOUND CI expected Active Input Handling=Both (2), got {activeInputHandler}.");
            }
            Debug.Log("SPINBOUND CI: Active Input Handling=Both (Input System + legacy Input Manager).");

            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
            PlayerSettings.WebGL.dataCaching = true;
            // GitHub Pages cannot guarantee Unity's required Content-Encoding: br header.
            // Preview builds therefore use Unity's JS fallback; release builds disable it
            // for hosts such as CrazyGames that are configured for native decompression.
            PlayerSettings.WebGL.decompressionFallback = !releaseBuild;
            PlayerSettings.WebGL.nameFilesAsHashes = releaseBuild;
            EditorUserBuildSettings.development = false;
        }

        private static int SetActiveInputHandlingBoth()
        {
            const string propertyName = "activeInputHandler";
            const int both = 2;

            Type buildProfileType = typeof(BuildProfile);
            FieldInfo globalPlayerSettingsField = buildProfileType.GetField(
                "s_GlobalPlayerSettings",
                BindingFlags.Static | BindingFlags.NonPublic);
            if (globalPlayerSettingsField == null)
            {
                throw new InvalidOperationException(
                    "SPINBOUND CI could not resolve Unity 6 global PlayerSettings through BuildProfile.");
            }

            PlayerSettings effectivePlayerSettings =
                globalPlayerSettingsField.GetValue(null) as PlayerSettings;

            BuildProfile activeBuildProfile = BuildProfile.GetActiveBuildProfile();
            if (activeBuildProfile != null)
            {
                FieldInfo profilePlayerSettingsField = buildProfileType.GetField(
                    "m_PlayerSettings",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                if (profilePlayerSettingsField == null)
                {
                    throw new InvalidOperationException(
                        "SPINBOUND CI could not resolve the active Unity 6 build-profile PlayerSettings override.");
                }

                PlayerSettings profilePlayerSettings =
                    profilePlayerSettingsField.GetValue(activeBuildProfile) as PlayerSettings;
                if (profilePlayerSettings != null)
                {
                    effectivePlayerSettings = profilePlayerSettings;
                }
            }

            if (effectivePlayerSettings == null)
            {
                throw new InvalidOperationException(
                    "SPINBOUND CI could not resolve an effective Unity 6 PlayerSettings object.");
            }

            var serializedPlayerSettings = new SerializedObject(effectivePlayerSettings);
            SerializedProperty activeInputHandler = serializedPlayerSettings.FindProperty(propertyName);
            if (activeInputHandler == null)
            {
                throw new InvalidOperationException(
                    $"SPINBOUND CI could not find serialized PlayerSettings property '{propertyName}'.");
            }

            activeInputHandler.intValue = both;
            serializedPlayerSettings.ApplyModifiedProperties();

            // Verify against a fresh SerializedObject so the assertion checks Unity's stored
            // value rather than only the local SerializedProperty cache.
            var verificationSettings = new SerializedObject(effectivePlayerSettings);
            SerializedProperty verificationProperty = verificationSettings.FindProperty(propertyName);
            if (verificationProperty == null)
            {
                throw new InvalidOperationException(
                    $"SPINBOUND CI could not re-read serialized PlayerSettings property '{propertyName}'.");
            }

            return verificationProperty.intValue;
        }

        private static void EnsureUrpPipeline()
        {
            Directory.CreateDirectory(GeneratedRenderingFolder);
            AssetDatabase.Refresh();

            // This repository is intentionally source-first and does not yet carry the full
            // Unity ProjectSettings set. Build a deterministic URP configuration explicitly
            // so CI cannot silently fall back to the Built-in Render Pipeline.
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
                {
                    throw new InvalidOperationException($"SPINBOUND CI required shader was not found: {shaderName}");
                }

                if (!shader.isSupported)
                {
                    throw new InvalidOperationException($"SPINBOUND CI required shader is unsupported for this build: {shaderName}");
                }
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
                    if (message.type == LogType.Error ||
                        message.type == LogType.Exception ||
                        message.type == LogType.Assert)
                    {
                        RecordDiagnostic($"BUILD MESSAGE step={step.name}, type={message.type}: {message.content}");
                    }
                }
            }
        }

        private static void RecordDiagnostic(string message)
        {
            try
            {
                string fullPath = Path.GetFullPath(DiagnosticPath);
                string directory = Path.GetDirectoryName(fullPath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.AppendAllText(
                    fullPath,
                    $"{DateTime.UtcNow:O} {message}{Environment.NewLine}");
            }
            catch (Exception diagnosticException)
            {
                Debug.LogWarning($"SPINBOUND CI could not persist diagnostics: {diagnosticException.Message}");
            }
        }

        private static void DeleteAssetIfPresent(string path)
        {
            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path) != null)
            {
                if (!AssetDatabase.DeleteAsset(path))
                {
                    throw new InvalidOperationException($"SPINBOUND CI could not delete stale generated asset: {path}");
                }
            }
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
