using UnityEngine;
using Spinbound.Presentation.Art;

namespace Spinbound.Presentation.World
{
    public sealed class DaisyHighlandsEnvironment : MonoBehaviour
    {
        [SerializeField] private int _seed=30101;

        private static readonly Vector3[] RoutePoints=
        {
            // Follow the authored safe route, but chamfer the 90-degree turns so the path
            // teaches the intended line without looking like a debug polyline.
            new Vector3(-8f,.11f,0f),new Vector3(-4.2f,.11f,0f),new Vector3(-3.35f,.11f,.45f),
            new Vector3(-3f,.11f,1.25f),new Vector3(-3f,.11f,3.25f),new Vector3(-2.35f,.11f,3.9f),
            new Vector3(3.25f,.11f,4f),new Vector3(3.9f,.11f,3.25f),new Vector3(4f,.11f,.8f),
            new Vector3(4.8f,.11f,.08f),new Vector3(10f,.11f,0f)
        };

        public void Build()
        {
            ClearChildren();
            UnityEngine.Random.InitState(_seed);

            var grass=SpinboundMaterialLibrary.CreateStylized("Highland Grass",new Color(.34f,.70f,.22f),new Color(.10f,.27f,.12f),new Color(.72f,.96f,.42f),.20f,0f);
            var grassLight=SpinboundMaterialLibrary.CreateStylized("Sunlit Grass",new Color(.47f,.79f,.27f),new Color(.13f,.31f,.12f),new Color(.82f,1f,.50f),.18f,0f);
            var earth=SpinboundMaterialLibrary.CreateStylized("Cliff Earth",new Color(.50f,.32f,.18f),new Color(.17f,.085f,.045f),new Color(.80f,.58f,.32f),.18f,0f);
            var stone=SpinboundMaterialLibrary.CreateStylized("Highland Stone",new Color(.62f,.64f,.56f),new Color(.22f,.23f,.20f),new Color(.88f,.91f,.76f),.31f,.01f);
            var path=SpinboundMaterialLibrary.CreateStylized("Garden Path",new Color(.82f,.72f,.48f),new Color(.30f,.22f,.12f),new Color(1f,.92f,.62f),.24f,0f);
            var wood=SpinboundMaterialLibrary.CreateStylized("Warm Wood",new Color(.47f,.28f,.13f),new Color(.15f,.07f,.03f),new Color(.78f,.52f,.24f),.28f,0f);
            var foliage=SpinboundMaterialLibrary.CreateFoliage("Highland Foliage",new Color(.18f,.58f,.13f),new Color(.70f,.94f,.28f),.085f);
            var whitePetal=SpinboundMaterialLibrary.CreateStylized("Daisy White",new Color(.98f,.97f,.90f),new Color(.38f,.34f,.25f),Color.white,.38f,0f);
            var pinkPetal=SpinboundMaterialLibrary.CreateStylized("Daisy Pink",new Color(1f,.54f,.70f),new Color(.42f,.10f,.20f),new Color(1f,.90f,.96f),.40f,0f);
            var bluePetal=SpinboundMaterialLibrary.CreateStylized("Daisy Blue",new Color(.52f,.76f,1f),new Color(.10f,.21f,.42f),new Color(.84f,.96f,1f),.44f,0f);
            var flowerCenter=SpinboundMaterialLibrary.CreateStylized("Daisy Gold",new Color(1f,.69f,.14f),new Color(.39f,.17f,.02f),new Color(1f,.96f,.52f),.56f,.04f);
            var cloud=SpinboundMaterialLibrary.CreateStylized("Cloud Soft",new Color(.91f,.96f,1f),new Color(.42f,.56f,.72f),Color.white,.12f,0f);
            SpinboundMaterialLibrary.ConfigureEmission(flowerCenter,new Color(1f,.48f,.05f),.18f);

            // Earth body + a shallow green crown gives the island a readable, toy-like layered silhouette.
            AddMesh("Floating Highland — Earth Body",ProceduralMeshFactory.CreateHighlandIsland(23f,12f,0f,-2.35f,52),earth,Vector3.zero,true);
            AddMesh("Floating Highland — Grass Crown",ProceduralMeshFactory.CreateHighlandIsland(22.65f,11.65f,.07f,-.19f,52),grass,new Vector3(0,.015f,0),true);
            AddCliffTerraces(earth,grassLight);
            AddPath(path,stone);
            AddRocks(stone);
            AddFences(wood);
            AddGrassTufts(foliage);
            AddFlowerClusters(foliage,whitePetal,pinkPetal,bluePetal,flowerCenter);
            AddBackgroundIslands(earth,grassLight,stone);
            AddCloudSea(cloud);

            DaisyLandmarkFactory.BuildHeartGarden(transform,new Vector3(4f,.12f,4f));
            DaisyLandmarkFactory.BuildFinishGate(transform,new Vector3(10f,.12f,0f));
            if(GetComponent<DaisyHighlandsLivingWorld>()==null) gameObject.AddComponent<DaisyHighlandsLivingWorld>();
        }

