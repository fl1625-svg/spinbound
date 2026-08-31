using System;
using System.Collections.Generic;
using Spinbound.Core.Gameplay;
using Spinbound.Core.Simulation;

namespace Spinbound.Meta
{
    public sealed class StageProgressRecord
    {
        private readonly string[] _orbitCoreIds;

        public StageProgressRecord(
            string stageId,
            bool hasCleared = false,
            float bestValidTimeSeconds = 0f,
            int bestDamageCount = -1,
            StageMasteryFlags masteryFlags = StageMasteryFlags.None,
            IReadOnlyList<string> orbitCoreIds = null)
        {
            if (string.IsNullOrWhiteSpace(stageId)) throw new ArgumentException("Stage id is required.", nameof(stageId));
            if (bestValidTimeSeconds < 0f) throw new ArgumentOutOfRangeException(nameof(bestValidTimeSeconds));
            if (bestDamageCount < -1) throw new ArgumentOutOfRangeException(nameof(bestDamageCount));

            StageId = stageId;
            HasCleared = hasCleared;
            BestValidTimeSeconds = bestValidTimeSeconds;
            BestDamageCount = bestDamageCount;
            MasteryFlags = masteryFlags;
            _orbitCoreIds = CopyDistinct(orbitCoreIds);
        }

        public string StageId { get; }
        public bool HasCleared { get; }
        public float BestValidTimeSeconds { get; }
        public int BestDamageCount { get; }
        public StageMasteryFlags MasteryFlags { get; }
        public IReadOnlyList<string> OrbitCoreIds => _orbitCoreIds;

        public StageProgressRecord Merge(
            RunResult result,
            IReadOnlyList<string> authoredOrbitCoreIds,
            float masterTimeSeconds)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            if (!string.Equals(StageId, result.StageId, StringComparison.Ordinal))
                throw new ArgumentException("Run result stage id does not match this progress record.", nameof(result));
            if (!result.Completed) return this;

            float bestTime = BestValidTimeSeconds;
            if (result.EligibleForStandardLeaderboard &&
                (bestTime <= 0f || result.DisplayTimeSeconds < bestTime))
            {
                bestTime = result.DisplayTimeSeconds;
            }

            int bestDamage = BestDamageCount;
            if (bestDamage < 0 || result.DamageCount < bestDamage)
                bestDamage = result.DamageCount;

            string[] mergedCores = MergeCores(_orbitCoreIds, result.OrbitCoreIds);

            StageMasteryFlags earned = StageMasteryFlags.Clear;
            if (result.Mode == RotorMode.Standard && !result.Practice)
                earned = StageMastery.Evaluate(result, authoredOrbitCoreIds, masterTimeSeconds);

            return new StageProgressRecord(
                StageId,
                hasCleared: true,
                bestValidTimeSeconds: bestTime,
                bestDamageCount: bestDamage,
                masteryFlags: MasteryFlags | earned,
                orbitCoreIds: mergedCores);
        }

        private static string[] CopyDistinct(IReadOnlyList<string> source)
        {
            if (source == null || source.Count == 0) return Array.Empty<string>();
            return MergeCores(Array.Empty<string>(), source);
        }

        private static string[] MergeCores(IReadOnlyList<string> first, IReadOnlyList<string> second)
        {
            var ordered = new List<string>();
            var seen = new HashSet<string>(StringComparer.Ordinal);

            Add(first, ordered, seen);
            Add(second, ordered, seen);
            return ordered.ToArray();
        }

        private static void Add(IReadOnlyList<string> source, List<string> ordered, HashSet<string> seen)
        {
            if (source == null) return;
            for (int i = 0; i < source.Count; i++)
            {
                string id = source[i];
                if (!string.IsNullOrWhiteSpace(id) && seen.Add(id))
                    ordered.Add(id);
            }
        }
    }
}
