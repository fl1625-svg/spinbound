using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Spinbound.Presentation.Art
{
    public static class RotorVisualFactory
    {
        private const float EndpointX = 1.47f;
        private const string ModelFolder = "Assets/SPINBOUND/Art/Models/Rotor";
        private const string MaterialFolder = "Assets/SPINBOUND/Art/Materials/Rotor";

        public static Transform BuildOrbitalExplorer(Transform parent)
        {
#if UNITY_EDITOR
            return BuildCommittedHero(parent);
#else
            return BuildRuntimeFallback(parent);
#endif
        }

#if UNITY_EDITOR
        private static Transform BuildCommittedHero(Transform parent)
        {
            var root = new GameObject("Orbital Explorer Visual").transform;
            root.SetParent(parent, false);
            CreateMarker("Left Endpoint Marker", root, new Vector3(-EndpointX, .12f, 0f));
            CreateMarker("Right Endpoint Marker", root, new Vector3(EndpointX, .12f, 0f));
            CreateMarker("Core Marker", root, new Vector3(0f, .16f, 0f));

            Material ceramic = LoadMaterial("RotorHeroCeramic.mat");
            Material metal = LoadMaterial("RotorHeroMetal.mat");
            Material mechanism = LoadMaterial("RotorHeroMechanism.mat");
            Material energy = LoadMaterial("RotorHeroEnergy.mat");

            Transform lod0 = InstantiateLod(0, root, ceramic, metal, mechanism, energy);
            Transform lod1 = InstantiateLod(1, root, ceramic, metal, mechanism, energy);
            Transform lod2 = InstantiateLod(2, root, ceramic, metal, mechanism, energy);

            Transform counterCore = FindRecursive(lod0, "CounterRotationCore");
            if (counterCore == null)
                throw new InvalidOperationException("LOD0 final Rotor model is missing CounterRotationCore.");
            var counter = new GameObject("Counter Rotation Mechanism").transform;
            counter.SetParent(lod0, false);
            counterCore.SetParent(counter, true);

            var lodGroup = root.gameObject.AddComponent<LODGroup>();
            lodGroup.fadeMode = LODFadeMode.CrossFade;
            lodGroup.animateCrossFading = true;
            lodGroup.SetLODs(new[]
            {
                new LOD(.42f, CollectRenderers(lod0)),
                new LOD(.17f, CollectRenderers(lod1)),
                new LOD(.045f, CollectRenderers(lod2)),
            });
            lodGroup.RecalculateBounds();
            return root;
        }

        private static Transform InstantiateLod(int lod, Transform root, Material ceramic, Material metal, Material mechanism, Material energy)
        {
            string path = $"{ModelFolder}/OrbitalExplorer_LOD{lod}.obj";
            GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (asset == null) throw new InvalidOperationException($"Missing final Orbital Explorer model asset: {path}");

            GameObject instance = UnityEngine.Object.Instantiate(asset);
            instance.name = $"LOD{lod}";
            instance.transform.SetParent(root, false);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;

            foreach (MeshRenderer renderer in instance.GetComponentsInChildren<MeshRenderer>(true))
            {
                renderer.sharedMaterial = ResolveMaterial(renderer.name, ceramic, metal, mechanism, energy);
                renderer.shadowCastingMode = ShadowCastingMode.On;
                renderer.receiveShadows = true;
            }
            return instance.transform;
        }

        private static Material ResolveMaterial(string name, Material ceramic, Material metal, Material mechanism, Material energy)
        {
            if (name.IndexOf("Energy", StringComparison.OrdinalIgnoreCase) >= 0) return energy;
            if (name.IndexOf("Metal", StringComparison.OrdinalIgnoreCase) >= 0) return metal;
            if (name.IndexOf("Mechanism", StringComparison.OrdinalIgnoreCase) >= 0 || name.IndexOf("Counter", StringComparison.OrdinalIgnoreCase) >= 0) return mechanism;
            return ceramic;
        }

        private static Material LoadMaterial(string fileName)
        {
            string path = $"{MaterialFolder}/{fileName}";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null) throw new InvalidOperationException($"Missing final Orbital Explorer material asset: {path}");
            if (material.shader == null || material.shader.name != "SPINBOUND/Rotor Hero")
                throw new InvalidOperationException($"Rotor material must use SPINBOUND/Rotor Hero: {path}");
            return material;
        }

        private static Renderer[] CollectRenderers(Transform root)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) throw new InvalidOperationException($"{root.name} contains no renderers.");
            return renderers;
        }
