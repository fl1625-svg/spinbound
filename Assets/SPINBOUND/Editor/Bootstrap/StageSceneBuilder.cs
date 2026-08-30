#if UNITY_EDITOR
using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Spinbound.Presentation;
using Spinbound.Presentation.Art;
using Spinbound.Presentation.CameraSystem;
using Spinbound.Presentation.Quality;
using Spinbound.Presentation.UI;
using Spinbound.Presentation.Vfx;
using Spinbound.Presentation.World;
using Spinbound.UnityRuntime;
using Spinbound.Worlds;

namespace Spinbound.EditorTools
{
    public static class StageSceneBuilder
    {
        private const string GeneratedPresentationFolder = "Assets/SPINBOUND/Generated/StagePresentation";

        public static Scene Build(StageDefinition definition, StagePresentationProfile profile)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            if (profile == null) throw new ArgumentNullException(nameof(profile));

            EnsureGeneratedPresentationFolder();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = definition.Id + " — " + definition.DisplayName;

            CreateAtmosphere();
            CreateSkybox(definition);
            CreateLighting();
            GameplayGeometryPresenter.Build(definition);

            // World 1 currently shares the calibrated Daisy Highlands presentation kit.
            // Gameplay geometry comes only from StageDefinition; Task 4 replaces this controlled
            // procedural environment with the final authored Daisy Meadow kit per stage.
            var environmentRoot = new GameObject("Daisy Meadow — Presentation");
            var environment = environmentRoot.AddComponent<DaisyHighlandsEnvironment>();
            environment.Build();
            CreateAuthoritativeObstacleArt(definition, environmentRoot.transform);

            RotorPresenter presenter = CreateRotor(out RotorFxDirector fx);
            Camera camera = CreateCamera(presenter.transform);
            CreatePostProcessing(definition, profile, camera);
            AdventureHud hud = AdventureHud.Build();

            var systems = new GameObject("Runtime Systems");
            systems.AddComponent<WebRenderQualityController>();
            var host = systems.AddComponent<UnityRotorGameHost>();
            host.Configure(presenter, hud, fx);
            host.ConfigureStageId(definition.Id);

            AssetDatabase.SaveAssets();
            return scene;
        }

        private static void CreateAtmosphere()
        {
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(.51f, .70f, .94f);
            RenderSettings.ambientEquatorColor = new Color(.46f, .57f, .47f);
            RenderSettings.ambientGroundColor = new Color(.20f, .16f, .12f);
            RenderSettings.ambientIntensity = 1.12f;
            RenderSettings.reflectionIntensity = 1.08f;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(.69f, .84f, .96f);
            RenderSettings.fogDensity = .0042f;
        }

        private static void CreateSkybox(StageDefinition definition)
        {
            Shader shader = Shader.Find("SPINBOUND/Highland Sky");
            if (shader == null)
                throw new InvalidOperationException("SPINBOUND stage builder requires the SPINBOUND/Highland Sky shader.");

            string path = $"{GeneratedPresentationFolder}/{SanitizeAssetName(definition.Id)}_Sky.mat";
            Material sky = CreateOrRefreshMaterial(path, () =>
            {
                var material = new Material(shader) { name = definition.Id + " Daisy Meadow Sky" };
                material.SetColor("_HorizonColor", new Color(.76f, .90f, 1f));
                material.SetColor("_ZenithColor", new Color(.16f, .49f, .90f));
                material.SetColor("_SunColor", new Color(1f, .90f, .60f));
                material.SetVector("_SunDirection", new Vector4(.35f, .55f, .75f, 0f));
                material.SetFloat("_SunPower", 78f);
                material.SetFloat("_SunStrength", .66f);
                return material;
            });
            RenderSettings.skybox = sky;
        }

