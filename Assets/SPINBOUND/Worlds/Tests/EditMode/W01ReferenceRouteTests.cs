using System;
using System.Collections.Generic;
using NUnit.Framework;
using Spinbound.Core.Reference;
using Spinbound.Core.Simulation;
using Spinbound.Worlds;
using Spinbound.Worlds.W01.DaisyMeadow;

namespace Spinbound.Worlds.Tests.EditMode
{
    public sealed class W01ReferenceRouteTests
    {
        [Test]
        public void World1Catalog_HasSixNormalsTrialAndBoss_WithUniqueIds()
        {
            IReadOnlyList<W01StageRouteContract> stages = W01ReferenceRoutes.All;

            Assert.That(stages, Has.Count.EqualTo(8));
            Assert.That(CountKind(stages, StageKind.Normal), Is.EqualTo(6));
            Assert.That(CountKind(stages, StageKind.Trial), Is.EqualTo(1));
            Assert.That(CountKind(stages, StageKind.Boss), Is.EqualTo(1));

            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (W01StageRouteContract contract in stages)
            {
                Assert.That(ids.Add(contract.Stage.Id), Is.True, $"Duplicate World 1 stage id: {contract.Stage.Id}");
                Assert.That(contract.Stage.Colliders, Is.Not.Empty, $"{contract.Stage.Id} needs authored gameplay geometry.");
                Assert.That(contract.Safe, Is.Not.Empty, $"{contract.Stage.Id} needs a safe reference route.");
            }
        }

        [TestCase(RotorMode.Standard)]
        [TestCase(RotorMode.Assist)]
        public void EverySafeRoute_ClearsWithoutContact(RotorMode mode)
        {
            foreach (W01StageRouteContract contract in W01ReferenceRoutes.All)
            {
                ReferenceRunResult result = Solve(contract.Stage, contract.Safe, mode);
                Assert.That(result.Cleared, Is.True, $"{contract.Stage.Id}: {result.Failure}");
                Assert.That(result.Hits, Is.Zero, contract.Stage.Id);
                Assert.That(result.MinimumClearanceMeters, Is.GreaterThan(0f), contract.Stage.Id);
            }
        }

        [TestCase(RotorMode.Standard)]
        [TestCase(RotorMode.Assist)]
        public void EveryNormalSkilledRoute_ClearsWithoutContact(RotorMode mode)
        {
            foreach (W01StageRouteContract contract in W01ReferenceRoutes.All)
            {
                if (contract.Stage.Kind != StageKind.Normal)
                    continue;

                Assert.That(contract.Skilled, Is.Not.Empty, $"{contract.Stage.Id} needs a skilled route.");
                ReferenceRunResult result = Solve(contract.Stage, contract.Skilled, mode);
                Assert.That(result.Cleared, Is.True, $"{contract.Stage.Id}: {result.Failure}");
                Assert.That(result.Hits, Is.Zero, contract.Stage.Id);
            }
        }

        [Test]
        public void NormalStageMasterTimes_FollowInitialEightPercentOrPointSevenFiveRule()
        {
            foreach (W01StageRouteContract contract in W01ReferenceRoutes.All)
            {
                if (contract.Stage.Kind != StageKind.Normal)
                    continue;

                ReferenceRunResult skilled = Solve(contract.Stage, contract.Skilled, RotorMode.Standard);
                Assert.That(skilled.Cleared, Is.True, $"{contract.Stage.Id}: {skilled.Failure}");

                float margin = MathF.Max(0.75f, skilled.ElapsedSeconds * 0.08f);
                float expected = MathF.Ceiling((skilled.ElapsedSeconds + margin) * 100f) / 100f;
                Assert.That(contract.Stage.MasterTimeSeconds, Is.EqualTo(expected).Within(0.001f), contract.Stage.Id);
            }
        }

        [Test]
        public void PerfectCornerTrial_IsFiveToTwentySeconds()
        {
            W01StageRouteContract trial = W01ReferenceRoutes.Get(W01_Trial_PerfectCorner.Definition.Id);
            ReferenceRunResult result = Solve(trial.Stage, trial.Safe, RotorMode.Standard);

            Assert.That(result.Cleared, Is.True, result.Failure);
            Assert.That(result.ElapsedSeconds, Is.InRange(5f, 20f));
        }

        [Test]
        public void BloomEngineBoss_UsesThreeDeterministicNavigationPhases()
        {
            Assert.That(W01_Boss_BloomEngine.Definition.Kind, Is.EqualTo(StageKind.Boss));
            Assert.That(W01_Boss_BloomEngine.Definition.DeterministicPhaseCount, Is.EqualTo(3));
        }

        private static int CountKind(IReadOnlyList<W01StageRouteContract> stages, StageKind kind)
        {
            int count = 0;
            foreach (W01StageRouteContract contract in stages)
            {
                if (contract.Stage.Kind == kind)
                    count++;
            }
            return count;
        }

        private static ReferenceRunResult Solve(
            StageDefinition stage,
            IReadOnlyList<ReferenceAction> route,
            RotorMode mode) =>
            ReferenceRunSolver.Solve(
                stage.StartFor(mode),
                stage.Colliders,
                route,
                stage.FinishCenter,
                stage.FinishRadius);
    }
}
