#if UNITY_EDITOR
using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using Spinbound.Presentation.World;
using Spinbound.Worlds;
using Spinbound.Worlds.W01.DaisyMeadow;

namespace Spinbound.EditorTools
{
    public static class BuildWorld1Scenes
    {
        private const string SceneFolder = "Assets/SPINBOUND/Worlds/W01/DaisyMeadow/Scenes";

        [MenuItem("SPINBOUND/4.0/Build All World 1 Scenes")]
        public static void BuildAll()
        {
            Directory.CreateDirectory(SceneFolder);
            foreach (W01StageRouteContract contract in W01ReferenceRoutes.All)
                BuildAndSave(contract.Stage);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static string BuildPreviewScene()
        {
            Directory.CreateDirectory(SceneFolder);
            string path = BuildAndSave(W01_01_FirstSpin.Definition);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return path;
        }

        public static string GetScenePath(StageDefinition definition)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            return $"{SceneFolder}/{Sanitize(definition.Id)}.unity";
        }

        private static string BuildAndSave(StageDefinition definition)
        {
            var profile = UnityEngine.ScriptableObject.CreateInstance<StagePresentationProfile>();
            profile.Configure("daisy-meadow", productionPreview: definition.Id == W01_01_FirstSpin.Id);
            var scene = StageSceneBuilder.Build(definition, profile);
            string path = GetScenePath(definition);
            EditorSceneManager.SaveScene(scene, path);
            UnityEngine.Object.DestroyImmediate(profile);
            return path;
        }

        private static string Sanitize(string value)
        {
            var builder = new StringBuilder(value.Length);
            foreach (char c in value)
            {
                if (char.IsLetterOrDigit(c) || c == '-' || c == '_') builder.Append(c);
                else builder.Append('_');
            }
            return builder.ToString();
        }
    }
}
#endif
