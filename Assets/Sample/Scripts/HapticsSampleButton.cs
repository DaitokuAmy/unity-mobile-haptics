using UnityEngine;
using UnityEngine.UI;

namespace UnityMobileHaptics.Sample {
    /// <summary>
    /// uGUI Button と HapticsSampleController を結び付ける補助コンポーネント
    /// </summary>
    [RequireComponent(typeof(Button))]
    public sealed class HapticsSampleButton : MonoBehaviour {
        [SerializeField]
        private HapticsSampleController _controller;

        [SerializeField]
        private HapticType _hapticType = HapticType.Selection;

        [SerializeField]
        private ImpactHapticType _loopHapticType = ImpactHapticType.Medium;

        [SerializeField]
        private bool _isLoop;

        [SerializeField]
        private bool _isStopButton;

        private Button _button;

        /// <summary>
        /// Button クリック時のイベントを登録
        /// </summary>
        private void Awake() {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnClick);
        }

        /// <summary>
        /// Button クリック時のイベントを解除
        /// </summary>
        private void OnDestroy() {
            if (_button == null) {
                return;
            }

            _button.onClick.RemoveListener(OnClick);
        }

        /// <summary>
        /// Button クリック時の振る舞いを実行
        /// </summary>
        private void OnClick() {
            if (_controller == null) {
                return;
            }

            if (_isStopButton) {
                _controller.Stop();
                return;
            }

            if (_isLoop) {
                _controller.PlayLoop(_loopHapticType);
                return;
            }

            _controller.Play(_hapticType);
        }

        /// <summary>
        /// Button の動作設定を初期化
        /// </summary>
        /// <param name="controller">操作先の controller</param>
        /// <param name="hapticType">振動種別</param>
        /// <param name="loopHapticType">Loop 用振動種別</param>
        /// <param name="isLoop">Loop 再生する場合は true</param>
        /// <param name="isStopButton">停止ボタンとして扱う場合は true</param>
        public void Setup(HapticsSampleController controller, HapticType hapticType, ImpactHapticType loopHapticType, bool isLoop, bool isStopButton) {
            _controller = controller;
            _hapticType = hapticType;
            _loopHapticType = loopHapticType;
            _isLoop = isLoop;
            _isStopButton = isStopButton;
        }
    }
}
