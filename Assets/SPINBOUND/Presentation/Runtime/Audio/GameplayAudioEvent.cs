using Spinbound.Core.Simulation;

namespace Spinbound.Presentation.Audio
{
    public enum GameplayAudioEventType : byte
    {
        RotorMotion = 0,
        SurfaceHit = 1,
        RotationSpring = 2,
        HeartZone = 3,
        StageClear = 4,
        StageFail = 5,
        UiConfirm = 6,
        UiCancel = 7,
    }

    public enum SurfaceMaterialFamily : byte
    {
        Generic = 0,
        Stone = 1,
        Wood = 2,
        Metal = 3,
        Glass = 4,
        Foliage = 5,
    }

    public readonly struct GameplayAudioEvent
    {
        public GameplayAudioEvent(
            GameplayAudioEventType type,
            SpeedTier speedTier = SpeedTier.Speed1,
            SurfaceMaterialFamily surface = SurfaceMaterialFamily.Generic,
            float severity = 0f)
        {
            Type = type;
            SpeedTier = speedTier;
            Surface = surface;
            Severity = severity < 0f ? 0f : severity > 1f ? 1f : severity;
        }

        public GameplayAudioEventType Type { get; }
        public SpeedTier SpeedTier { get; }
        public SurfaceMaterialFamily Surface { get; }
        public float Severity { get; }
    }
}
