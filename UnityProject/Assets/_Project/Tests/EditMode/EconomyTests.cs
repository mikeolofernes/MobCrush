using MobCrush.Data;
using MobCrush.Meta.Equipment;
using NUnit.Framework;
using UnityEngine;

namespace MobCrush.Tests
{
    /// <summary>Equipment/economy math tests (Loop 22).</summary>
    public sealed class EconomyTests
    {
        private EconomyConfig _economy;
        private EquipmentDefinition _sword;

        [SetUp]
        public void SetUp()
        {
            _economy = ScriptableObject.CreateInstance<EconomyConfig>();
            _sword = ScriptableObject.CreateInstance<EquipmentDefinition>();
            _sword.Id = "sword";
            _sword.Slot = EquipmentSlot.Weapon;
            _sword.MainStatBase = 10f;
        }

        [Test]
        public void MainStat_ScalesWithRarityAndEnhancement()
        {
            float common0 = EquipmentMath.ComputeMainStat(_sword, _economy, EquipmentRarity.Common, 0);
            float mythic0 = EquipmentMath.ComputeMainStat(_sword, _economy, EquipmentRarity.Mythic, 0);
            float common10 = EquipmentMath.ComputeMainStat(_sword, _economy, EquipmentRarity.Common, 10);

            Assert.AreEqual(10f, common0, 0.001f);
            Assert.AreEqual(70f, mythic0, 0.001f);                       // GDD ladder: ×7
            Assert.AreEqual(10f * (1f + 0.08f * 10), common10, 0.001f);  // enhancement bonus
        }

        [Test]
        public void EnhanceCost_GrowsExponentially()
        {
            long c0 = _economy.GetEnhanceCost(0);
            long c10 = _economy.GetEnhanceCost(10);
            long c20 = _economy.GetEnhanceCost(20);

            // Growth^10 ratio holds between decades (integer truncation tolerance).
            Assert.Greater((double)c10 / c0, 4.0);
            Assert.Greater((double)c20 / c10, 4.0);
        }

        [Test]
        public void Mythic_CannotFuse()
        {
            Assert.IsTrue(EquipmentMath.CanFuseRarity(EquipmentRarity.Legendary));
            Assert.IsFalse(EquipmentMath.CanFuseRarity(EquipmentRarity.Mythic));
        }

        [Test]
        public void RarityRoll_LuckShiftsAwayFromCommon()
        {
            var table = ScriptableObject.CreateInstance<RarityTable>();
            int commonsNoLuck = CountCommons(table, luck: 0f, seed: 42);
            int commonsHighLuck = CountCommons(table, luck: 0.5f, seed: 42);

            Assert.Less(commonsHighLuck, commonsNoLuck);
        }

        private static int CountCommons(RarityTable table, float luck, int seed)
        {
            var rng = new System.Random(seed);
            int commons = 0;
            for (int i = 0; i < 2000; i++)
                if (table.Roll(rng, luck) == Rarity.Common) commons++;
            return commons;
        }
    }
}