        private static void CreateLighting()
        {
            var key = new GameObject("Sun — Warm Key");
            var light = key.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.62f;
            light.color = new Color(1f, .91f, .76f);
            light.shadows = LightShadows.Soft;
            light.shadowStrength = .78f;
            light.shadowBias = .035f;
            key.transform.rotation = Quaternion.Euler(48f, -36f, 0f);

            var rim = new GameObject("Sky Rim — Cool Fill");
            var fill = rim.AddComponent<Light>();
            fill.type = LightType.Directional;
            fill.intensity = .30f;
            fill.color = new Color(.48f, .74f, 1f);
            fill.shadows = LightShadows.None;
            rim.transform.rotation = Quaternion.Euler(62f, 145f, 0f);
        }

        private static RotorPresenter CreateRotor(out RotorFxDirector fx)
        {
            var root = new GameObject("Orbital Explorer — Player");
            var presenter = root.AddComponent<RotorPresenter>();
            var visual = RotorVisualFactory.BuildOrbitalExplorer(root.transform);
            presenter.Configure(visual);
            fx = RotorFxDirector.Build(root.transform);
            return presenter;
        }

        private static Camera CreateCamera(Transform target)
        {
            var go = new GameObject("Adventure Camera — Precision Cinematic");
            go.tag = "MainCamera";
            var camera = go.AddComponent<Camera>();
            camera.fieldOfView = 41f;
            camera.nearClipPlane = .08f;
            camera.farClipPlane = 240f;
            camera.clearFlags = CameraClearFlags.Skybox;
            go.transform.position = new Vector3(-4.2f, 16.8f, -17.8f);
            go.transform.rotation = Quaternion.Euler(42f, 18f, 0f);

            var cameraData = go.AddComponent<UniversalAdditionalCameraData>();
            cameraData.renderPostProcessing = true;
            cameraData.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;

            var rig = go.AddComponent<PrecisionCameraRig>();
            rig.Configure(target);
            return camera;
        }

