using UnityEngine;

namespace UnityMobileHaptics.Platforms {
    /// <summary>
    /// Android 向け振動実装
    /// </summary>
    internal sealed class AndroidMobileHapticsPlatform : IMobileHapticsPlatform {
#if UNITY_ANDROID && !UNITY_EDITOR
        private const string BridgeClassName = "com.daitokuamy.unitymobilehaptics.UnityMobileHapticsAndroidBridge";

        private static AndroidJavaClass s_bridgeClass;
#endif

        public bool IsSupported {
            get {
#if UNITY_ANDROID && !UNITY_EDITOR
                return true;
#else
                return false;
#endif
            }
        }

        /// <inheritdoc />
        public void Play(HapticType type) {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (TryInvokeBridge("play", (int)type, false)) {
                return;
            }
#endif
        }

        /// <inheritdoc />
        public void PlayLoop(ImpactHapticType type) {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (TryInvokeBridge("play", (int)ToHapticType(type), true)) {
                return;
            }
#endif
        }

        /// <inheritdoc />
        public void Stop() {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (TryInvokeBridge("stop")) {
                return;
            }
#endif
        }

#if UNITY_ANDROID && !UNITY_EDITOR
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
        /// Android ネイティブブリッジを安全に呼び出す
        /// </summary>
        /// <param name="methodName">呼び出すメソッド名</param>
        /// <param name="args">メソッド引数</param>
        /// <returns>呼び出しに成功した場合は true</returns>
        private static bool TryInvokeBridge(string methodName, params object[] args) {
            try {
                var bridgeClass = s_bridgeClass ??= new AndroidJavaClass(BridgeClassName);
                bridgeClass.CallStatic(methodName, args);
                return true;
            }
            catch (AndroidJavaException) {
                return false;
            }
        }
#endif
    }
}
