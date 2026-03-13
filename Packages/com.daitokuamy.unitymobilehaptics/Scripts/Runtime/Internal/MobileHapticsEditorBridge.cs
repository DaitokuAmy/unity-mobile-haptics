using System;

namespace UnityMobileHaptics.Internal {
    /// <summary>
    /// Runtime と Editor シミュレーション間の再生状態を中継
    /// </summary>
    internal static class MobileHapticsEditorBridge {
        private static bool s_hasPlaybackRequest;
        private static bool s_isPlaying;
        private static bool s_isSupported;
        private static bool s_isPulsePlayback;
        private static bool s_isLoopingPulse;
        private static HapticType s_lastType;
        private static float s_pulseIntensity;
        private static float s_pulseDurationSeconds;
        private static long s_lastUpdatedUtcTicks;

        /// <summary>
        /// シミュレーション状態が更新された時に発火
        /// </summary>
        internal static event Action StateChanged;

        /// <summary>一度でも再生要求を受け取った場合は true</summary>
        internal static bool HasPlaybackRequest => s_hasPlaybackRequest;

        /// <summary>現在再生中の場合は true</summary>
        internal static bool IsPlaying => s_isPlaying;

        /// <summary>実行経路が実機再生をサポートしている場合は true</summary>
        internal static bool IsSupported => s_isSupported;

        /// <summary>最後に要求された振動種別</summary>
        internal static HapticType LastType => s_lastType;

        /// <summary>最後の再生要求が可変制御振動の場合は true</summary>
        internal static bool IsPulsePlayback => s_isPulsePlayback;

        /// <summary>最後の可変制御振動がループ再生の場合は true</summary>
        internal static bool IsLoopingPulse => s_isLoopingPulse;

        /// <summary>最後に要求された可変制御振動の強度</summary>
        internal static float PulseIntensity => s_pulseIntensity;

        /// <summary>最後に要求された可変制御振動の時間</summary>
        internal static float PulseDurationSeconds => s_pulseDurationSeconds;

        /// <summary>最後に状態更新された UTC 時刻の ticks</summary>
        internal static long LastUpdatedUtcTicks => s_lastUpdatedUtcTicks;

        /// <summary>
        /// 再生要求をシミュレーション状態へ反映
        /// </summary>
        /// <param name="type">振動種別</param>
        /// <param name="isSupported">実行経路が実機再生に対応している場合は true</param>
        internal static void NotifyPlayed(HapticType type, bool isSupported) {
            s_hasPlaybackRequest = true;
            s_isPlaying = true;
            s_isSupported = isSupported;
            s_isPulsePlayback = false;
            s_isLoopingPulse = false;
            s_lastType = type;
            s_pulseIntensity = 0f;
            s_pulseDurationSeconds = 0f;
            s_lastUpdatedUtcTicks = DateTime.UtcNow.Ticks;
            StateChanged?.Invoke();
        }

        /// <summary>
        /// 可変制御振動の再生要求をシミュレーション状態へ反映
        /// </summary>
        /// <param name="intensity">振動強度</param>
        /// <param name="durationSeconds">振動時間</param>
        /// <param name="loop">ループ再生する場合は true</param>
        /// <param name="isSupported">実行経路が実機再生に対応している場合は true</param>
        internal static void NotifyPulsePlayed(float intensity, float durationSeconds, bool loop, bool isSupported) {
            s_hasPlaybackRequest = true;
            s_isPlaying = true;
            s_isSupported = isSupported;
            s_isPulsePlayback = true;
            s_isLoopingPulse = loop;
            s_pulseIntensity = intensity;
            s_pulseDurationSeconds = durationSeconds;
            s_lastUpdatedUtcTicks = DateTime.UtcNow.Ticks;
            StateChanged?.Invoke();
        }

        /// <summary>
        /// 停止要求をシミュレーション状態へ反映
        /// </summary>
        /// <param name="isSupported">実行経路が実機再生に対応している場合は true</param>
        internal static void NotifyStopped(bool isSupported) {
            s_isPlaying = false;
            s_isSupported = isSupported;
            s_lastUpdatedUtcTicks = DateTime.UtcNow.Ticks;
            StateChanged?.Invoke();
        }
    }
}