        private static void CreatePostProcessing(StageDefinition definition, StagePresentationProfile stageProfile, Camera camera)
        {
            if (camera == null) throw new ArgumentNullException(nameof(camera));

            string path = $"{GeneratedPresentationFolder}/{SanitizeAssetName(definition.Id)}_PostProcess.asset";
            VolumeProfile existing = AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);
            if (existing != null)
                AssetDatabase.DeleteAsset(path);

            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, path);

            var bloom = profile.Add<Bloom>();
            bloom.active = true;
            bloom.threshold.Override(.92f);
            bloom.intensity.Override(stageProfile.ProductionPreview ? .29f : .25f);
            bloom.scatter.Override(.67f);

            var tonemap = profile.Add<Tonemapping>();
            tonemap.active = true;
            tonemap.mode.Override(TonemappingMode.ACES);

            var color = profile.Add<ColorAdjustments>();
            color.active = true;
            color.contrast.Override(9f);
            color.saturation.Override(stageProfile.ProductionPreview ? 11f : 9f);
            color.postExposure.Override(.16f);

            var white = profile.Add<WhiteBalance>();
            white.active = true;
            white.temperature.Override(4f);
            white.tint.Override(1f);

            var vignette = profile.Add<Vignette>();
            vignette.active = true;
            vignette.intensity.Override(.085f);
            vignette.smoothness.Override(.72f);

            var volumeGo = new GameObject("Global Color & Bloom");
            var volume = volumeGo.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 10f;
            volume.sharedProfile = profile;
        }

        private static void CreateAuthoritativeObstacleArt(StageDefinition definition, Transform parent)
        {
            Material stone = CreateOrRefreshMaterial(
                GeneratedPresentationFolder + "/CourseStone.mat",
                () => SpinboundMaterialLibrary.CreateStylized(
                    "Course Stone",
                    new Color(.54f, .57f, .50f),
                    new Color(.18f, .20f, .17f),
                    new Color(.80f, .86f, .70f),
                    .30f,
                    .01f));
            Material moss = CreateOrRefreshMaterial(
                GeneratedPresentationFolder + "/CourseMossCap.mat",
                () => SpinboundMaterialLibrary.CreateStylized(
                    "Course Moss Cap",
                    new Color(.34f, .66f, .20f),
                    new Color(.10f, .25f, .09f),
                    new Color(.72f, .94f, .38f),
                    .20f,
                    0f));
            Material baseMat = CreateOrRefreshMaterial(
                GeneratedPresentationFolder + "/CourseBaseShadow.mat",
                () => SpinboundMaterialLibrary.CreateStylized(
                    "Course Base Shadow",
                    new Color(.30f, .31f, .28f),
                    new Color(.10f, .11f, .10f),
                    new Color(.52f, .57f, .48f),
                    .22f,
                    0f));

            var root = new GameObject($"Stage Presentation Obstacles — {definition.Id}");
            root.transform.SetParent(parent, false);

            foreach (var collider in definition.Colliders)
            {
                float width = collider.Max.X - collider.Min.X;
                float depth = collider.Max.Y - collider.Min.Y;
                Vector3 center = new Vector3(
                    (collider.Min.X + collider.Max.X) * .5f,
                    0f,
                    (collider.Min.Y + collider.Max.Y) * .5f);

                var obstacleRoot = new GameObject($"Obstacle Visual — {collider.Id}");
                obstacleRoot.transform.SetParent(root.transform, false);
                obstacleRoot.transform.localPosition = center;
                var binding = obstacleRoot.AddComponent<StageSemanticBinding>();
                binding.Configure(collider.Id);

                AddObstacleLayer(
                    obstacleRoot.transform,
                    $"STONE_BASE_{collider.Id}",
                    ProceduralMeshFactory.CreateBeveledBlock(
                        collider.Id + "_Base",
                        new Vector3(width + .08f, .24f, depth + .08f),
                        Mathf.Min(.10f, Mathf.Min(width, depth) * .10f)),
                    baseMat,
                    .14f);

                AddObstacleLayer(
                    obstacleRoot.transform,
                    $"STONE_{collider.Id}",
                    ProceduralMeshFactory.CreateBeveledBlock(
                        collider.Id,
                        new Vector3(width, 1.05f, depth),
                        Mathf.Min(.12f, Mathf.Min(width, depth) * .12f)),
                    stone,
                    .62f);

                AddObstacleLayer(
                    obstacleRoot.transform,
                    $"MOSS_CAP_{collider.Id}",
                    ProceduralMeshFactory.CreateBeveledBlock(
                        collider.Id + "_MossCap",
                        new Vector3(width + .04f, .10f, depth + .04f),
                        Mathf.Min(.08f, Mathf.Min(width, depth) * .08f)),
                    moss,
                    1.17f);
            }
        }

        private static void AddObstacleLayer(Transform parent, string name, Mesh mesh, Material material, float height)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = Vector3.up * height;
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.On;
            renderer.receiveShadows = true;
        }

        private static Material CreateOrRefreshMaterial(string assetPath, Func<Material> factory)
        {
            Material fresh = factory();
            Material existing = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
            if (existing == null)
            {
                AssetDatabase.CreateAsset(fresh, assetPath);
                return fresh;
            }

            EditorUtility.CopySerialized(fresh, existing);
            UnityEngine.Object.DestroyImmediate(fresh);
            EditorUtility.SetDirty(existing);
            return existing;
        }

        private static void EnsureGeneratedPresentationFolder()
        {
            if (Directory.Exists(GeneratedPresentationFolder)) return;
            Directory.CreateDirectory(GeneratedPresentationFolder);
            AssetDatabase.Refresh();
        }

        private static string SanitizeAssetName(string value)
        {
            var builder = new StringBuilder(value.Length);
            foreach (char c in value)
            {
                if (char.IsLetterOrDigit(c) || c == '-' || c == '_') builder.Append(c);
                else builder.Append('_');
            }
            return builder.ToString();
        }
    }
}
#endif
