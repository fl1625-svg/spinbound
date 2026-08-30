#if UNITY_EDITOR
using System;
using UnityEngine;
using Spinbound.Presentation.World;
using Spinbound.Worlds;

namespace Spinbound.EditorTools
{
    public sealed class GameplayGeometryBuildReport
    {
        public GameplayGeometryBuildReport(GameObject root, int wallCount, int zoneCount, int springCount, int goalCount)
        {
            Root = root;
            WallCount = wallCount;
            ZoneCount = zoneCount;
            SpringCount = springCount;
            GoalCount = goalCount;
        }

        public GameObject Root { get; }
        public int WallCount { get; }
        public int ZoneCount { get; }
        public int SpringCount { get; }
        public int GoalCount { get; }
    }

    /// <summary>
    /// Creates hidden Unity-side proxies from authoritative Core stage geometry.
    /// Runtime collision still comes from StageDefinition + CollisionWorld.
    /// </summary>
    public static class GameplayGeometryPresenter
    {
        public const string GameplayCollisionLayerName = "GameplayCollision";

        public static GameplayGeometryBuildReport Build(StageDefinition definition)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));

            int layer = LayerMask.NameToLayer(GameplayCollisionLayerName);
            if (layer < 0)
                throw new InvalidOperationException($"Required Unity layer '{GameplayCollisionLayerName}' is not configured.");

            var root = new GameObject($"Gameplay Geometry — {definition.Id}");
            root.layer = layer;

            int walls = 0;
            foreach (var collider in definition.Colliders)
            {
                var go = new GameObject($"Gameplay Wall — {collider.Id}");
                go.layer = layer;
                go.transform.SetParent(root.transform, false);

                float width = collider.Max.X - collider.Min.X;
                float depth = collider.Max.Y - collider.Min.Y;
                go.transform.localPosition = new Vector3(
                    (collider.Min.X + collider.Max.X) * 0.5f,
                    0.65f,
                    (collider.Min.Y + collider.Max.Y) * 0.5f);

                var box = go.AddComponent<BoxCollider>();
                box.size = new Vector3(width, 1.3f, depth);
                box.isTrigger = true;

                var binding = go.AddComponent<StageSemanticBinding>();
                binding.Configure(collider.Id);
                walls++;
            }

            var goal = new GameObject($"Gameplay Goal — {definition.Id}");
            goal.layer = layer;
            goal.transform.SetParent(root.transform, false);
            goal.transform.localPosition = new Vector3(definition.FinishCenter.X, 0.35f, definition.FinishCenter.Y);
            var goalCollider = goal.AddComponent<SphereCollider>();
            goalCollider.radius = definition.FinishRadius;
            goalCollider.isTrigger = true;
            var goalBinding = goal.AddComponent<StageSemanticBinding>();
            goalBinding.Configure(definition.Id + "-goal");

            return new GameplayGeometryBuildReport(root, walls, 0, 0, 1);
        }
    }
}
#endif
