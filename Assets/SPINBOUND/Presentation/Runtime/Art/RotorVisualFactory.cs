using UnityEngine;

namespace Spinbound.Presentation.Art
{
    public static class RotorVisualFactory
    {
        public static Transform BuildOrbitalExplorer(Transform parent)
        {
            var root=new GameObject("Orbital Explorer Visual").transform; root.SetParent(parent,false);
            var shell=SpinboundMaterialLibrary.CreateStylized("Rotor Ceramic",new Color(.86f,.91f,.96f),new Color(.18f,.25f,.34f),new Color(.58f,.88f,1f),.56f,.05f);
            var metal=SpinboundMaterialLibrary.CreateStylized("Rotor Metal",new Color(.20f,.26f,.34f),new Color(.07f,.09f,.13f),new Color(.30f,.70f,1f),.78f,.68f);
            var accent=SpinboundMaterialLibrary.CreateStylized("Rotor Accent",new Color(.16f,.60f,1f),new Color(.04f,.16f,.35f),new Color(.65f,.95f,1f),.72f,.22f);
            AddMesh("Precision Arm",root,ProceduralMeshFactory.CreateRotorArm(),shell,Vector3.zero,Quaternion.identity,Vector3.one);
            AddMesh("Central Hub",root,ProceduralMeshFactory.CreateRotorHub(),metal,new Vector3(0,.035f,0),Quaternion.identity,Vector3.one);
            AddMesh("Energy Ring",root,ProceduralMeshFactory.CreateRotorHub(.255f,.205f,32),accent,new Vector3(0,.065f,0),Quaternion.identity,Vector3.one);
            AddPod(root,new Vector3(1.47f,0,0),shell,metal,accent);
            AddPod(root,new Vector3(-1.47f,0,0),shell,metal,accent);
            return root;
        }
        private static void AddPod(Transform root,Vector3 pos,Material shell,Material metal,Material accent)
        {
            AddMesh("End Pod Shell",root,ProceduralMeshFactory.CreateRotorHub(.225f,.18f,24),shell,pos,Quaternion.identity,Vector3.one);
            AddMesh("End Pod Core",root,ProceduralMeshFactory.CreateRotorHub(.145f,.205f,24),metal,pos+Vector3.up*.025f,Quaternion.identity,Vector3.one);
            AddMesh("End Pod Accent",root,ProceduralMeshFactory.CreateRotorHub(.085f,.22f,20),accent,pos+Vector3.up*.04f,Quaternion.identity,Vector3.one);
        }
        private static GameObject AddMesh(string name,Transform parent,Mesh mesh,Material material,Vector3 localPosition,Quaternion localRotation,Vector3 scale)
        {
            var go=new GameObject(name);go.transform.SetParent(parent,false);go.transform.localPosition=localPosition;go.transform.localRotation=localRotation;go.transform.localScale=scale;
            go.AddComponent<MeshFilter>().sharedMesh=mesh;var r=go.AddComponent<MeshRenderer>();r.sharedMaterial=material;r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.On;r.receiveShadows=true;return go;
        }
    }
}
