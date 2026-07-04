using System;
using System.Collections.Generic;

namespace MobCrush.Gameplay.Upgrades
{
    /// <summary>
    /// UI contract for the draft screen. Gameplay talks to this interface; the UGUI
    /// implementation (with card animations) lives in MobCrush.UI (Loop 17) —
    /// keeping the drafting rules testable without any canvas.
    /// </summary>
    public interface IUpgradeDraftView
    {
        /// <summary>Show cards; invoke onChosen exactly once with the picked index.</summary>
        void Show(IReadOnlyList<DraftOption> options, Action<int> onChosen);
        void Hide();
    }
}
