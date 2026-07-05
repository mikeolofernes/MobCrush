using MobCrush.Core.Services;
using MobCrush.Meta.Inventory;
using MobCrush.UI;
using UnityEngine;

namespace MobCrush.App
{
    /// <summary>
    /// Menu scene composition: injects meta services into views that need them
    /// (post-review fix — InventoryView.Initialize previously had no caller).
    /// TryGet: the Menu scene must still open in isolation in the editor without Boot.
    /// </summary>
    public sealed class MenuInstaller : MonoBehaviour
    {
        [SerializeField] private InventoryView _inventoryView;
        [SerializeField] private MissionsView _missionsView;

        private void Start()
        {
            bool anyService = false;

            if (ServiceLocator.TryGet<InventoryService>(out var inventory))
            {
                anyService = true;
                if (_inventoryView != null) _inventoryView.Initialize(inventory);
            }

            if (ServiceLocator.TryGet<Meta.Progression.DailyMissionService>(out var missions))
            {
                anyService = true;
                if (_missionsView != null) _missionsView.Initialize(missions);
            }

            if (!anyService)
                Debug.LogWarning("MenuInstaller: no meta services registered — " +
                                 "running Menu scene without Boot? Meta UI stays inert.");
        }
    }
}
