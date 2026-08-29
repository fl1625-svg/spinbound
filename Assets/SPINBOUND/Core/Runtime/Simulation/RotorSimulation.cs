using System;

namespace Spinbound.Core.Simulation
{
    public static class RotorSimulation
    {
        public static RotorState Step(in RotorState state, in RotorIntent intent, float fixedDt)
        {
            if (fixedDt <= 0f || float.IsNaN(fixedDt) || float.IsInfinity(fixedDt))
            {
                throw new ArgumentOutOfRangeException(nameof(fixedDt), "fixedDt must be finite and > 0.");
            }

            var direction = RotorMath.NormalizeMove(intent.MoveDirection);
            var speed = RotorTuning.SpeedFor(intent.SpeedTier);
            var nextPosition = state.Position + direction * speed * fixedDt;
            var nextAngle = Wrap360(state.AngleDeg + state.AngularVelocityDegPerSecond * fixedDt);

            return state.With(position: nextPosition, angleDeg: nextAngle);
        }

        internal static float Wrap360(float degrees)
        {
            var wrapped = degrees % 360f;
            return wrapped < 0f ? wrapped + 360f : wrapped;
        }
    }
}
