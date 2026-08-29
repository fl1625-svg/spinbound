using System.Numerics;

namespace Spinbound.Core.Simulation
{
    public static class RotorMath
    {
        public static Vector2 NormalizeMove(Vector2 value)
        {
            var lengthSquared = value.LengthSquared();
            if (lengthSquared <= 1e-12f)
            {
                return Vector2.Zero;
            }

            if (lengthSquared <= 1.000001f)
            {
                return value;
            }

            return Vector2.Normalize(value);
        }

        public static float WrapDeg(float degrees)
        {
            var wrapped = degrees % 360f;
            return wrapped < 0f ? wrapped + 360f : wrapped;
        }

        public static Vector2 MoveToward(Vector2 current, Vector2 target, float maxDelta)
        {
            if (maxDelta <= 0f)
            {
                return current;
            }

            var delta = target - current;
            var distance = delta.Length();
            if (distance <= maxDelta || distance <= float.Epsilon)
            {
                return target;
            }

            return current + delta / distance * maxDelta;
        }
    }
}
