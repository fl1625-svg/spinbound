using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Spinbound.Presentation.Quality
{
    public sealed class WebRenderQualityController : MonoBehaviour
    {
        [SerializeField] private RenderQualityTier _tier = RenderQualityTier.High;
        [SerializeField] private bool _applyOnAwake = true;

        public RenderQualityTier Tier => _tier;
        public RenderQualityBudget CurrentBudget => RenderQualityBudget.For(_tier);

        private void Awake()
        {
            if (_applyOnAwake) Apply(_tier);
        }

        public void Apply(RenderQualityTier tier)
        {
            _tier = tier;
            var budget = RenderQualityBudget.For(tier);
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = budget.TargetFps;

            var urp = QualitySettings.renderPipeline as UniversalRenderPipelineAsset ?? GraphicsSettings.defaultRenderPipeline as UniversalRenderPipelineAsset;
            if (urp != null)
            {
                urp.renderScale = budget.RenderScale;
                urp.msaaSampleCount = budget.MsaaSamples;
                urp.shadowDistance = budget.ShadowDistance;
            }
        }
    }
}
