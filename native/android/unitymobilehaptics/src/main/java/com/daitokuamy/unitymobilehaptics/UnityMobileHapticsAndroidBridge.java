package com.daitokuamy.unitymobilehaptics;

import android.content.Context;
import android.os.Build;
import android.os.VibrationEffect;
import android.os.Vibrator;
import android.view.HapticFeedbackConstants;
import android.view.View;
import android.view.Window;

import com.unity3d.player.UnityPlayer;

/**
 * Unity から呼び出す Android 振動ブリッジ
 */
public final class UnityMobileHapticsAndroidBridge {
    // Loop 再生時は短い無音区間を入れて連続振動を近似する
    private static final long LOOP_PAUSE_MILLIS = 40L;
    private static final long LOOP_START_DELAY_MILLIS = 0L;
    private static final long MIN_PULSE_DURATION_MILLIS = 10L;

    private UnityMobileHapticsAndroidBridge() {
    }

    /**
     * ネイティブ定義済みの振動を再生する
     *
     * @param hapticType C# 側の HapticType に対応する値
     */
    public static void play(int hapticType) {
        Vibrator vibrator = getVibrator();
        if (vibrator == null || !vibrator.hasVibrator()) {
            return;
        }

        vibrator.cancel();

        if (tryPerformViewHapticFeedback(hapticType)) {
            return;
        }

        if (tryPlayPredefinedEffect(vibrator, hapticType)) {
            return;
        }

        long duration = getFallbackDurationMillis(hapticType);
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            vibrator.vibrate(createFallbackOneShotEffect(hapticType, duration));
            return;
        }

        vibrator.vibrate(duration);
    }

    /**
     * 強度と時間を指定する可変制御振動を再生する
     *
     * @param intensity 0.0 から 1.0 の振動強度
     * @param durationSeconds 1 パルスの再生時間
     * @param isLoop true の場合は継続再生
     */
    public static void playPulse(float intensity, float durationSeconds, boolean isLoop) {
        Vibrator vibrator = getVibrator();
        if (vibrator == null || !vibrator.hasVibrator()) {
            return;
        }

        vibrator.cancel();

        float clampedIntensity = clamp01(intensity);
        long durationMillis = Math.max(MIN_PULSE_DURATION_MILLIS, Math.round(durationSeconds * 1000f));
        int amplitude = getAmplitudeFromIntensity(clampedIntensity);

        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            VibrationEffect effect = isLoop
                ? createLoopEffect(durationMillis, amplitude)
                : VibrationEffect.createOneShot(durationMillis, amplitude);
            vibrator.vibrate(effect);
            return;
        }

        if (isLoop) {
            vibrator.vibrate(new long[] { LOOP_START_DELAY_MILLIS, durationMillis, LOOP_PAUSE_MILLIS }, 0);
            return;
        }

        vibrator.vibrate(durationMillis);
    }

    /**
     * 再生中の振動を停止する
     */
    public static void stop() {
        Vibrator vibrator = getVibrator();
        if (vibrator == null) {
            return;
        }

        vibrator.cancel();
    }

    /**
     * Unity の currentActivity から Vibrator を取得する
     */
    private static Vibrator getVibrator() {
        Context context = UnityPlayer.currentActivity;
        if (context == null) {
            return null;
        }

        return (Vibrator)context.getSystemService(Context.VIBRATOR_SERVICE);
    }

    /**
     * View ベースのシステムハプティクスを優先して再生する
     */
    private static boolean tryPerformViewHapticFeedback(int hapticType) {
        View view = getDecorView();
        if (view == null) {
            return false;
        }

        int feedbackConstant = getFeedbackConstant(hapticType);
        if (feedbackConstant == Integer.MIN_VALUE) {
            return false;
        }

        return view.performHapticFeedback(feedbackConstant);
    }

    /**
     * 振動種別に対応する定義済みエフェクトを再生する
     */
    private static boolean tryPlayPredefinedEffect(Vibrator vibrator, int hapticType) {
        if (Build.VERSION.SDK_INT < Build.VERSION_CODES.Q) {
            return false;
        }

        VibrationEffect effect;
        switch (hapticType) {
            case 0:
                effect = VibrationEffect.createPredefined(VibrationEffect.EFFECT_TICK);
                break;
            default:
                return false;
        }

        vibrator.vibrate(effect);
        return true;
    }

    /**
     * 振動種別に対応する View ハプティクス定数を返す
     */
    private static int getFeedbackConstant(int hapticType) {
        switch (hapticType) {
            case 0:
                if (Build.VERSION.SDK_INT >= 34) {
                    return HapticFeedbackConstants.SEGMENT_TICK;
                }

                return HapticFeedbackConstants.CLOCK_TICK;
            case 1:
                if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.R) {
                    return HapticFeedbackConstants.CONFIRM;
                }

                return Integer.MIN_VALUE;
            case 3:
                if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.R) {
                    return HapticFeedbackConstants.REJECT;
                }

                return Integer.MIN_VALUE;
            default:
                return Integer.MIN_VALUE;
        }
    }

    /**
     * Fallback 用 OneShot エフェクトを生成する
     */
    private static VibrationEffect createFallbackOneShotEffect(int hapticType, long duration) {
        if (hapticType == 1) {
            return VibrationEffect.createWaveform(new long[] { 0L, 24L, 36L, 32L }, -1);
        }

        if (hapticType == 2) {
            return VibrationEffect.createWaveform(new long[] { 0L, 20L, 30L, 20L }, -1);
        }

        if (hapticType == 3) {
            return VibrationEffect.createWaveform(new long[] { 0L, 28L, 40L, 28L, 36L, 36L }, -1);
        }

        return VibrationEffect.createOneShot(duration, getAmplitude(hapticType));
    }

    /**
     * Loop 用エフェクトを生成する
     */
    private static VibrationEffect createLoopEffect(long duration, int amplitude) {
        return VibrationEffect.createWaveform(
            new long[] { 0L, duration, LOOP_PAUSE_MILLIS },
            new int[] { 0, amplitude, 0 },
            0
        );
    }

    /**
     * currentActivity から描画中 View を取得する
     */
    private static View getDecorView() {
        if (UnityPlayer.currentActivity == null) {
            return null;
        }

        Window window = UnityPlayer.currentActivity.getWindow();
        if (window == null) {
            return null;
        }

        return window.getDecorView();
    }

    /**
     * 振動種別ごとの再生時間を返す
     */
    private static long getFallbackDurationMillis(int hapticType) {
        switch (hapticType) {
            case 0:
                return 20L;
            case 1:
            case 2:
            case 3:
                return 40L;
            default:
                return 24L;
        }
    }

    /**
     * 振動種別ごとの振幅を返す
     */
    private static int getAmplitude(int hapticType) {
        if (Build.VERSION.SDK_INT < Build.VERSION_CODES.O) {
            return VibrationEffect.DEFAULT_AMPLITUDE;
        }

        switch (hapticType) {
            default:
                return 180;
        }
    }

    /**
     * 0.0 から 1.0 の値に丸め込む
     */
    private static float clamp01(float value) {
        return Math.max(0f, Math.min(1f, value));
    }

    /**
     * 強度から Android の振幅へ変換する
     */
    private static int getAmplitudeFromIntensity(float intensity) {
        if (Build.VERSION.SDK_INT < Build.VERSION_CODES.O) {
            return VibrationEffect.DEFAULT_AMPLITUDE;
        }

        return Math.max(1, Math.round(255f * clamp01(intensity)));
    }
}
