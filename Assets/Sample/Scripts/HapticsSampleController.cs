using UnityEngine;
using TMPro;

namespace UnityMobileHaptics.Sample {
    /// <summary>
    /// Unity Mobile Haptics の uGUI サンプル全体を制御
    /// </summary>
    public sealed class HapticsSampleController : MonoBehaviour {
        [SerializeField]
        private TMP_Text _titleText;

        [SerializeField]
        private TMP_Text _statusText;

        /// <summary>
        /// 初期表示を更新
        /// </summary>
        private void Awake() {
            ApplyTitle();
            RefreshStatus("Ready");
        }

        /// <summary>
        /// 画面表示用ラベルを設定
        /// </summary>
        /// <param name="titleText">タイトル表示ラベル</param>
        /// <param name="statusText">状態表示ラベル</param>
        public void Configure(TMP_Text titleText, TMP_Text statusText) {
            _titleText = titleText;
            _statusText = statusText;
            ApplyTitle();
            RefreshStatus("Ready");
        }

        /// <summary>
        /// 単発振動を再生
        /// </summary>
        /// <param name="type">振動種別</param>
        public void Play(HapticType type) {
            MobileHaptics.Play(type);
            RefreshStatus($"Play: {type}");
        }

        /// <summary>
        /// Loop 振動を再生
        /// </summary>
        /// <param name="type">振動種別</param>
        public void PlayLoop(HapticType type) {
            MobileHaptics.PlayLoop(type);
            RefreshStatus($"PlayLoop: {type}");
        }

        /// <summary>
        /// 再生中の振動を停止
        /// </summary>
        public void Stop() {
            MobileHaptics.Stop();
            RefreshStatus("Stop");
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
        /// タイトル表示を更新
        /// </summary>
        private void ApplyTitle() {
            if (_titleText == null) {
                return;
            }

            _titleText.text = "Unity Mobile Haptics Sample";
        }
    }
}
