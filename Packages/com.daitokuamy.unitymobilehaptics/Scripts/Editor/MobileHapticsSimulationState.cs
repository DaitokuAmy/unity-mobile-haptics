using System;
using UnityEngine;
using UnityMobileHaptics.Internal;

namespace UnityMobileHaptics.Editor {
    /// <summary>
    /// Editor シミュレーション表示用の状態
    /// </summary>
    internal sealed class MobileHapticsSimulationState {
        /// <summary>一度でも再生要求を受け取った場合は true</summary>
        public bool HasPlaybackRequest { get; private set; }

        /// <summary>現在再生中の場合は true</summary>
        public bool IsPlaying { get; private set; }

        /// <summary>実行経路が実機再生をサポートしている場合は true</summary>
        public bool IsSupported { get; private set; }

        /// <summary>最後に再生要求されたネイティブ振動種別</summary>
        public HapticType LastType { get; private set; }

        /// <summary>最後の再生要求が可変制御振動の場合は true</summary>
        public bool IsPulsePlayback { get; private set; }

        /// <summary>最後の可変制御振動の強度</summary>
        public float PulseIntensity { get; private set; }

        /// <summary>最後の可変制御振動の時間</summary>
        public float PulseDurationSeconds { get; private set; }

        /// <summary>ループ再生の場合は true</summary>
        public bool IsLoopMode { get; private set; }

        /// <summary>表示用の最後の振動名</summary>
        public string LastTypeText => !HasPlaybackRequest ? "None" : IsPulsePlayback ? "Pulse" : LastType.ToString();

        /// <summary>表示用の再生モード文字列</summary>
        public string PlayModeText => !HasPlaybackRequest ? "None" : IsLoopMode ? "Loop" : "OneShot";

        /// <summary>最後に状態更新された UTC 時刻</summary>
        public DateTime LastUpdatedUtc { get; private set; }

        /// <summary>現在の表示上で再生中の場合は true</summary>
        public bool IsActiveVisual { get; private set; }

        /// <summary>現在の表示用強度</summary>
        public float VisualIntensity { get; private set; }

        /// <summary>現在の進行率</summary>
        public float Progress { get; private set; }

        /// <summary>残り秒数</summary>
        public float RemainingSeconds { get; private set; }

        /// <summary>現在の種別に対する想定再生時間</summary>
        public float ExpectedDurationSeconds { get; private set; }

        /// <summary>表示アクセント色</summary>
        public Color AccentColor { get; private set; }

        /// <summary>
        /// Runtime 側の状態を読み取り直す
        /// </summary>
        public void Refresh() {
            HasPlaybackRequest = MobileHapticsEditorBridge.HasPlaybackRequest;
            IsPlaying = MobileHapticsEditorBridge.IsPlaying;
            IsSupported = MobileHapticsEditorBridge.IsSupported;
            LastType = MobileHapticsEditorBridge.LastType;
            IsPulsePlayback = MobileHapticsEditorBridge.IsPulsePlayback;
            IsLoopMode = MobileHapticsEditorBridge.IsLoopingPulse;
            PulseIntensity = MobileHapticsEditorBridge.PulseIntensity;
            PulseDurationSeconds = MobileHapticsEditorBridge.PulseDurationSeconds;
            LastUpdatedUtc = MobileHapticsEditorBridge.LastUpdatedUtcTicks > 0
                ? new DateTime(MobileHapticsEditorBridge.LastUpdatedUtcTicks, DateTimeKind.Utc)
                : DateTime.MinValue;
            ExpectedDurationSeconds = GetExpectedDuration();
            AccentColor = GetAccentColor();
            Tick(EditorTime());
        }

        /// <summary>
        /// 時間経過に応じた表示状態を更新する
        /// </summary>
        public void Tick(double nowUtcSeconds) {
            if (!HasPlaybackRequest || LastUpdatedUtc == DateTime.MinValue) {
                ResetVisualState();
                return;
            }

            var elapsedSeconds = Math.Max(0d, nowUtcSeconds - new DateTimeOffset(LastUpdatedUtc).ToUnixTimeMilliseconds() / 1000d);
            if (IsLoopMode) {
                UpdateLoop(elapsedSeconds);
                return;
            }

            UpdateOneShot(elapsedSeconds);
        }

