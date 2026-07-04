using System;
using UnityEngine;

namespace MobCrush.Core.SceneFlow
{
    /// <summary>
    /// Top-level state machine (plain C#, not a MonoBehaviour — nothing here needs a
    /// Unity lifecycle, which keeps it unit-testable without a scene).
    /// Side effects owned here: timeScale for the pause-like states, because pausing is a
    /// global flow concern, not something individual systems should each decide.
    /// </summary>
    public sealed class GameManager : IGameStateMachine
    {
        public GameState Current { get; private set; } = GameState.Booting;
        public event Action<GameState, GameState> StateChanged;

        public void Set(GameState next)
        {
            if (next == Current) return;

            var previous = Current;
            Current = next;

            // why here: exactly one owner for timeScale prevents the classic
            // "two systems fight over pause" bug.
            Time.timeScale = IsFrozen(next) ? 0f : 1f;

            StateChanged?.Invoke(previous, next);
        }

        private static bool IsFrozen(GameState state) =>
            state == GameState.LevelUpPause || state == GameState.Paused;
    }
}
