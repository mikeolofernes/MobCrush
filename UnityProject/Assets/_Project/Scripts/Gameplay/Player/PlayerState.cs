namespace MobCrush.Gameplay.Player
{
    /// <summary>
    /// Player character states. Deliberately small — a survivor-game player has few
    /// exclusive modes; upgrades/pauses are global GameState concerns, not player states.
    /// </summary>
    public enum PlayerState
    {
        Idle,
        Moving,
        Dead
    }
}