        private void AddCliffTerraces(Material earth,Material grass)
        {
            var terraceA=AddMesh("Cliff Terrace — West",ProceduralMeshFactory.CreateHighlandIsland(5.2f,3.4f,0f,-1.05f,30),earth,new Vector3(-7.1f,-1.05f,4.9f),true);
            terraceA.transform.localRotation=Quaternion.Euler(0f,-9f,0f);
            var capA=AddMesh("Cliff Terrace Grass — West",ProceduralMeshFactory.CreateHighlandIsland(4.8f,3.0f,.05f,-.14f,30),grass,new Vector3(-7.1f,-1.00f,4.9f),true);
            capA.transform.localRotation=terraceA.transform.localRotation;

            var terraceB=AddMesh("Cliff Terrace — East",ProceduralMeshFactory.CreateHighlandIsland(4.5f,3.0f,0f,-1.0f,28),earth,new Vector3(7.7f,-1.18f,-3.6f),true);
            terraceB.transform.localRotation=Quaternion.Euler(0f,16f,0f);
            var capB=AddMesh("Cliff Terrace Grass — East",ProceduralMeshFactory.CreateHighlandIsland(4.1f,2.65f,.05f,-.14f,28),grass,new Vector3(7.7f,-1.13f,-3.6f),true);
            capB.transform.localRotation=terraceB.transform.localRotation;
        }

        private void AddPath(Material pathMaterial,Material edgeMaterial)
        {
            for(int i=0;i<RoutePoints.Length-1;i++)
            {
                Vector3 a=RoutePoints[i];
                Vector3 b=RoutePoints[i+1];
                Vector3 d=b-a;
                float length=new Vector2(d.x,d.z).magnitude+.22f;
                float yaw=-Mathf.Atan2(d.z,d.x)*Mathf.Rad2Deg;
                Vector3 midpoint=(a+b)*.5f;
                midpoint.y=.115f;

                var slab=AddMesh($"Garden Path {i:00}",ProceduralMeshFactory.CreateBeveledBlock($"GardenPath_{i:00}",new Vector3(length,.105f,1.62f),.045f),pathMaterial,midpoint,true);
                slab.transform.localRotation=Quaternion.Euler(0f,yaw,0f);

                Vector3 side=new Vector3(-d.z,0f,d.x).normalized;
                for(int edge=-1;edge<=1;edge+=2)
                {
                    Vector3 edgePos=midpoint+side*(.91f*edge);
                    var border=AddMesh($"Path Edge {i:00} {edge}",ProceduralMeshFactory.CreateBeveledBlock("PathEdge",new Vector3(length,.09f,.14f),.035f),edgeMaterial,edgePos,true);
                    border.transform.localRotation=slab.transform.localRotation;
                }
            }
        }

        private void AddRocks(Material mat)
        {
            Vector3[] positions=
            {
                new Vector3(-8.5f,.42f,4.2f),new Vector3(-4.1f,.34f,-3.6f),new Vector3(1.6f,.42f,-4.2f),
                new Vector3(8.9f,.45f,4.2f),new Vector3(5.5f,.30f,5.1f),new Vector3(-9.4f,.30f,-2.5f)
            };
            for(int i=0;i<positions.Length;i++)
            {
                float s=.58f+UnityEngine.Random.value*.46f;
                var go=AddMesh("Rock Cluster",ProceduralMeshFactory.CreateRock(s,.70f+s*.28f,4,10),mat,positions[i],true);
                go.transform.localRotation=Quaternion.Euler(0,UnityEngine.Random.Range(0,360),UnityEngine.Random.Range(-6f,6f));
                go.transform.localScale=new Vector3(1f,UnityEngine.Random.Range(.82f,1.18f),UnityEngine.Random.Range(.82f,1.12f));
            }
        }

        private void AddFences(Material mat)
        {
            for(int i=0;i<6;i++)
            {
                float x=-8.1f+i*3.15f;
                AddFenceSegment(new Vector3(x,.08f,-4.15f),mat,0f);
                if(i<5) AddFenceSegment(new Vector3(x+1.15f,.08f,5.18f),mat,0f);
            }
        }

