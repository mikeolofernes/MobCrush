using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MobCrush.Core.SceneFlow
{
    /// <summary>
    /// Default <see cref="ISceneLoader"/> built on SceneManager async APIs.
    /// why async/await over coroutines: loading flows compose (await load, then warm pools,
    /// then fade) and surface exceptions instead of silently dying (Loop 0 §5 rule 9).
    /// The loading overlay is a persistent CanvasGroup owned by the bootstrap scene,
    /// injected here so this service stays UI-framework agnostic.
    /// </summary>
    public sealed class SceneLoaderService : ISceneLoader
    {
        private readonly CanvasGroup _loadingOverlay;
        private const float FadeSeconds = 0.2f; // cosmetic only; real tuning in GameConfig if it ever matters

        public SceneLoaderService(CanvasGroup loadingOverlay)
        {
            _loadingOverlay = loadingOverlay;
        }

        public async Task LoadSceneAsync(string sceneName, bool showLoadingScreen = true)
        {
            if (showLoadingScreen) await Fade(1f);

            var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            while (!op.isDone)
                await Task.Yield();

            if (showLoadingScreen) await Fade(0f);
        }

        public async Task LoadAdditiveAsync(string sceneName)
        {
            var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            while (!op.isDone)
                await Task.Yield();
        }

        public async Task UnloadAdditiveAsync(string sceneName)
        {
            var scene = SceneManager.GetSceneByName(sceneName);
            if (!scene.isLoaded) return;
            var op = SceneManager.UnloadSceneAsync(scene);
            while (op != null && !op.isDone)
                await Task.Yield();
        }

        private async Task Fade(float target)
        {
            if (_loadingOverlay == null) return;
            _loadingOverlay.blocksRaycasts = target > 0.5f;

            float start = _loadingOverlay.alpha;
            float t = 0f;
            while (t < FadeSeconds)
            {
                // unscaledDeltaTime: fades must run while the game is paused (timeScale 0).
                t += Time.unscaledDeltaTime;
                _loadingOverlay.alpha = Mathf.Lerp(start, target, t / FadeSeconds);
                await Task.Yield();
            }
            _loadingOverlay.alpha = target;
        }
    }
}
