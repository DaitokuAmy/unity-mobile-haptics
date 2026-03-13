using UnityEditor;
using UnityMobileHaptics.Internal;
using UnityEngine;

namespace UnityMobileHaptics.Editor {
    /// <summary>
    /// 振動状態のシミュレーションを表示する EditorWindow
    /// </summary>
    internal sealed class MobileHapticsSimulationWindow : EditorWindow {
        private readonly MobileHapticsSimulationState _state = new MobileHapticsSimulationState();
        private readonly MobileHapticsSimulationView _view = new MobileHapticsSimulationView();
        private Vector2 _scrollPosition;

        /// <summary>
        /// シミュレーションウィンドウを開く
        /// </summary>
        [MenuItem("Window/Unity Mobile Haptics/Simulation")]
        private static void Open() {
            var window = GetWindow<MobileHapticsSimulationWindow>();
            window.titleContent = new UnityEngine.GUIContent("Haptics");
            window.minSize = new Vector2(280f, 160f);
            window.Show();
        }

        /// <summary>
        /// ウィンドウ有効化時の初期化
        /// </summary>
        private void OnEnable() {
            _state.Refresh();
            MobileHapticsEditorBridge.StateChanged += OnStateChanged;
            EditorApplication.update += OnEditorUpdate;
        }

        /// <summary>
        /// ウィンドウ無効化時の後始末
        /// </summary>
        private void OnDisable() {
            MobileHapticsEditorBridge.StateChanged -= OnStateChanged;
            EditorApplication.update -= OnEditorUpdate;
        }

        /// <summary>
        /// ウィンドウ内容を描画
        /// </summary>
        private void OnGUI() {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            _view.Draw(_state);
            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// Editor 更新に合わせて再描画
        /// </summary>
        private void OnEditorUpdate() {
            _state.Tick(System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000d);
            Repaint();
        }

        /// <summary>
        /// Runtime 側の状態更新をウィンドウへ反映
        /// </summary>
        private void OnStateChanged() {
            _state.Refresh();
            Repaint();
        }
    }
}
