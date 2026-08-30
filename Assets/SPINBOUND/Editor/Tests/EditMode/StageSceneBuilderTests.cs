#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using Spinbound.UnityRuntime;
using Spinbound.Worlds;
using Spinbound.Worlds.W01.DaisyMeadow;

namespace Spinbound.EditorTools.Tests.EditMode
{
    public sealed class StageSceneBuilderTests
    {
        [Test]
        public void GenericStageSceneTypes_ArePresent()
        {
            Assert.That(Resolve("Spinbound.EditorTools.StageSceneBuilder, Spinbound.Editor"), Is.Not.Null);
            Assert.That(Resolve("Spinbound.EditorTools.GameplayGeometryPresenter, Spinbound.Editor"), Is.Not.Null);
            Assert.That(Resolve("Spinbound.EditorTools.BuildWorld1Scenes, Spinbound.Editor"), Is.Not.Null);
            Assert.That(Resolve("Spinbound.Presentation.World.StagePresentationProfile, Spinbound.Presentation"), Is.Not.Null);
            Assert.That(Resolve("Spinbound.Presentation.World.StageSemanticBinding, Spinbound.Presentation"), Is.Not.Null);
        }

        [Test]
        public void W0101GameplayGeometry_MatchesDefinition_AndIsHiddenOnDedicatedLayer()
        {
            Type presenterType = Require("Spinbound.EditorTools.GameplayGeometryPresenter, Spinbound.Editor");
            Type bindingType = Require("Spinbound.Presentation.World.StageSemanticBinding, Spinbound.Presentation");
            MethodInfo build = presenterType.GetMethod("Build", BindingFlags.Public | BindingFlags.Static);
            Assert.That(build, Is.Not.Null, "GameplayGeometryPresenter.Build(StageDefinition) must exist.");

            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            object report = build.Invoke(null, new object[] { W01_01_FirstSpin.Definition });
            Assert.That(report, Is.Not.Null);

            Assert.That(ReadInt(report, "WallCount"), Is.EqualTo(W01_01_FirstSpin.Definition.Colliders.Count));
            Assert.That(ReadInt(report, "ZoneCount"), Is.Zero);
            Assert.That(ReadInt(report, "SpringCount"), Is.Zero);
            Assert.That(ReadInt(report, "GoalCount"), Is.EqualTo(1));

            GameObject root = ReadObject<GameObject>(report, "Root");
            Assert.That(root, Is.Not.Null);
            Assert.That(root.transform.childCount, Is.EqualTo(W01_01_FirstSpin.Definition.Colliders.Count + 1));

            foreach (Transform child in root.transform)
            {
                Assert.That(LayerMask.LayerToName(child.gameObject.layer), Is.EqualTo("GameplayCollision"), child.name);
                foreach (Renderer renderer in child.GetComponentsInChildren<Renderer>(true))
                    Assert.That(renderer.enabled, Is.False, $"{child.name} renderer must stay hidden in production.");

                Component binding = child.GetComponent(bindingType);
                Assert.That(binding, Is.Not.Null, $"{child.name} must carry a semantic binding component.");
                string semanticId = (string)bindingType.GetProperty("SemanticId")?.GetValue(binding);
                Assert.That(semanticId, Is.Not.Null.And.Not.Empty, child.name);
            }
        }

        [Test]
        public void World1ScenePaths_AreUniqueForAllEightContracts()
        {
            Type batchType = Require("Spinbound.EditorTools.BuildWorld1Scenes, Spinbound.Editor");
            MethodInfo getScenePath = batchType.GetMethod("GetScenePath", BindingFlags.Public | BindingFlags.Static);
            Assert.That(getScenePath, Is.Not.Null, "BuildWorld1Scenes.GetScenePath(StageDefinition) must exist.");

            var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (W01StageRouteContract contract in W01ReferenceRoutes.All)
            {
                string path = (string)getScenePath.Invoke(null, new object[] { contract.Stage });
                Assert.That(path, Does.StartWith("Assets/SPINBOUND/Worlds/W01/DaisyMeadow/Scenes/"));
                Assert.That(path, Does.EndWith(".unity"));
                Assert.That(paths.Add(path), Is.True, $"Duplicate generated scene path: {path}");
            }

            Assert.That(paths.Count, Is.EqualTo(8));
        }

        [Test]
        public void UnityRotorGameHost_ExposesStageIdConfiguration()
        {
            MethodInfo configureStage = typeof(UnityRotorGameHost).GetMethod(
                "ConfigureStageId",
                BindingFlags.Public | BindingFlags.Instance,
                null,
                new[] { typeof(string) },
                null);

            Assert.That(configureStage, Is.Not.Null, "UnityRotorGameHost must be stage-driven instead of hard-coded to W01-01.");
        }

