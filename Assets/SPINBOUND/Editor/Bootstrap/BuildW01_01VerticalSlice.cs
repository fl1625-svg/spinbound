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

        [MenuItem("SPINBOUND/4.0/Build W01-01 Production Preview")]
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
            Debug.Log($"SPINBOUND 4.0 production preview generated: {ScenePath}. Run EditMode/PlayMode gates before calling it release-ready.");
        }

        private static void CreateAtmosphere()
        {
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(.51f,.70f,.94f);
            RenderSettings.ambientEquatorColor = new Color(.46f,.57f,.47f);
            RenderSettings.ambientGroundColor = new Color(.20f,.16f,.12f);
            RenderSettings.ambientIntensity = 1.12f;
            RenderSettings.reflectionIntensity = 1.08f;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(.69f,.84f,.96f);
            RenderSettings.fogDensity = .0042f;
        }

        private static void CreateSkybox()
        {
            var shader=Shader.Find("SPINBOUND/Highland Sky");
            if(shader==null)return;
            var sky=new Material(shader){name="Daisy Highlands Sky"};
            sky.SetColor("_HorizonColor",new Color(.76f,.90f,1f));
            sky.SetColor("_ZenithColor",new Color(.16f,.49f,.90f));
            sky.SetColor("_SunColor",new Color(1f,.90f,.60f));
            sky.SetVector("_SunDirection",new Vector4(.35f,.55f,.75f,0));
            sky.SetFloat("_SunPower",78f);
            sky.SetFloat("_SunStrength",.66f);
            RenderSettings.skybox=sky;
        }

        private static void CreateLighting()
        {
            var key = new GameObject("Sun — Warm Key");
            var light=key.AddComponent<Light>();
            light.type=LightType.Directional;
            light.intensity=1.62f;
            light.color=new Color(1f,.91f,.76f);
            light.shadows=LightShadows.Soft;
            light.shadowStrength=.78f;
            light.shadowBias=.035f;
            key.transform.rotation=Quaternion.Euler(48f,-36f,0);

            var rim = new GameObject("Sky Rim — Cool Fill");
            var fill=rim.AddComponent<Light>();
            fill.type=LightType.Directional;
            fill.intensity=.30f;
            fill.color=new Color(.48f,.74f,1f);
            fill.shadows=LightShadows.None;
            rim.transform.rotation=Quaternion.Euler(62f,145f,0);
        }

        private static Camera CreateCamera(Transform target)
        {
            var go=new GameObject("Adventure Camera — Precision Cinematic");
            go.tag="MainCamera";
            var cam=go.AddComponent<Camera>();
            cam.fieldOfView=41f;
            cam.nearClipPlane=.08f;
            cam.farClipPlane=240f;
            cam.clearFlags=CameraClearFlags.Skybox;
            go.transform.position=new Vector3(-4.2f,16.8f,-17.8f);
            go.transform.rotation=Quaternion.Euler(42f,18f,0);
            var data=go.AddComponent<UniversalAdditionalCameraData>();
            data.renderPostProcessing=true;
            data.antialiasing=AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            var rig=go.AddComponent<PrecisionCameraRig>();
            rig.Configure(target);
            return cam;
        }

        private static void CreatePostProcessing(Camera camera)
        {
            var existing=AssetDatabase.LoadAssetAtPath<VolumeProfile>(ProfilePath);
            if(existing!=null)AssetDatabase.DeleteAsset(ProfilePath);
            var profile=ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile,ProfilePath);

            var bloom=profile.Add<Bloom>();
            bloom.active=true;
            bloom.threshold.Override(.92f);
            bloom.intensity.Override(.29f);
            bloom.scatter.Override(.67f);

            var tonemap=profile.Add<Tonemapping>();
            tonemap.active=true;
            tonemap.mode.Override(TonemappingMode.ACES);

            var color=profile.Add<ColorAdjustments>();
            color.active=true;
            color.contrast.Override(9f);
            color.saturation.Override(11f);
            color.postExposure.Override(.16f);

            var white=profile.Add<WhiteBalance>();
            white.active=true;
            white.temperature.Override(4f);
            white.tint.Override(1f);

            var vignette=profile.Add<Vignette>();
            vignette.active=true;
            vignette.intensity.Override(.085f);
            vignette.smoothness.Override(.72f);

            var volumeGo=new GameObject("Global Color & Bloom");
            var volume=volumeGo.AddComponent<Volume>();
            volume.isGlobal=true;
            volume.priority=10f;
            volume.profile=profile;
        }

        private static RotorPresenter CreateRotor(out RotorFxDirector fx)
        {
            var root=new GameObject("Orbital Explorer — Player");
            var presenter=root.AddComponent<RotorPresenter>();
            var visual=RotorVisualFactory.BuildOrbitalExplorer(root.transform);
            presenter.Configure(visual);
            fx=RotorFxDirector.Build(root.transform);
            return presenter;
        }

        private static void CreateAuthoritativeObstacleArt(Transform parent)
        {
            var stone=SpinboundMaterialLibrary.CreateStylized("Course Stone",new Color(.54f,.57f,.50f),new Color(.18f,.20f,.17f),new Color(.80f,.86f,.70f),.30f,.01f);
            var moss=SpinboundMaterialLibrary.CreateStylized("Course Moss Cap",new Color(.34f,.66f,.20f),new Color(.10f,.25f,.09f),new Color(.72f,.94f,.38f),.20f,0f);
            var baseMat=SpinboundMaterialLibrary.CreateStylized("Course Base Shadow",new Color(.30f,.31f,.28f),new Color(.10f,.11f,.10f),new Color(.52f,.57f,.48f),.22f,0f);
            var root=new GameObject("Authoritative Obstacles — Visual Only");
            root.transform.SetParent(parent,false);

            foreach(var c in W01_01CourseDefinition.Colliders)
            {
                float width=c.Max.X-c.Min.X;
                float depth=c.Max.Y-c.Min.Y;
                Vector3 center=new Vector3((c.Min.X+c.Max.X)*.5f,0f,(c.Min.Y+c.Max.Y)*.5f);

                var baseGo=new GameObject($"STONE_BASE_{c.Id}");
                baseGo.transform.SetParent(root.transform,false);
                baseGo.transform.localPosition=center+Vector3.up*.14f;
                baseGo.AddComponent<MeshFilter>().sharedMesh=ProceduralMeshFactory.CreateBeveledBlock(c.Id+"_Base",new Vector3(width+.08f,.24f,depth+.08f),Mathf.Min(.10f,Mathf.Min(width,depth)*.10f));
                var baseRenderer=baseGo.AddComponent<MeshRenderer>();
                baseRenderer.sharedMaterial=baseMat;
                baseRenderer.shadowCastingMode=ShadowCastingMode.On;
                baseRenderer.receiveShadows=true;

                var go=new GameObject($"STONE_{c.Id}");
                go.transform.SetParent(root.transform,false);
                go.transform.localPosition=center+Vector3.up*.62f;
                go.AddComponent<MeshFilter>().sharedMesh=ProceduralMeshFactory.CreateBeveledBlock(c.Id,new Vector3(width,1.05f,depth),Mathf.Min(.12f,Mathf.Min(width,depth)*.12f));
                var renderer=go.AddComponent<MeshRenderer>();
                renderer.sharedMaterial=stone;
                renderer.shadowCastingMode=ShadowCastingMode.On;
                renderer.receiveShadows=true;

                var capGo=new GameObject($"MOSS_CAP_{c.Id}");
                capGo.transform.SetParent(root.transform,false);
                capGo.transform.localPosition=center+Vector3.up*1.17f;
                capGo.AddComponent<MeshFilter>().sharedMesh=ProceduralMeshFactory.CreateBeveledBlock(c.Id+"_MossCap",new Vector3(width+.04f,.10f,depth+.04f),Mathf.Min(.08f,Mathf.Min(width,depth)*.08f));
                var capRenderer=capGo.AddComponent<MeshRenderer>();
                capRenderer.sharedMaterial=moss;
                capRenderer.shadowCastingMode=ShadowCastingMode.On;
                capRenderer.receiveShadows=true;
            }
        }
    }
}
#endif
