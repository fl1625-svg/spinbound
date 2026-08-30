using UnityEngine;
using Spinbound.Core.Simulation;
using NumericsVector2 = System.Numerics.Vector2;

namespace Spinbound.Presentation
{
    public sealed class RotorPresenter : MonoBehaviour
    {
        private static readonly int SpeedStateId = Shader.PropertyToID("_SpeedState");
        private static readonly int EmissionStrengthId = Shader.PropertyToID("_EmissionStrength");

        [SerializeField] private float _visualHeight = 0.55f;
        [SerializeField] private Transform _rotorVisual;

        private Transform _counterRotation;
        private Renderer[] _heroRenderers = System.Array.Empty<Renderer>();
        private MaterialPropertyBlock _properties;
        private SpeedTier _speedTier;
        private float _presentationTime;
        private float _hitRecoilRemaining;
        private float _healRechargeRemaining;
        private Vector3 _restLocalPosition;
        private Vector3 _restLocalScale = Vector3.one;

        public void Configure(Transform rotorVisual)
        {
            _rotorVisual = rotorVisual;
            _counterRotation = FindRecursive(_rotorVisual, "Counter Rotation Mechanism");
            _heroRenderers = _rotorVisual != null ? _rotorVisual.GetComponentsInChildren<Renderer>(true) : System.Array.Empty<Renderer>();
            _properties ??= new MaterialPropertyBlock();
            if (_rotorVisual != null)
            {
                _restLocalPosition = _rotorVisual.localPosition;
                _restLocalScale = _rotorVisual.localScale;
            }
            ApplyMaterialState();
        }

        public void Apply(in RotorState state)
        {
            transform.position = ToWorld(state.Position, _visualHeight);
            Transform target = _rotorVisual != null ? _rotorVisual : transform;
            target.rotation = Quaternion.Euler(0f, -state.AngleDeg, 0f);
        }

        public void SetSpeedTier(SpeedTier tier)
        {
            _speedTier = tier;
            ApplyMaterialState();
        }

        public void PlayHitRecoil() => _hitRecoilRemaining = 0.28f;

        public void PlayHealRecharge() => _healRechargeRemaining = 0.62f;

        public void AdvancePresentation(float deltaTime)
        {
            if (deltaTime < 0f) return;
            _presentationTime += deltaTime;
            _hitRecoilRemaining = Mathf.Max(0f, _hitRecoilRemaining - deltaTime);
            _healRechargeRemaining = Mathf.Max(0f, _healRechargeRemaining - deltaTime);

            float speedMultiplier = _speedTier switch
            {
                SpeedTier.Speed2 => 1.45f,
                SpeedTier.Speed3 => 2.10f,
                _ => 1f,
            };

            if (_counterRotation != null)
                _counterRotation.localRotation = Quaternion.Euler(0f, _presentationTime * 92f * speedMultiplier, 0f);

            if (_rotorVisual != null)
            {
                float recoil01 = _hitRecoilRemaining / 0.28f;
                float recoil = Mathf.Sin(recoil01 * Mathf.PI * 4f) * recoil01 * 0.075f;
                _rotorVisual.localPosition = _restLocalPosition + new Vector3(0f, 0f, recoil);

                float heal01 = _healRechargeRemaining / 0.62f;
                float healPulse = Mathf.Sin((1f - heal01) * Mathf.PI) * heal01 * 0.075f;
                float speedPulse = _speedTier == SpeedTier.Speed3 ? Mathf.Sin(_presentationTime * 8f) * 0.008f : 0f;
                _rotorVisual.localScale = _restLocalScale * (1f + healPulse + speedPulse);
            }

            ApplyMaterialState();
        }

        private void Update() => AdvancePresentation(Time.unscaledDeltaTime);

        private void ApplyMaterialState()
        {
            if (_heroRenderers == null || _heroRenderers.Length == 0) return;
            _properties ??= new MaterialPropertyBlock();
            float speed = (float)_speedTier;
            float healBoost = _healRechargeRemaining > 0f ? 0.35f * (_healRechargeRemaining / 0.62f) : 0f;

            foreach (Renderer renderer in _heroRenderers)
            {
                if (renderer == null) continue;
                renderer.GetPropertyBlock(_properties);
                _properties.SetFloat(SpeedStateId, speed);
                Material material = renderer.sharedMaterial;
                float baseEmission = material != null && material.HasProperty(EmissionStrengthId)
                    ? material.GetFloat(EmissionStrengthId)
                    : 0f;
                _properties.SetFloat(EmissionStrengthId, baseEmission + healBoost);
                renderer.SetPropertyBlock(_properties);
                _properties.Clear();
            }
        }

        private static Transform FindRecursive(Transform root, string name)
        {
            if (root == null) return null;
            if (root.name == name) return root;
            for (int i = 0; i < root.childCount; i++)
            {
                Transform match = FindRecursive(root.GetChild(i), name);
                if (match != null) return match;
            }
            return null;
        }

        private static Vector3 ToWorld(NumericsVector2 p, float y) => new(p.X, y, p.Y);
    }
}
