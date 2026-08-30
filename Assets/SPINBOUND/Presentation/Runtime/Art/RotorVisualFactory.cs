using UnityEngine;

namespace Spinbound.Presentation.Art
{
    public static class RotorVisualFactory
    {
        public static Transform BuildOrbitalExplorer(Transform parent)
        {
            var root=new GameObject("Orbital Explorer Visual").transform;
            root.SetParent(parent,false);

            var shell=SpinboundMaterialLibrary.CreateStylized(
                "Rotor Ceramic",new Color(.90f,.94f,.98f),new Color(.22f,.30f,.40f),new Color(.66f,.92f,1f),.58f,.04f);
            var metal=SpinboundMaterialLibrary.CreateStylized(
                "Rotor Metal",new Color(.17f,.22f,.30f),new Color(.045f,.06f,.09f),new Color(.32f,.68f,1f),.82f,.72f);
            var accent=SpinboundMaterialLibrary.CreateStylized(
                "Rotor Energy Glass",new Color(.10f,.48f,.96f),new Color(.02f,.10f,.26f),new Color(.70f,.98f,1f),.84f,.18f);
            SpinboundMaterialLibrary.ConfigureEmission(accent,new Color(.08f,.54f,1f),1.15f);

            AddMesh("Lower Mechanism",root,ProceduralMeshFactory.CreateRotorArm(1.36f,.078f,.082f),metal,new Vector3(0,-.045f,0),Quaternion.identity,Vector3.one);
            AddMesh("Precision Ceramic Arm",root,ProceduralMeshFactory.CreateRotorArm(1.42f,.12f,.065f),shell,new Vector3(0,.025f,0),Quaternion.identity,Vector3.one);
            AddMesh("Central Mechanism",root,ProceduralMeshFactory.CreateRotorHub(.38f,.15f,36),metal,new Vector3(0,.015f,0),Quaternion.identity,Vector3.one);
            AddMesh("Energy Halo",root,ProceduralMeshFactory.CreateRotorHub(.305f,.185f,36),accent,new Vector3(0,.075f,0),Quaternion.identity,Vector3.one);
            AddMesh("Core Cap",root,ProceduralMeshFactory.CreateRotorHub(.16f,.205f,28),shell,new Vector3(0,.09f,0),Quaternion.identity,Vector3.one);

            AddPod(root,new Vector3(1.47f,0,0),shell,metal,accent);
            AddPod(root,new Vector3(-1.47f,0,0),shell,metal,accent);
            return root;
        }

        private static void AddPod(Transform root,Vector3 pos,Material shell,Material metal,Material accent)
        {
            AddMesh("End Pod Mechanism",root,ProceduralMeshFactory.CreateRotorHub(.255f,.16f,28),metal,pos+Vector3.down*.015f,Quaternion.identity,Vector3.one);
            AddMesh("End Pod Ceramic Shell",root,ProceduralMeshFactory.CreateRotorHub(.222f,.19f,28),shell,pos+Vector3.up*.025f,Quaternion.identity,Vector3.one);
            AddMesh("End Pod Energy Lens",root,ProceduralMeshFactory.CreateRotorHub(.105f,.225f,24),accent,pos+Vector3.up*.065f,Quaternion.identity,Vector3.one);
            AddMesh("End Pod Cap",root,ProceduralMeshFactory.CreateRotorHub(.055f,.235f,20),metal,pos+Vector3.up*.072f,Quaternion.identity,Vector3.one);
        }

        private static GameObject AddMesh(string name,Transform parent,Mesh mesh,Material material,Vector3 localPosition,Quaternion localRotation,Vector3 scale)
        {
            var go=new GameObject(name);
            go.transform.SetParent(parent,false);
            go.transform.localPosition=localPosition;
            go.transform.localRotation=localRotation;
            go.transform.localScale=scale;
            go.AddComponent<MeshFilter>().sharedMesh=mesh;
            var r=go.AddComponent<MeshRenderer>();
            r.sharedMaterial=material;
            r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.On;
            r.receiveShadows=true;
            return go;
        }
    }
}
