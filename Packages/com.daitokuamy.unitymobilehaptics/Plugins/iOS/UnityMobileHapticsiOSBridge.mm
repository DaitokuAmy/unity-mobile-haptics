#import "UnityMobileHapticsiOSBridge.h"

#import <UIKit/UIKit.h>
#import <dispatch/dispatch.h>

static const uint64_t kPulseStepMillis = 40;
static const uint64_t kLoopPauseMillis = 40;

// 可変制御振動の近似再生に使うタイマーを保持する
static dispatch_source_t s_pulseTimer;
static float s_pulseIntensity = 0.5f;
static NSInteger s_pulseStepsPerCycle = 0;
static NSInteger s_remainingPulseSteps = 0;
static NSInteger s_remainingPauseSteps = 0;
static BOOL s_isLoopingPulse = NO;

static void StopPulseTimer(void);
static void PlayHapticType(int hapticType);
static void PlayPulseImpact(float intensity);
static void StartPulsePlayback(float intensity, float durationSeconds, bool isLoop);
static void TickPulsePlayback(void);
static void EnsureLifecycleObserversRegistered(void);
static NSInteger StepsFromDurationSeconds(float durationSeconds);

// C# 側から呼ばれるネイティブ定義済み振動の再生入口
void UnityMobileHapticsPlay(int hapticType) {
    dispatch_async(dispatch_get_main_queue(), ^{
        EnsureLifecycleObserversRegistered();
        StopPulseTimer();
        PlayHapticType(hapticType);
    });
}

// C# 側から呼ばれる可変制御振動の再生入口
void UnityMobileHapticsPlayPulse(float intensity, float durationSeconds, bool isLoop) {
    dispatch_async(dispatch_get_main_queue(), ^{
        EnsureLifecycleObserversRegistered();
        StopPulseTimer();
        StartPulsePlayback(intensity, durationSeconds, isLoop);
    });
}

// C# 側から呼ばれる振動停止入口
void UnityMobileHapticsStop(void) {
    dispatch_async(dispatch_get_main_queue(), ^{
        StopPulseTimer();
    });
}

// 可変制御振動用タイマーを停止して破棄する
static void StopPulseTimer(void) {
    if (s_pulseTimer != nil) {
        dispatch_source_cancel(s_pulseTimer);
        s_pulseTimer = nil;
    }

    s_pulseStepsPerCycle = 0;
    s_remainingPulseSteps = 0;
    s_remainingPauseSteps = 0;
    s_isLoopingPulse = NO;
}

// 可変制御振動の再生状態を初期化する
static void StartPulsePlayback(float intensity, float durationSeconds, bool isLoop) {
    s_pulseIntensity = fmaxf(0.0f, fminf(1.0f, intensity));
    s_pulseStepsPerCycle = StepsFromDurationSeconds(durationSeconds);
    s_remainingPulseSteps = s_pulseStepsPerCycle;
    s_remainingPauseSteps = 0;
    s_isLoopingPulse = isLoop ? YES : NO;

    TickPulsePlayback();
    if (s_remainingPulseSteps <= 0 && !s_isLoopingPulse) {
        return;
    }

    s_pulseTimer = dispatch_source_create(DISPATCH_SOURCE_TYPE_TIMER, 0, 0, dispatch_get_main_queue());
    dispatch_source_set_timer(
        s_pulseTimer,
        dispatch_time(DISPATCH_TIME_NOW, (int64_t)(kPulseStepMillis * NSEC_PER_MSEC)),
        kPulseStepMillis * NSEC_PER_MSEC,
        5 * NSEC_PER_MSEC
    );
    dispatch_source_set_event_handler(s_pulseTimer, ^{
        TickPulsePlayback();
    });
    dispatch_resume(s_pulseTimer);
}