        private void AddFenceSegment(Vector3 p,Material mat,float yaw)
        {
            var root=new GameObject("Garden Fence").transform;
            root.SetParent(transform,false);
            root.localPosition=p;
            root.localRotation=Quaternion.Euler(0f,yaw,0f);
            AddMeshTo(root,"Fence Rail Low",ProceduralMeshFactory.CreateBeveledBlock("FenceRail",new Vector3(2.25f,.11f,.12f),.03f),mat,new Vector3(0,.31f,0));
            AddMeshTo(root,"Fence Rail High",ProceduralMeshFactory.CreateBeveledBlock("FenceRail",new Vector3(2.25f,.11f,.12f),.03f),mat,new Vector3(0,.59f,0));
            AddMeshTo(root,"Fence Post L",ProceduralMeshFactory.CreateBeveledBlock("FencePost",new Vector3(.16f,.86f,.16f),.035f),mat,new Vector3(-.92f,.39f,0));
            AddMeshTo(root,"Fence Post R",ProceduralMeshFactory.CreateBeveledBlock("FencePost",new Vector3(.16f,.86f,.16f),.035f),mat,new Vector3(.92f,.39f,0));
        }

        private void AddGrassTufts(Material foliage)
        {
            int placed=0;
            for(int attempt=0;attempt<420 && placed<125;attempt++)
            {
                float x=UnityEngine.Random.Range(-10.2f,10.7f);
                float z=UnityEngine.Random.Range(-4.9f,5.0f);
                if(IsNearRoute(x,z,.95f) || new Vector2(x/10.9f,z/5.15f).sqrMagnitude>1f) continue;
                var blade=new GameObject("Grass Tuft");
                blade.transform.SetParent(transform,false);
                blade.transform.localPosition=new Vector3(x,.105f,z);
                blade.transform.localRotation=Quaternion.Euler(UnityEngine.Random.Range(-4f,4f),UnityEngine.Random.Range(0f,360f),UnityEngine.Random.Range(-4f,4f));
                blade.transform.localScale=Vector3.one*UnityEngine.Random.Range(.65f,1.2f);
                blade.AddComponent<MeshFilter>().sharedMesh=ProceduralMeshFactory.CreatePetalBlade(.045f,.28f);
                var renderer=blade.AddComponent<MeshRenderer>();
                renderer.sharedMaterial=foliage;
                renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows=true;
                placed++;
            }
        }

        private void AddFlowerClusters(Material foliage,Material white,Material pink,Material blue,Material center)
        {
            Material[] petals={white,pink,blue};
            int placed=0;
            for(int attempt=0;attempt<360 && placed<30;attempt++)
            {
                float x=UnityEngine.Random.Range(-9.9f,10.2f);
                float z=UnityEngine.Random.Range(-4.6f,4.9f);
                if(IsNearRoute(x,z,1.45f) || new Vector2(x/10.6f,z/5f).sqrMagnitude>1f) continue;
                BuildDaisyCluster(new Vector3(x,.12f,z),foliage,petals[placed%petals.Length],center,UnityEngine.Random.Range(.70f,1.15f));
                placed++;
            }
        }

        private void BuildDaisyCluster(Vector3 position,Material foliage,Material petal,Material center,float scale)
        {
            var root=new GameObject("Daisy").transform;
            root.SetParent(transform,false);
            root.localPosition=position;
            root.localRotation=Quaternion.Euler(0,UnityEngine.Random.Range(0f,360f),0);
            root.localScale=Vector3.one*scale;

            float stemHeight=.34f;
            AddMeshTo(root,"Stem",ProceduralMeshFactory.CreateBeveledBlock("DaisyStem",new Vector3(.045f,stemHeight,.045f),.012f),foliage,new Vector3(0,stemHeight*.5f,0),false);
            for(int i=0;i<6;i++)
            {
                var p=AddMeshTo(root,$"Petal {i:00}",ProceduralMeshFactory.CreatePetalBlade(.085f,.27f),petal,new Vector3(0,stemHeight-.01f,0),false);
                p.transform.localRotation=Quaternion.Euler(82f,i*60f,0f);
            }
            AddMeshTo(root,"Center",ProceduralMeshFactory.CreateRotorHub(.085f,.055f,16),center,new Vector3(0,stemHeight+.015f,0),false);
        }

