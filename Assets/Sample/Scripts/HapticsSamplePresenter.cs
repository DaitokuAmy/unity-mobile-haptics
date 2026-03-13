using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UnityMobileHaptics.Sample {
    /// <summary>
    /// Unity Mobile Haptics の uGUI サンプル全体を制御
    /// </summary>
    public sealed class HapticsSamplePresenter : MonoBehaviour {
        [SerializeField]
        private TMP_Text _statusText;
        [SerializeField]
        private Button _selectionButton;
        [SerializeField]
        private Button _successButton;
        [SerializeField]
        private Button _warningButton;
        [SerializeField]
        private Button _errorButton;
        [SerializeField]
        private Slider _pulseIntensitySlider;
        [SerializeField]
        private TMP_Text _pulseIntensityValueText;
        [SerializeField]
        private Slider _pulseDurationSlider;
        [SerializeField]
        private TMP_Text _pulseDurationValueText;
        [SerializeField]
        private Toggle _pulseLoopToggle;
        [SerializeField]
        private Button _pulseButton;
        [SerializeField]
        private Button _stopButton;

        [SerializeField, Range(0.0f, 1.0f)]
        private float _defaultPulseIntensity = 0.5f;
        [SerializeField, Range(0.01f, 10.0f)]
        private float _defaultPulseDurationSeconds = 0.5f;

        /// <summary>
        /// 初期表示を更新
        /// </summary>
        private void Awake() {
            RegisterSliderEvents();
            RegisterButtonEvents();
            RefreshPulseLabels();
            RefreshStatus("Ready");
            
            _pulseIntensitySlider.value = _defaultPulseIntensity;
            _pulseDurationSlider.value = _defaultPulseDurationSeconds;
        }

        private void RegisterButtonEvents() {
            _selectionButton.onClick.AddListener(() => Play(HapticType.Selection));
            _successButton.onClick.AddListener(() => Play(HapticType.Success));
            _warningButton.onClick.AddListener(() => Play(HapticType.Warning));
            _errorButton.onClick.AddListener(() => Play(HapticType.Error));
            _pulseButton.onClick.AddListener(() => PlayPulse(GetPulseIntensity(), GetPulseDurationSeconds(), _pulseLoopToggle.isOn));
            _stopButton.onClick.AddListener(Stop);
        }

        private void UnregisterButtonEvents() {
            _selectionButton.onClick.RemoveAllListeners();
            _successButton.onClick.RemoveAllListeners();
            _warningButton.onClick.RemoveAllListeners();
            _errorButton.onClick.RemoveAllListeners();
            _pulseButton.onClick.RemoveAllListeners();
            _stopButton.onClick.RemoveAllListeners();
        }

        private float GetPulseIntensity() {
            return Mathf.Clamp01(_pulseIntensitySlider.value);
        }

        private float GetPulseDurationSeconds() {
            return Mathf.Clamp(_pulseDurationSlider.value, 0.01f, 10.0f);
        }

        /// <summary>
        /// 破棄時にスライダーイベントを解除
        /// </summary>
        private void OnDestroy() {
            UnregisterButtonEvents();
            UnregisterSliderEvents();
        }

        /// <summary>
        /// 単発振動を再生
        /// </summary>
        /// <param name="type">振動種別</param>
        private void Play(HapticType type) {
            MobileHaptics.Play(type);
            RefreshStatus($"Play: {type}");
        }

        /// <summary>
        /// 可変制御振動を再生
        /// </summary>
        /// <param name="intensity">振動強度</param>
        /// <param name="durationSeconds">振動時間</param>
        /// <param name="loop">ループ再生する場合は true</param>
        private void PlayPulse(float intensity, float durationSeconds, bool loop) {
            MobileHaptics.PlayPulse(intensity, durationSeconds, loop);
            RefreshStatus(loop
                ? $"PlayPulse: {intensity:0.00}, {durationSeconds:0.00}s, Loop"
                : $"PlayPulse: {intensity:0.00}, {durationSeconds:0.00}s");
        }

        /// <summary>
        /// 再生中の振動を停止
        /// </summary>
        private void Stop() {
            MobileHaptics.Stop();
            RefreshStatus("Stop");
        }

        /// <summary>
        /// スライダーイベントを登録
        /// </summary>
        private void RegisterSliderEvents() {
            if (_pulseIntensitySlider != null) {
                _pulseIntensitySlider.onValueChanged.AddListener(OnSliderValueChanged);
            }

            if (_pulseDurationSlider != null) {
                _pulseDurationSlider.onValueChanged.AddListener(OnSliderValueChanged);
            }
        }

        /// <summary>
        /// スライダーイベントを解除
        /// </summary>
        private void UnregisterSliderEvents() {
            if (_pulseIntensitySlider != null) {
                _pulseIntensitySlider.onValueChanged.RemoveListener(OnSliderValueChanged);
            }

            if (_pulseDurationSlider != null) {
                _pulseDurationSlider.onValueChanged.RemoveListener(OnSliderValueChanged);
            }
        }

        /// <summary>
        /// pulse パラメータ表示ラベルを更新
        /// </summary>
        private void RefreshPulseLabels() {
            if (_pulseIntensityValueText != null) {
                _pulseIntensityValueText.text = $"{GetPulseIntensity():0.00}";
            }

            if (_pulseDurationValueText != null) {
                _pulseDurationValueText.text = $"{GetPulseDurationSeconds():0.00}s";
            }
        }

        /// <summary>
        /// 画面表示用の状態文字列を更新
        /// </summary>
        /// <param name="actionText">最後に実行した操作</param>
        private void RefreshStatus(string actionText) {
            if (_statusText == null) {
                return;
            }

            _statusText.text = $"Supported: {MobileHaptics.IsSupported}\nLast Action: {actionText}";
        }

        /// <summary>
        /// スライダー変更時に値表示を更新
        /// </summary>
        /// <param name="_">通知された値</param>
        private void OnSliderValueChanged(float _) {
            RefreshPulseLabels();
        }
    }
}