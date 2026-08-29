using System.Numerics;

namespace Spinbound.Core.Simulation
{
    public readonly struct RotorState
    {
        public RotorState(
            Vector2 position,
            float angleDeg,
            float angularVelocityDegPerSecond,
            RotationDirection defaultDirection,
            RotorMode mode,
            Vector2 bumpVelocity)
        {
            Position = position;
            AngleDeg = angleDeg;
            AngularVelocityDegPerSecond = angularVelocityDegPerSecond;
            DefaultDirection = defaultDirection;
            Mode = mode;
            BumpVelocity = bumpVelocity;
        }

        public Vector2 Position { get; }
        public float AngleDeg { get; }
        public float AngularVelocityDegPerSecond { get; }
        public RotationDirection DefaultDirection { get; }
        public RotorMode Mode { get; }
        public Vector2 BumpVelocity { get; }

        public float HalfLengthMeters => RotorTuning.HalfLengthFor(Mode);
        public float RadiusMeters => RotorTuning.RadiusMeters;

        public RotorState With(
            Vector2? position = null,
            float? angleDeg = null,
            float? angularVelocity = null,
            RotationDirection? defaultDirection = null,
            RotorMode? mode = null,
            Vector2? bumpVelocity = null) =>
            new(
                position ?? Position,
                angleDeg ?? AngleDeg,
                angularVelocity ?? AngularVelocityDegPerSecond,
                defaultDirection ?? DefaultDirection,
                mode ?? Mode,
                bumpVelocity ?? BumpVelocity);
    }
}
