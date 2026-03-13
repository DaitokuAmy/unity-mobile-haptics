namespace UnityMobileHaptics {
    /// <summary>
    /// 可変制御振動の停止に利用するハンドル
    /// </summary>
    public readonly struct HapticPlaybackHandle {
        private readonly int _playbackId;

        /// <summary>
        /// 指定した再生 ID に紐づくハンドルを生成
        /// </summary>
        /// <param name="playbackId">再生 ID</param>
        internal HapticPlaybackHandle(int playbackId) {
            _playbackId = playbackId;
        }

        /// <summary>現在の再生を停止できる状態の場合は true</summary>
        public bool IsValid => MobileHaptics.IsPlaybackHandleValid(_playbackId);

        /// <summary>
        /// このハンドルに対応する再生を停止
        /// </summary>
        public void Stop() {
            MobileHaptics.Stop(_playbackId);
        }
    }
}
