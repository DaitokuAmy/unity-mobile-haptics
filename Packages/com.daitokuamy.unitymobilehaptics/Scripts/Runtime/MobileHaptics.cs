using UnityMobileHaptics.Internal;
using UnityEngine;

namespace UnityMobileHaptics {
    /// <summary>
    /// モバイル端末向け振動制御の公開 API
    /// </summary>
    public static class MobileHaptics {
        // 現在の実行環境に対応する実装を遅延生成して保持する。
        private static IMobileHapticsPlatform s_platform;

        // 可変制御振動の再生ごとに払い出す連番。
        private static int s_nextPlaybackId;

        // 現在有効な停止ハンドルに対応する再生 ID。
        private static int s_activePlaybackId;

        // 現在の可変制御振動がループ再生中の場合は true。
        private static bool s_isActivePlaybackLoop;

        // OneShot の可変制御振動が自動的に無効化される時刻。
        private static float s_activePlaybackEndTime;

        /// <summary>現在の実行環境で振動再生をサポートしている場合は true</summary>
        public static bool IsSupported => Platform.IsSupported;

        /// <summary>有効状態</summary>
        public static bool IsEnabled { get; set; } = true;

        /// <summary>現在の実行環境に対応した振動実装</summary>
        private static IMobileHapticsPlatform Platform => s_platform ??= MobileHapticsPlatformFactory.Create();

        /// <summary>
        /// 単発振動を再生
        /// </summary>
        /// <param name="type">振動種別</param>
        public static void Play(HapticType type) {
            if (!IsEnabled) {
                return;
            }
            
            Platform.Play(type);
            ClearActivePlayback();
            MobileHapticsEditorBridge.NotifyPlayed(type, Platform.IsSupported);
        }

        /// <summary>
        /// 強度と時間を指定する可変制御振動を再生
        /// </summary>
        /// <param name="intensity">振動強度</param>
        /// <param name="durationSeconds">振動時間</param>
        /// <param name="loop">停止されるまで繰り返す場合は true</param>
        /// <returns>停止用ハンドル</returns>
        public static HapticPlaybackHandle PlayPulse(float intensity, float durationSeconds, bool loop = false) {
            if (!IsEnabled) {
                return default;
            }
            
            var clampedIntensity = Mathf.Clamp01(intensity);
            var clampedDuration = Mathf.Max(0.01f, durationSeconds);
            var isSupported = Platform.IsSupported;

            Platform.PlayPulse(clampedIntensity, clampedDuration, loop);
            MobileHapticsEditorBridge.NotifyPulsePlayed(clampedIntensity, clampedDuration, loop, isSupported);

            if (!isSupported) {
                ClearActivePlayback();
                return default;
            }

            s_activePlaybackId = ++s_nextPlaybackId;
            s_isActivePlaybackLoop = loop;
            s_activePlaybackEndTime = loop ? float.PositiveInfinity : Time.realtimeSinceStartup + clampedDuration;
            return new HapticPlaybackHandle(s_activePlaybackId);
        }

        /// <summary>
        /// 再生中の振動を停止
        /// </summary>
        public static void Stop() {
            Platform.Stop();
            ClearActivePlayback();
            MobileHapticsEditorBridge.NotifyStopped(Platform.IsSupported);
        }

        /// <summary>
        /// 指定したハンドル ID が現在の再生を停止可能な状態かを判定
        /// </summary>
        /// <param name="playbackId">判定対象の再生 ID</param>
        /// <returns>現在の再生を停止できる場合は true</returns>
        internal static bool IsPlaybackHandleValid(int playbackId) {
            if (playbackId == 0 || playbackId != s_activePlaybackId) {
                return false;
            }

            if (s_isActivePlaybackLoop) {
                return true;
            }

            if (Time.realtimeSinceStartup <= s_activePlaybackEndTime) {
                return true;
            }

            ClearActivePlayback();
            return false;
        }

        /// <summary>
        /// 指定したハンドル ID に対応する再生を停止
        /// </summary>
        /// <param name="playbackId">停止対象の再生 ID</param>
        internal static void Stop(int playbackId) {
            if (!IsPlaybackHandleValid(playbackId)) {
                return;
            }

            Stop();
        }

        /// <summary>
        /// 現在の可変制御振動に紐づくハンドル状態を無効化
        /// </summary>
        private static void ClearActivePlayback() {
            s_activePlaybackId = 0;
            s_isActivePlaybackLoop = false;
            s_activePlaybackEndTime = 0f;
        }
    }
}
