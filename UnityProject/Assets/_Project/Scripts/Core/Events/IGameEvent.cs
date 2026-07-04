namespace MobCrush.Core.Events
{
    /// <summary>
    /// Marker for event payloads published through <see cref="IEventBus"/>.
    /// why: constraining events to structs keeps dispatch allocation-free (Loop 0 §5 rule 6)
    /// and makes payloads immutable-by-copy, so subscribers can never corrupt shared state.
    /// </summary>
    public interface IGameEvent
    {
    }
}
