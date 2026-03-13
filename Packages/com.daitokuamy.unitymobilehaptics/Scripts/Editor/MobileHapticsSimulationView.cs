using UnityEditor;
using UnityEngine;

namespace UnityMobileHaptics.Editor {
    /// <summary>
    /// 振動シミュレーション用 UI 描画
    /// </summary>
    internal sealed class MobileHapticsSimulationView {
        private static readonly Color SurfaceColor = new Color(0.11f, 0.13f, 0.17f);
        private static readonly Color PanelColor = new Color(0.15f, 0.18f, 0.24f);
        private static readonly Color TrackColor = new Color(0.2f, 0.24f, 0.3f);
        private static readonly Color IdleColor = new Color(0.27f, 0.31f, 0.37f);
        private const float ButtonHeight = 24f;
        private const float ButtonSpacing = 6f;
        private const float ButtonMinWidth = 72f;
        private const float ButtonMaxWidth = 96f;

        private GUIStyle _titleStyle;
        private GUIStyle _captionStyle;
        private GUIStyle _metricStyle;
        private GUIStyle _buttonStyle;

        /// <summary>
        /// シミュレーションウィンドウの UI を描画
        /// </summary>
        /// <param name="state">表示対象のシミュレーション状態</param>
        public void Draw(MobileHapticsSimulationState state) {
            EnsureStyles();

            using (new EditorGUILayout.VerticalScope()) {
                DrawHeader();
                GUILayout.Space(6f);
                DrawVisualizer(state);
                GUILayout.Space(8f);
                DrawTimeline(state);
                GUILayout.Space(8f);
                DrawMetrics(state);
                GUILayout.Space(10f);
                DrawQuickTestButtons();
            }
        }

        /// <summary>
        /// 見出しを描画する
        /// </summary>
        private void DrawHeader() {
            EditorGUILayout.LabelField("Haptics Simulation", _titleStyle);
            EditorGUILayout.LabelField("Editor 上で振動の強さと継続時間を視覚化", _captionStyle);
        }

        /// <summary>
        /// メインの可視化領域を描画する
        /// </summary>
        private static void DrawVisualizer(MobileHapticsSimulationState state) {
            var rect = GUILayoutUtility.GetRect(10f, 144f, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, SurfaceColor);

            var center = new Vector2(rect.x + rect.width * 0.5f, rect.y + rect.height * 0.52f);
            var baseRadius = Mathf.Min(rect.width, rect.height) * 0.18f;
            var outerRadius = baseRadius + baseRadius * state.VisualIntensity * 1.35f;
            var innerRadius = baseRadius * 0.72f + baseRadius * state.VisualIntensity * 0.28f;
            var glowColor = state.IsActiveVisual ? state.AccentColor : IdleColor;
            glowColor.a = state.IsActiveVisual ? 0.18f + state.VisualIntensity * 0.25f : 0.2f;

            Handles.BeginGUI();
            Handles.color = new Color(1f, 1f, 1f, 0.04f);
            Handles.DrawSolidDisc(center, Vector3.forward, baseRadius * 2.1f);
            Handles.color = glowColor;
            Handles.DrawSolidDisc(center, Vector3.forward, outerRadius);
            Handles.color = state.IsActiveVisual ? state.AccentColor : IdleColor;
            Handles.DrawSolidDisc(center, Vector3.forward, innerRadius);
            Handles.color = new Color(1f, 1f, 1f, 0.1f);
            Handles.DrawWireDisc(center, Vector3.forward, baseRadius * 1.45f);
            Handles.DrawWireDisc(center, Vector3.forward, baseRadius * 2f);
            Handles.EndGUI();

            var badgeRect = new Rect(rect.x + 14f, rect.y + 12f, 92f, 20f);
            DrawBadge(badgeRect, state.IsActiveVisual ? "ACTIVE" : "IDLE", state.IsActiveVisual ? state.AccentColor : IdleColor);

            var modeRect = new Rect(rect.xMax - 92f, rect.y + 12f, 78f, 20f);
            DrawBadge(modeRect, state.PlayModeText.ToUpperInvariant(), state.IsLoopMode ? new Color(0.91f, 0.51f, 0.27f) : new Color(0.32f, 0.72f, 0.98f));

            var typeRect = new Rect(rect.x, rect.yMax - 34f, rect.width, 18f);
            GUI.Label(typeRect, state.LastTypeText, EditorStyles.whiteLargeLabel);
        }

