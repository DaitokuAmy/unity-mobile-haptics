namespace UnityMobileHaptics.Platforms {
    /// <summary>
    /// 非対応環境向けの no-op 実装
    /// </summary>
    internal sealed class UnsupportedMobileHapticsPlatform : IMobileHapticsPlatform {
        public bool IsSupported => false;

        /// <inheritdoc />
        public void Play(HapticType type) {
        }

        /// <inheritdoc />
        public void PlayLoop(HapticType type) {
        }

        /// <inheritdoc />
        public void Stop() {
        }
    }
}
