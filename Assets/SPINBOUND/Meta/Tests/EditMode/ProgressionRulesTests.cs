using System;
using NUnit.Framework;
using Spinbound.Core.Gameplay;
using Spinbound.Core.Simulation;
using Spinbound.Worlds.W01.DaisyMeadow;

namespace Spinbound.Meta.Tests.EditMode
{
    public sealed class ProgressionRulesTests
    {
        [Test]
        public void WorseStandardTimeNeverReplacesPersonalBest()
        {
            var progress = new PlayerProgress();
            progress.Merge(Result(W01_01_FirstSpin.Id, 20f, RotorMode.Standard), Array.Empty<string>(), 30f);
            progress.Merge(Result(W01_01_FirstSpin.Id, 25f, RotorMode.Standard), Array.Empty<string>(), 30f);

            Assert.That(progress.Get(W01_01_FirstSpin.Id).BestValidTimeSeconds, Is.EqualTo(20f).Within(.001f));
        }

        [Test]
        public void AssistClearUnlocksMainProgressionWithoutWritingStandardBestTime()
        {
            var progress = new PlayerProgress();
            progress.Merge(Result(W01_01_FirstSpin.Id, 12f, RotorMode.Assist), Array.Empty<string>(), 30f);

            Assert.That(progress.HasCleared(W01_01_FirstSpin.Id), Is.True);
            Assert.That(progress.Get(W01_01_FirstSpin.Id).BestValidTimeSeconds, Is.EqualTo(0f));
            Assert.That(ProgressionRules.IsUnlocked(W01_02_BloomingGates.Id, progress), Is.True);
        }

        [Test]
        public void TrialIsHiddenUntilW01_03PerfectButMainRouteContinuesOnClear()
        {
            var progress = new PlayerProgress(new[]
            {
                new StageProgressRecord(W01_03_GardenSwitchback.Id, hasCleared: true, masteryFlags: StageMasteryFlags.Clear),
            });

            Assert.That(ProgressionRules.IsUnlocked(W01_04_WindmillWalk.Id, progress), Is.True);
            Assert.That(ProgressionRules.IsVisible(W01_Trial_PerfectCorner.Id, progress), Is.False);

            progress.Replace(new StageProgressRecord(
                W01_03_GardenSwitchback.Id,
                hasCleared: true,
                masteryFlags: StageMasteryFlags.Clear | StageMasteryFlags.Perfect));

            Assert.That(ProgressionRules.IsUnlocked(W01_Trial_PerfectCorner.Id, progress), Is.True);
            Assert.That(ProgressionRules.IsVisible(W01_Trial_PerfectCorner.Id, progress), Is.True);
        }

        [Test]
        public void BossUnlocksFromFestivalRunAndTrialIsNotInMainNextChain()
        {
            var progress = new PlayerProgress(new[]
            {
                new StageProgressRecord(W01_06_FestivalRun.Id, hasCleared: true),
            });

            Assert.That(ProgressionRules.IsUnlocked(W01_Boss_BloomEngine.Id, progress), Is.True);
            Assert.That(ProgressionRules.GetMainNextStageId(W01_06_FestivalRun.Id), Is.EqualTo(W01_Boss_BloomEngine.Id));
            Assert.That(ProgressionRules.GetMainNextStageId(W01_03_GardenSwitchback.Id), Is.EqualTo(W01_04_WindmillWalk.Id));
        }

        [Test]
        public void WorldMapGraphContainsEightNodesWithTrialBranch()
        {
            WorldMapGraph graph = WorldMapGraph.CreateWorld1();
            Assert.That(graph.Nodes.Count, Is.EqualTo(8));
            Assert.That(graph.Get(W01_03_GardenSwitchback.Id).Neighbors, Does.Contain(W01_Trial_PerfectCorner.Id));
            Assert.That(graph.Get(W01_06_FestivalRun.Id).Neighbors, Does.Contain(W01_Boss_BloomEngine.Id));
        }

        private static RunResult Result(string stageId, float time, RotorMode mode)
        {
            return new RunResult(
                stageId,
                time,
                0f,
                damageCount: 0,
                orbitCoreIds: Array.Empty<string>(),
                mode: mode,
                practice: false,
                completed: true);
        }
    }
}
