#if UNITY_EDITOR
using System.IO;
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
using Spinbound.Worlds.W01.DaisyHighlands;

namespace Spinbound.EditorTools
{
    public static class BuildW01_01VerticalSlice
    {
        private const string SceneFolder = "Assets/SPINBOUND/Worlds/W01/DaisyHighlands/Scenes";
        private const string ScenePath = SceneFolder + "/W01_01_VerticalSlice.unity";
        private const string ProfilePath = SceneFolder + "/W01_01_PostProcess.asset";

        [MenuItem("SPINBOUND/3.0/Build W01-01 AAA Vertical Slice")]
        public static void Build()
        {
            Directory.CreateDirectory(SceneFolder);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateAtmosphere();
            CreateSkybox();
            CreateLighting();
            var environmentRoot = new GameObject("Daisy Highlands — Production Environment");
            var environment = environmentRoot.AddComponent<DaisyHighlandsEnvironment>();
            environment.Build();
            CreateAuthoritativeObstacleArt(environmentRoot.transform);
            var presenter = CreateRotor(out var fx);
            var camera = CreateCamera(presenter.transform);
            CreatePostProcessing(camera);
            var hud = AdventureHud.Build();
            var systems = new GameObject("Runtime Systems");
            systems.AddComponent<WebRenderQualityController>();
            var host = systems.AddComponent<UnityRotorGameHost>();
            host.Configure(presenter, hud, fx);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeGameObject = presenter.gameObject;
            Debug.Log($"SPINBOUND 3.0 vertical slice scene generated: {ScenePath}. Run EditMode/PlayMode gates before calling it release-ready.");
        }

        private static void CreateAtmosphere()
        {
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(.42f,.64f,.88f);
            RenderSettings.ambientEquatorColor = new Color(.40f,.50f,.42f);
            RenderSettings.ambientGroundColor = new Color(.15f,.13f,.11f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(.62f,.80f,.94f);
            RenderSettings.fogDensity = .0055f;
        }


        private static void CreateSkybox()
        {
            var shader=Shader.Find("SPINBOUND/Highland Sky");
            if(shader==null)return;
            var sky=new Material(shader){name="Daisy Highlands Sky"};
            sky.SetColor("_HorizonColor",new Color(.70f,.86f,1f));
            sky.SetColor("_ZenithColor",new Color(.18f,.47f,.84f));
            sky.SetColor("_SunColor",new Color(1f,.88f,.62f));
            sky.SetVector("_SunDirection",new Vector4(.35f,.55f,.75f,0));
            sky.SetFloat("_SunPower",72f);sky.SetFloat("_SunStrength",.58f);
            RenderSettings.skybox=sky;
        }

        private static void CreateLighting()
        {
            var key = new GameObject("Sun — Warm Key");
            var light=key.AddComponent<Light>();light.type=LightType.Directional;light.intensity=1.45f;light.color=new Color(1f,.89f,.74f);light.shadows=LightShadows.Soft;light.shadowStrength=.86f;light.shadowBias=.035f;key.transform.rotation=Quaternion.Euler(48f,-36f,0);
            var rim = new GameObject("Sky Rim — Cool Fill");
            var fill=rim.AddComponent<Light>();fill.type=LightType.Directional;fill.intensity=.24f;fill.color=new Color(.44f,.70f,1f);fill.shadows=LightShadows.None;rim.transform.rotation=Quaternion.Euler(62f,145f,0);
        }

        private static Camera CreateCamera(Transform target)
        {
            var go=new GameObject("Adventure Camera — Precision Cinematic");go.tag="MainCamera";var cam=go.AddComponent<Camera>();cam.fieldOfView=40f;cam.nearClipPlane=.08f;cam.farClipPlane=220f;cam.clearFlags=CameraClearFlags.Skybox;go.transform.position=new Vector3(-4.2f,16.8f,-17.8f);go.transform.rotation=Quaternion.Euler(42f,18f,0);
            var data=go.AddComponent<UniversalAdditionalCameraData>();data.renderPostProcessing=true;data.antialiasing=AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            var rig=go.AddComponent<PrecisionCameraRig>();rig.Configure(target);return cam;
        }

        private static void CreatePostProcessing(Camera camera)
        {
            var existing=AssetDatabase.LoadAssetAtPath<VolumeProfile>(ProfilePath);if(existing!=null)AssetDatabase.DeleteAsset(ProfilePath);
            var profile=ScriptableObject.CreateInstance<VolumeProfile>();AssetDatabase.CreateAsset(profile,ProfilePath);
            var bloom=profile.Add<Bloom>();bloom.active=true;bloom.threshold.Override(1.05f);bloom.intensity.Override(.22f);bloom.scatter.Override(.62f);
            var tonemap=profile.Add<Tonemapping>();tonemap.active=true;tonemap.mode.Override(TonemappingMode.ACES);
            var color=profile.Add<ColorAdjustments>();color.active=true;color.contrast.Override(8f);color.saturation.Override(5f);color.postExposure.Override(.08f);
            var vignette=profile.Add<Vignette>();vignette.active=true;vignette.intensity.Override(.13f);vignette.smoothness.Override(.70f);
            var volumeGo=new GameObject("Global Color & Bloom");var volume=volumeGo.AddComponent<Volume>();volume.isGlobal=true;volume.priority=10f;volume.profile=profile;
        }

        private static RotorPresenter CreateRotor(out RotorFxDirector fx)
        {
            var root=new GameObject("Orbital Explorer — Player");var presenter=root.AddComponent<RotorPresenter>();var visual=RotorVisualFactory.BuildOrbitalExplorer(root.transform);presenter.Configure(visual);fx=RotorFxDirector.Build(root.transform);return presenter;
        }

        private static void CreateAuthoritativeObstacleArt(Transform parent)
        {
            var material=SpinboundMaterialLibrary.CreateStylized("Course Stone",new Color(.41f,.43f,.39f),new Color(.16f,.18f,.16f),new Color(.67f,.74f,.60f),.28f,.02f);
            var root=new GameObject("Authoritative Obstacles — Visual Only");root.transform.SetParent(parent,false);
            foreach(var c in W01_01CourseDefinition.Colliders)
            {
                float width=c.Max.X-c.Min.X, depth=c.Max.Y-c.Min.Y;
                var go=new GameObject($"STONE_{c.Id}");go.transform.SetParent(root.transform,false);go.transform.localPosition=new Vector3((c.Min.X+c.Max.X)*.5f,.56f,(c.Min.Y+c.Max.Y)*.5f);
                go.AddComponent<MeshFilter>().sharedMesh=ProceduralMeshFactory.CreateBeveledBlock(c.Id,new Vector3(width,1.12f,depth),Mathf.Min(.12f,Mathf.Min(width,depth)*.12f));
                var renderer=go.AddComponent<MeshRenderer>();renderer.sharedMaterial=material;renderer.shadowCastingMode=ShadowCastingMode.On;renderer.receiveShadows=true;
            }
        }
    }
}
#endif
