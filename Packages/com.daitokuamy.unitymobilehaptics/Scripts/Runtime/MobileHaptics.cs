using UnityMobileHaptics.Internal;

namespace UnityMobileHaptics {
    /// <summary>
    /// モバイル端末向け振動制御の公開 API
    /// </summary>
    public static class MobileHaptics {
        private static IMobileHapticsPlatform s_platform;

        /// <summary>現在の実行環境で振動再生をサポートしている場合は true</summary>
        public static bool IsSupported => Platform.IsSupported;

        /// <summary>現在の実行環境に対応した振動実装</summary>
        private static IMobileHapticsPlatform Platform => s_platform ??= MobileHapticsPlatformFactory.Create();

        /// <summary>
        /// 単発振動を再生
        /// </summary>
        /// <param name="type">振動種別</param>
        public static void Play(HapticType type) {
            Platform.Play(type);
            MobileHapticsEditorBridge.NotifyPlayed(type, HapticPlayMode.OneShot, Platform.IsSupported);
        }

        /// <summary>
        /// 停止されるまで継続振動を再生
        /// </summary>
        /// <param name="type">振動種別</param>
        public static void PlayLoop(HapticType type) {
            Platform.PlayLoop(type);
            MobileHapticsEditorBridge.NotifyPlayed(type, HapticPlayMode.Loop, Platform.IsSupported);
        }

        /// <summary>
        /// 再生中の振動を停止
        /// </summary>
        public static void Stop() {
            Platform.Stop();
            MobileHapticsEditorBridge.NotifyStopped(Platform.IsSupported);
        }
    }
}