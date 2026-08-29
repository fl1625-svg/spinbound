using System.Numerics;
using Spinbound.Core.Reference;
using Spinbound.Core.Simulation;

namespace Spinbound.Worlds.W01.DaisyHighlands
{
    public static class W01_01ReferenceRoute
    {
        public static readonly ReferenceAction[] Safe =
        {
            ReferenceAction.MoveTo(new Vector2(-3f, 0f), SpeedTier.Speed2, 3.0f),
            ReferenceAction.MoveTo(new Vector2(-3f, 4f), SpeedTier.Speed1, 3.0f),
            ReferenceAction.MoveTo(new Vector2(4f, 4f), SpeedTier.Speed2, 4.0f),
            ReferenceAction.Wait(0.75f),
            ReferenceAction.MoveTo(new Vector2(4f, 0f), SpeedTier.Speed1, 3.0f),
            ReferenceAction.MoveTo(new Vector2(10f, 0f), SpeedTier.Speed2, 4.0f),
        };
    }
}
