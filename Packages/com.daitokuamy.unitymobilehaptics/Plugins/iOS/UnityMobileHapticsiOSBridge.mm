#import "UnityMobileHapticsiOSBridge.h"

#import <UIKit/UIKit.h>
#import <dispatch/dispatch.h>

// Loop 再生時に利用するタイマーを保持する
static dispatch_source_t s_loopTimer;
static int s_loopHapticType = 0;

static void StopLoopTimer(void);
static void PlayHapticType(int hapticType);
static void EnsureLifecycleObserversRegistered(void);

// C# 側から呼ばれる振動再生入口
void UnityMobileHapticsPlay(int hapticType, bool isLoop) {
    // UIFeedbackGenerator はメインスレッド上で扱う
    dispatch_async(dispatch_get_main_queue(), ^{
        EnsureLifecycleObserversRegistered();
        StopLoopTimer();
        PlayHapticType(hapticType);

        if (!isLoop) {
            return;
        }

        // iOS に無限振動 API はないため、タイマーで近似する
        s_loopHapticType = hapticType;
        s_loopTimer = dispatch_source_create(DISPATCH_SOURCE_TYPE_TIMER, 0, 0, dispatch_get_main_queue());
        dispatch_source_set_timer(s_loopTimer, dispatch_time(DISPATCH_TIME_NOW, 200 * NSEC_PER_MSEC), 200 * NSEC_PER_MSEC, 20 * NSEC_PER_MSEC);
        dispatch_source_set_event_handler(s_loopTimer, ^{
            PlayHapticType(s_loopHapticType);
        });
        dispatch_resume(s_loopTimer);
    });
}

// C# 側から呼ばれる振動停止入口
void UnityMobileHapticsStop(void) {
    dispatch_async(dispatch_get_main_queue(), ^{
        StopLoopTimer();
    });
}

// Loop 用タイマーを停止して破棄する
static void StopLoopTimer(void) {
    if (s_loopTimer == nil) {
        return;
    }

    dispatch_source_cancel(s_loopTimer);
    s_loopTimer = nil;
}

// アプリ非アクティブ化時に Loop を停止する通知監視を登録する
static void EnsureLifecycleObserversRegistered(void) {
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        NSNotificationCenter *notificationCenter = [NSNotificationCenter defaultCenter];
        [notificationCenter addObserverForName:UIApplicationWillResignActiveNotification
                                        object:nil
                                         queue:[NSOperationQueue mainQueue]
                                    usingBlock:^(__unused NSNotification *notification) {
                                        StopLoopTimer();
                                    }];
        [notificationCenter addObserverForName:UIApplicationDidEnterBackgroundNotification
                                        object:nil
                                         queue:[NSOperationQueue mainQueue]
                                    usingBlock:^(__unused NSNotification *notification) {
                                        StopLoopTimer();
                                    }];
    });
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
            case 4: {
                UIImpactFeedbackGenerator *generator = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleLight];
                [generator prepare];
                [generator impactOccurred];
                break;
            }
            case 5: {
                UIImpactFeedbackGenerator *generator = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleMedium];
                [generator prepare];
                [generator impactOccurred];
                break;
            }
            case 6: {
                UIImpactFeedbackGenerator *generator = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleHeavy];
                [generator prepare];
                [generator impactOccurred];
                break;
            }
            default: {
                UIImpactFeedbackGenerator *generator = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleMedium];
                [generator prepare];
                [generator impactOccurred];
                break;
            }
        }
    }
}
