using System;
using UnityEngine;

namespace MobCrush.Core.Input
{
    /// <summary>
    /// Device-agnostic input (Loop 0 §2: touch/keyboard/gamepad from Day 1 so a PC port
    /// is never blocked by input coupling). Gameplay reads a normalized move vector and
    /// never touches the Input System directly.
    /// </summary>
    public interface IInputService
    {
        /// <summary>Normalized (magnitude ≤ 1) movement vector this frame; zero when idle or in UI mode.</summary>
        Vector2 Move { get; }

        /// <summary>Fired when the user requests pause (Escape / gamepad Start / HUD button intent).</summary>
        event Action PauseRequested;

        /// <summary>Switches action maps: gameplay input off while menus are up, UI stays live.</summary>
        void SetUiMode(bool uiMode);
    }
}
