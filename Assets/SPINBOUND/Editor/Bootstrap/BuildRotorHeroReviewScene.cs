#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Spinbound.Core.Simulation;
using Spinbound.Presentation;
using Spinbound.Presentation.Art;

namespace Spinbound.EditorTools
{
    public static class BuildRotorHeroReviewScene
    {
        public const string ScenePath = "Assets/SPINBOUND/Scenes/Reviews/RotorHeroReview.unity";
        public const string CaptureFolder = "Logs/UnityDiagnostics/RotorHero";

        [MenuItem("SPINBOUND/Build/Rotor Hero Review Scene")]
        public static void Build()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "RotorHeroReview";

            Camera camera78 = CreateCamera("Review Camera 78deg", new Vector3(0f, 10f, -2.2f), new Vector3(78f, 0f, 0f), true);
            Camera camera45 = CreateCamera("Review Camera 45deg", new Vector3(0f, 6.5f, -6.5f), new Vector3(45f, 0f, 0f), false);
            camera45.gameObject.SetActive(false);

            CreateLighting();
            CreateNeutralStage();
            CreateHeroStation("Speed 1", new Vector3(-4.0f, 0f, 0f), SpeedTier.Speed1);
            CreateHeroStation("Speed 2", Vector3.zero, SpeedTier.Speed2);
            CreateHeroStation("Speed 3", new Vector3(4.0f, 0f, 0f), SpeedTier.Speed3);

            string directory = Path.GetDirectoryName(ScenePath);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();
            Selection.activeGameObject = camera78.gameObject;
        }

        public static void CaptureForCi()
        {
            Build();
            string folder = Path.GetFullPath(CaptureFolder);
            Directory.CreateDirectory(folder);
            CaptureCamera("Review Camera 78deg", Path.Combine(folder, "OrbitalExplorer-78deg.png"));
            CaptureCamera("Review Camera 45deg", Path.Combine(folder, "OrbitalExplorer-45deg.png"));
        }

        private static void CaptureCamera(string cameraName, string outputPath)
        {
            Camera camera = Resources.FindObjectsOfTypeAll<Camera>()
                .FirstOrDefault(candidate => candidate != null && candidate.name == cameraName &&
                    candidate.gameObject.scene.IsValid() && candidate.gameObject.scene == SceneManager.GetActiveScene());
            if (camera == null)
                throw new InvalidOperationException($"Rotor Hero review camera not found: {cameraName}");

            bool wasActive = camera.gameObject.activeSelf;
            camera.gameObject.SetActive(true);
            const int width = 1600;
            const int height = 900;
            var target = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32)
            {
                name = cameraName + " Capture",
                antiAliasing = 4
            };
            target.Create();

            var request = new RenderPipeline.StandardRequest { destination = target };
            RenderPipeline.SubmitRenderRequest(camera, request);

            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = target;
            var image = new Texture2D(width, height, TextureFormat.RGB24, false, false);
            image.ReadPixels(new Rect(0f, 0f, width, height), 0, 0, false);
            image.Apply(false, false);
            File.WriteAllBytes(outputPath, image.EncodeToPNG());

            RenderTexture.active = previous;
            target.Release();
            UnityEngine.Object.DestroyImmediate(image);
            UnityEngine.Object.DestroyImmediate(target);
            camera.gameObject.SetActive(wasActive);
            Debug.Log($"SPINBOUND Hero review capture: {outputPath}");
        }

        private static void CreateHeroStation(string label, Vector3 position, SpeedTier tier)
        {
            var root = new GameObject(label);
            root.transform.position = position;
            var presenter = root.AddComponent<RotorPresenter>();
            Transform visual = RotorVisualFactory.BuildOrbitalExplorer(root.transform);
            presenter.Configure(visual);
            presenter.SetSpeedTier(tier);
            presenter.AdvancePresentation(tier == SpeedTier.Speed3 ? 0.22f : 0.10f);
            visual.GetComponent<LODGroup>()?.ForceLOD(0);

            var labelObject = new GameObject(label + " Label");
            labelObject.transform.position = position + new Vector3(0f, 0.05f, -1.35f);
            labelObject.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            TextMesh text = labelObject.AddComponent<TextMesh>();
            text.text = label;
            text.fontSize = 48;
            text.characterSize = 0.08f;
            text.anchor = TextAnchor.MiddleCenter;
            text.color = new Color(0.68f, 0.73f, 0.80f);
        }

        private static void CreateNeutralStage()
        {
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Neutral Review Plinth";
            floor.transform.position = new Vector3(0f, -0.22f, 0f);
            floor.transform.localScale = new Vector3(13f, 0.35f, 4.8f);
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = "Rotor Review Neutral" };
            material.SetColor("_BaseColor", new Color(0.105f, 0.12f, 0.145f));
            material.SetFloat("_Smoothness", 0.18f);
            floor.GetComponent<MeshRenderer>().sharedMaterial = material;
        }

        private static void CreateLighting()
        {
            Light key = CreateDirectional("Warm Key", new Vector3(48f, -34f, 0f), new Color(1f, 0.88f, 0.73f), 1.25f);
            key.shadows = LightShadows.Soft;
            CreateDirectional("Cool Fill", new Vector3(55f, 145f, 0f), new Color(0.53f, 0.72f, 1f), 0.54f);

            var rimObject = new GameObject("Energy Rim");
            Light rim = rimObject.AddComponent<Light>();
            rim.type = LightType.Point;
            rim.transform.position = new Vector3(0f, 3.5f, 2.4f);
            rim.color = new Color(0.25f, 0.68f, 1f);
            rim.intensity = 7.5f;
            rim.range = 9f;
            rim.shadows = LightShadows.None;

            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.15f, 0.18f, 0.23f);
            RenderSettings.fog = false;
        }

        private static Light CreateDirectional(string name, Vector3 euler, Color color, float intensity)
        {
            var go = new GameObject(name);
            go.transform.rotation = Quaternion.Euler(euler);
            Light light = go.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = color;
            light.intensity = intensity;
            return light;
        }

        private static Camera CreateCamera(string name, Vector3 position, Vector3 euler, bool main)
        {
            var go = new GameObject(name);
            go.transform.position = position;
            go.transform.rotation = Quaternion.Euler(euler);
            Camera camera = go.AddComponent<Camera>();
            camera.fieldOfView = 38f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.035f, 0.045f, 0.065f);
            camera.nearClipPlane = 0.05f;
            camera.farClipPlane = 80f;
            if (main) go.tag = "MainCamera";
            return camera;
        }
    }
}
#endif
