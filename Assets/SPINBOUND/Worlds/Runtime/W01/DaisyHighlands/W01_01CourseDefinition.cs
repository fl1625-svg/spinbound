using System.Collections.Generic;
using System.Numerics;
using Spinbound.Core.Collision;
using Spinbound.Core.Simulation;

namespace Spinbound.Worlds.W01.DaisyHighlands
{
    public static class W01_01CourseDefinition
    {
        public static RotorState StartState => new(
            new Vector2(-8f, 0f),
            0f,
            -RotorTuning.BaseAngularSpeedDegPerSecond,
            RotationDirection.Clockwise,
            RotorMode.Standard,
            Vector2.Zero);

        public static Vector2 FinishCenter => new(10f, 0f);
        public const float FinishRadius = 0.65f;
        public static Vector2 HeartGardenCenter => new(4f, 4f);
        public const float HeartGardenRadius = 1.25f;

        private static readonly CourseCollider[] CourseColliders =
        {
            new("north-rim", new Vector2(-10f, 6.5f), new Vector2(12f, 7.2f)),
            new("south-rim", new Vector2(-10f, -4.2f), new Vector2(12f, -3.5f)),
            new("west-rim", new Vector2(-10.7f, -4.2f), new Vector2(-10f, 7.2f)),
            new("east-rim", new Vector2(12f, -4.2f), new Vector2(12.7f, 7.2f)),
            new("garden-rock-mass", new Vector2(-0.8f, -1.5f), new Vector2(1.2f, 1.5f)),
            new("upper-grove", new Vector2(6.2f, 4.8f), new Vector2(8.0f, 6.5f)),
            new("lower-grove", new Vector2(5.8f, -3.5f), new Vector2(7.4f, -2.0f)),
        };

        public static IReadOnlyList<CourseCollider> Colliders => CourseColliders;

        public static RotorState StartFor(RotorMode mode) => StartState.With(mode: mode);
    }
}
