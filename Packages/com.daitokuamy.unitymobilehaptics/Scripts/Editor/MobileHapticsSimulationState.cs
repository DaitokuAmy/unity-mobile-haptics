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

        /// <summary>最後に再生要求された振動種別</summary>
        public HapticType LastType { get; private set; }

        /// <summary>表示用の最後の振動種別</summary>
        public string LastTypeText => HasPlaybackRequest ? LastType.ToString() : "None";

        /// <summary>表示用の再生モード文字列</summary>
        public string PlayModeText => HasPlaybackRequest ? _playMode.ToString() : "None";

        /// <summary>最後に状態更新された UTC 時刻</summary>
        public DateTime LastUpdatedUtc { get; private set; }

        /// <summary>表示用の最終更新時刻文字列</summary>
        public string LastUpdatedText => LastUpdatedUtc == DateTime.MinValue ? "Never" : LastUpdatedUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");

        /// <summary>現在の表示上で再生中の場合は true</summary>
        public bool IsActiveVisual { get; private set; }

        /// <summary>現在の表示用強度</summary>
        public float VisualIntensity { get; private set; }

        /// <summary>Oneshot の進行率</summary>
        public float OneShotProgress { get; private set; }

        /// <summary>Oneshot の残り秒数</summary>
        public float RemainingSeconds { get; private set; }

        /// <summary>現在の種別に対する想定再生時間</summary>
        public float ExpectedDurationSeconds { get; private set; }

        /// <summary>表示アクセント色</summary>
        public Color AccentColor { get; private set; }

        /// <summary>現在の再生が Loop の場合は true</summary>
        public bool IsLoopMode => _playMode == HapticPlayMode.Loop;

        private HapticPlayMode _playMode;

        /// <summary>
        /// Runtime 側の状態を読み取り直す
        /// </summary>
        public void Refresh() {
            HasPlaybackRequest = MobileHapticsEditorBridge.HasPlaybackRequest;
            IsPlaying = MobileHapticsEditorBridge.IsPlaying;
            IsSupported = MobileHapticsEditorBridge.IsSupported;
            LastType = MobileHapticsEditorBridge.LastType;
            _playMode = MobileHapticsEditorBridge.PlayMode;
            LastUpdatedUtc = MobileHapticsEditorBridge.LastUpdatedUtcTicks > 0
                ? new DateTime(MobileHapticsEditorBridge.LastUpdatedUtcTicks, DateTimeKind.Utc)
                : DateTime.MinValue;
            ExpectedDurationSeconds = GetExpectedDuration(LastType);
            AccentColor = GetAccentColor(LastType);
            Tick(EditorTime());
        }

        /// <summary>
        /// 時間経過に応じた表示状態を更新する
        /// </summary>
        public void Tick(double nowUtcSeconds) {
            if (!HasPlaybackRequest || LastUpdatedUtc == DateTime.MinValue) {
                IsActiveVisual = false;
                VisualIntensity = 0f;
                OneShotProgress = 0f;
                RemainingSeconds = 0f;
                return;
            }

            var elapsedSeconds = Math.Max(0d, nowUtcSeconds - new DateTimeOffset(LastUpdatedUtc).ToUnixTimeMilliseconds() / 1000d);
            if (_playMode == HapticPlayMode.OneShot) {
                UpdateOneShot(elapsedSeconds);
                return;
            }

            UpdateLoop(elapsedSeconds);
        }

        /// <summary>
        /// OneShot の表示状態を更新する
        /// </summary>
        private void UpdateOneShot(double elapsedSeconds) {
            if (!IsPlaying) {
                IsActiveVisual = false;
                VisualIntensity = 0f;
                OneShotProgress = 1f;
                RemainingSeconds = 0f;
                return;
            }

            var duration = Mathf.Max(ExpectedDurationSeconds, 0.01f);
            var progress = Mathf.Clamp01((float)(elapsedSeconds / duration));
            var pulse = Mathf.Sin(progress * Mathf.PI);
            var strength = GetBaseStrength(LastType);

            IsActiveVisual = progress < 1f;
            VisualIntensity = IsActiveVisual ? pulse * strength : 0f;
            OneShotProgress = progress;
            RemainingSeconds = IsActiveVisual ? Mathf.Max(0f, duration - (float)elapsedSeconds) : 0f;
        }

        /// <summary>
        /// Loop の表示状態を更新する
        /// </summary>
        private void UpdateLoop(double elapsedSeconds) {
            if (!IsPlaying) {
                IsActiveVisual = false;
                VisualIntensity = 0f;
                OneShotProgress = 0f;
                RemainingSeconds = 0f;
                return;
            }

            var cycle = Mathf.Max(ExpectedDurationSeconds, 0.18f) * 1.6f;
            var phase = Mathf.Repeat((float)(elapsedSeconds / cycle), 1f);
            var wave = 0.35f + Mathf.Sin(phase * Mathf.PI * 2f) * 0.25f + 0.4f;

            IsActiveVisual = true;
            VisualIntensity = Mathf.Clamp01(wave * GetBaseStrength(LastType));
            OneShotProgress = phase;
            RemainingSeconds = 0f;
        }

        /// <summary>
        /// 現在時刻を UTC 秒で取得する
        /// </summary>
        private static double EditorTime() {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000d;
        }

        /// <summary>
        /// 振動種別ごとの想定再生時間を返す
        /// </summary>
        private static float GetExpectedDuration(HapticType type) {
            switch (type) {
                case HapticType.Selection:
                    return 0.08f;
                case HapticType.Success:
                    return 0.18f;
                case HapticType.Warning:
                    return 0.24f;
                case HapticType.Error:
                    return 0.3f;
                case HapticType.LightImpact:
                    return 0.09f;
                case HapticType.MediumImpact:
                    return 0.13f;
                case HapticType.HeavyImpact:
                    return 0.18f;
                default:
                    return 0.12f;
            }
        }

        /// <summary>
        /// 振動種別ごとの基礎強度を返す
        /// </summary>
        private static float GetBaseStrength(HapticType type) {
            switch (type) {
                case HapticType.Selection:
                    return 0.3f;
                case HapticType.Success:
                    return 0.55f;
                case HapticType.Warning:
                    return 0.72f;
                case HapticType.Error:
                    return 0.95f;
                case HapticType.LightImpact:
                    return 0.4f;
                case HapticType.MediumImpact:
                    return 0.68f;
                case HapticType.HeavyImpact:
                    return 0.9f;
                default:
                    return 0.5f;
            }
        }

        /// <summary>
        /// 振動種別ごとのアクセント色を返す
        /// </summary>
        private static Color GetAccentColor(HapticType type) {
            switch (type) {
                case HapticType.Selection:
                    return new Color(0.37f, 0.73f, 0.96f);
                case HapticType.Success:
                    return new Color(0.32f, 0.83f, 0.55f);
                case HapticType.Warning:
                    return new Color(0.98f, 0.73f, 0.28f);
                case HapticType.Error:
                    return new Color(0.96f, 0.39f, 0.37f);
                case HapticType.LightImpact:
                    return new Color(0.54f, 0.76f, 0.98f);
                case HapticType.MediumImpact:
                    return new Color(0.66f, 0.54f, 0.98f);
                case HapticType.HeavyImpact:
                    return new Color(0.98f, 0.48f, 0.79f);
                default:
                    return new Color(0.7f, 0.78f, 0.88f);
            }
        }
    }
}
