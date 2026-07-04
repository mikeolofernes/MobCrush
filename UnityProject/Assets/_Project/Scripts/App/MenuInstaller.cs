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

        private void Start()
        {
            if (ServiceLocator.TryGet<InventoryService>(out var inventory))
            {
                if (_inventoryView != null) _inventoryView.Initialize(inventory);
            }
            else
            {
                Debug.LogWarning("MenuInstaller: InventoryService not registered — " +
                                 "running Menu scene without Boot? Inventory UI stays inert.");
            }
        }
    }
}
