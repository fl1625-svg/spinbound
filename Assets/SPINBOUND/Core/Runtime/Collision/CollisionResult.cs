namespace Spinbound.Core.Collision
{
    public readonly struct CollisionResult
    {
        public CollisionResult(bool hit, string colliderId, bool lethal, float clearanceMeters, float sweepT)
        {
            Hit = hit;
            ColliderId = colliderId;
            Lethal = lethal;
            ClearanceMeters = clearanceMeters;
            SweepT = sweepT;
        }

        public bool Hit { get; }
        public string ColliderId { get; }
        public bool Lethal { get; }
        public float ClearanceMeters { get; }
        public float SweepT { get; }

        public static CollisionResult Clear(float clearanceMeters, float sweepT = 1f) =>
            new(false, string.Empty, false, clearanceMeters, sweepT);
    }
}
