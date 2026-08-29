using System.Numerics;
using Spinbound.Core.Simulation;

namespace Spinbound.Core.Reference
{
    public enum ReferenceActionKind : byte { MoveTo, Wait }

    public readonly struct ReferenceAction
    {
        private ReferenceAction(ReferenceActionKind kind, Vector2 target, SpeedTier tier, float seconds)
        {
            Kind = kind;
            Target = target;
            Tier = tier;
            Seconds = seconds;
        }

        public ReferenceActionKind Kind { get; }
        public Vector2 Target { get; }
        public SpeedTier Tier { get; }
        public float Seconds { get; }

        public static ReferenceAction MoveTo(Vector2 target, SpeedTier tier, float maxSeconds) =>
            new(ReferenceActionKind.MoveTo, target, tier, maxSeconds);

        public static ReferenceAction Wait(float seconds) =>
            new(ReferenceActionKind.Wait, Vector2.Zero, SpeedTier.Speed1, seconds);
    }
}
