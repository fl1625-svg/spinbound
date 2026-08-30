using UnityEngine;
using Spinbound.Presentation.Art;

namespace Spinbound.Presentation.World
{
    public static class DaisyLandmarkFactory
    {
        public static Transform BuildHeartGarden(Transform parent, Vector3 center)
        {
            var root=new GameObject("Heart Garden — Recovery Landmark").transform;
            root.SetParent(parent,false);
            root.localPosition=center;

            var stone=SpinboundMaterialLibrary.CreateStylized("Heart Garden Stone",new Color(.73f,.72f,.63f),new Color(.26f,.25f,.22f),new Color(.94f,.96f,.80f),.34f,0f);
            var flower=SpinboundMaterialLibrary.CreateStylized("Heart Garden Bloom",new Color(1f,.48f,.66f),new Color(.38f,.07f,.16f),new Color(1f,.88f,.95f),.48f,0f);
            var core=SpinboundMaterialLibrary.CreateStylized("Heart Garden Core",new Color(1f,.72f,.28f),new Color(.42f,.16f,.025f),new Color(1f,.96f,.62f),.66f,.10f);
            var leaf=SpinboundMaterialLibrary.CreateStylized("Heart Garden Leaf",new Color(.26f,.68f,.20f),new Color(.08f,.23f,.06f),new Color(.72f,.96f,.36f),.26f,0f);
            SpinboundMaterialLibrary.ConfigureEmission(flower,new Color(1f,.20f,.46f),.48f);
            SpinboundMaterialLibrary.ConfigureEmission(core,new Color(1f,.58f,.12f),.66f);

            // Stone ring reads as a safe pocket from the gameplay camera.
            for(int i=0;i<14;i++)
            {
                float a=Mathf.PI*2f*i/14f;
                float radius=1.12f + Mathf.Sin(i*2.17f)*.06f;
                var p=new Vector3(Mathf.Cos(a)*radius,.14f,Mathf.Sin(a)*radius);
                var rock=Add(root,$"Garden Stone {i:00}",ProceduralMeshFactory.CreateRock(.22f,.27f,3,8),stone,p);
                rock.transform.localRotation=Quaternion.Euler(0f,i*31f,0f);
            }

            // A low pedestal separates the recovery landmark from ordinary decoration.
            Add(root,"Garden Pedestal",ProceduralMeshFactory.CreateBeveledBlock("HeartGardenPedestal",new Vector3(1.28f,.16f,1.28f),.14f),stone,new Vector3(0,.11f,0));

            for(int i=0;i<8;i++)
            {
                var blade=Add(root,$"Heart Leaf {i:00}",ProceduralMeshFactory.CreatePetalBlade(.15f,.58f),leaf,new Vector3(0,.22f,0));
                blade.transform.localRotation=Quaternion.Euler(84f,i*45f+22.5f,0f);
            }

            for(int i=0;i<10;i++)
            {
                float a=Mathf.PI*2f*i/10f;
                var petal=Add(root,$"Heart Petal {i:00}",ProceduralMeshFactory.CreatePetalBlade(.13f,.52f),flower,new Vector3(0,.34f,0));
                petal.transform.localRotation=Quaternion.Euler(82f,a*Mathf.Rad2Deg,0f);
            }
            Add(root,"Heart Core",ProceduralMeshFactory.CreateRotorHub(.22f,.10f,24),core,new Vector3(0,.38f,0));
            Add(root,"Heart Beacon",ProceduralMeshFactory.CreateRotorHub(.10f,.22f,20),core,new Vector3(0,.58f,0));

            var lightGo=new GameObject("Garden Glow");
            lightGo.transform.SetParent(root,false);
            lightGo.transform.localPosition=new Vector3(0,.82f,0);
            var light=lightGo.AddComponent<Light>();
            light.type=LightType.Point;
            light.color=new Color(1f,.34f,.58f);
            light.intensity=1.25f;
            light.range=3.8f;
            light.shadows=LightShadows.None;
            return root;
        }