        [Test]
        public void W0101ProductionScene_PreservesCalibratedVisualStack_AndSemanticPresentationBindings()
        {
            Type builderType = Require("Spinbound.EditorTools.StageSceneBuilder, Spinbound.Editor");
            Type profileType = Require("Spinbound.Presentation.World.StagePresentationProfile, Spinbound.Presentation");
            Type bindingType = Require("Spinbound.Presentation.World.StageSemanticBinding, Spinbound.Presentation");

            var profile = ScriptableObject.CreateInstance(profileType);
            MethodInfo configureProfile = profileType.GetMethod("Configure", BindingFlags.Public | BindingFlags.Instance);
            Assert.That(configureProfile, Is.Not.Null);
            configureProfile.Invoke(profile, new object[] { "daisy-meadow", true });

            MethodInfo build = builderType.GetMethod("Build", BindingFlags.Public | BindingFlags.Static);
            Assert.That(build, Is.Not.Null);
            build.Invoke(null, new object[] { W01_01_FirstSpin.Definition, profile });

            Camera camera = UnityEngine.Object.FindFirstObjectByType<Camera>();
            Assert.That(camera, Is.Not.Null);
            Assert.That(camera.clearFlags, Is.EqualTo(CameraClearFlags.Skybox));
            Assert.That(camera.fieldOfView, Is.EqualTo(41f).Within(.01f));
            Assert.That(RenderSettings.skybox, Is.Not.Null);
            Assert.That(RenderSettings.skybox.shader, Is.Not.Null);
            Assert.That(RenderSettings.skybox.shader.name, Is.EqualTo("SPINBOUND/Highland Sky"));

            Component cameraData = camera.GetComponents<Component>()
                .FirstOrDefault(component => component != null &&
                    component.GetType().FullName == "UnityEngine.Rendering.Universal.UniversalAdditionalCameraData");
            Assert.That(cameraData, Is.Not.Null, "Production camera must keep URP additional camera data.");
            Assert.That(ReadMember<bool>(cameraData, "renderPostProcessing"), Is.True);
            object antialiasing = ReadMember<object>(cameraData, "antialiasing");
            Assert.That(antialiasing.ToString(), Is.EqualTo("SubpixelMorphologicalAntiAliasing"));

            Component volume = FindComponentByFullName("UnityEngine.Rendering.Volume");
            Assert.That(volume, Is.Not.Null, "Production scene must contain a global post-processing volume.");
            Assert.That(ReadMember<bool>(volume, "isGlobal"), Is.True);
            object volumeProfile = ReadMember<object>(volume, "sharedProfile");
            Assert.That(volumeProfile, Is.Not.Null);

            var volumeComponentNames = ReadCollection(volumeProfile, "components")
                .Cast<object>()
                .Where(item => item != null)
                .Select(item => item.GetType().Name)
                .ToHashSet(StringComparer.Ordinal);
            CollectionAssert.IsSupersetOf(
                volumeComponentNames,
                new[] { "Bloom", "Tonemapping", "ColorAdjustments", "WhiteBalance", "Vignette" });

            Component[] bindings = UnityEngine.Object.FindObjectsByType<Component>(FindObjectsSortMode.None)
                .Where(component => component != null && component.GetType() == bindingType)
                .ToArray();

            PropertyInfo semanticIdProperty = bindingType.GetProperty("SemanticId", BindingFlags.Public | BindingFlags.Instance);
            Assert.That(semanticIdProperty, Is.Not.Null);
            foreach (var collider in W01_01_FirstSpin.Definition.Colliders)
            {
                Component visibleBinding = bindings.FirstOrDefault(binding =>
                    string.Equals((string)semanticIdProperty.GetValue(binding), collider.Id, StringComparison.Ordinal) &&
                    binding.GetComponentsInChildren<Renderer>(true).Any(renderer => renderer.enabled));
                Assert.That(visibleBinding, Is.Not.Null,
                    $"Presentation art for gameplay collider '{collider.Id}' must bind by semantic ID and remain visible.");
            }

            UnityEngine.Object.DestroyImmediate(profile);
        }

        private static Type Resolve(string assemblyQualifiedName) => Type.GetType(assemblyQualifiedName, throwOnError: false);

        private static Type Require(string assemblyQualifiedName)
        {
            Type type = Resolve(assemblyQualifiedName);
            Assert.That(type, Is.Not.Null, $"Missing required type: {assemblyQualifiedName}");
            return type;
        }

        private static int ReadInt(object instance, string propertyName)
        {
            PropertyInfo property = instance.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            Assert.That(property, Is.Not.Null, $"Missing report property {propertyName}.");
            return (int)property.GetValue(instance);
        }

        private static T ReadObject<T>(object instance, string propertyName) where T : class
        {
            PropertyInfo property = instance.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            Assert.That(property, Is.Not.Null, $"Missing report property {propertyName}.");
            return property.GetValue(instance) as T;
        }

        private static T ReadMember<T>(object instance, string memberName)
        {
            Type type = instance.GetType();
            PropertyInfo property = type.GetProperty(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (property != null)
                return (T)property.GetValue(instance);

            FieldInfo field = type.GetField(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.That(field, Is.Not.Null, $"Missing member {type.FullName}.{memberName}.");
            return (T)field.GetValue(instance);
        }

        private static Component FindComponentByFullName(string fullName)
        {
            foreach (GameObject root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            {
                Component match = root.GetComponentsInChildren<Component>(true)
                    .FirstOrDefault(component => component != null && component.GetType().FullName == fullName);
                if (match != null) return match;
            }
            return null;
        }

        private static IEnumerable ReadCollection(object instance, string memberName)
        {
            Type type = instance.GetType();
            PropertyInfo property = type.GetProperty(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (property != null)
                return property.GetValue(instance) as IEnumerable;

            FieldInfo field = type.GetField(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.That(field, Is.Not.Null, $"Missing collection member {type.FullName}.{memberName}.");
            return field.GetValue(instance) as IEnumerable;
        }
    }
}
#endif
