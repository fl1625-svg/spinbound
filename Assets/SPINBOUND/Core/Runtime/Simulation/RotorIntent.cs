using System.Numerics;

namespace Spinbound.Core.Simulation
{
    public readonly struct RotorIntent
    {
        public RotorIntent(Vector2 moveDirection, SpeedTier speedTier)
        {
            MoveDirection = moveDirection;
            SpeedTier = speedTier;
        }

        public Vector2 MoveDirection { get; }
        public SpeedTier SpeedTier { get; }

        public static RotorIntent Idle => new(Vector2.Zero, SpeedTier.Speed1);
    }
}
