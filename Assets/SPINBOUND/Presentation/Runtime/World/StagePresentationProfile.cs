using UnityEngine;

namespace Spinbound.Presentation.World
{
    /// <summary>
    /// Presentation-only stage settings. Gameplay geometry and collision remain owned by StageDefinition.
    /// </summary>
    [CreateAssetMenu(menuName = "SPINBOUND/Stage Presentation Profile", fileName = "StagePresentationProfile")]
    public sealed class StagePresentationProfile : ScriptableObject
    {
        [SerializeField] private string _themeId = "daisy-meadow";
        [SerializeField] private bool _productionPreview = true;

        public string ThemeId => _themeId;
        public bool ProductionPreview => _productionPreview;

        public void Configure(string themeId, bool productionPreview = true)
        {
            _themeId = string.IsNullOrWhiteSpace(themeId) ? "daisy-meadow" : themeId;
            _productionPreview = productionPreview;
        }
    }
}
