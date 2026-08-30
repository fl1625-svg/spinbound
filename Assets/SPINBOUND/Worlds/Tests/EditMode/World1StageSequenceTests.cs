using NUnit.Framework;
using Spinbound.Worlds.W01.DaisyMeadow;

namespace Spinbound.Worlds.Tests.EditMode
{
    public sealed class World1StageSequenceTests
    {
        [Test]
        public void SequenceContainsAllEightAuthoredStagesInPlayableOrder()
        {
            Assert.That(World1StageSequence.Count, Is.EqualTo(World1StageSequence.ExpectedStageCount));
            Assert.That(World1StageSequence.ExpectedStageCount, Is.EqualTo(8));

            string[] expected =
            {
                W01_01_FirstSpin.Id,
                W01_02_BloomingGates.Id,
                W01_03_GardenSwitchback.Id,
                W01_04_WindmillWalk.Id,
                W01_05_HiddenHedgeway.Id,
                W01_06_FestivalRun.Id,
                W01_Trial_PerfectCorner.Id,
                W01_Boss_BloomEngine.Id,
            };

            for (int i = 0; i < expected.Length; i++)
                Assert.That(World1StageSequence.Get(i).Id, Is.EqualTo(expected[i]), $"Unexpected stage at index {i}");
        }

        [Test]
        public void TryGetNextAdvancesUntilBossThenStops()
        {
            for (int i = 0; i < World1StageSequence.Count - 1; i++)
            {
                var current = World1StageSequence.Get(i);
                Assert.That(World1StageSequence.TryGetNext(current.Id, out var next), Is.True);
                Assert.That(next.Id, Is.EqualTo(World1StageSequence.Get(i + 1).Id));
            }

            Assert.That(World1StageSequence.TryGetNext(W01_Boss_BloomEngine.Id, out _), Is.False);
        }

        [Test]
        public void IndexOfReturnsMinusOneForUnknownStage()
        {
            Assert.That(World1StageSequence.IndexOf("W99-UNKNOWN"), Is.EqualTo(-1));
        }
    }
}
