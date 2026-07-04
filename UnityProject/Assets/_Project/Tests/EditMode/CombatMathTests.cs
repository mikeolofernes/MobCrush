using MobCrush.Data;
using MobCrush.Gameplay.Combat;
using NUnit.Framework;
using UnityEngine;

namespace MobCrush.Tests
{
    /// <summary>Domain math tests (Loop 22): StatSheet stacking + the damage formula.</summary>
    public sealed class CombatMathTests
    {
        [Test]
        public void StatSheet_Formula_Is_BasePlusFlat_TimesPercent()
        {
            var sheet = new StatSheet();
            sheet.SetBase(StatType.MaxHp, 100f);
            sheet.AddFlat(StatType.MaxHp, 20f);
            sheet.AddPercent(StatType.MaxHp, 0.5f);

            Assert.AreEqual((100f + 20f) * 1.5f, sheet.Get(StatType.MaxHp), 0.001f);
        }

        [Test]
        public void StatSheet_ClearModifiers_KeepsBase()
        {
            var sheet = new StatSheet();
            sheet.SetBase(StatType.MoveSpeed, 5f);
            sheet.AddPercent(StatType.MoveSpeed, 1f);
            sheet.ClearModifiers();

            Assert.AreEqual(5f, sheet.Get(StatType.MoveSpeed), 0.001f);
        }

        [Test]
        public void Damage_AppliesDamagePercent()
        {
            var sheet = new StatSheet();
            sheet.AddPercent(StatType.DamagePercent, 0.25f);
            sheet.SetBase(StatType.CritChance, 0f); // no crits: deterministic
            var calc = new DamageCalculator(sheet, new System.Random(1));

            var info = calc.Resolve(100f, Vector2.zero);
            Assert.AreEqual(125f, info.Amount, 0.001f);
            Assert.IsFalse(info.Critical);
        }

        [Test]
        public void Damage_CritChanceOne_AlwaysCrits_WithCritDamage()
        {
            var sheet = new StatSheet();
            sheet.SetBase(StatType.CritChance, 1f);
            sheet.SetBase(StatType.CritDamage, 2f);
            var calc = new DamageCalculator(sheet, new System.Random(1));

            var info = calc.Resolve(50f, Vector2.zero);
            Assert.IsTrue(info.Critical);
            Assert.AreEqual(100f, info.Amount, 0.001f);
        }

        [Test]
        public void ExperienceCurve_IsMonotonicallyIncreasing()
        {
            var curve = ScriptableObject.CreateInstance<ExperienceCurve>();
            float previous = 0f;
            for (int level = 1; level <= curve.MaxLevel; level++)
            {
                float required = curve.GetRequiredXp(level);
                Assert.Greater(required, previous, $"Level {level} requirement must exceed level {level - 1}");
                previous = required;
            }
        }
    }
}
