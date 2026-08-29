using UnityEngine;
using Spinbound.Core.Simulation;
using NumericsVector2 = System.Numerics.Vector2;

namespace Spinbound.Presentation
{
    public sealed class RotorPresenter : MonoBehaviour
    {
        [SerializeField] private float _visualHeight = 0.55f;
        [SerializeField] private Transform _rotorVisual;

        public void Configure(Transform rotorVisual) => _rotorVisual = rotorVisual;

        public void Apply(in RotorState state)
        {
            transform.position = ToWorld(state.Position, _visualHeight);
            var target = _rotorVisual != null ? _rotorVisual : transform;
            target.rotation = Quaternion.Euler(0f, -state.AngleDeg, 0f);
        }

        private static Vector3 ToWorld(NumericsVector2 p, float y) => new(p.X, y, p.Y);
    }
}
