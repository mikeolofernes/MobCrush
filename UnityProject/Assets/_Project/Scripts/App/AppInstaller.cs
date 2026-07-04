using System;
using System.Collections.Generic;
using MobCrush.Core.Events;
using MobCrush.Core.Save;
using MobCrush.Core.Services;
using MobCrush.Data;
using MobCrush.LiveOps;
using MobCrush.Meta.Equipment;
using MobCrush.Meta.Inventory;
using MobCrush.Meta.Progression;
using UnityEngine;

namespace MobCrush.App
{
    /// <summary>
    /// Meta/LiveOps composition root (post-review fix — these services were previously
    /// constructed nowhere). Lives in the Boot scene beside GameBootstrap and persists.
    ///
    /// why the App assembly exists: Core cannot reference Meta, and Gameplay/Meta must not
    /// reference each other (Loop 3 §3) — so the outermost layer that sees everything is
    /// the only place cross-assembly composition can happen.
    ///
    /// why Start(), not Awake(): GameBootstrap registers core services synchronously in its
    /// Awake before its first await; Start is guaranteed to run after every Awake, so the
    /// locator is populated regardless of script execution order.
    /// </summary>
    public sealed class AppInstaller : MonoBehaviour
    {
        [Header("Data (single source of truth for all meta content)")]
        [SerializeField] private EconomyConfig _economy;
        [SerializeField] private List<EquipmentDefinition> _equipmentDefinitions = new();
        [SerializeField] private List<TalentDefinition> _talentDefinitions = new();

        private DailyMissionService _missions;
        private RunRewardGranter _rewards;

        private void Awake() => DontDestroyOnLoad(gameObject);

        private void Start()
        {
            var save = ServiceLocator.Get<ISaveService>();
            var events = ServiceLocator.Get<IEventBus>();

            // --- Definition registries (id → asset), validated once here. ---
            var equipmentById = BuildRegistry(_equipmentDefinitions, d => d.Id, "EquipmentDefinition");
            var talentsById = BuildRegistry(_talentDefinitions, d => d.Id, "TalentDefinition");

            // --- Meta services, dependency order. ---
            var wallet = new MetaProgressionService(save, events, talentsById);
            var equipment = new EquipmentService(save, events, _economy, wallet, equipmentById);
            var inventory = new InventoryService(save, events, equipment, _economy);
            _missions = new DailyMissionService(save, events, wallet);
            _rewards = new RunRewardGranter(events, wallet, equipment, _economy, _equipmentDefinitions);

            ServiceLocator.Register(wallet);
            ServiceLocator.Register(equipment);
            ServiceLocator.Register(inventory);
            ServiceLocator.Register(_missions);

            // --- LiveOps: null-object defaults until real SDK adapters replace them (Loop 23). ---
            ServiceLocator.Register<IAnalyticsService>(new NullAnalyticsService());
            ServiceLocator.Register<IAdsService>(new NullAdsService());
            ServiceLocator.Register<IRemoteConfigService>(new NullRemoteConfigService());

            // Welcome-back grant (Loop 16); safe no-op on first launch.
            wallet.ClaimOfflineProgress(DateTime.UtcNow);
        }

        private void OnDestroy()
        {
            _missions?.Dispose();
            _rewards?.Dispose();
        }

        private static IReadOnlyDictionary<string, T> BuildRegistry<T>(
            List<T> assets, Func<T, string> idSelector, string label) where T : UnityEngine.Object
        {
            var map = new Dictionary<string, T>(assets.Count);
            foreach (var asset in assets)
            {
                if (asset == null) continue;
                string id = idSelector(asset);
                if (string.IsNullOrEmpty(id))
                {
                    Debug.LogError($"AppInstaller: {label} '{asset.name}' has an empty Id — skipped.", asset);
                    continue;
                }
                if (!map.TryAdd(id, asset))
                    Debug.LogError($"AppInstaller: duplicate {label} id '{id}' ('{asset.name}') — skipped.", asset);
            }
            return map;
        }
    }
}
