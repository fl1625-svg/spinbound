using System.Numerics;
using NUnit.Framework;
using Spinbound.Core.Simulation;

namespace Spinbound.Core.Tests.EditMode
{
    public sealed class RotorSimulationTests
    {
        [TestCase(SpeedTier.Speed1, 2.2f)]
        [TestCase(SpeedTier.Speed2, 3.3f)]
        [TestCase(SpeedTier.Speed3, 4.4f)]
        public void OneSecondOfMovement_UsesExactOneOnePointFiveTwoRatios(SpeedTier tier, float expected)
        {
            Assert.That(RotorTuning.SpeedFor(tier), Is.EqualTo(expected).Within(0.0001f));
        }

        [Test]
        public void DiagonalInput_DoesNotIncreaseSpeedMagnitude()
        {
            var v = RotorMath.NormalizeMove(new Vector2(1f, 1f));
            Assert.That(v.Length(), Is.EqualTo(1f).Within(0.0001f));
        }

        [Test]
        public void RotorState_StoresAuthoritativeFourPointZeroFields()
        {
            var state = new RotorState(
                new Vector2(1.5f, -2f),
                123f,
                -60f,
                RotationDirection.Clockwise,
                RotorMode.Standard,
                new Vector2(0.5f, 0.25f));

            Assert.That(state.Position, Is.EqualTo(new Vector2(1.5f, -2f)));
            Assert.That(state.AngleDeg, Is.EqualTo(123f));
            Assert.That(state.AngularVelocityDegPerSecond, Is.EqualTo(-60f));
            Assert.That(state.DefaultDirection, Is.EqualTo(RotationDirection.Clockwise));
            Assert.That(state.Mode, Is.EqualTo(RotorMode.Standard));
            Assert.That(state.BumpVelocity, Is.EqualTo(new Vector2(0.5f, 0.25f)));
            Assert.That(state.HalfLengthMeters, Is.EqualTo(RotorTuning.StandardHalfLengthMeters));
            Assert.That(state.RadiusMeters, Is.EqualTo(RotorTuning.RadiusMeters));
        }

        [Test]
        public void AssistMode_ChangesOnlyRotorLengthBaseline()
        {
            var standard = new RotorState(Vector2.Zero, 0f, -60f, RotationDirection.Clockwise, RotorMode.Standard, Vector2.Zero);
            var assist = new RotorState(Vector2.Zero, 0f, -60f, RotationDirection.Clockwise, RotorMode.Assist, Vector2.Zero);

            Assert.That(standard.HalfLengthMeters, Is.EqualTo(1.44f).Within(0.0001f));
            Assert.That(assist.HalfLengthMeters, Is.EqualTo(1.08f).Within(0.0001f));
            Assert.That(standard.RadiusMeters, Is.EqualTo(assist.RadiusMeters));
        }

        [Test]
        public void With_ReplacesOnlyRequestedRotorStateFields()
        {
            var before = new RotorState(Vector2.Zero, 10f, -60f, RotationDirection.Clockwise, RotorMode.Standard, Vector2.Zero);
            var after = before.With(angleDeg: 25f, bumpVelocity: Vector2.UnitX);

            Assert.That(after.Position, Is.EqualTo(before.Position));
            Assert.That(after.AngleDeg, Is.EqualTo(25f));
            Assert.That(after.AngularVelocityDegPerSecond, Is.EqualTo(before.AngularVelocityDegPerSecond));
            Assert.That(after.DefaultDirection, Is.EqualTo(before.DefaultDirection));
            Assert.That(after.Mode, Is.EqualTo(before.Mode));
            Assert.That(after.BumpVelocity, Is.EqualTo(Vector2.UnitX));
        }
    }
}
