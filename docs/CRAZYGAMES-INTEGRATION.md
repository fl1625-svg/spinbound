# CrazyGames Unity integration boundary

SPINBOUND 3.0 compiles without the CrazyGames package. After importing the latest official CrazyGames Unity SDK, add the `CRAZYGAMES_SDK` scripting define to enable `CrazyGamesPlatformBridge`.

Rules:
- `GameplayStart()` when the player enters/resumes actual play.
- `GameplayStop()` on pause, menu, fail/result, or level transition.
- Do not emit gameplay stop/start for browser focus changes; CrazyGames handles focus itself.
- Unity loadingStart/loadingStop is not used; the Unity loader owns initial loading.
- Release builds target CrazyGames Custom Build and Brotli compression.
