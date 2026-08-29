using UnityEngine;
using Spinbound.Presentation.Art;

namespace Spinbound.Presentation.World
{
    public static class DaisyLandmarkFactory
    {
        public static Transform BuildHeartGarden(Transform parent, Vector3 center)
        {
            var root=new GameObject("Heart Garden — Recovery Landmark").transform;root.SetParent(parent,false);root.localPosition=center;
            var stone=SpinboundMaterialLibrary.CreateStylized("Heart Garden Stone",new Color(.70f,.69f,.60f),new Color(.25f,.25f,.22f),new Color(.90f,.94f,.74f),.34f,0f);
            var flower=SpinboundMaterialLibrary.CreateStylized("Heart Garden Bloom",new Color(1f,.48f,.64f),new Color(.38f,.08f,.16f),new Color(1f,.85f,.92f),.45f,0f);
            for(int i=0;i<12;i++){float a=Mathf.PI*2f*i/12f;var p=new Vector3(Mathf.Cos(a)*1.05f,.12f,Mathf.Sin(a)*1.05f);Add(root,$"Garden Stone {i:00}",ProceduralMeshFactory.CreateRock(.22f,.25f,3,8),stone,p);}
            for(int i=0;i<8;i++){float a=Mathf.PI*2f*i/8f;var p=new Vector3(Mathf.Cos(a)*.66f,.18f,Mathf.Sin(a)*.66f);var petal=Add(root,$"Bloom {i:00}",ProceduralMeshFactory.CreatePetalBlade(.12f,.46f),flower,p);petal.transform.localRotation=Quaternion.Euler(90f,-a*Mathf.Rad2Deg,0);}
            var lightGo=new GameObject("Garden Glow");lightGo.transform.SetParent(root,false);lightGo.transform.localPosition=new Vector3(0,.5f,0);var light=lightGo.AddComponent<Light>();light.type=LightType.Point;light.color=new Color(1f,.38f,.58f);light.intensity=.85f;light.range=3.0f;light.shadows=LightShadows.None;
            return root;
        }

        public static Transform BuildFinishGate(Transform parent, Vector3 center)
        {
            var root=new GameObject("Finish — Highland Bloom Gate").transform;root.SetParent(parent,false);root.localPosition=center;
            var wood=SpinboundMaterialLibrary.CreateStylized("Finish Wood",new Color(.40f,.22f,.10f),new Color(.13f,.06f,.025f),new Color(.76f,.48f,.20f),.30f,0f);
            var gold=SpinboundMaterialLibrary.CreateStylized("Finish Gold",new Color(1f,.66f,.14f),new Color(.38f,.15f,.02f),new Color(1f,.93f,.58f),.74f,.35f);
            Add(root,"Left Pier",ProceduralMeshFactory.CreateBeveledBlock("FinishPier",new Vector3(.34f,2.2f,.34f),.08f),wood,new Vector3(-1.15f,1.0f,0));
            Add(root,"Right Pier",ProceduralMeshFactory.CreateBeveledBlock("FinishPier",new Vector3(.34f,2.2f,.34f),.08f),wood,new Vector3(1.15f,1.0f,0));
            Add(root,"Crown",ProceduralMeshFactory.CreateBeveledBlock("FinishCrown",new Vector3(2.65f,.34f,.42f),.08f),wood,new Vector3(0,2.0f,0));
            Add(root,"Goal Emblem",ProceduralMeshFactory.CreateRotorHub(.34f,.12f,28),gold,new Vector3(0,2.03f,-.24f));
            return root;
        }

        private static GameObject Add(Transform parent,string name,Mesh mesh,Material material,Vector3 pos)
        {
            var go=new GameObject(name);go.transform.SetParent(parent,false);go.transform.localPosition=pos;go.AddComponent<MeshFilter>().sharedMesh=mesh;var r=go.AddComponent<MeshRenderer>();r.sharedMaterial=material;r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.On;r.receiveShadows=true;return go;
        }
    }
}
