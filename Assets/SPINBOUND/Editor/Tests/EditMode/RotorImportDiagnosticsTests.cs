#if UNITY_EDITOR
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Spinbound.EditorTools.Tests.EditMode
{
    public sealed class RotorImportDiagnosticsTests
    {
        [Test]
        public void LOD0_ReportsImportedHierarchyAndMeshAssets()
        {
            const string path = "Assets/SPINBOUND/Art/Models/Rotor/OrbitalExplorer_LOD0.obj";
            GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Assert.That(asset, Is.Not.Null);

            GameObject instance = Object.Instantiate(asset);
            try
            {
                string transforms = string.Join(" | ", instance
                    .GetComponentsInChildren<Transform>(true)
                    .Select(t => t.name));
                string filters = string.Join(" | ", instance
                    .GetComponentsInChildren<MeshFilter>(true)
                    .Select(f => $"GO={f.name},Mesh={(f.sharedMesh != null ? f.sharedMesh.name : "<null>")},Path={(f.sharedMesh != null ? AssetDatabase.GetAssetPath(f.sharedMesh) : "<null>")}"));
                string subAssets = string.Join(" | ", AssetDatabase
                    .LoadAllAssetsAtPath(path)
                    .Select(o => $"{o.GetType().Name}:{o.name}"));
                ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
                string importerPaths = importer == null
                    ? "<no ModelImporter>"
                    : string.Join(" | ", importer.transformPaths ?? new string[0]);

                Assert.Fail(
                    $"LOD0 IMPORT DIAGNOSTIC\nTRANSFORMS: {transforms}\nMESH_FILTERS: {filters}\nSUB_ASSETS: {subAssets}\nIMPORTER_PATHS: {importerPaths}");
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }
        }
    }
}
#endif