        public static Transform BuildFinishGate(Transform parent, Vector3 center)
        {
            var root=new GameObject("Finish — Highland Bloom Gate").transform;
            root.SetParent(parent,false);
            root.localPosition=center;

            var wood=SpinboundMaterialLibrary.CreateStylized("Finish Wood",new Color(.44f,.25f,.11f),new Color(.13f,.06f,.025f),new Color(.80f,.52f,.22f),.30f,0f);
            var gold=SpinboundMaterialLibrary.CreateStylized("Finish Gold",new Color(1f,.68f,.14f),new Color(.38f,.15f,.02f),new Color(1f,.95f,.60f),.76f,.28f);
            var petal=SpinboundMaterialLibrary.CreateStylized("Finish Daisy",new Color(.98f,.96f,.86f),new Color(.40f,.31f,.19f),Color.white,.40f,0f);
            var stone=SpinboundMaterialLibrary.CreateStylized("Finish Plinth Stone",new Color(.67f,.68f,.60f),new Color(.22f,.23f,.20f),new Color(.92f,.94f,.80f),.32f,0f);
            var ribbon=SpinboundMaterialLibrary.CreateStylized("Finish Ribbon",new Color(.20f,.67f,.98f),new Color(.03f,.18f,.42f),new Color(.72f,.94f,1f),.48f,.06f);
            SpinboundMaterialLibrary.ConfigureEmission(gold,new Color(1f,.48f,.06f),.72f);
            SpinboundMaterialLibrary.ConfigureEmission(ribbon,new Color(.08f,.46f,1f),.38f);

            Add(root,"Left Plinth",ProceduralMeshFactory.CreateBeveledBlock("FinishPlinth",new Vector3(.76f,.30f,.82f),.12f),stone,new Vector3(-1.2f,.14f,0));
            Add(root,"Right Plinth",ProceduralMeshFactory.CreateBeveledBlock("FinishPlinth",new Vector3(.76f,.30f,.82f),.12f),stone,new Vector3(1.2f,.14f,0));
            Add(root,"Left Pier",ProceduralMeshFactory.CreateBeveledBlock("FinishPier",new Vector3(.38f,2.35f,.42f),.09f),wood,new Vector3(-1.2f,1.30f,0));
            Add(root,"Right Pier",ProceduralMeshFactory.CreateBeveledBlock("FinishPier",new Vector3(.38f,2.35f,.42f),.09f),wood,new Vector3(1.2f,1.30f,0));
            Add(root,"Crown",ProceduralMeshFactory.CreateBeveledBlock("FinishCrown",new Vector3(2.85f,.40f,.48f),.09f),wood,new Vector3(0,2.36f,0));

            var leftWing=Add(root,"Crown Ribbon Left",ProceduralMeshFactory.CreateBeveledBlock("FinishRibbon",new Vector3(1.06f,.18f,.12f),.05f),ribbon,new Vector3(-.82f,2.61f,-.08f));
            leftWing.transform.localRotation=Quaternion.Euler(0f,0f,8f);
            var rightWing=Add(root,"Crown Ribbon Right",ProceduralMeshFactory.CreateBeveledBlock("FinishRibbon",new Vector3(1.06f,.18f,.12f),.05f),ribbon,new Vector3(.82f,2.61f,-.08f));
            rightWing.transform.localRotation=Quaternion.Euler(0f,0f,-8f);

            Add(root,"Goal Emblem Back",ProceduralMeshFactory.CreateRotorHub(.48f,.10f,32),wood,new Vector3(0,2.38f,-.25f));
            Add(root,"Goal Emblem",ProceduralMeshFactory.CreateRotorHub(.37f,.16f,30),gold,new Vector3(0,2.38f,-.32f));
            Add(root,"Goal Core",ProceduralMeshFactory.CreateRotorHub(.15f,.19f,24),ribbon,new Vector3(0,2.39f,-.36f));

            BuildGateDaisy(root,new Vector3(-.86f,2.56f,-.04f),petal,gold,.78f);
            BuildGateDaisy(root,new Vector3(.86f,2.56f,-.04f),petal,gold,.78f);

            var glowGo=new GameObject("Finish Glow");
            glowGo.transform.SetParent(root,false);
            glowGo.transform.localPosition=new Vector3(0,2.30f,-.42f);
            var glow=glowGo.AddComponent<Light>();
            glow.type=LightType.Point;
            glow.color=new Color(1f,.68f,.20f);
            glow.intensity=1.45f;
            glow.range=4.8f;
            glow.shadows=LightShadows.None;
            return root;
        }

        private static void BuildGateDaisy(Transform parent,Vector3 position,Material petals,Material center,float scale)
        {
            var root=new GameObject("Gate Daisy").transform;
            root.SetParent(parent,false);
            root.localPosition=position;
            root.localScale=Vector3.one*scale;
            for(int i=0;i<6;i++)
            {
                var p=Add(root,$"Petal {i:00}",ProceduralMeshFactory.CreatePetalBlade(.10f,.34f),petals,Vector3.zero);
                p.transform.localRotation=Quaternion.Euler(82f,i*60f,0f);
            }
            Add(root,"Center",ProceduralMeshFactory.CreateRotorHub(.11f,.07f,18),center,new Vector3(0,.025f,0));
        }

        private static GameObject Add(Transform parent,string name,Mesh mesh,Material material,Vector3 pos)
        {
            var go=new GameObject(name);
            go.transform.SetParent(parent,false);
            go.transform.localPosition=pos;
            go.AddComponent<MeshFilter>().sharedMesh=mesh;
            var r=go.AddComponent<MeshRenderer>();
            r.sharedMaterial=material;
            r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.On;
            r.receiveShadows=true;
            return go;
        }
    }
}
