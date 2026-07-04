using System;
using System.Collections;
using System.Collections.Generic;
using MobCrush.Data;
using MobCrush.Gameplay.Upgrades;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MobCrush.UI
{
    /// <summary>
    /// UGUI implementation of IUpgradeDraftView (Loop 11 contract): shows up to 4 cards
    /// with a staggered scale-in (unscaled time — the game is frozen), rarity coloring
    /// and the synergy-link icon. First tap wins; input is disarmed immediately after.
    /// </summary>
    public sealed class UpgradeDraftView : MonoBehaviour, IUpgradeDraftView
    {
        [Serializable]
        public sealed class Card
        {
            public GameObject Root;
            public Button Button;
            public Image Background;
            public Image Icon;
            public TMP_Text Title;
            public TMP_Text Description;
            public GameObject SynergyBadge;
        }

        [SerializeField] private GameObject _panel;
        [SerializeField] private Card[] _cards = new Card[4];
        [SerializeField] private float _cardPopSeconds = 0.15f;

        private static readonly Color[] RarityColors =
        {
            new(0.75f, 0.75f, 0.75f), // Common
            new(0.35f, 0.65f, 1f),    // Rare
            new(0.75f, 0.4f, 1f),     // Epic
            new(1f, 0.8f, 0.2f)       // Legendary
        };

        private Action<int> _onChosen;
        private bool _armed;

        public void Show(IReadOnlyList<DraftOption> options, Action<int> onChosen)
        {
            _onChosen = onChosen;
            _panel.SetActive(true);

            for (int i = 0; i < _cards.Length; i++)
            {
                var card = _cards[i];
                bool used = i < options.Count;
                card.Root.SetActive(used);
                if (!used) continue;

                var option = options[i];
                card.Title.text = option.Title;
                card.Description.text = option.Kind is DraftOptionKind.NewPassive or DraftOptionKind.PassiveLevel
                    ? option.Passive.Description
                    : option.Weapon.Description;
                card.Icon.sprite = option.Kind is DraftOptionKind.NewPassive or DraftOptionKind.PassiveLevel
                    ? option.Passive.Icon
                    : option.Weapon.Icon;
                card.Background.color = RarityColors[Mathf.Clamp((int)option.Rarity, 0, RarityColors.Length - 1)];
                card.SynergyBadge.SetActive(option.SynergyHint);

                int index = i; // capture per card
                card.Button.onClick.RemoveAllListeners();
                card.Button.onClick.AddListener(() => Choose(index));
            }

            _armed = false;
            StopAllCoroutines();
            StartCoroutine(PopIn(options.Count));
        }

        public void Hide()
        {
            StopAllCoroutines();
            _panel.SetActive(false);
        }

        private void Choose(int index)
        {
            if (!_armed) return; // ignore taps during the pop animation (accidental double-tap guard)
            _armed = false;
            _onChosen?.Invoke(index);
        }

        private IEnumerator PopIn(int count)
        {
            // Staggered scale 0→1 on unscaled time (timeScale is 0 during drafts).
            for (int i = 0; i < count; i++)
                _cards[i].Root.transform.localScale = Vector3.zero;

            for (int i = 0; i < count; i++)
            {
                float t = 0f;
                var tr = _cards[i].Root.transform;
                while (t < _cardPopSeconds)
                {
                    t += Time.unscaledDeltaTime;
                    float k = Mathf.Clamp01(t / _cardPopSeconds);
                    tr.localScale = Vector3.one * (1.06f * k - 0.06f * k * k / 1f); // slight overshoot
                    yield return null;
                }
                tr.localScale = Vector3.one;
            }
            _armed = true;
        }
    }
}