        /// <summary>
        /// OneShot の表示状態を更新する
        /// </summary>
        private void UpdateOneShot(double elapsedSeconds) {
            if (!IsPlaying) {
                IsActiveVisual = false;
                VisualIntensity = 0f;
                Progress = 1f;
                RemainingSeconds = 0f;
                return;
            }

            var duration = Mathf.Max(ExpectedDurationSeconds, 0.01f);
            var progress = Mathf.Clamp01((float)(elapsedSeconds / duration));
            var pulse = Mathf.Sin(progress * Mathf.PI);
            var strength = GetBaseStrength();

            IsActiveVisual = progress < 1f;
            VisualIntensity = IsActiveVisual ? pulse * strength : 0f;
            Progress = progress;
            RemainingSeconds = IsActiveVisual ? Mathf.Max(0f, duration - (float)elapsedSeconds) : 0f;
        }

        /// <summary>
        /// Loop の表示状態を更新する
        /// </summary>
        private void UpdateLoop(double elapsedSeconds) {
            if (!IsPlaying) {
                ResetVisualState();
                return;
            }

            var pulseDuration = Mathf.Max(ExpectedDurationSeconds, 0.05f);
            var cycle = pulseDuration + 0.04f;
            var phaseSeconds = (float)(elapsedSeconds % cycle);
            var activeSeconds = Mathf.Min(phaseSeconds, pulseDuration);
            var activeProgress = Mathf.Clamp01(activeSeconds / pulseDuration);
            var pulse = Mathf.Sin(activeProgress * Mathf.PI);

            IsActiveVisual = phaseSeconds <= pulseDuration;
            VisualIntensity = IsActiveVisual ? pulse * GetBaseStrength() : 0f;
            Progress = Mathf.Clamp01(phaseSeconds / cycle);
            RemainingSeconds = 0f;
        }

        /// <summary>
        /// 現在時刻を UTC 秒で取得する
        /// </summary>
        private static double EditorTime() {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000d;
        }

        /// <summary>
        /// 見た目上の再生時間を返す
        /// </summary>
        private float GetExpectedDuration() {
            if (IsPulsePlayback) {
                return Mathf.Max(PulseDurationSeconds, 0.01f);
            }

            switch (LastType) {
                case HapticType.Selection:
                    return 0.08f;
                case HapticType.Success:
                    return 0.18f;
                case HapticType.Warning:
                    return 0.24f;
                case HapticType.Error:
                    return 0.3f;
                default:
                    return 0.12f;
            }
        }

        /// <summary>
        /// 見た目上の強度を返す
        /// </summary>
        private float GetBaseStrength() {
            if (IsPulsePlayback) {
                return Mathf.Clamp01(PulseIntensity);
            }

            switch (LastType) {
                case HapticType.Selection:
                    return 0.3f;
                case HapticType.Success:
                    return 0.55f;
                case HapticType.Warning:
                    return 0.72f;
                case HapticType.Error:
                    return 0.95f;
                default:
                    return 0.5f;
            }
        }

        /// <summary>
        /// 表示色を返す
        /// </summary>
        private Color GetAccentColor() {
            if (IsPulsePlayback) {
                return IsLoopMode
                    ? new Color(0.98f, 0.65f, 0.3f)
                    : new Color(0.93f, 0.48f, 0.37f);
            }

            switch (LastType) {
                case HapticType.Selection:
                    return new Color(0.37f, 0.73f, 0.96f);
                case HapticType.Success:
                    return new Color(0.32f, 0.83f, 0.55f);
                case HapticType.Warning:
                    return new Color(0.98f, 0.73f, 0.28f);
                case HapticType.Error:
                    return new Color(0.96f, 0.39f, 0.37f);
                default:
                    return new Color(0.7f, 0.78f, 0.88f);
            }
        }

        /// <summary>
        /// 表示状態を初期化する
        /// </summary>
        private void ResetVisualState() {
            IsActiveVisual = false;
            VisualIntensity = 0f;
            Progress = 0f;
            RemainingSeconds = 0f;
        }
    }
}