        private void AddBackgroundIslands(Material earth,Material grass,Material stone)
        {
            Vector3[] positions=
            {
                new Vector3(-18f,-3.2f,8f),new Vector3(17f,-3.8f,9.5f),new Vector3(-20f,-4.1f,-7f),
                new Vector3(21f,-4.5f,-5f),new Vector3(2f,-5.2f,18f)
            };
            Vector2[] sizes=
            {
                new Vector2(7.5f,4.4f),new Vector2(6.4f,3.8f),new Vector2(5.6f,3.4f),new Vector2(7.0f,4.0f),new Vector2(9.0f,4.6f)
            };
            for(int i=0;i<positions.Length;i++)
            {
                var body=AddMesh($"Background Island {i:00}",ProceduralMeshFactory.CreateHighlandIsland(sizes[i].x,sizes[i].y,0f,-1.8f,30),earth,positions[i],true);
                body.transform.localRotation=Quaternion.Euler(0f,i*19f-22f,0f);
                var cap=AddMesh($"Background Island Grass {i:00}",ProceduralMeshFactory.CreateHighlandIsland(sizes[i].x*.94f,sizes[i].y*.92f,.06f,-.15f,30),grass,positions[i]+Vector3.up*.04f,true);
                cap.transform.localRotation=body.transform.localRotation;
                var rock=AddMesh($"Background Boulder {i:00}",ProceduralMeshFactory.CreateRock(.55f,.75f,3,8),stone,positions[i]+new Vector3(sizes[i].x*.13f,.42f,0f),true);
                rock.transform.localScale=new Vector3(1.1f,1.15f,.85f);
            }
        }

        private void AddCloudSea(Material cloud)
        {
            for(int i=0;i<22;i++)
            {
                float a=Mathf.PI*2f*i/22f;
                float radius=13.5f+(i%4)*2.1f;
                Vector3 p=new Vector3(Mathf.Cos(a)*radius,-3.6f-(i%3)*.18f,Mathf.Sin(a)*radius*.62f);
                var puff=AddMesh($"Cloud Puff {i:00}",ProceduralMeshFactory.CreateRock(1.15f,.68f,3,10),cloud,p,false);
                puff.transform.localScale=new Vector3(2.1f+(i%3)*.35f,.75f,1.35f+(i%4)*.16f);
                puff.transform.localRotation=Quaternion.Euler(0f,i*23f,0f);
            }
        }

        private bool IsNearRoute(float x,float z,float clearance)
        {
            Vector2 p=new Vector2(x,z);
            for(int i=0;i<RoutePoints.Length-1;i++)
            {
                Vector2 a=new Vector2(RoutePoints[i].x,RoutePoints[i].z);
                Vector2 b=new Vector2(RoutePoints[i+1].x,RoutePoints[i+1].z);
                Vector2 ab=b-a;
                float denom=Mathf.Max(.0001f,ab.sqrMagnitude);
                float t=Mathf.Clamp01(Vector2.Dot(p-a,ab)/denom);
                if(Vector2.Distance(p,a+ab*t)<=clearance) return true;
            }
            return false;
        }

        private GameObject AddMesh(string name,Mesh mesh,Material mat,Vector3 pos,bool castShadows)
        {
            var go=new GameObject(name);
            go.transform.SetParent(transform,false);
            go.transform.localPosition=pos;
            go.AddComponent<MeshFilter>().sharedMesh=mesh;
            var r=go.AddComponent<MeshRenderer>();
            r.sharedMaterial=mat;
            r.shadowCastingMode=castShadows?UnityEngine.Rendering.ShadowCastingMode.On:UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows=true;
            return go;
        }

        private static GameObject AddMeshTo(Transform parent,string name,Mesh mesh,Material mat,Vector3 pos,bool castShadows=true)
        {
            var go=new GameObject(name);
            go.transform.SetParent(parent,false);
            go.transform.localPosition=pos;
            go.AddComponent<MeshFilter>().sharedMesh=mesh;
            var r=go.AddComponent<MeshRenderer>();
            r.sharedMaterial=mat;
            r.shadowCastingMode=castShadows?UnityEngine.Rendering.ShadowCastingMode.On:UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows=true;
            return go;
        }

        private void ClearChildren()
        {
            for(int i=transform.childCount-1;i>=0;i--) DestroyImmediateSafe(transform.GetChild(i).gameObject);
        }

        private static void DestroyImmediateSafe(UnityEngine.Object obj)
        {
            if(Application.isPlaying) Destroy(obj); else DestroyImmediate(obj);
        }
    }
}
