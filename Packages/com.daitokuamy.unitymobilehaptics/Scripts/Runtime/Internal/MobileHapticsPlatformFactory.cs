using UnityEngine;
using UnityMobileHaptics.Platforms;

namespace UnityMobileHaptics.Internal {
    /// <summary>
    /// 実行環境に応じた振動実装を生成
    /// </summary>
    internal static class MobileHapticsPlatformFactory {
        /// <summary>
        /// 現在の実行環境に対応した振動実装を生成
        /// </summary>
        internal static IMobileHapticsPlatform Create() {
            switch (Application.platform) {
                case RuntimePlatform.Android:
                    return new AndroidMobileHapticsPlatform();
                case RuntimePlatform.IPhonePlayer:
                    return new IosMobileHapticsPlatform();
                default:
                    return new UnsupportedMobileHapticsPlatform();
            }
        }
    }
}
