#if UNITY_EDITOR
using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Spinbound.Core.Simulation;
using Spinbound.Presentation;
using Spinbound.Presentation.Art;
using NumericsVector2 = System.Numerics.Vector2;

namespace Spinbound.EditorTools.Tests.EditMode
{
    public sealed class RotorHeroTests
    {
        [Test]
        public void RotorHeroShader_ExistsWithProductionStateProperties()
        {
            Shader shader = Shader.Find("SPINBOUND/Rotor Hero");
            Assert.That(shader, Is.Not.Null, "Final Orbital Explorer requires the dedicated SPINBOUND/Rotor Hero shader.");

            var material = new Material(shader);
            try
            {
                Assert.That(material.HasProperty("_CeramicColor"), Is.True);
                Assert.That(material.HasProperty("_MetalColor"), Is.True);
                Assert.That(material.HasProperty("_MechanismColor"), Is.True);
                Assert.That(material.HasProperty("_EnergyColor"), Is.True);
                Assert.That(material.HasProperty("_EmissionStrength"), Is.True);
                Assert.That(material.HasProperty("_SpeedState"), Is.True);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(material);
            }
        }

        [Test]
        public void OrbitalExplorer_HasThreeLodsAndReadableEndpointCoreMarkers()
        {
            var parent = new GameObject("Rotor Hero Test Root");
            try
            {
                Transform visual = RotorVisualFactory.BuildOrbitalExplorer(parent.transform);
                Assert.That(visual, Is.Not.Null);

                LODGroup lodGroup = visual.GetComponent<LODGroup>();
                Assert.That(lodGroup, Is.Not.Null, "Orbital Explorer hero must own an LODGroup.");
                LOD[] lods = lodGroup.GetLODs();
                Assert.That(lods.Length, Is.EqualTo(3), "Hero requires LOD0/LOD1/LOD2.");
                Assert.That(lods.All(lod => lod.renderers != null && lod.renderers.Length > 0), Is.True);

                Assert.That(FindRecursive(visual, "Left Endpoint Marker"), Is.Not.Null);
                Assert.That(FindRecursive(visual, "Right Endpoint Marker"), Is.Not.Null);
                Assert.That(FindRecursive(visual, "Core Marker"), Is.Not.Null);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(parent);
            }
        }

