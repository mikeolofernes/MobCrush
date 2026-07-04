using System.Threading.Tasks;

namespace MobCrush.Core.SceneFlow
{
    /// <summary>Async scene transitions with an optional loading overlay (Loop 3 §12).</summary>
    public interface ISceneLoader
    {
        /// <summary>Loads a scene by name, optionally showing the loading screen while activation completes.</summary>
        Task LoadSceneAsync(string sceneName, bool showLoadingScreen = true);

        /// <summary>Additively loads a content scene (e.g. Stage_1) on top of the Game scene.</summary>
        Task LoadAdditiveAsync(string sceneName);

        Task UnloadAdditiveAsync(string sceneName);
    }
}
