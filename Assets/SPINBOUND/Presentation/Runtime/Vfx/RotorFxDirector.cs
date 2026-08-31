using UnityEngine;
using Spinbound.Core.Simulation;
using Spinbound.Presentation.UI;

namespace Spinbound.Presentation.Vfx
{
    public sealed class RotorFxDirector : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _trail;
        [SerializeField] private Light _hubLight;
        private SpeedTier _tier = SpeedTier.Speed1;
        private float _intensity = 1f;
        private bool _reduceMotion;

        public void ApplyAccessibility(AccessibilitySettings settings)
        {
            if (settings == null) return;
            _intensity = Mathf.Clamp01(settings.VfxIntensity);
            _reduceMotion = settings.ReduceMotion;
            SetSpeedTier(_tier);
        }

        public void SetSpeedTier(SpeedTier tier)
        {
            _tier = tier;
            float motionMultiplier = _reduceMotion ? .35f : 1f;
            float effective = _intensity * motionMultiplier;

            if (_trail != null)
            {
                float baseRate = tier switch
                {
                    SpeedTier.Speed1 => 4f,
                    SpeedTier.Speed2 => 11f,
                    _ => 22f,
                };
                float baseSpeed = tier switch
                {
                    SpeedTier.Speed1 => .12f,
                    SpeedTier.Speed2 => .22f,
                    _ => .42f,
                };

                var emission = _trail.emission;
                emission.rateOverTime = baseRate * effective;
                var main = _trail.main;
                main.startSpeed = baseSpeed * Mathf.Lerp(.45f, 1f, effective);
            }

            if (_hubLight != null)
            {
                float baseLight = tier switch
                {
                    SpeedTier.Speed1 => .55f,
                    SpeedTier.Speed2 => .85f,
                    _ => 1.25f,
                };
                _hubLight.intensity = baseLight * Mathf.Lerp(.45f, 1f, _intensity);
            }
        }

        public static RotorFxDirector Build(Transform parent)
        {
            var root = new GameObject("Rotor FX");
            root.transform.SetParent(parent, false);
            var director = root.AddComponent<RotorFxDirector>();

            var ps = root.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 1f;
            main.loop = true;
            main.startLifetime = .32f;
            main.startSize = .055f;
            main.startColor = new Color(.35f, .78f, 1f, .52f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = .13f;
            var emission = ps.emission;
            emission.rateOverTime = 4f;

            var lightObject = new GameObject("Hub Glow");
            lightObject.transform.SetParent(parent, false);
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = 2.4f;
            light.intensity = .55f;
            light.color = new Color(.25f, .72f, 1f);
            light.shadows = LightShadows.None;

            director._trail = ps;
            director._hubLight = light;
            return director;
        }
    }
}
