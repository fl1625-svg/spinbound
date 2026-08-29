namespace Spinbound.Platform
{
    public sealed class NoopPlatformBridge : IPlatformBridge
    {
        public bool IsAvailable => false;
        public void GameplayStart() { }
        public void GameplayStop() { }
    }
}
