using System;
using System.Collections.Generic;
using UnityEngine;

namespace MobCrush.LiveOps
{
    // Null-object implementations: registered by default so every consumer works before
    // (and without) real SDKs. Ads "succeed" in dev builds so reward flows are testable.

    public sealed class NullAnalyticsService : IAnalyticsService
    {
        public void Track(string eventName) => Log(eventName);
        public void Track(string eventName, IReadOnlyDictionary<string, object> parameters) => Log(eventName);
        public void SetUserProperty(string key, string value) { }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private static void Log(string eventName) => Debug.Log($"[Analytics] {eventName}");
    }

    public sealed class NullAdsService : IAdsService
    {
        public bool IsRewardedReady => true;

        public void ShowRewarded(string placementId, Action<bool> onFinished)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            onFinished?.Invoke(true);  // dev: instant success so revive/chest flows are playable
#else
            onFinished?.Invoke(false); // prod without an SDK: never grant unearned rewards
#endif
        }

        public void ShowInterstitial(string placementId) { }
    }

    public sealed class NullRemoteConfigService : IRemoteConfigService
    {
        public void Fetch(Action onCompleted = null) => onCompleted?.Invoke();
        public float GetFloat(string key, float defaultValue) => defaultValue;
        public int GetInt(string key, int defaultValue) => defaultValue;
        public bool GetBool(string key, bool defaultValue) => defaultValue;
        public string GetString(string key, string defaultValue) => defaultValue;

        public int GetExperimentBucket(string experimentKey, int bucketCount) =>
            bucketCount <= 0 ? 0
            : Mathf.Abs((SystemInfo.deviceUniqueIdentifier + experimentKey).GetHashCode()) % bucketCount;
    }
}
