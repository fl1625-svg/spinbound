namespace Spinbound.Core.Simulation
{
    public static class RotorTuning
    {
        public const int FixedHz = 120;
        public const float FixedDeltaSeconds = 1f / FixedHz;
        public const float BaseAngularSpeedDegPerSecond = 60f;

        public const float Speed1MetersPerSecond = 2.2f;
        public const float Speed2MetersPerSecond = Speed1MetersPerSecond * 1.5f;
        public const float Speed3MetersPerSecond = Speed1MetersPerSecond * 2f;

        public const float StandardHalfLengthMeters = 1.44f;
        public const float AssistHalfLengthMeters = 1.08f;
        public const float RadiusMeters = 0.16f;

        public const float CollisionAngularMagnitudeDegPerSecond =
            BaseAngularSpeedDegPerSecond * (1024f / 182f);
        public const float AngularRecoveryDegPerSecondSquared =
            BaseAngularSpeedDegPerSecond * (91f / 182f) * 60f;
        public const float CollisionBumpMetersPerSecond =
            Speed1MetersPerSecond * (2f / 1.5f);
        public const float BumpDecayPerTick120Hz = 0.8660254038f;
        public const int DamageInvulnerabilityTicks = 40;

        public static float SpeedFor(SpeedTier tier) => tier switch
        {
            SpeedTier.Speed1 => Speed1MetersPerSecond,
            SpeedTier.Speed2 => Speed2MetersPerSecond,
            SpeedTier.Speed3 => Speed3MetersPerSecond,
            _ => Speed1MetersPerSecond,
        };

        public static float HalfLengthFor(RotorMode mode) =>
            mode == RotorMode.Assist ? AssistHalfLengthMeters : StandardHalfLengthMeters;
    }
}
