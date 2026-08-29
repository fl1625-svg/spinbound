using Spinbound.Core.Simulation;

namespace Spinbound.Core.Gameplay
{
    public static class SpeedTierResolver
    {
        public static SpeedTier Resolve(bool buttonA, bool buttonB)
        {
            if (buttonA && buttonB) return SpeedTier.Speed3;
            if (buttonA || buttonB) return SpeedTier.Speed2;
            return SpeedTier.Speed1;
        }
    }
}
