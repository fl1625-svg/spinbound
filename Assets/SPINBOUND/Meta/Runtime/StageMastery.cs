using System;
using System.Collections.Generic;
using Spinbound.Core.Gameplay;

namespace Spinbound.Meta
{
    [Flags]
    public enum StageMasteryFlags : byte
    {
        None = 0,
        Clear = 1 << 0,
        Perfect = 1 << 1,
        MasterTime = 1 << 2,
        AllOrbitCores = 1 << 3,
        Crown = 1 << 4,
    }

    public static class StageMastery
    {
        public static StageMasteryFlags Evaluate(
            RunResult result,
            IReadOnlyList<string> authoredOrbitCoreIds,
            float masterTimeSeconds)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            if (!result.Completed) return StageMasteryFlags.None;

            StageMasteryFlags flags = StageMasteryFlags.Clear;
            if (result.Perfect)
                flags |= StageMasteryFlags.Perfect;

            if (masterTimeSeconds > 0f && result.DisplayTimeSeconds <= masterTimeSeconds)
                flags |= StageMasteryFlags.MasterTime;

            if (HasAllOrbitCores(result.OrbitCoreIds, authoredOrbitCoreIds))
                flags |= StageMasteryFlags.AllOrbitCores;

            StageMasteryFlags crownRequirements =
                StageMasteryFlags.Perfect |
                StageMasteryFlags.MasterTime |
                StageMasteryFlags.AllOrbitCores;

            if ((flags & crownRequirements) == crownRequirements)
                flags |= StageMasteryFlags.Crown;

            return flags;
        }

        private static bool HasAllOrbitCores(
            IReadOnlyList<string> collected,
            IReadOnlyList<string> authored)
        {
            if (authored == null || authored.Count == 0)
                return true;
            if (collected == null || collected.Count == 0)
                return false;

            var set = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < collected.Count; i++)
            {
                if (!string.IsNullOrEmpty(collected[i]))
                    set.Add(collected[i]);
            }

            for (int i = 0; i < authored.Count; i++)
            {
                if (string.IsNullOrEmpty(authored[i]) || !set.Contains(authored[i]))
                    return false;
            }

            return true;
        }
    }
}
