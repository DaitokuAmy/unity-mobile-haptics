using System.Runtime.InteropServices;

namespace UnityMobileHaptics.Platforms {
    /// <summary>
    /// iOS 向け振動実装
    /// </summary>
    internal sealed class IosMobileHapticsPlatform : IMobileHapticsPlatform {
        public bool IsSupported {
            get {
#if UNITY_IOS && !UNITY_EDITOR
                return true;
#else
                return false;
#endif
            }
        }

        /// <inheritdoc />
        public void Play(HapticType type) {
#if UNITY_IOS && !UNITY_EDITOR
            UnityMobileHapticsPlay((int)type);
#endif
        }

        /// <inheritdoc />
        public void PlayPulse(float intensity, float durationSeconds, bool loop) {
#if UNITY_IOS && !UNITY_EDITOR
            UnityMobileHapticsPlayPulse(intensity, durationSeconds, loop);
#endif
        }

        /// <inheritdoc />
        public void Stop() {
#if UNITY_IOS && !UNITY_EDITOR
            UnityMobileHapticsStop();
#endif
        }

#if UNITY_IOS && !UNITY_EDITOR
        /// <summary>
        /// iOS ネイティブブリッジ経由で振動を再生
        /// </summary>
        /// <param name="hapticType">振動種別</param>
        [DllImport("__Internal")]
        private static extern void UnityMobileHapticsPlay(int hapticType);

        /// <summary>
        /// iOS ネイティブブリッジ経由で可変制御振動を再生
        /// </summary>
        /// <param name="intensity">振動強度</param>
        /// <param name="durationSeconds">振動時間</param>
        /// <param name="isLoop">継続再生する場合は true</param>
        [DllImport("__Internal")]
        private static extern void UnityMobileHapticsPlayPulse(float intensity, float durationSeconds, bool isLoop);

        /// <summary>
        /// iOS ネイティブブリッジ経由で振動を停止
        /// </summary>
        [DllImport("__Internal")]
        private static extern void UnityMobileHapticsStop();
#endif
    }
}
