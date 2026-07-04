using System;
using System.Collections.Generic;
using UnityEngine;

namespace MobCrush.Core.Events
{
    /// <summary>
    /// Default <see cref="IEventBus"/>: one handler list per event type, stored on the
    /// instance. (Post-review fix: an earlier per-closed-generic static cache leaked every
    /// bus instance for the process lifetime — visible in play-mode test runs — for an
    /// unmeasured micro-optimization. One dictionary lookup per publish is well within budget.)
    /// Publish iterates by index in reverse so handlers may unsubscribe themselves during
    /// dispatch; handlers added during dispatch run from the next publish.
    /// </summary>
    public sealed class EventBus : IEventBus
    {
        private readonly Dictionary<Type, object> _handlers = new();

        private List<Action<T>> GetList<T>() where T : struct, IGameEvent
        {
            if (_handlers.TryGetValue(typeof(T), out var existing))
                return (List<Action<T>>)existing;

            var list = new List<Action<T>>(8);
            _handlers[typeof(T)] = list;
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
