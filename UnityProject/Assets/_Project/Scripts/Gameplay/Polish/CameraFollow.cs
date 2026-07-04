using UnityEngine;

namespace MobCrush.Gameplay.Polish
{
    /// <summary>
    /// Smoothed camera follow + shake receiver (Loop 19). LateUpdate so the target's
    /// movement for the frame is final. Shake is applied as a post-offset so it never
    /// pollutes the follow position (no drift after shaking stops).
    /// </summary>
    public sealed class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private float _smoothTime = 0.15f;

        private Vector3 _velocity;
        private float _shakeRemaining;
        private float _shakeAmplitude;

        public void Shake(float amplitude, float duration)
        {
            // Strongest request wins; overlapping small shakes don't stack into nausea.
            _shakeAmplitude = Mathf.Max(_shakeAmplitude, amplitude);
            _shakeRemaining = Mathf.Max(_shakeRemaining, duration);
        }

        private void LateUpdate()
        {
            if (_target == null) return;

            Vector3 desired = new(_target.position.x, _target.position.y, transform.position.z);
            Vector3 smoothed = Vector3.SmoothDamp(transform.position, desired, ref _velocity, _smoothTime);

            if (_shakeRemaining > 0f)
            {
                _shakeRemaining -= Time.deltaTime;
                float falloff = Mathf.Clamp01(_shakeRemaining); // linear die-off in the last second
                smoothed += (Vector3)(Random.insideUnitCircle * (_shakeAmplitude * falloff));
                if (_shakeRemaining <= 0f) _shakeAmplitude = 0f;
            }

            transform.position = smoothed;
        }
    }
}