// 1 ティック分の可変制御振動を進行させる
static void TickPulsePlayback(void) {
    if (s_remainingPulseSteps > 0) {
        PlayPulseImpact(s_pulseIntensity);
        s_remainingPulseSteps--;

        if (s_remainingPulseSteps == 0) {
            if (s_isLoopingPulse) {
                s_remainingPauseSteps = MAX(1, (NSInteger)(kLoopPauseMillis / kPulseStepMillis));
            }
            else {
                StopPulseTimer();
            }
        }

        return;
    }

    if (s_remainingPauseSteps > 0) {
        s_remainingPauseSteps--;
        if (s_remainingPauseSteps == 0) {
            s_remainingPulseSteps = s_pulseStepsPerCycle;
        }

        return;
    }

    if (s_isLoopingPulse) {
        s_remainingPulseSteps = s_pulseStepsPerCycle;
        return;
    }

    StopPulseTimer();
}

// アプリ非アクティブ化時に可変制御振動を停止する通知監視を登録する
static void EnsureLifecycleObserversRegistered(void) {
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        NSNotificationCenter *notificationCenter = [NSNotificationCenter defaultCenter];
        [notificationCenter addObserverForName:UIApplicationWillResignActiveNotification
                                        object:nil
                                         queue:[NSOperationQueue mainQueue]
                                    usingBlock:^(__unused NSNotification *notification) {
                                        StopPulseTimer();
                                    }];
        [notificationCenter addObserverForName:UIApplicationDidEnterBackgroundNotification
                                        object:nil
                                         queue:[NSOperationQueue mainQueue]
                                    usingBlock:^(__unused NSNotification *notification) {
                                        StopPulseTimer();
                                    }];
    });
}

// 振動時間をタイマー進行用ステップ数へ変換する
static NSInteger StepsFromDurationSeconds(float durationSeconds) {
    float clampedDuration = fmaxf(0.01f, durationSeconds);
    return MAX(1, (NSInteger)ceilf(clampedDuration * 1000.0f / (float)kPulseStepMillis));
}

// HapticType の整数値を iOS のフィードバック API へマッピングする
static void PlayHapticType(int hapticType) {
    if (@available(iOS 10.0, *)) {
        switch (hapticType) {
            case 0: {
                UISelectionFeedbackGenerator *generator = [[UISelectionFeedbackGenerator alloc] init];
                [generator prepare];
                [generator selectionChanged];
                break;
            }
            case 1: {
                UINotificationFeedbackGenerator *generator = [[UINotificationFeedbackGenerator alloc] init];
                [generator prepare];
                [generator notificationOccurred:UINotificationFeedbackTypeSuccess];
                break;
            }
            case 2: {
                UINotificationFeedbackGenerator *generator = [[UINotificationFeedbackGenerator alloc] init];
                [generator prepare];
                [generator notificationOccurred:UINotificationFeedbackTypeWarning];
                break;
            }
            case 3: {
                UINotificationFeedbackGenerator *generator = [[UINotificationFeedbackGenerator alloc] init];
                [generator prepare];
                [generator notificationOccurred:UINotificationFeedbackTypeError];
                break;
            }
            default: {
                UISelectionFeedbackGenerator *generator = [[UISelectionFeedbackGenerator alloc] init];
                [generator prepare];
                [generator selectionChanged];
                break;
            }
        }
    }
}

// 指定強度の可変制御振動を 1 ティック分だけ近似再生する
static void PlayPulseImpact(float intensity) {
    float clampedIntensity = fmaxf(0.0f, fminf(1.0f, intensity));

    if (@available(iOS 13.0, *)) {
        UIImpactFeedbackGenerator *generator = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleRigid];
        [generator prepare];
        [generator impactOccurredWithIntensity:fmaxf(0.01f, clampedIntensity)];
        return;
    }

    if (@available(iOS 10.0, *)) {
        UIImpactFeedbackStyle style;
        if (clampedIntensity < 0.34f) {
            style = UIImpactFeedbackStyleLight;
        }
        else if (clampedIntensity < 0.67f) {
            style = UIImpactFeedbackStyleMedium;
        }
        else {
            style = UIImpactFeedbackStyleHeavy;
        }

        UIImpactFeedbackGenerator *generator = [[UIImpactFeedbackGenerator alloc] initWithStyle:style];
        [generator prepare];
        [generator impactOccurred];
    }
}
