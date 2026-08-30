#if UNITY_EDITOR
using System;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Spinbound.Presentation;
using Spinbound.Presentation.Art;
using Spinbound.Presentation.CameraSystem;
using Spinbound.Presentation.Quality;
using Spinbound.Presentation.UI;
using Spinbound.Presentation.Vfx;
using Spinbound.Presentation.World;
using Spinbound.UnityRuntime;
using Spinbound.Worlds;
using Spinbound.Worlds.W01.DaisyHighlands;

namespace Spinbound.EditorTools
{
    public static class StageSceneBuilder
    {
        public static Scene Build(StageDefinition definition, StagePresentationProfile profile)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            if (profile == null) throw new ArgumentNullException(nameof(profile));

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = definition.Id + " — " + definition.DisplayName;

            CreateLighting();
            GameplayGeometryPresenter.Build(definition);

            // World 1 currently shares the calibrated Daisy Highlands presentation kit.
            // Task 4 will replace this controlled procedural preview with the final authored Daisy Meadow kit.
            var environmentRoot = new GameObject("Daisy Meadow — Presentation");
            var environment = environmentRoot.AddComponent<DaisyHighlandsEnvironment>();
            environment.Build();

            RotorPresenter presenter = CreateRotor(out RotorFxDirector fx);
            CreateCamera(presenter.transform);
            AdventureHud hud = AdventureHud.Build();

            var systems = new GameObject("Runtime Systems");
            systems.AddComponent<WebRenderQualityController>();
            var host = systems.AddComponent<UnityRotorGameHost>();
            host.Configure(presenter, hud, fx);
            host.ConfigureStageId(definition.Id);

            return scene;
        }

        private static void CreateLighting()
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(.51f, .70f, .94f);
            RenderSettings.ambientEquatorColor = new Color(.46f, .57f, .47f);
            RenderSettings.ambientGroundColor = new Color(.20f, .16f, .12f);
            RenderSettings.ambientIntensity = 1.12f;
            RenderSettings.reflectionIntensity = 1.08f;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(.69f, .84f, .96f);
            RenderSettings.fogDensity = .0042f;

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
            var rig = go.AddComponent<PrecisionCameraRig>();
            rig.Configure(target);
            return camera;
        }
    }
}
#endif
