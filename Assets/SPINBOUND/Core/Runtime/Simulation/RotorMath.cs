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
    }
}
