using MobCrush.Core.Events;
using UnityEngine;

namespace MobCrush.Core.Save
{
    /// <summary>
    /// Autosave triggers (Loop 20): run end + app pause/quit. Currency, equipment and
    /// talent mutations already save at the mutation site (their services own atomicity);
    /// this covers the flow-level moments those services can't see. Mobile-critical:
    /// OnApplicationPause is often the LAST code that runs before the OS kills the process.
    /// </summary>
    public sealed class AutosaveController : MonoBehaviour
    {
        private ISaveService _save;
        private IEventBus _events;

        public void Initialize(ISaveService save, IEventBus events)
        {
            _save = save;
            _events = events;
            _events.Subscribe<RunEndedEvent>(OnRunEnded);
        }

        private void OnDestroy() => _events?.Unsubscribe<RunEndedEvent>(OnRunEnded);

        private void OnRunEnded(RunEndedEvent _) => _save.Save();

        private void OnApplicationPause(bool paused)
        {
            if (paused) _save?.Save();
        }

        private void OnApplicationQuit() => _save?.Save();
    }
}
