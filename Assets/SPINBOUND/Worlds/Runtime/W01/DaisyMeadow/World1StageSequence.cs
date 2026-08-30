using System;
using Spinbound.Worlds;

namespace Spinbound.Worlds.W01.DaisyMeadow
{
    /// <summary>
    /// Runtime-facing World 1 stage order. The authored route catalog remains the single source of truth.
    /// </summary>
    public static class World1StageSequence
    {
        public const int ExpectedStageCount = 8;

        public static int Count => W01ReferenceRoutes.All.Count;

        public static StageDefinition Get(int index)
        {
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            return W01ReferenceRoutes.All[index].Stage;
        }

        public static int IndexOf(string stageId)
        {
            if (string.IsNullOrWhiteSpace(stageId)) return -1;

            for (int i = 0; i < Count; i++)
            {
                if (string.Equals(Get(i).Id, stageId, StringComparison.Ordinal))
                    return i;
            }

            return -1;
        }

        public static bool TryGetNext(string stageId, out StageDefinition next)
        {
            int index = IndexOf(stageId);
            if (index < 0 || index + 1 >= Count)
            {
                next = null;
                return false;
            }

            next = Get(index + 1);
            return true;
        }
    }
}
