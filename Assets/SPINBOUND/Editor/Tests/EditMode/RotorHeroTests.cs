#if UNITY_EDITOR
using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
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
