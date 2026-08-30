using System;
using NUnit.Framework;
using Spinbound.Core.Gameplay;
using Spinbound.Core.Simulation;

namespace Spinbound.Meta.Tests.EditMode
{
    public sealed class StageMasteryTests
    {
        [Test]
        public void StandardPerfectMasterRunEarnsCrownAndLeaderboardEligibility()
        {
            var result = new RunResult(
                "W01-TEST",
                rawTimeSeconds: 10f,
                penaltySeconds: 0f,
                damageCount: 0,
                orbitCoreIds: Array.Empty<string>(),
                mode: RotorMode.Standard,
                practice: false,
                completed: true);

            StageMasteryFlags flags = StageMastery.Evaluate(result, Array.Empty<string>(), 12f);

            Assert.That(result.EligibleForStandardLeaderboard, Is.True);
            Assert.That(flags.HasFlag(StageMasteryFlags.Clear), Is.True);
            Assert.That(flags.HasFlag(StageMasteryFlags.Perfect), Is.True);
            Assert.That(flags.HasFlag(StageMasteryFlags.MasterTime), Is.True);
            Assert.That(flags.HasFlag(StageMasteryFlags.AllOrbitCores), Is.True);
            Assert.That(flags.HasFlag(StageMasteryFlags.Crown), Is.True);
        }

        [Test]
        public void AssistRunIsNeverStandardLeaderboardEligible()
        {
            var result = new RunResult(
                "W01-TEST",
                8f,
                0f,
                0,
                Array.Empty<string>(),
                RotorMode.Assist,
                practice: false,
                completed: true);

            Assert.That(result.EligibleForStandardLeaderboard, Is.False);
        }

        [Test]
        public void MissingAuthoredCorePreventsCrown()
        {
            var result = new RunResult(
                "W01-TEST",
                8f,
                0f,
                0,
                new[] { "core-a" },
                RotorMode.Standard,
                practice: false,
                completed: true);

            StageMasteryFlags flags = StageMastery.Evaluate(result, new[] { "core-a", "core-b" }, 12f);

            Assert.That(flags.HasFlag(StageMasteryFlags.Crown), Is.False);
            Assert.That(flags.HasFlag(StageMasteryFlags.AllOrbitCores), Is.False);
        }
    }
}
