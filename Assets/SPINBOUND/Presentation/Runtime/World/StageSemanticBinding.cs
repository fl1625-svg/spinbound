using System;
using UnityEngine;

namespace Spinbound.Presentation.World
{
    /// <summary>
    /// Stable semantic identity for presentation/editor objects generated from gameplay definitions.
    /// Consumers bind to this ID instead of depending on hierarchy names.
    /// </summary>
    public sealed class StageSemanticBinding : MonoBehaviour
    {
        [SerializeField] private string _semanticId;

        public string SemanticId => _semanticId;

        public void Configure(string semanticId)
        {
            if (string.IsNullOrWhiteSpace(semanticId))
                throw new ArgumentException("Semantic id is required.", nameof(semanticId));

            _semanticId = semanticId;
        }
    }
}
