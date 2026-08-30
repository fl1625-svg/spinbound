using System;
using System.Collections.Generic;
using System.Numerics;
using Spinbound.Worlds.W01.DaisyMeadow;

namespace Spinbound.Meta
{
    public enum WorldMapNodeKind : byte
    {
        Normal,
        Trial,
        Boss,
    }

    public sealed class WorldMapNode
    {
        private readonly string[] _neighbors;

        public WorldMapNode(string stageId, Vector2 position, WorldMapNodeKind kind, params string[] neighbors)
        {
            if (string.IsNullOrWhiteSpace(stageId)) throw new ArgumentException("Stage id is required.", nameof(stageId));
            StageId = stageId;
            Position = position;
            Kind = kind;
            _neighbors = neighbors ?? Array.Empty<string>();
        }

        public string StageId { get; }
        public Vector2 Position { get; }
        public WorldMapNodeKind Kind { get; }
        public IReadOnlyList<string> Neighbors => _neighbors;
    }

    public sealed class WorldMapGraph
    {
        private readonly WorldMapNode[] _nodes;
        private readonly Dictionary<string, WorldMapNode> _byId;

        private WorldMapGraph(WorldMapNode[] nodes)
        {
            _nodes = nodes ?? throw new ArgumentNullException(nameof(nodes));
            _byId = new Dictionary<string, WorldMapNode>(StringComparer.Ordinal);
            for (int i = 0; i < nodes.Length; i++)
                _byId[nodes[i].StageId] = nodes[i];
        }

        public IReadOnlyList<WorldMapNode> Nodes => _nodes;

        public WorldMapNode Get(string stageId)
        {
            if (!_byId.TryGetValue(stageId, out WorldMapNode node))
                throw new KeyNotFoundException($"Unknown world-map stage id: {stageId}");
            return node;
        }

        public static WorldMapGraph CreateWorld1()
        {
            return new WorldMapGraph(new[]
            {
                new WorldMapNode(W01_01_FirstSpin.Id, new Vector2(-7f, -2f), WorldMapNodeKind.Normal, W01_02_BloomingGates.Id),
                new WorldMapNode(W01_02_BloomingGates.Id, new Vector2(-4.5f, 1.2f), WorldMapNodeKind.Normal, W01_01_FirstSpin.Id, W01_03_GardenSwitchback.Id),
                new WorldMapNode(W01_03_GardenSwitchback.Id, new Vector2(-1.5f, -.4f), WorldMapNodeKind.Normal, W01_02_BloomingGates.Id, W01_04_WindmillWalk.Id, W01_Trial_PerfectCorner.Id),
                new WorldMapNode(W01_04_WindmillWalk.Id, new Vector2(1.7f, 2.0f), WorldMapNodeKind.Normal, W01_03_GardenSwitchback.Id, W01_05_HiddenHedgeway.Id),
                new WorldMapNode(W01_05_HiddenHedgeway.Id, new Vector2(4.7f, .1f), WorldMapNodeKind.Normal, W01_04_WindmillWalk.Id, W01_06_FestivalRun.Id),
                new WorldMapNode(W01_06_FestivalRun.Id, new Vector2(7.6f, 2.2f), WorldMapNodeKind.Normal, W01_05_HiddenHedgeway.Id, W01_Boss_BloomEngine.Id),
                new WorldMapNode(W01_Trial_PerfectCorner.Id, new Vector2(-.4f, -4.6f), WorldMapNodeKind.Trial, W01_03_GardenSwitchback.Id),
                new WorldMapNode(W01_Boss_BloomEngine.Id, new Vector2(11.0f, 0f), WorldMapNodeKind.Boss, W01_06_FestivalRun.Id),
            });
        }
    }
}
