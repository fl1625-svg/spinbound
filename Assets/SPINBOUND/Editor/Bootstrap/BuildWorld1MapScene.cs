#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Spinbound.Presentation.WorldMap;
using Spinbound.UnityRuntime;

namespace Spinbound.EditorTools
{
    public static class BuildWorld1MapScene
    {
        public const string ScenePath = "Assets/SPINBOUND/Scenes/WorldMap_W01.unity";

        [MenuItem("SPINBOUND/4.0/Build World 1 Diorama Map")]
        public static string BuildAndSave()
        {
            Directory.CreateDirectory("Assets/SPINBOUND/Scenes");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "WorldMap_W01";

            ConfigureAtmosphere();
            CreateLighting();
            CreateCamera();

            var controllerGo = new GameObject("World 1 Diorama Map Controller");
            var controller = controllerGo.AddComponent<WorldMapController>();
            var hostGo = new GameObject("World Map Runtime Host");
            var host = hostGo.AddComponent<WorldMapSceneHost>();
            host.Configure(controller);

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return ScenePath;
        }

        private static void ConfigureAtmosphere()
        {
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(.52f, .74f, .98f);
            RenderSettings.ambientEquatorColor = new Color(.42f, .58f, .40f);
            RenderSettings.ambientGroundColor = new Color(.18f, .13f, .10f);
            RenderSettings.ambientIntensity = 1.18f;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(.72f, .87f, .98f);
            RenderSettings.fogDensity = .008f;
        }

        private static void CreateLighting()
        {
            var sun = new GameObject("Map Sun — Warm Key");
            var light = sun.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.55f;
            light.color = new Color(1f, .92f, .76f);
            light.shadows = LightShadows.Soft;
            light.shadowStrength = .72f;
            sun.transform.rotation = Quaternion.Euler(48f, -38f, 0f);

            var rim = new GameObject("Map Sky Fill");
            var fill = rim.AddComponent<Light>();
            fill.type = LightType.Directional;
            fill.intensity = .28f;
            fill.color = new Color(.43f, .68f, 1f);
            fill.shadows = LightShadows.None;
            rim.transform.rotation = Quaternion.Euler(60f, 142f, 0f);
        }

        private static void CreateCamera()
        {
            var go = new GameObject("World Map Camera");
            go.tag = "MainCamera";
            var camera = go.AddComponent<Camera>();
            camera.fieldOfView = 42f;
            camera.nearClipPlane = .08f;
            camera.farClipPlane = 120f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(.55f, .78f, .98f);
            go.transform.position = new Vector3(2.0f, 15.8f, -15.6f);
            go.transform.LookAt(new Vector3(2.0f, 0f, -.7f));

            var data = go.AddComponent<UniversalAdditionalCameraData>();
            data.renderPostProcessing = false;
            data.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
        }
    }
}
#endif
