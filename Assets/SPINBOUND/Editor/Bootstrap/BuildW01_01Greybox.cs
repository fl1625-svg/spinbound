#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Spinbound.Presentation;
using Spinbound.Presentation.CameraSystem;
using Spinbound.Presentation.Quality;
using Spinbound.UnityRuntime;
using Spinbound.Worlds.W01.DaisyHighlands;

namespace Spinbound.EditorTools
{
    public static class BuildW01_01Greybox
    {
        private const string SceneFolder = "Assets/SPINBOUND/Worlds/W01/DaisyHighlands/Scenes";
        private const string ScenePath = SceneFolder + "/W01_01_Greybox.unity";

        [MenuItem("SPINBOUND/3.0/Build W01-01 Professional Greybox")]
        public static void Build()
        {
            Directory.CreateDirectory(SceneFolder);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateLighting();
            var camera = CreateCamera();
            CreateGround();
            CreateCourseBounds();
            var presenter = CreateRotor();
            camera.GetComponent<PrecisionCameraRig>().Configure(presenter.transform);
            var systems = new GameObject("Runtime Systems");
            systems.AddComponent<WebRenderQualityController>();
            var host = systems.AddComponent<UnityRotorGameHost>();
            host.Configure(presenter);

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();
            Selection.activeGameObject = host.gameObject;
            Debug.Log($"SPINBOUND 3.0 greybox created: {ScenePath}. Greybox is engineering geometry, not final art.");
        }

        private static void CreateLighting()
        {
            var lightObject = new GameObject("Key Sun");
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.35f;
            light.color = new Color(1f, 0.94f, 0.84f);
            light.shadows = LightShadows.Soft;
            lightObject.transform.rotation = Quaternion.Euler(48f, -32f, 0f);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.50f, 0.66f, 0.84f);
            RenderSettings.ambientEquatorColor = new Color(0.35f, 0.43f, 0.40f);
            RenderSettings.ambientGroundColor = new Color(0.18f, 0.16f, 0.13f);
        }

        private static GameObject CreateCamera()
        {
            var cameraObject = new GameObject("Adventure Camera");
            var camera = cameraObject.AddComponent<Camera>();
            cameraObject.tag = "MainCamera";
            camera.fieldOfView = 43f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 200f;
            cameraObject.transform.position = new Vector3(-2f, 19f, -20f);
            cameraObject.transform.rotation = Quaternion.Euler(42f, 20f, 0f);
            cameraObject.AddComponent<PrecisionCameraRig>();
            return cameraObject;
        }

        private static void CreateGround()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Greybox Highland Mass";
            ground.transform.position = new Vector3(1f, -0.6f, 1.5f);
            ground.transform.localScale = new Vector3(23f, 1.0f, 12f);
            RemovePhysicsCollider(ground);
            SetColor(ground, new Color(0.36f, 0.57f, 0.27f));

            var path = GameObject.CreatePrimitive(PrimitiveType.Cube);
            path.name = "Greybox Main Route";
            path.transform.position = new Vector3(1f, -0.04f, 1.5f);
            path.transform.localScale = new Vector3(20f, 0.08f, 7.5f);
            RemovePhysicsCollider(path);
            SetColor(path, new Color(0.58f, 0.45f, 0.28f));
        }

        private static void CreateCourseBounds()
        {
            var root = new GameObject("Authoritative Bounds Visualization");
            foreach (var c in W01_01CourseDefinition.Colliders)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = $"BOUND_{c.Id}";
                var width = c.Max.X - c.Min.X;
                var depth = c.Max.Y - c.Min.Y;
                go.transform.SetParent(root.transform);
                go.transform.position = new Vector3((c.Min.X + c.Max.X) * 0.5f, 0.7f, (c.Min.Y + c.Max.Y) * 0.5f);
                go.transform.localScale = new Vector3(width, 1.4f, depth);
                RemovePhysicsCollider(go);
                SetColor(go, new Color(0.30f, 0.25f, 0.20f));
            }
        }

        private static RotorPresenter CreateRotor()
        {
            var root = new GameObject("Rotor_Greybox");
            var presenter = root.AddComponent<RotorPresenter>();

            var arm = GameObject.CreatePrimitive(PrimitiveType.Cube);
            arm.name = "Rotor Arm";
            arm.transform.SetParent(root.transform, false);
            arm.transform.localScale = new Vector3(2.88f, 0.16f, 0.22f);
            RemovePhysicsCollider(arm);
            SetColor(arm, new Color(0.90f, 0.92f, 0.95f));

            var hub = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            hub.name = "Rotor Hub";
            hub.transform.SetParent(root.transform, false);
            hub.transform.localScale = new Vector3(0.34f, 0.10f, 0.34f);
            RemovePhysicsCollider(hub);
            SetColor(hub, new Color(0.19f, 0.55f, 0.95f));
            return presenter;
        }

        private static void RemovePhysicsCollider(GameObject go)
        {
            var collider = go.GetComponent<Collider>();
            if (collider != null) Object.DestroyImmediate(collider);
        }

        private static void SetColor(GameObject go, Color color)
        {
            var renderer = go.GetComponent<Renderer>();
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var material = new Material(shader) { color = color };
            renderer.sharedMaterial = material;
        }
    }
}
#endif
