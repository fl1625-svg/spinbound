using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Spinbound.Presentation.Art
{
    public static class RotorVisualFactory
    {
        private const float EndpointX = 1.47f;

        public static Transform BuildOrbitalExplorer(Transform parent)
        {
            var root = new GameObject("Orbital Explorer Visual").transform;
            root.SetParent(parent, false);

            CreateMarker("Left Endpoint Marker", root, new Vector3(-EndpointX, 0.08f, 0f));
            CreateMarker("Right Endpoint Marker", root, new Vector3(EndpointX, 0.08f, 0f));
            CreateMarker("Core Marker", root, new Vector3(0f, 0.10f, 0f));

            Material ceramic = CreateHeroMaterial("Rotor Hero Ceramic", 0f, 0.60f, 0.02f, 0.15f);
            Material metal = CreateHeroMaterial("Rotor Hero Brushed Metal", 1f, 0.76f, 0.88f, 0.08f);
            Material mechanism = CreateHeroMaterial("Rotor Hero Dark Mechanism", 2f, 0.48f, 0.46f, 0.02f);
            Material energy = CreateHeroMaterial("Rotor Hero Energy Glass", 3f, 0.88f, 0.04f, 0.95f);

            var lod0Root = CreateLodRoot("LOD0", root);
            var lod1Root = CreateLodRoot("LOD1", root);
            var lod2Root = CreateLodRoot("LOD2", root);

            List<Renderer> lod0 = BuildLod0(lod0Root, ceramic, metal, mechanism, energy);
            List<Renderer> lod1 = BuildLod1(lod1Root, ceramic, metal, mechanism, energy);
            List<Renderer> lod2 = BuildLod2(lod2Root, ceramic, mechanism, energy);

            var lodGroup = root.gameObject.AddComponent<LODGroup>();
            lodGroup.fadeMode = LODFadeMode.CrossFade;
            lodGroup.animateCrossFading = true;
            lodGroup.SetLODs(new[]
            {
                new LOD(0.48f, lod0.ToArray()),
                new LOD(0.20f, lod1.ToArray()),
                new LOD(0.055f, lod2.ToArray()),
            });
            lodGroup.RecalculateBounds();
            return root;
        }

        private static List<Renderer> BuildLod0(Transform root, Material ceramic, Material metal, Material mechanism, Material energy)
        {
            var renderers = new List<Renderer>();
            var mechanismRoot = new GameObject("Counter Rotation Mechanism").transform;
            mechanismRoot.SetParent(root, false);
            renderers.Add(AddMesh("Lower Mechanism Rail", root, ProceduralMeshFactory.CreateRotorArm(1.36f, 0.080f, 0.082f), mechanism, new Vector3(0f, -0.050f, 0f), Quaternion.identity, Vector3.one));
            renderers.Add(AddMesh("Brushed Metal Spine", root, ProceduralMeshFactory.CreateRotorArm(1.30f, 0.066f, 0.098f), metal, new Vector3(0f, 0.005f, 0f), Quaternion.identity, Vector3.one));
            renderers.Add(AddMesh("Precision Ceramic Shell", root, ProceduralMeshFactory.CreateRotorArm(1.42f, 0.122f, 0.068f), ceramic, new Vector3(0f, 0.045f, 0f), Quaternion.identity, Vector3.one));
            renderers.Add(AddMesh("Central Mechanism", mechanismRoot, ProceduralMeshFactory.CreateRotorHub(0.395f, 0.16f, 40), mechanism, new Vector3(0f, 0.025f, 0f), Quaternion.identity, Vector3.one));
            renderers.Add(AddMesh("Central Metal Bezel", mechanismRoot, ProceduralMeshFactory.CreateRotorHub(0.335f, 0.19f, 40), metal, new Vector3(0f, 0.070f, 0f), Quaternion.identity, Vector3.one));
            renderers.Add(AddMesh("Energy Halo", root, ProceduralMeshFactory.CreateRotorHub(0.285f, 0.215f, 40), energy, new Vector3(0f, 0.105f, 0f), Quaternion.identity, Vector3.one));
            renderers.Add(AddMesh("Core Ceramic Cap", root, ProceduralMeshFactory.CreateRotorHub(0.155f, 0.235f, 32), ceramic, new Vector3(0f, 0.125f, 0f), Quaternion.identity, Vector3.one));
            renderers.Add(AddMesh("Core Energy Lens", root, ProceduralMeshFactory.CreateRotorHub(0.078f, 0.255f, 28), energy, new Vector3(0f, 0.143f, 0f), Quaternion.identity, Vector3.one));
            AddDetailedPod(root, -EndpointX, ceramic, metal, mechanism, energy, renderers);
            AddDetailedPod(root, EndpointX, ceramic, metal, mechanism, energy, renderers);
            return renderers;
        }

        private static List<Renderer> BuildLod1(Transform root, Material ceramic, Material metal, Material mechanism, Material energy)
        {
            var renderers = new List<Renderer>();
            renderers.Add(AddMesh("LOD1 Mechanism", root, ProceduralMeshFactory.CreateRotorArm(1.36f, 0.075f, 0.080f), mechanism, new Vector3(0f, -0.030f, 0f), Quaternion.identity, Vector3.one));
            renderers.Add(AddMesh("LOD1 Ceramic Arm", root, ProceduralMeshFactory.CreateRotorArm(1.42f, 0.118f, 0.064f), ceramic, new Vector3(0f, 0.040f, 0f), Quaternion.identity, Vector3.one));
            renderers.Add(AddMesh("LOD1 Core", root, ProceduralMeshFactory.CreateRotorHub(0.34f, 0.20f, 28), metal, new Vector3(0f, 0.075f, 0f), Quaternion.identity, Vector3.one));
            renderers.Add(AddMesh("LOD1 Core Lens", root, ProceduralMeshFactory.CreateRotorHub(0.18f, 0.23f, 24), energy, new Vector3(0f, 0.11f, 0f), Quaternion.identity, Vector3.one));
            AddSimplePod(root, -EndpointX, ceramic, energy, renderers, 24);
            AddSimplePod(root, EndpointX, ceramic, energy, renderers, 24);
            return renderers;
        }

        private static List<Renderer> BuildLod2(Transform root, Material ceramic, Material mechanism, Material energy)
        {
            var renderers = new List<Renderer>();
            renderers.Add(AddMesh("LOD2 Silhouette Arm", root, ProceduralMeshFactory.CreateRotorArm(1.42f, 0.122f, 0.070f), ceramic, new Vector3(0f, 0.035f, 0f), Quaternion.identity, Vector3.one));
            renderers.Add(AddMesh("LOD2 Core Silhouette", root, ProceduralMeshFactory.CreateRotorHub(0.34f, 0.19f, 18), mechanism, new Vector3(0f, 0.075f, 0f), Quaternion.identity, Vector3.one));
            renderers.Add(AddMesh("LOD2 Core Glow", root, ProceduralMeshFactory.CreateRotorHub(0.16f, 0.21f, 16), energy, new Vector3(0f, 0.10f, 0f), Quaternion.identity, Vector3.one));
            AddSimplePod(root, -EndpointX, ceramic, energy, renderers, 16);
            AddSimplePod(root, EndpointX, ceramic, energy, renderers, 16);
            return renderers;
        }

        private static void AddDetailedPod(Transform root, float x, Material ceramic, Material metal, Material mechanism, Material energy, List<Renderer> renderers)
        {
            Vector3 p = new Vector3(x, 0f, 0f);
            renderers.Add(AddMesh("End Pod Mechanism", root, ProceduralMeshFactory.CreateRotorHub(0.270f, 0.17f, 32), mechanism, p + Vector3.down * 0.010f, Quaternion.identity, Vector3.one));
            renderers.Add(AddMesh("End Pod Metal Collar", root, ProceduralMeshFactory.CreateRotorHub(0.245f, 0.19f, 32), metal, p + Vector3.up * 0.020f, Quaternion.identity, Vector3.one));
            renderers.Add(AddMesh("End Pod Ceramic Shell", root, ProceduralMeshFactory.CreateRotorHub(0.218f, 0.215f, 32), ceramic, p + Vector3.up * 0.050f, Quaternion.identity, Vector3.one));
            renderers.Add(AddMesh("End Pod Energy Lens", root, ProceduralMeshFactory.CreateRotorHub(0.108f, 0.245f, 28), energy, p + Vector3.up * 0.085f, Quaternion.identity, Vector3.one));
            renderers.Add(AddMesh("End Pod Precision Cap", root, ProceduralMeshFactory.CreateRotorHub(0.050f, 0.258f, 20), metal, p + Vector3.up * 0.095f, Quaternion.identity, Vector3.one));
        }

        private static void AddSimplePod(Transform root, float x, Material ceramic, Material energy, List<Renderer> renderers, int segments)
        {
            Vector3 p = new Vector3(x, 0.045f, 0f);
            renderers.Add(AddMesh("Endpoint Shell", root, ProceduralMeshFactory.CreateRotorHub(0.23f, 0.19f, segments), ceramic, p, Quaternion.identity, Vector3.one));
            renderers.Add(AddMesh("Endpoint Lens", root, ProceduralMeshFactory.CreateRotorHub(0.10f, 0.215f, Mathf.Max(12, segments - 4)), energy, p + Vector3.up * 0.035f, Quaternion.identity, Vector3.one));
        }

        private static Transform CreateLodRoot(string name, Transform parent)
        {
            var root = new GameObject(name).transform;
            root.SetParent(parent, false);
            return root;
        }

        private static void CreateMarker(string name, Transform parent, Vector3 localPosition)
        {
            var marker = new GameObject(name).transform;
            marker.SetParent(parent, false);
            marker.localPosition = localPosition;
        }

        private static Material CreateHeroMaterial(string name, float role, float smoothness, float metallic, float emission)
        {
            Shader shader = Shader.Find("SPINBOUND/Rotor Hero") ?? Shader.Find("Universal Render Pipeline/Lit");
            var material = new Material(shader) { name = name };
            if (material.HasProperty("_Role")) material.SetFloat("_Role", role);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
            if (material.HasProperty("_EmissionStrength")) material.SetFloat("_EmissionStrength", emission);
            if (material.HasProperty("_SpeedState")) material.SetFloat("_SpeedState", 0f);
            return material;
        }

        private static MeshRenderer AddMesh(string name, Transform parent, Mesh mesh, Material material, Vector3 localPosition, Quaternion localRotation, Vector3 scale)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localRotation = localRotation;
            go.transform.localScale = scale;
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.On;
            renderer.receiveShadows = true;
            return renderer;
        }
    }
}