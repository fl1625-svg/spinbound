using System.Numerics;

namespace Spinbound.Core.Collision
{
    public readonly struct Capsule2D
    {
        public Capsule2D(Vector2 center, float angleDeg, float halfLength, float radius)
        {
            Center = center;
            AngleDeg = angleDeg;
            HalfLength = halfLength;
            Radius = radius;
        }

        public Vector2 Center { get; }
        public float AngleDeg { get; }
        public float HalfLength { get; }
        public float Radius { get; }

        public Vector2 Start => Center - Geometry2D.DirectionFromDegrees(AngleDeg) * HalfLength;
        public Vector2 End => Center + Geometry2D.DirectionFromDegrees(AngleDeg) * HalfLength;
    }
}
