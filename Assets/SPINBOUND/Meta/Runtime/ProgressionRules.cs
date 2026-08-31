using System;
using Spinbound.Worlds.W01.DaisyMeadow;

namespace Spinbound.Meta
{
    public static class ProgressionRules
    {
        public static bool IsUnlocked(string stageId, PlayerProgress progress)
        {
            if (progress == null) throw new ArgumentNullException(nameof(progress));

            return stageId switch
            {
                W01_01_FirstSpin.Id => true,
                W01_02_BloomingGates.Id => progress.HasCleared(W01_01_FirstSpin.Id),
                W01_03_GardenSwitchback.Id => progress.HasCleared(W01_02_BloomingGates.Id),
                W01_04_WindmillWalk.Id => progress.HasCleared(W01_03_GardenSwitchback.Id),
                W01_05_HiddenHedgeway.Id => progress.HasCleared(W01_04_WindmillWalk.Id),
                W01_06_FestivalRun.Id => progress.HasCleared(W01_05_HiddenHedgeway.Id),
                W01_Trial_PerfectCorner.Id => progress.HasMastery(W01_03_GardenSwitchback.Id, StageMasteryFlags.Perfect),
                W01_Boss_BloomEngine.Id => progress.HasCleared(W01_06_FestivalRun.Id),
                _ => false,
            };
        }

        public static bool IsVisible(string stageId, PlayerProgress progress)
        {
            if (string.Equals(stageId, W01_Trial_PerfectCorner.Id, StringComparison.Ordinal))
                return IsUnlocked(stageId, progress);

            return World1StageSequence.IndexOf(stageId) >= 0;
        }

        public static string GetMainNextStageId(string stageId)
        {
            return stageId switch
            {
                W01_01_FirstSpin.Id => W01_02_BloomingGates.Id,
                W01_02_BloomingGates.Id => W01_03_GardenSwitchback.Id,
                W01_03_GardenSwitchback.Id => W01_04_WindmillWalk.Id,
                W01_04_WindmillWalk.Id => W01_05_HiddenHedgeway.Id,
                W01_05_HiddenHedgeway.Id => W01_06_FestivalRun.Id,
                W01_06_FestivalRun.Id => W01_Boss_BloomEngine.Id,
                _ => string.Empty,
            };
        }
    }
}
