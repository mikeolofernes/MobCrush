using MobCrush.Core.Events;
using MobCrush.Core.Pooling;
using MobCrush.Core.Services;
using TMPro;
using UnityEngine;

namespace MobCrush.Gameplay.Polish
{
    /// <summary>
    /// Pooled floating damage numbers (Loop 19), fed by DamageDealtEvent.
    /// Throttled to MaxPerFrame so a 200-hit nova doesn't spawn 200 labels — beyond a
    /// few, extra numbers add cost, not information. Crits render bigger and gold.
    /// </summary>
    public sealed class DamageNumberSystem : MonoBehaviour
    {
        [SerializeField] private GameObject _numberPrefab; // TMP_Text + DamageNumber + TimedPoolReturn
        [SerializeField] private int _maxPerFrame = 6;

        private IEventBus _events;
        private IPoolService _pool;
        private int _spawnedThisFrame;
        private int _lastFrame;

        private void Awake()
        {
            _events = ServiceLocator.Get<IEventBus>();
            _pool = ServiceLocator.Get<IPoolService>();
            _events.Subscribe<DamageDealtEvent>(OnDamageDealt);
        }

        private void OnDestroy() => _events.Unsubscribe<DamageDealtEvent>(OnDamageDealt);

        private void OnDamageDealt(DamageDealtEvent e)
        {
            if (Time.frameCount != _lastFrame)
            {
                _lastFrame = Time.frameCount;
                _spawnedThisFrame = 0;
            }
            if (_spawnedThisFrame >= _maxPerFrame && !e.Critical) return; // crits always show — they're the payoff
            _spawnedThisFrame++;

            var go = _pool.Get(_numberPrefab, e.Position + (Vector3)(Random.insideUnitCircle * 0.3f), Quaternion.identity);
            go.GetComponent<DamageNumber>().Show(e.Amount, e.Critical);
        }
    }

    /// <summary>One floating label: rises and fades over its lifetime, then pool-returns via TimedPoolReturn.</summary>
    public sealed class DamageNumber : MonoBehaviour, IPoolable
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField] private float _riseSpeed = 1.5f;
        [SerializeField] private float _lifetime = 0.6f;

        private float _age;

        public void Show(float amount, bool critical)
        {
            _text.SetText("{0}", Mathf.RoundToInt(amount));
            _text.fontSize = critical ? 7f : 4.5f;
            _text.color = critical ? new Color(1f, 0.8f, 0.2f) : Color.white;
        }

        private void Update()
        {
            _age += Time.deltaTime;
            transform.position += Vector3.up * (_riseSpeed * Time.deltaTime);

            var c = _text.color;
            c.a = 1f - Mathf.Clamp01(_age / _lifetime); // linear fade-out
            _text.color = c;
        }

        public void OnSpawned() => _age = 0f;
        public void OnDespawned() { }
    }
}
