using System;
using System.Collections.Generic;
using System.Numerics;

namespace Spinbound.Core.Collision
{
    public sealed class CollisionWorld
    {
        private readonly CourseCollider[] _colliders;

        public CollisionWorld(IReadOnlyList<CourseCollider> colliders)
        {
            if (colliders == null) throw new ArgumentNullException(nameof(colliders));
            _colliders = new CourseCollider[colliders.Count];
            for (var i = 0; i < colliders.Count; i++) _colliders[i] = colliders[i];
        }

        public CollisionResult TestCapsule(in Capsule2D capsule)
        {
            var radiusSquared = capsule.Radius * capsule.Radius;
            var bestClearance = float.PositiveInfinity;
            var start = capsule.Start;
            var end = capsule.End;

            for (var i = 0; i < _colliders.Length; i++)
            {
                var collider = _colliders[i];
                var distanceSquared = Geometry2D.DistanceSquaredSegmentToAabb(start, end, collider.Min, collider.Max);
                var distance = MathF.Sqrt(MathF.Max(0f, distanceSquared));
                var clearance = distance - capsule.Radius;
                if (clearance < bestClearance) bestClearance = clearance;

                if (distanceSquared <= radiusSquared + 1e-8f)
                {
                    return new CollisionResult(true, collider.Id, collider.Lethal, clearance, 1f);
                }
            }

            return CollisionResult.Clear(bestClearance);
        }

        public CollisionResult SweepCapsule(in Capsule2D start, in Capsule2D end)
        {
            var travel = Vector2.Distance(start.Center, end.Center);
            var angularTravel = MathF.Abs(Geometry2D.ShortestDeltaDegrees(start.AngleDeg, end.AngleDeg));
            var translationStep = MathF.Max(0.01f, MathF.Min(start.Radius, end.Radius) * 0.25f);
            var translationSlices = (int)MathF.Ceiling(travel / translationStep);
            var angularSlices = (int)MathF.Ceiling(angularTravel / 1f);
            var slices = Math.Max(1, Math.Max(translationSlices, angularSlices));
            var angleDelta = Geometry2D.ShortestDeltaDegrees(start.AngleDeg, end.AngleDeg);
            var minClearance = float.PositiveInfinity;

            for (var i = 0; i <= slices; i++)
            {
                var t = i / (float)slices;
                var capsule = new Capsule2D(
                    Vector2.Lerp(start.Center, end.Center, t),
                    start.AngleDeg + angleDelta * t,
                    start.HalfLength + (end.HalfLength - start.HalfLength) * t,
                    start.Radius + (end.Radius - start.Radius) * t);
                var result = TestCapsule(capsule);
                if (result.ClearanceMeters < minClearance) minClearance = result.ClearanceMeters;
                if (result.Hit)
                {
                    return new CollisionResult(true, result.ColliderId, result.Lethal, result.ClearanceMeters, t);
                }
            }

            return CollisionResult.Clear(minClearance);
        }
    }
}
