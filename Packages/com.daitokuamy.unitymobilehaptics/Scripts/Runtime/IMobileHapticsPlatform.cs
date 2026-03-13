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
        /// 強度と時間を指定する可変制御振動を再生
        /// </summary>
        /// <param name="intensity">振動強度</param>
        /// <param name="durationSeconds">振動時間</param>
        /// <param name="loop">停止されるまで繰り返す場合は true</param>
        void PlayPulse(float intensity, float durationSeconds, bool loop);

        /// <summary>
        /// 再生中の振動を停止
        /// </summary>
        void Stop();
    }
}