        /// <summary>
        /// 時間進行と終了状態を描画する
        /// </summary>
        private void DrawTimeline(MobileHapticsSimulationState state) {
            var rect = GUILayoutUtility.GetRect(10f, 68f, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, PanelColor);

            var trackRect = new Rect(rect.x + 14f, rect.y + 20f, rect.width - 28f, 12f);
            EditorGUI.DrawRect(trackRect, TrackColor);

            if (state.IsLoopMode) {
                if (state.IsActiveVisual) {
                    var segmentWidth = trackRect.width * 0.18f;
                    var startX = trackRect.x + (trackRect.width - segmentWidth) * state.Progress;
                    var fillRect = new Rect(startX, trackRect.y, segmentWidth, trackRect.height);
                    EditorGUI.DrawRect(fillRect, state.AccentColor);
                    GUI.Label(new Rect(trackRect.x, rect.y + 40f, rect.width - 28f, 18f), "Loop pulse is cycling", _captionStyle);
                }
                else {
                    GUI.Label(new Rect(trackRect.x, rect.y + 40f, rect.width - 28f, 18f), "Loop stopped", _captionStyle);
                }

                return;
            }

            var progress = state.IsActiveVisual ? state.Progress : 1f;
            var fillRectOneShot = new Rect(trackRect.x, trackRect.y, trackRect.width * progress, trackRect.height);
            EditorGUI.DrawRect(fillRectOneShot, state.AccentColor);

            var markerRect = new Rect(trackRect.x + trackRect.width * progress - 1f, trackRect.y - 4f, 2f, trackRect.height + 8f);
            EditorGUI.DrawRect(markerRect, Color.white);

            var statusText = state.IsActiveVisual
                ? $"OneShot active {state.RemainingSeconds:0.000}s left"
                : "OneShot finished";

            GUI.Label(new Rect(trackRect.x, rect.y + 40f, rect.width - 28f, 18f), statusText, _captionStyle);
        }

        /// <summary>
        /// 各種メトリクスをカード表示する
        /// </summary>
        private void DrawMetrics(MobileHapticsSimulationState state) {
            using (new EditorGUILayout.HorizontalScope()) {
                DrawMetricCard("Intensity", $"{Mathf.RoundToInt(state.VisualIntensity * 100f)}%", state.AccentColor);
                DrawMetricCard("Duration", $"{state.ExpectedDurationSeconds:0.000}s", state.AccentColor);
                DrawMetricCard("Updated", state.LastUpdatedUtc == System.DateTime.MinValue ? "--" : state.LastUpdatedUtc.ToLocalTime().ToString("HH:mm:ss"), state.IsSupported ? new Color(0.35f, 0.82f, 0.56f) : new Color(0.7f, 0.7f, 0.76f));
            }
        }

        /// <summary>
        /// クイックテストボタンを描画する
        /// </summary>
        private void DrawQuickTestButtons() {
            EditorGUILayout.LabelField("Quick Test", _titleStyle);
            DrawActionSection(
                "Notification",
                "短い通知系の OneShot",
                new Color(0.32f, 0.72f, 0.98f),
                ("Selection", () => MobileHaptics.Play(HapticType.Selection)),
                ("Success", () => MobileHaptics.Play(HapticType.Success)),
                ("Warning", () => MobileHaptics.Play(HapticType.Warning)),
                ("Error", () => MobileHaptics.Play(HapticType.Error))
            );
            DrawActionSection(
                "Pulse",
                "強度と時間を指定する可変制御振動",
                new Color(0.93f, 0.48f, 0.37f),
                ("Soft", () => MobileHaptics.PlayPulse(0.25f, 0.05f)),
                ("Medium", () => MobileHaptics.PlayPulse(0.55f, 0.12f)),
                ("Strong", () => MobileHaptics.PlayPulse(0.9f, 0.2f))
            );
            DrawActionSection(
                "Loop",
                "Pulse を停止まで反復再生",
                new Color(0.98f, 0.65f, 0.3f),
                ("Loop Soft", () => MobileHaptics.PlayPulse(0.25f, 0.05f, true)),
                ("Loop Mid", () => MobileHaptics.PlayPulse(0.55f, 0.12f, true)),
                ("Loop Strong", () => MobileHaptics.PlayPulse(0.9f, 0.2f, true))
            );
            DrawActionSection(
                "Control",
                "現在の再生を停止",
                new Color(0.93f, 0.38f, 0.37f),
                ("Stop", MobileHaptics.Stop)
            );
        }

