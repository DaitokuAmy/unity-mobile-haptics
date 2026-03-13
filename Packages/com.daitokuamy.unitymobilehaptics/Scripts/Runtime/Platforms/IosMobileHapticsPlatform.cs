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
            UnityMobileHapticsPlay((int)type, false);
#endif
        }

        /// <inheritdoc />
        public void PlayLoop(HapticType type) {
#if UNITY_IOS && !UNITY_EDITOR
            UnityMobileHapticsPlay((int)type, true);
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
        /// <param name="isLoop">継続再生する場合は true</param>
        [DllImport("__Internal")]
        private static extern void UnityMobileHapticsPlay(int hapticType, bool isLoop);

        /// <summary>
        /// iOS ネイティブブリッジ経由で振動を停止
        /// </summary>
        [DllImport("__Internal")]
        private static extern void UnityMobileHapticsStop();
#endif
    }
}
