using UnityEngine;

namespace Spinbound.Presentation.CameraSystem
{
    public sealed class PrecisionCameraRig : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private Vector3 _offset = new(2.5f, 15.5f, -16.5f);
        [SerializeField] private float _positionSmoothTime = 0.16f;
        [SerializeField] private float _lookAheadMeters = 1.1f;
        [SerializeField] private float _lookAheadSmoothTime = 0.20f;

        private Vector3 _positionVelocity;
        private Vector3 _lookAheadVelocity;
        private Vector3 _lastTargetPosition;
        private Vector3 _smoothedLookAhead;

        public void Configure(Transform target)
        {
            _target = target;
            if (_target != null) _lastTargetPosition = _target.position;
        }

        private void LateUpdate()
        {
            if (_target == null) return;
            var delta = _target.position - _lastTargetPosition;
            _lastTargetPosition = _target.position;
            delta.y = 0f;
            var desiredLookAhead = delta.sqrMagnitude > 1e-6f ? delta.normalized * _lookAheadMeters : Vector3.zero;
            _smoothedLookAhead = Vector3.SmoothDamp(_smoothedLookAhead, desiredLookAhead, ref _lookAheadVelocity, _lookAheadSmoothTime);
            var desiredPosition = _target.position + _offset + _smoothedLookAhead;
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref _positionVelocity, _positionSmoothTime);
            // Rotation is deliberately authored and stable during precision traversal.
        }
    }
}
