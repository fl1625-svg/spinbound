#if CRAZYGAMES_SDK
using CrazyGames;
#endif

namespace Spinbound.Platform
{
    public sealed class CrazyGamesPlatformBridge : IPlatformBridge
    {
#if CRAZYGAMES_SDK
        public bool IsAvailable => CrazySDK.IsAvailable && CrazySDK.IsInitialized;
        public void GameplayStart() { if (IsAvailable) CrazySDK.Game.GameplayStart(); }
        public void GameplayStop() { if (IsAvailable) CrazySDK.Game.GameplayStop(); }
#else
        public bool IsAvailable => false;
        public void GameplayStart() { }
        public void GameplayStop() { }
#endif
    }
}
