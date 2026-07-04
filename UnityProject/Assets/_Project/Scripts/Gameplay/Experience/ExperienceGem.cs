using MobCrush.Core.Pooling;
using UnityEngine;

namespace MobCrush.Gameplay.Experience
{
    /// <summary>
    /// One XP pickup. No Update(): ExperienceSystem ticks all gems centrally (they can
    /// number in the hundreds). Behavior: inert until the player's pickup radius touches
    /// it, then accelerates toward the player (the "magnet" feel) and grants XP on contact.
    /// </summary>
    public sealed class ExperienceGem : MonoBehaviour, IPoolable
    {
        [SerializeField] private SpriteRenderer _renderer;

        public float XpValue { get; private set; }
        public bool Attracting { get; private set; }

        private float _speed;

        public void Setup(float xpValue)
        {
            XpValue = xpValue;
            // Visual tier by value: small/medium/large read at a glance (GDD §6).
            if (_renderer != null)
                _renderer.color = xpValue >= 20f ? new Color(1f, 0.5f, 0.1f)
                                : xpValue >= 5f ? new Color(0.3f, 0.9f, 1f)
                                : new Color(0.4f, 1f, 0.4f);
        }

        public void BeginAttract() => Attracting = true;

        /// <summary>Returns true when collected. Attraction accelerates — snappy, no orbiting.</summary>
        public bool TickAttract(Vector2 playerPos, float dt)
        {
            if (!Attracting) return false;

            _speed += 40f * dt; // acceleration constant = feel; collection is binary so balance is untouched
            Vector2 pos = transform.position;
            Vector2 to = playerPos - pos;
            float dist = to.magnitude;
            float step = _speed * dt;

            if (step >= dist || dist < 0.3f) return true; // collected

            transform.position = pos + to * (step / dist);
            return false;
        }

        public void OnSpawned()
        {
            Attracting = false;
            _speed = 4f;
        }

        public void OnDespawned() { }
    }
}
