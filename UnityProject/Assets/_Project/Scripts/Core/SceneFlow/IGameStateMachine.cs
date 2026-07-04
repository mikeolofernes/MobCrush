using System;

namespace MobCrush.Core.SceneFlow
{
    /// <summary>
    /// Owns the top-level game state. Systems query <see cref="Current"/> or listen to
    /// <see cref="StateChanged"/>; only flow-owning code (GameManager, UI intents) may Set.
    /// </summary>
    public interface IGameStateMachine
    {
        GameState Current { get; }
        event Action<GameState, GameState> StateChanged; // (from, to)
        void Set(GameState next);
    }
}
