using System;
using System.Numerics;

namespace Spinbound.Core.Collision
{
    public readonly struct CourseCollider
    {
        public CourseCollider(string id, Vector2 min, Vector2 max, bool lethal = false)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Collider id is required.", nameof(id));
            if (min.X > max.X || min.Y > max.Y) throw new ArgumentException("AABB min must be <= max.");
            Id = id;
            Min = min;
            Max = max;
            Lethal = lethal;
        }

        public string Id { get; }
        public Vector2 Min { get; }
        public Vector2 Max { get; }
        public bool Lethal { get; }
    }
}
