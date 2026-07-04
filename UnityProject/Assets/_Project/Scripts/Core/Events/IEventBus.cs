using System;

namespace MobCrush.Core.Events
{
    /// <summary>
    /// Typed publish/subscribe hub — the only sanctioned channel for cross-assembly
    /// communication (Loop 3 §7). Keeps Gameplay, Meta and UI decoupled.
    /// </summary>
    public interface IEventBus
    {
        void Subscribe<T>(Action<T> handler) where T : struct, IGameEvent;
        void Unsubscribe<T>(Action<T> handler) where T : struct, IGameEvent;
        void Publish<T>(in T evt) where T : struct, IGameEvent;
    }
}
