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
        public void PlayLoop(ImpactHapticType type) {
#if UNITY_IOS && !UNITY_EDITOR
            UnityMobileHapticsPlay((int)ToHapticType(type), true);
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
        /// Loop 用振動種別をネイティブ用の種別へ変換
        /// </summary>
        /// <param name="type">Loop 用振動種別</param>
        /// <returns>ネイティブ側と整合する振動種別</returns>
        private static HapticType ToHapticType(ImpactHapticType type) {
            switch (type) {
                case ImpactHapticType.Light:
                    return HapticType.LightImpact;
                case ImpactHapticType.Medium:
                    return HapticType.MediumImpact;
                case ImpactHapticType.Heavy:
                    return HapticType.HeavyImpact;
                default:
                    return HapticType.MediumImpact;
            }
        }

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
