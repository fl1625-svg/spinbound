using System;
using System.Numerics;

namespace Spinbound.Core.Collision
{
    internal static class Geometry2D
    {
        private const float Epsilon = 1e-7f;

        public static float DistanceSquaredPointToSegment(Vector2 point, Vector2 a, Vector2 b)
        {
            var ab = b - a;
            var denom = ab.LengthSquared();
            if (denom <= Epsilon)
            {
                return Vector2.DistanceSquared(point, a);
            }

            var t = Vector2.Dot(point - a, ab) / denom;
            t = Math.Clamp(t, 0f, 1f);
            var closest = a + ab * t;
            return Vector2.DistanceSquared(point, closest);
        }

        public static float DistanceSquaredSegmentToSegment(Vector2 a0, Vector2 a1, Vector2 b0, Vector2 b1)
        {
            if (SegmentsIntersect(a0, a1, b0, b1))
            {
                return 0f;
            }

            return MathF.Min(
                MathF.Min(DistanceSquaredPointToSegment(a0, b0, b1), DistanceSquaredPointToSegment(a1, b0, b1)),
                MathF.Min(DistanceSquaredPointToSegment(b0, a0, a1), DistanceSquaredPointToSegment(b1, a0, a1)));
        }

        public static float DistanceSquaredPointToAabb(Vector2 p, Vector2 min, Vector2 max)
        {
            var dx = p.X < min.X ? min.X - p.X : p.X > max.X ? p.X - max.X : 0f;
            var dy = p.Y < min.Y ? min.Y - p.Y : p.Y > max.Y ? p.Y - max.Y : 0f;
            return dx * dx + dy * dy;
        }

        public static float DistanceSquaredSegmentToAabb(Vector2 a, Vector2 b, Vector2 min, Vector2 max)
        {
            if (PointInsideAabb(a, min, max) || PointInsideAabb(b, min, max))
            {
                return 0f;
            }

            var bl = new Vector2(min.X, min.Y);
            var br = new Vector2(max.X, min.Y);
            var tr = new Vector2(max.X, max.Y);
            var tl = new Vector2(min.X, max.Y);

            var d0 = DistanceSquaredSegmentToSegment(a, b, bl, br);
            if (d0 <= Epsilon) return 0f;
            var d1 = DistanceSquaredSegmentToSegment(a, b, br, tr);
            if (d1 <= Epsilon) return 0f;
            var d2 = DistanceSquaredSegmentToSegment(a, b, tr, tl);
            if (d2 <= Epsilon) return 0f;
            var d3 = DistanceSquaredSegmentToSegment(a, b, tl, bl);
            if (d3 <= Epsilon) return 0f;

            return MathF.Min(MathF.Min(d0, d1), MathF.Min(d2, d3));
        }

        public static Vector2 DirectionFromDegrees(float angleDeg)
        {
            var radians = angleDeg * (MathF.PI / 180f);
            return new Vector2(MathF.Cos(radians), MathF.Sin(radians));
        }

        public static float ShortestDeltaDegrees(float fromDeg, float toDeg)
        {
            var delta = (toDeg - fromDeg) % 360f;
            if (delta > 180f) delta -= 360f;
            if (delta < -180f) delta += 360f;
            return delta;
        }

        private static bool PointInsideAabb(Vector2 p, Vector2 min, Vector2 max) =>
            p.X >= min.X && p.X <= max.X && p.Y >= min.Y && p.Y <= max.Y;

        private static bool SegmentsIntersect(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            var o1 = Cross(b - a, c - a);
            var o2 = Cross(b - a, d - a);
            var o3 = Cross(d - c, a - c);
            var o4 = Cross(d - c, b - c);

            if (((o1 > Epsilon && o2 < -Epsilon) || (o1 < -Epsilon && o2 > Epsilon)) &&
                ((o3 > Epsilon && o4 < -Epsilon) || (o3 < -Epsilon && o4 > Epsilon)))
            {
                return true;
            }

            return (MathF.Abs(o1) <= Epsilon && OnSegment(a, b, c)) ||
                   (MathF.Abs(o2) <= Epsilon && OnSegment(a, b, d)) ||
                   (MathF.Abs(o3) <= Epsilon && OnSegment(c, d, a)) ||
                   (MathF.Abs(o4) <= Epsilon && OnSegment(c, d, b));
        }

        private static bool OnSegment(Vector2 a, Vector2 b, Vector2 p) =>
            p.X >= MathF.Min(a.X, b.X) - Epsilon && p.X <= MathF.Max(a.X, b.X) + Epsilon &&
            p.Y >= MathF.Min(a.Y, b.Y) - Epsilon && p.Y <= MathF.Max(a.Y, b.Y) + Epsilon;

        private static float Cross(Vector2 a, Vector2 b) => a.X * b.Y - a.Y * b.X;
    }
}
