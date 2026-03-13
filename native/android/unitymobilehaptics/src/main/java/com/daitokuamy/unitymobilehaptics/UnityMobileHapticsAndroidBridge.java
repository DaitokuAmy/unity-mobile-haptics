package com.daitokuamy.unitymobilehaptics;

import android.content.Context;
import android.os.Build;
import android.os.VibrationEffect;
import android.os.Vibrator;

import com.unity3d.player.UnityPlayer;

/**
 * Unity から呼び出す Android 振動ブリッジ
 */
public final class UnityMobileHapticsAndroidBridge {
    // Loop 再生時は短い無音区間を入れて連続振動を近似する
    private static final long LOOP_PAUSE_MILLIS = 40L;
    private static final long LOOP_START_DELAY_MILLIS = 0L;

    private UnityMobileHapticsAndroidBridge() {
    }

    /**
     * 振動を再生する
     *
     * @param hapticType C# 側の HapticType に対応する値
     * @param isLoop true の場合は継続再生
     */
    public static void play(int hapticType, boolean isLoop) {
        Vibrator vibrator = getVibrator();
        if (vibrator == null || !vibrator.hasVibrator()) {
            return;
        }

        long duration = getDurationMillis(hapticType);
        int amplitude = getAmplitude(hapticType);

        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            VibrationEffect effect = isLoop
                ? VibrationEffect.createWaveform(new long[] { 0L, duration, LOOP_PAUSE_MILLIS }, new int[] { 0, amplitude, 0 }, 0)
                : VibrationEffect.createOneShot(duration, amplitude);
            vibrator.vibrate(effect);
            return;
        }

        if (isLoop) {
            vibrator.vibrate(new long[] { LOOP_START_DELAY_MILLIS, duration, LOOP_PAUSE_MILLIS }, 0);
            return;
        }

        vibrator.vibrate(duration);
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
     * 振動種別ごとの再生時間を返す
     */
    private static long getDurationMillis(int hapticType) {
        switch (hapticType) {
            case 0:
                return 20L;
            case 1:
            case 2:
            case 3:
                return 40L;
            case 4:
                return 18L;
            case 5:
                return 28L;
            case 6:
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
            case 4:
                return 80;
            case 5:
                return 160;
            case 6:
                return 255;
            default:
                return 180;
        }
    }
}
