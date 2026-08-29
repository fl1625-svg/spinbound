using System;
using System.Numerics;
using NUnit.Framework;
using Spinbound.Core.Collision;
using Spinbound.Core.Gameplay;
using Spinbound.Core.Simulation;

namespace Spinbound.Core.Tests.EditMode
{
    public sealed class GameplayFlowTests
    {
        [TestCase(false, false, SpeedTier.Speed1)]
        [TestCase(true, false, SpeedTier.Speed2)]
        [TestCase(false, true, SpeedTier.Speed2)]
        [TestCase(true, true, SpeedTier.Speed3)]
        public void SpeedButtons_ResolveExactTruthTable(bool buttonA, bool buttonB, SpeedTier expected)
        {
            Assert.That(SpeedTierResolver.Resolve(buttonA, buttonB), Is.EqualTo(expected));
        }

        [Test]
        public void DifferentRenderRates_ProduceSameAuthoritativeStateAfterOneSecond()
        {
            var state60 = RunForOneSecond(1f / 60f);
            var state30 = RunForOneSecond(1f / 30f);
            Assert.That(state60.Position.X, Is.EqualTo(state30.Position.X).Within(0.0001f));
            Assert.That(state60.AngleDeg, Is.EqualTo(state30.AngleDeg).Within(0.0001f));
            Assert.That(state60.Position.X, Is.EqualTo(4.4f).Within(0.0005f));
        }

        [Test]
        public void RestartFromCheckpoint_UsesAuthoredRespawnWithoutResettingRunStats()
        {
            var initial = State(Vector2.Zero, 0f);
            var session = new RunSession(initial);
            session.RegisterHit();
            session.AdvanceTo(State(new Vector2(2f, 0f), 30f), 1f);
            session.SetCheckpoint(new CheckpointSnapshot("cp-01", State(new Vector2(5f, 2f), 90f)));
            session.AdvanceTo(State(new Vector2(8f, 3f), 140f), 1f);

            session.RestartFromCheckpoint();

            Assert.That(session.State.Position, Is.EqualTo(new Vector2(5f, 2f)));
            Assert.That(session.State.AngleDeg, Is.EqualTo(90f));
            Assert.That(session.Hits, Is.EqualTo(1));
            Assert.That(session.ElapsedSeconds, Is.EqualTo(2f));
        }

        private static RotorState RunForOneSecond(float renderDelta)
        {
            var initial = State(Vector2.Zero, 180f);
            var session = new RunSession(initial);
            var runner = new FixedStepRotorRunner(new CollisionWorld(Array.Empty<CourseCollider>()), session);
            var input = new PlayerInputState(Vector2.UnitX, buttonA: true, buttonB: true);
            var frames = (int)MathF.Round(1f / renderDelta);
            for (var i = 0; i < frames; i++) runner.Tick(renderDelta, input);
            return session.State;
        }

        private static RotorState State(Vector2 position, float angleDeg) =>
            new(position, angleDeg, -RotorTuning.BaseAngularSpeedDegPerSecond,
                RotationDirection.Clockwise, RotorMode.Standard, Vector2.Zero);
    }
}
