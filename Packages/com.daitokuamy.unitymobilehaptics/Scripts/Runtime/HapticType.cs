namespace UnityMobileHaptics {
    /// <summary>
    /// 振動種別
    /// </summary>
    public enum HapticType {
        /// <summary>選択操作向けの軽い振動</summary>
        Selection,

        /// <summary>成功通知向けの振動</summary>
        Success,

        /// <summary>警告通知向けの振動</summary>
        Warning,

        /// <summary>エラー通知向けの振動</summary>
        Error,
    }
}
