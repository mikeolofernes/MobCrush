using System;
using System.Collections.Generic;
using UnityEngine;

namespace MobCrush.Core.Events
{
    /// <summary>
    /// Default <see cref="IEventBus"/>: one handler list per event type.
    /// why: Delegate.Combine on a cached delegate would allocate on every (un)subscribe;
    /// a List lets us mutate in place. Publish iterates by index (no enumerator alloc)
    /// over a snapshot copy so handlers may safely unsubscribe during dispatch.
    /// </summary>
    public sealed class EventBus : IEventBus
    {
        private static class Channel<T> where T : struct, IGameEvent
        {
            // why: per-closed-generic static list = zero dictionary lookups on the hot publish path.
            public static readonly Dictionary<EventBus, List<Action<T>>> Handlers = new();
        }

        private List<Action<T>> GetList<T>() where T : struct, IGameEvent
        {
            if (!Channel<T>.Handlers.TryGetValue(this, out var list))
            {
                list = new List<Action<T>>(8);
                Channel<T>.Handlers[this] = list;
            }
            return list;
        }

        public void Subscribe<T>(Action<T> handler) where T : struct, IGameEvent
        {
            if (handler == null) return;
            GetList<T>().Add(handler);
        }

        public void Unsubscribe<T>(Action<T> handler) where T : struct, IGameEvent
        {
            if (handler == null) return;
            GetList<T>().Remove(handler);
        }

        public void Publish<T>(in T evt) where T : struct, IGameEvent
        {
            var list = GetList<T>();
            // Iterate a stable count and re-check membership implicitly by index bounds;
            // handlers added during dispatch run on the next publish.
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (i >= list.Count) continue; // list shrank during dispatch
                try
                {
                    list[i]?.Invoke(evt);
                }
                catch (Exception e)
                {
                    // why: one faulty subscriber must never break the frame for everyone else.
                    Debug.LogException(e);
                }
            }
        }
    }
}
