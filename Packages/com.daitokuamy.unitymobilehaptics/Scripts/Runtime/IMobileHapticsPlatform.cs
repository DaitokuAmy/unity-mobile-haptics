namespace UnityMobileHaptics {
    /// <summary>
    /// プラットフォーム別振動実装の共通契約
    /// </summary>
    internal interface IMobileHapticsPlatform {
        /// <summary>現在の実行環境で振動再生をサポートしている場合は true</summary>
        bool IsSupported { get; }

        /// <summary>
        /// 単発振動を再生
        /// </summary>
        /// <param name="type">振動種別</param>
        void Play(HapticType type);

        /// <summary>
        /// 停止されるまで継続振動を再生
        /// </summary>
        /// <param name="type">振動種別</param>
        void PlayLoop(ImpactHapticType type);

        /// <summary>
        /// 再生中の振動を停止
        /// </summary>
        void Stop();
    }
}
