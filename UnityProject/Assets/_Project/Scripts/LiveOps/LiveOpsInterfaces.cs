using System;
using System.Collections.Generic;

namespace MobCrush.LiveOps
{
    /// <summary>
    /// SDK-facing seams (Loop 23). The game codes against these from Day 1 of LiveOps;
    /// concrete ad/analytics/config SDKs are adapter classes added at integration time —
    /// keeping SDK bloat OUT of every other assembly (Loop 0 risk #6) and making
    /// store builds without SDKs (e.g. for review) a composition-root switch.
    /// </summary>
    public interface IAnalyticsService
    {
        void Track(string eventName);
        void Track(string eventName, IReadOnlyDictionary<string, object> parameters);
        void SetUserProperty(string key, string value);
    }

    public interface IAdsService
    {
        bool IsRewardedReady { get; }
        /// <summary>onFinished(true) only on a completed watch — the reward gate.</summary>
        void ShowRewarded(string placementId, Action<bool> onFinished);
        void ShowInterstitial(string placementId);
    }

    public interface IRemoteConfigService
    {
        /// <summary>Fetch is fire-and-forget at boot; values fall back to defaults until it lands.</summary>
        void Fetch(Action onCompleted = null);
        float GetFloat(string key, float defaultValue);
        int GetInt(string key, int defaultValue);
        bool GetBool(string key, bool defaultValue);
        string GetString(string key, string defaultValue);

        /// <summary>A/B: stable bucket [0..bucketCount) derived from a persistent user id + experiment key.</summary>
        int GetExperimentBucket(string experimentKey, int bucketCount);
    }
}
