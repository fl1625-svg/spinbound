namespace Spinbound.Platform
{
    public interface IPlatformBridge
    {
        bool IsAvailable { get; }
        void GameplayStart();
        void GameplayStop();
    }
}