        /// <summary>
        /// 見出し付きの操作セクションを描画する
        /// </summary>
        private void DrawActionSection(string title, string caption, Color accentColor, params (string Label, System.Action Action)[] actions) {
            var viewWidth = EditorGUIUtility.currentViewWidth - 40f;
            var maxColumnsByWidth = Mathf.Max(1, Mathf.FloorToInt((viewWidth + ButtonSpacing) / (ButtonMinWidth + ButtonSpacing)));
            var columns = Mathf.Clamp(Mathf.Min(actions.Length, maxColumnsByWidth), 1, 4);
            var rowCount = Mathf.CeilToInt(actions.Length / (float)columns);
            var sectionHeight = 48f + rowCount * ButtonHeight + Mathf.Max(0, rowCount - 1) * 4f;
            var rect = GUILayoutUtility.GetRect(10f, sectionHeight, GUILayout.ExpandWidth(true));

            EditorGUI.DrawRect(rect, PanelColor);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, 4f, rect.height), accentColor);

            GUI.Label(new Rect(rect.x + 12f, rect.y + 8f, rect.width - 24f, 18f), title, _titleStyle);
            GUI.Label(new Rect(rect.x + 12f, rect.y + 24f, rect.width - 24f, 16f), caption, _captionStyle);

            var availableWidth = rect.width - 24f;
            var rawButtonWidth = (availableWidth - ButtonSpacing * (columns - 1)) / columns;
            var buttonWidth = Mathf.Clamp(rawButtonWidth, ButtonMinWidth, ButtonMaxWidth);
            var rowWidth = buttonWidth * columns + ButtonSpacing * (columns - 1);
            var startX = rect.x + 12f + Mathf.Max(0f, (availableWidth - rowWidth) * 0.5f);

            for (var i = 0; i < actions.Length; i++) {
                var row = i / columns;
                var column = i % columns;
                var x = startX + (buttonWidth + ButtonSpacing) * column;
                var y = rect.y + 42f + (ButtonHeight + 4f) * row;
                var buttonRect = new Rect(x, y, buttonWidth, ButtonHeight);
                if (GUI.Button(buttonRect, actions[i].Label, _buttonStyle)) {
                    actions[i].Action();
                }
            }
        }

        /// <summary>
        /// メトリクスカードを描画する
        /// </summary>
        private void DrawMetricCard(string label, string value, Color accentColor) {
            var rect = GUILayoutUtility.GetRect(10f, 60f, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, PanelColor);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, 4f, rect.height), accentColor);
            GUI.Label(new Rect(rect.x + 12f, rect.y + 10f, rect.width - 18f, 16f), label, _captionStyle);
            GUI.Label(new Rect(rect.x + 12f, rect.y + 26f, rect.width - 18f, 22f), value, _metricStyle);
        }

        /// <summary>
        /// バッジを描画する
        /// </summary>
        private static void DrawBadge(Rect rect, string text, Color color) {
            var background = color;
            background.a = 0.18f;
            EditorGUI.DrawRect(rect, background);
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 2f, rect.width, 2f), color);
            GUI.Label(rect, text, new GUIStyle(EditorStyles.miniBoldLabel) {
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white },
            });
        }

        /// <summary>
        /// GUIStyle を初期化する
        /// </summary>
        private void EnsureStyles() {
            if (_titleStyle != null) {
                return;
            }

            _titleStyle = new GUIStyle(EditorStyles.boldLabel) {
                fontSize = 13,
                normal = { textColor = Color.white },
            };
            _captionStyle = new GUIStyle(EditorStyles.miniLabel) {
                fontSize = 10,
                normal = { textColor = new Color(0.76f, 0.8f, 0.88f) },
            };
            _metricStyle = new GUIStyle(EditorStyles.label) {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white },
            };
            _buttonStyle = new GUIStyle(GUI.skin.button) {
                fontStyle = FontStyle.Bold,
                fixedHeight = ButtonHeight,
            };
        }
    }
}
