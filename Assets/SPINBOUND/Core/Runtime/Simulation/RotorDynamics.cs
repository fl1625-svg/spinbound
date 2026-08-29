using System.Numerics;

namespace Spinbound.Core.Simulation
{
    public static class RotorDynamics
    {
        private const float BumpDampingPerSecond = 7.5f;

        public static RotorState IntegrateFree(RotorState state, RotorIntent intent, float dt)
        {
            var move = RotorMath.NormalizeMove(intent.Move);
            var speed = RotorTuning.SpeedFor(intent.SpeedTier);
            var bump = RotorMath.MoveToward(
                state.BumpVelocity,
                Vector2.Zero,
                BumpDampingPerSecond * dt);

            var position = state.Position + (move * speed + bump) * dt;
            var angle = RotorMath.WrapDeg(
                state.AngleDeg + state.AngularVelocityDegPerSecond * dt);

            return state.With(
                position: position,
                angleDeg: angle,
                bumpVelocity: bump);
        }
    }
}