        [Test]
        public void FinalOrbitalExplorer_UsesCommittedModelAndMaterialAssets()
        {
            string[] modelPaths =
            {
                "Assets/SPINBOUND/Art/Models/Rotor/OrbitalExplorer_LOD0.obj",
                "Assets/SPINBOUND/Art/Models/Rotor/OrbitalExplorer_LOD1.obj",
                "Assets/SPINBOUND/Art/Models/Rotor/OrbitalExplorer_LOD2.obj",
            };
            foreach (string path in modelPaths)
                Assert.That(AssetDatabase.LoadAssetAtPath<GameObject>(path), Is.Not.Null, $"Missing committed final Rotor model asset: {path}");

            string[] materialPaths =
            {
                "Assets/SPINBOUND/Art/Materials/Rotor/RotorHeroCeramic.mat",
                "Assets/SPINBOUND/Art/Materials/Rotor/RotorHeroMetal.mat",
                "Assets/SPINBOUND/Art/Materials/Rotor/RotorHeroMechanism.mat",
                "Assets/SPINBOUND/Art/Materials/Rotor/RotorHeroEnergy.mat",
            };
            foreach (string path in materialPaths)
                Assert.That(AssetDatabase.LoadAssetAtPath<Material>(path), Is.Not.Null, $"Missing committed final Rotor material asset: {path}");

            var parent = new GameObject("Rotor Asset Contract");
            try
            {
                Transform visual = RotorVisualFactory.BuildOrbitalExplorer(parent.transform);
                MeshFilter[] filters = visual.GetComponentsInChildren<MeshFilter>(true);
                Assert.That(filters.Length, Is.GreaterThan(0));
                Assert.That(filters.All(filter => filter.sharedMesh != null &&
                    AssetDatabase.GetAssetPath(filter.sharedMesh).StartsWith("Assets/SPINBOUND/Art/Models/Rotor/", StringComparison.Ordinal)), Is.True,
                    "Final Hero rendering must come from committed Rotor model assets, not transient procedural meshes.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(parent);
            }
        }

        [Test]
        public void RotorHeroReviewCameras_KeepAllEndpointsInsideFrame()
        {
            BuildRotorHeroReviewScene.Build();
            Camera[] cameras = Resources.FindObjectsOfTypeAll<Camera>()
                .Where(camera => camera != null && camera.gameObject.scene.IsValid() && camera.gameObject.scene.name == "RotorHeroReview")
                .ToArray();
            Transform[] endpoints = Resources.FindObjectsOfTypeAll<Transform>()
                .Where(transform => transform != null && transform.gameObject.scene.IsValid() && transform.gameObject.scene.name == "RotorHeroReview" &&
                    (transform.name == "Left Endpoint Marker" || transform.name == "Right Endpoint Marker"))
                .ToArray();

            Assert.That(cameras.Select(camera => camera.name), Does.Contain("Review Camera 78deg"));
            Assert.That(cameras.Select(camera => camera.name), Does.Contain("Review Camera 45deg"));
            Assert.That(endpoints.Length, Is.EqualTo(6));

            foreach (Camera camera in cameras.Where(camera => camera.name.StartsWith("Review Camera", StringComparison.Ordinal)))
            {
                bool wasActive = camera.gameObject.activeSelf;
                camera.gameObject.SetActive(true);
                foreach (Transform endpoint in endpoints)
                {
                    Vector3 viewport = camera.WorldToViewportPoint(endpoint.position);
                    Assert.That(viewport.z, Is.GreaterThan(0f), $"{endpoint.name} is behind {camera.name}.");
                    Assert.That(viewport.x, Is.InRange(0.04f, 0.96f), $"{camera.name} clips a Rotor endpoint horizontally.");
                    Assert.That(viewport.y, Is.InRange(0.08f, 0.92f), $"{camera.name} clips a Rotor endpoint vertically.");
                }
                camera.gameObject.SetActive(wasActive);
            }
        }

        [Test]
        public void RotorHub_HasClosedTopAndBottomFaces()
        {
            Mesh hub = ProceduralMeshFactory.CreateRotorHub(0.34f, 0.18f, 24);
            try
            {
                Assert.That(CountTrianglesFacing(hub, Vector3.up), Is.GreaterThan(0),
                    "Hero hubs need a real top cap so the 78-degree gameplay camera does not see an empty tube.");
                Assert.That(CountTrianglesFacing(hub, Vector3.down), Is.GreaterThan(0),
                    "Hero hubs need a real bottom cap for a closed production silhouette.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(hub);
            }
        }

        [Test]
        public void CounterRotationMechanism_IsConfinedToCentralCore()
        {
            var parent = new GameObject("Rotor Counter Rotation Contract");
            try
            {
                Transform visual = RotorVisualFactory.BuildOrbitalExplorer(parent.transform);
                Transform counterRotation = FindRecursive(visual, "Counter Rotation Mechanism");
                Assert.That(counterRotation, Is.Not.Null);

                MeshFilter[] rotatingMeshes = counterRotation.GetComponentsInChildren<MeshFilter>(true);
                Assert.That(rotatingMeshes.Length, Is.GreaterThan(0));
                Assert.That(rotatingMeshes.All(filter => filter.sharedMesh != null && filter.sharedMesh.bounds.extents.x <= 0.55f), Is.True,
                    "Counter-rotation must stay inside the central mechanism; full-length rails would form an X against the gameplay silhouette.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(parent);
            }
        }

        [Test]
        public void RotorPresenter_ExposesPresentationOnlySpeedHitHealState()
        {
            Type presenter = typeof(RotorPresenter);
            Assert.That(presenter.GetMethod("SetSpeedTier", new[] { typeof(SpeedTier) }), Is.Not.Null);
            Assert.That(presenter.GetMethod("PlayHitRecoil", Type.EmptyTypes), Is.Not.Null);
            Assert.That(presenter.GetMethod("PlayHealRecharge", Type.EmptyTypes), Is.Not.Null);
            Assert.That(presenter.GetMethod("AdvancePresentation", new[] { typeof(float) }), Is.Not.Null,
                "Presentation animation needs a deterministic tick for tests and runtime Update.");
        }

        [Test]
        public void PresentationState_DoesNotMutateCoreCollisionCapsule()
        {
            var go = new GameObject("Rotor Presenter Collision Contract");
            try
            {
                var presenter = go.AddComponent<RotorPresenter>();
                Transform visual = RotorVisualFactory.BuildOrbitalExplorer(go.transform);
                presenter.Configure(visual);

                var state = new RotorState(
                    NumericsVector2.Zero,
                    32f,
                    -RotorTuning.BaseAngularSpeedDegPerSecond,
                    RotationDirection.Clockwise,
                    RotorMode.Standard,
                    NumericsVector2.Zero);

                float halfLength = state.HalfLengthMeters;
                float radius = state.RadiusMeters;

                presenter.Apply(state);
                Invoke(presenter, "SetSpeedTier", SpeedTier.Speed3);
                Invoke(presenter, "PlayHitRecoil");
                Invoke(presenter, "AdvancePresentation", .15f);
                Invoke(presenter, "PlayHealRecharge");
                Invoke(presenter, "AdvancePresentation", .35f);

                Assert.That(state.HalfLengthMeters, Is.EqualTo(halfLength));
                Assert.That(state.RadiusMeters, Is.EqualTo(radius));
                Assert.That(state.Position, Is.EqualTo(NumericsVector2.Zero));
                Assert.That(state.AngleDeg, Is.EqualTo(32f));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void RotorHeroReviewSceneBuilder_IsAvailableForSpeedStateScreenshots()
        {
            Type builder = Type.GetType("Spinbound.EditorTools.BuildRotorHeroReviewScene, Spinbound.Editor", false);
            Assert.That(builder, Is.Not.Null, "Task 3 requires a neutral hero review scene builder.");
            Assert.That(builder.GetMethod("Build", BindingFlags.Public | BindingFlags.Static), Is.Not.Null);
        }

        private static int CountTrianglesFacing(Mesh mesh, Vector3 direction)
        {
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;
            int count = 0;
            for (int i = 0; i < triangles.Length; i += 3)
            {
                Vector3 a = vertices[triangles[i]];
                Vector3 b = vertices[triangles[i + 1]];
                Vector3 c = vertices[triangles[i + 2]];
                Vector3 normal = Vector3.Cross(b - a, c - a).normalized;
                if (Vector3.Dot(normal, direction) > 0.98f) count++;
            }
            return count;
        }

        private static Transform FindRecursive(Transform root, string name)
        {
            if (root.name == name) return root;
            for (int i = 0; i < root.childCount; i++)
            {
                Transform match = FindRecursive(root.GetChild(i), name);
                if (match != null) return match;
            }
            return null;
        }

        private static void Invoke(object target, string methodName, params object[] args)
        {
            Type[] signature = args.Select(arg => arg.GetType()).ToArray();
            MethodInfo method = target.GetType().GetMethod(methodName, signature);
            Assert.That(method, Is.Not.Null, $"Missing required presentation method: {methodName}");
            method.Invoke(target, args);
        }
    }
}
#endif
