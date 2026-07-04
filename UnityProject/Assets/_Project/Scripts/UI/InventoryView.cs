using System.Collections.Generic;
using MobCrush.Core.Events;
using MobCrush.Core.Services;
using MobCrush.Data;
using MobCrush.Meta.Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MobCrush.UI
{
    /// <summary>
    /// Inventory screen (Loop 17): item grid with slot filter + sort dropdowns, equipped
    /// row, tap-to-equip, merge-all button. Rows come from a small internal pool of
    /// pre-instantiated widgets (no Instantiate per refresh — mobile UI rule).
    /// The InventoryService reference is injected by the Menu scene installer.
    /// </summary>
    public sealed class InventoryView : MonoBehaviour
    {
        [System.Serializable]
        public sealed class ItemWidget
        {
            public GameObject Root;
            public Image Icon;
            public Image RarityFrame;
            public TMP_Text Label;
            public Button Button;
        }

        [SerializeField] private ItemWidget[] _widgets = new ItemWidget[30]; // one grid page; paging over pooling pages
        [SerializeField] private TMP_Dropdown _slotFilter;
        [SerializeField] private TMP_Dropdown _sortMode;
        [SerializeField] private Button _mergeAllButton;
        [SerializeField] private TMP_Text _gearScoreText;

        private static readonly Color[] RarityColors =
        {
            new(0.7f, 0.7f, 0.7f), new(0.35f, 0.8f, 0.35f), new(0.35f, 0.65f, 1f),
            new(0.75f, 0.4f, 1f), new(1f, 0.55f, 0.15f)
        };

        private InventoryService _inventory;
        private IEventBus _events;

        /// <summary>Called by the Menu scene installer (composition root) after services exist.</summary>
        public void Initialize(InventoryService inventory)
        {
            _inventory = inventory;
            _events = ServiceLocator.Get<IEventBus>();
            _events.Subscribe<EquipmentChangedEvent>(OnEquipmentChanged);

            _slotFilter.onValueChanged.AddListener(_ => Refresh());
            _sortMode.onValueChanged.AddListener(_ => Refresh());
            _mergeAllButton.onClick.AddListener(() => _inventory.MergeAll()); // refresh arrives via event

            Refresh();
        }

        private void OnDestroy() => _events?.Unsubscribe<EquipmentChangedEvent>(OnEquipmentChanged);

        private void OnEquipmentChanged(EquipmentChangedEvent _) => Refresh();

        private void Refresh()
        {
            if (_inventory == null) return;

            EquipmentSlot? slot = _slotFilter.value == 0 ? null : (EquipmentSlot)(_slotFilter.value - 1);
            var sort = (InventorySort)_sortMode.value;
            List<Core.Save.SaveModel.OwnedEquipment> items = _inventory.GetItems(slot, null, sort);

            for (int i = 0; i < _widgets.Length; i++)
            {
                var widget = _widgets[i];
                bool used = i < items.Count;
                widget.Root.SetActive(used);
                if (!used) continue;

                var item = items[i];
                var def = _inventory.GetDefinitionFor(item);
                widget.Icon.sprite = def != null ? def.Icon : null;
                widget.RarityFrame.color = RarityColors[Mathf.Clamp(item.Rarity, 0, RarityColors.Length - 1)];
                widget.Label.SetText("+{0}", item.EnhancementLevel);

                string id = item.InstanceId;
                widget.Button.onClick.RemoveAllListeners();
                widget.Button.onClick.AddListener(() => _inventory.Equip(id));
            }

            _gearScoreText.SetText("GS {0}", _inventory.GetTotalGearScore());
        }
    }
}