#endif

        private static Transform BuildRuntimeFallback(Transform parent)
        {
            var root = new GameObject("Orbital Explorer Visual").transform;
            root.SetParent(parent, false);
            CreateMarker("Left Endpoint Marker", root, new Vector3(-EndpointX, .08f, 0f));
            CreateMarker("Right Endpoint Marker", root, new Vector3(EndpointX, .08f, 0f));
            CreateMarker("Core Marker", root, new Vector3(0f, .10f, 0f));

            Material ceramic = CreateHeroMaterial("Rotor Hero Ceramic", 0f, .62f, .02f, .04f);
            Material mechanism = CreateHeroMaterial("Rotor Hero Mechanism", 2f, .46f, .58f, 0f);
            Material energy = CreateHeroMaterial("Rotor Hero Energy", 3f, .94f, .04f, 1f);
            var renderers = new List<Renderer>();
            Transform lod0 = CreateLodRoot("LOD0", root);
            Transform lod1 = CreateLodRoot("LOD1", root);
            Transform lod2 = CreateLodRoot("LOD2", root);
            renderers.Add(AddMesh("Fallback Shell", lod0, ProceduralMeshFactory.CreateRotorArm(1.40f, .12f, .065f), ceramic, new Vector3(0f,.05f,0f)));
            AddFallbackPods(lod0, ceramic, mechanism, energy, renderers, 20);
            Renderer[] r0 = renderers.ToArray();
            renderers.Clear();
            renderers.Add(AddMesh("Fallback Shell", lod1, ProceduralMeshFactory.CreateRotorArm(1.40f, .12f, .065f), ceramic, new Vector3(0f,.05f,0f)));
            AddFallbackPods(lod1, ceramic, mechanism, energy, renderers, 16);
            Renderer[] r1 = renderers.ToArray();
            renderers.Clear();
            renderers.Add(AddMesh("Fallback Shell", lod2, ProceduralMeshFactory.CreateRotorArm(1.40f, .12f, .065f), ceramic, new Vector3(0f,.05f,0f)));
            AddFallbackPods(lod2, ceramic, mechanism, energy, renderers, 12);
            Renderer[] r2 = renderers.ToArray();
            var counter = new GameObject("Counter Rotation Mechanism").transform;
            counter.SetParent(lod0, false);
            var counterRenderer = AddMesh("Counter Core", counter, ProceduralMeshFactory.CreateRotorHub(.22f,.07f,18), mechanism, new Vector3(0f,.16f,0f));
            var r0List = new List<Renderer>(r0) { counterRenderer };
            var group = root.gameObject.AddComponent<LODGroup>();
            group.SetLODs(new[] { new LOD(.42f,r0List.ToArray()), new LOD(.17f,r1), new LOD(.045f,r2) });
            group.RecalculateBounds();
            return root;
        }

        private static void AddFallbackPods(Transform root, Material ceramic, Material mechanism, Material energy, List<Renderer> renderers, int segments)
        {
            foreach (float x in new[] {-EndpointX, EndpointX})
            {
                Vector3 p = new Vector3(x,.08f,0f);
                renderers.Add(AddMesh("Endpoint Mechanism", root, ProceduralMeshFactory.CreateRotorHub(.27f,.12f,segments), mechanism, p));
                renderers.Add(AddMesh("Endpoint Ceramic", root, ProceduralMeshFactory.CreateRotorHub(.21f,.14f,segments), ceramic, p + Vector3.up*.04f));
                renderers.Add(AddMesh("Endpoint Energy", root, ProceduralMeshFactory.CreateRotorHub(.10f,.16f,Mathf.Max(12,segments-4)), energy, p + Vector3.up*.08f));
            }
        }

        private static Transform FindRecursive(Transform root, string name)
        {
            if (root == null) return null;
            if (root.name == name) return root;
            for (int i=0;i<root.childCount;i++) { Transform match = FindRecursive(root.GetChild(i),name); if (match != null) return match; }
            return null;
        }
        private static Transform CreateLodRoot(string name, Transform parent) { var t = new GameObject(name).transform; t.SetParent(parent,false); return t; }
        private static void CreateMarker(string name, Transform parent, Vector3 position) { var t = new GameObject(name).transform; t.SetParent(parent,false); t.localPosition = position; }
        private static Material CreateHeroMaterial(string name,float role,float smooth,float metal,float emission)
        {
            Shader shader = Shader.Find("SPINBOUND/Rotor Hero") ?? Shader.Find("Universal Render Pipeline/Lit");
            var m = new Material(shader) { name=name };
            if(m.HasProperty("_Role"))m.SetFloat("_Role",role); if(m.HasProperty("_Smoothness"))m.SetFloat("_Smoothness",smooth);
            if(m.HasProperty("_Metallic"))m.SetFloat("_Metallic",metal); if(m.HasProperty("_EmissionStrength"))m.SetFloat("_EmissionStrength",emission);
            return m;
        }
        private static MeshRenderer AddMesh(string name,Transform parent,Mesh mesh,Material material,Vector3 position)
        {
            var go=new GameObject(name); go.transform.SetParent(parent,false); go.transform.localPosition=position; go.AddComponent<MeshFilter>().sharedMesh=mesh;
            var r=go.AddComponent<MeshRenderer>(); r.sharedMaterial=material; r.shadowCastingMode=ShadowCastingMode.On; r.receiveShadows=true; return r;
        }
    }
}
