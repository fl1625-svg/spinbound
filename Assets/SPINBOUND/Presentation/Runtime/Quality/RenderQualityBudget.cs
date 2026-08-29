namespace Spinbound.Presentation.Quality
{
    public readonly struct RenderQualityBudget
    {
        public RenderQualityBudget(float renderScale, int msaaSamples, float shadowDistance, int targetFps, float vegetationDensity, bool postProcessing)
        {
            RenderScale = renderScale;
            MsaaSamples = msaaSamples;
            ShadowDistance = shadowDistance;
            TargetFps = targetFps;
            VegetationDensity = vegetationDensity;
            PostProcessing = postProcessing;
        }

        public float RenderScale { get; }
        public int MsaaSamples { get; }
        public float ShadowDistance { get; }
        public int TargetFps { get; }
        public float VegetationDensity { get; }
        public bool PostProcessing { get; }

        public static RenderQualityBudget For(RenderQualityTier tier) => tier switch
        {
            RenderQualityTier.High => new(1.00f, 4, 60f, 60, 1.00f, true),
            RenderQualityTier.Medium => new(0.85f, 2, 36f, 60, 0.70f, true),
            _ => new(0.70f, 1, 20f, 30, 0.40f, false),
        };
    }
}
