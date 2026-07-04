namespace MobCrush.Core.SceneFlow
{
    /// <summary>Top-level application states driven by <see cref="IGameStateMachine"/> (Loop 3 §12).</summary>
    public enum GameState
    {
        Booting,
        MainMenu,
        LoadingRun,
        Playing,
        LevelUpPause, // gameplay frozen while the player drafts an upgrade
        Paused,       // user-initiated pause menu
        RunEnding,    // death/victory sequence playing out
        Results
    }
}
