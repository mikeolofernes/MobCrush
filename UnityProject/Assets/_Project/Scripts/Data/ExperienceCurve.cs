using UnityEngine;

namespace MobCrush.Data
{
    /// <summary>
    /// XP required per level (GDD §15.1: curves, not constants). Formula:
    /// required(level) = Base × level^Exponent × curveMultiplier(level/MaxLevel)
    /// — the power term sets the macro shape; the AnimationCurve lets designers hand-tune
    /// local bumps (e.g. slow levels 8-10 to stretch mid-run tension) without touching the formula.
    /// </summary>
    [CreateAssetMenu(menuName = "MobCrush/Progression/Experience Curve", fileName = "SO_ExperienceCurve")]
    public sealed class ExperienceCurve : ScriptableObject
    {
        [Min(1f)] public float BaseRequirement = 5f;
        [Min(1f)] public float Exponent = 1.35f;
        [Min(1)] public int MaxLevel = 60;
        [Tooltip("Multiplier sampled at level/MaxLevel. Flat 1.0 = pure power curve.")]
        public AnimationCurve ShapeMultiplier = AnimationCurve.Constant(0f, 1f, 1f);

        public float GetRequiredXp(int level)
        {
            int clamped = Mathf.Clamp(level, 1, MaxLevel);
            float shaped = ShapeMultiplier.Evaluate((float)clamped / MaxLevel);
            return BaseRequirement * Mathf.Pow(clamped, Exponent) * Mathf.Max(0.01f, shaped);
        }
    }
}
