#pragma once

#ifdef __cplusplus
extern "C" {
#endif

// C# 側から呼び出す iOS 振動再生入口
void UnityMobileHapticsPlay(int hapticType);

// C# 側から呼び出す iOS 可変制御振動再生入口
void UnityMobileHapticsPlayPulse(float intensity, float durationSeconds, bool isLoop);

// C# 側から呼び出す iOS 振動停止入口
void UnityMobileHapticsStop(void);

#ifdef __cplusplus
}
#endif
