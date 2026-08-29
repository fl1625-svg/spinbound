using System;
using UnityEngine;
using Spinbound.Presentation.Art;

namespace Spinbound.Presentation.World
{
    public sealed class DaisyHighlandsEnvironment : MonoBehaviour
    {
        [SerializeField] private int _seed=30101;
        public void Build()
        {
            ClearChildren(); UnityEngine.Random.InitState(_seed);
            var grass=SpinboundMaterialLibrary.CreateStylized("Highland Grass",new Color(.30f,.64f,.22f),new Color(.10f,.24f,.13f),new Color(.62f,.88f,.35f),.22f,0f);
            var earth=SpinboundMaterialLibrary.CreateStylized("Cliff Earth",new Color(.43f,.29f,.18f),new Color(.16f,.09f,.06f),new Color(.72f,.52f,.30f),.18f,0f);
            var stone=SpinboundMaterialLibrary.CreateStylized("Highland Stone",new Color(.47f,.50f,.46f),new Color(.18f,.21f,.20f),new Color(.72f,.78f,.70f),.34f,.02f);
            var wood=SpinboundMaterialLibrary.CreateStylized("Warm Wood",new Color(.42f,.25f,.12f),new Color(.14f,.07f,.03f),new Color(.72f,.48f,.22f),.28f,0f);
            var foliage=SpinboundMaterialLibrary.CreateFoliage("Highland Foliage",new Color(.20f,.55f,.14f),new Color(.65f,.88f,.26f),.075f);
            AddMesh("Floating Highland",ProceduralMeshFactory.CreateHighlandIsland(),grass,Vector3.zero);
            AddUndersideBand(earth);
            AddPath(stone);
            AddRocks(stone);
            AddFences(wood);
            AddFlowerFields(foliage);
            DaisyLandmarkFactory.BuildHeartGarden(transform, new Vector3(4f, .10f, 4f));
            DaisyLandmarkFactory.BuildFinishGate(transform, new Vector3(10f, .10f, 0f));
            gameObject.AddComponent<DaisyHighlandsLivingWorld>();
        }
        private void AddUndersideBand(Material mat)
        {
            var mesh=ProceduralMeshFactory.CreateHighlandIsland(22.3f,11.4f,-.06f,-2.15f,48);AddMesh("Cliff Underside",mesh,mat,new Vector3(0,-.04f,0));
        }
        private void AddPath(Material mat)
        {
            Vector3[] points={new(-8,0.08f,0),new(-5,0.08f,1.5f),new(-2.6f,0.08f,2.8f),new(1.7f,0.08f,3.4f),new(4.0f,0.08f,4f),new(6.7f,0.08f,2.2f),new(8.3f,0.08f,.7f),new(10f,0.08f,0)};
            for(int i=0;i<points.Length-1;i++){var a=points[i];var b=points[i+1];var go=CreateBoxMesh($"Path_{i:00}",b-a,new Vector3((a+b).x*.5f,.08f,(a+b).z*.5f),1.6f,.08f,mat);go.transform.rotation=Quaternion.Euler(0,-Mathf.Atan2((b-a).z,(b-a).x)*Mathf.Rad2Deg,0);}
        }
        private void AddRocks(Material mat){Vector3[] p={new(-6.8f,.4f,4.2f),new(-3.4f,.35f,-2.4f),new(2.6f,.45f,-2.6f),new(8.7f,.48f,4.1f),new(5.2f,.28f,5.3f)};for(int i=0;i<p.Length;i++){var s=.7f+UnityEngine.Random.value*.45f;var go=AddMesh("RockCluster",ProceduralMeshFactory.CreateRock(s,.7f+s*.25f),mat,p[i]);go.transform.rotation=Quaternion.Euler(0,UnityEngine.Random.Range(0,360),UnityEngine.Random.Range(-7,7));}}
        private void AddFences(Material mat){for(int i=0;i<7;i++){float x=-8.5f+i*2.9f;AddFenceSegment(new Vector3(x,.34f,-3.15f),mat);if(i<5)AddFenceSegment(new Vector3(x+1.2f,.34f,5.8f),mat);}}
        private void AddFenceSegment(Vector3 p,Material mat){CreateBoxMesh("Fence Rail",Vector3.right,p,2.2f,.10f,mat);CreateBoxMesh("Fence Post",Vector3.up,p+new Vector3(-.9f,.25f,0),.12f,.72f,mat);CreateBoxMesh("Fence Post",Vector3.up,p+new Vector3(.9f,.25f,0),.12f,.72f,mat);}
        private void AddFlowerFields(Material foliage)
        {
            for(int i=0;i<78;i++)
            {
                float x=UnityEngine.Random.Range(-9.2f,10.8f),z=UnityEngine.Random.Range(-3.0f,5.8f); if(Mathf.Abs(z)<1.25f && x<1.5f)continue;
                var root=new GameObject("Daisy").transform;root.SetParent(transform,false);root.localPosition=new Vector3(x,.10f,z);root.localRotation=Quaternion.Euler(0,UnityEngine.Random.Range(0,360),UnityEngine.Random.Range(-4f,4f));float s=UnityEngine.Random.Range(.55f,1.15f);root.localScale=Vector3.one*s;
                var blade=new GameObject("Petal");blade.transform.SetParent(root,false);blade.transform.localRotation=Quaternion.Euler(90,0,0);blade.AddComponent<MeshFilter>().sharedMesh=ProceduralMeshFactory.CreatePetalBlade(.07f,.34f);blade.AddComponent<MeshRenderer>().sharedMaterial=foliage;
            }
        }
        private GameObject CreateBoxMesh(string name,Vector3 direction,Vector3 pos,float length,float thickness,Material mat){var go=AddMesh(name,ProceduralMeshFactory.CreateRotorArm(length*.5f,thickness*.5f,thickness*.5f),mat,pos);return go;}
        private GameObject AddMesh(string name,Mesh mesh,Material mat,Vector3 pos){var go=new GameObject(name);go.transform.SetParent(transform,false);go.transform.localPosition=pos;go.AddComponent<MeshFilter>().sharedMesh=mesh;var r=go.AddComponent<MeshRenderer>();r.sharedMaterial=mat;r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.On;r.receiveShadows=true;return go;}
        private void ClearChildren(){for(int i=transform.childCount-1;i>=0;i--)DestroyImmediateSafe(transform.GetChild(i).gameObject);}
        private static void DestroyImmediateSafe(UnityEngine.Object obj){if(Application.isPlaying)Destroy(obj);else DestroyImmediate(obj);}
    }
}
