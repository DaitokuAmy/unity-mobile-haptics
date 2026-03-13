# unity-mobile-haptics

Unity のモバイル向けハプティクス再生ライブラリです。`iOS` と `Android` の差異を吸収し、同じ API で振動を再生できます。

## Features

- `MobileHaptics.Play(...)` で単発のハプティクスを再生
- `MobileHaptics.PlayLoop(...)` で `Impact` 系のみ継続再生を開始し、`MobileHaptics.Stop()` で停止
- `Selection` / `Success` / `Warning` / `Error` / `LightImpact` / `MediumImpact` / `HeavyImpact` をサポート
- `UnityEditor` 上では Simulation Window で再生状態を視覚的に確認可能
- 未対応環境では安全に no-op として動作

## Requirements

- Unity `6000.0` 以降
- 対応プラットフォーム:
  - `iOS`
  - `Android`

## Installation

### Install via Package Manager

1. Unity の `Window > Package Manager` を開く
2. `+` ボタンから `Add package from git URL...` を選ぶ
3. 以下を入力してインストールする

```text
https://github.com/DaitokuAmy/unity-mobile-haptics.git?path=/Packages/com.daitokuamy.unitymobilehaptics
```

バージョンを指定する場合は末尾にタグを付けます。

```text
https://github.com/DaitokuAmy/unity-mobile-haptics.git?path=/Packages/com.daitokuamy.unitymobilehaptics#2.0.0
```

<!-- TODO: Package Manager の追加手順画像をここに追加 -->

### Install via manifest.json

`Packages/manifest.json` の `dependencies` に以下を追加します。

```json
{
  "dependencies": {
    "com.daitokuamy.unitymobilehaptics": "https://github.com/DaitokuAmy/unity-mobile-haptics.git?path=/Packages/com.daitokuamy.unitymobilehaptics"
  }
}
```

## Quick Start

```csharp
using UnityEngine;
using UnityMobileHaptics;

public sealed class HapticsExample : MonoBehaviour {
    public void PlaySuccess() {
        if (!MobileHaptics.IsSupported) {
            return;
        }

        MobileHaptics.Play(HapticType.Success);
    }

    public void StartLoop() {
        MobileHaptics.PlayLoop(ImpactHapticType.Medium);
    }

    public void StopLoop() {
        MobileHaptics.Stop();
    }
}
```

## API

### `MobileHaptics`

```csharp
public static class MobileHaptics {
    public static bool IsSupported { get; }

    public static void Play(HapticType type);
    public static void PlayLoop(ImpactHapticType type);
    public static void Stop();
}
```

- `IsSupported`
  - 現在の実行環境でハプティクス再生に対応している場合は `true`
- `Play(HapticType type)`
  - 単発のハプティクスを再生
- `PlayLoop(ImpactHapticType type)`
  - `Light` / `Medium` / `Heavy` の `Impact` 系のみ継続再生を開始
- `Stop()`
  - 再生中のハプティクスを停止

### `HapticType`

利用できる振動種別は以下です。

- `Selection`
- `Success`
- `Warning`
- `Error`
- `LightImpact`
- `MediumImpact`
- `HeavyImpact`

### `ImpactHapticType`

Loop 再生で利用できる振動種別は以下です。

- `Light`
- `Medium`
- `Heavy`

## Platform Notes

### iOS

- iOS 標準の `UIFeedbackGenerator` 系 API を利用します
- `PlayLoop` は無限振動 API ではなく、タイマーによる近似再生です
- アプリが非アクティブ化またはバックグラウンド遷移した場合は loop を停止します

### Android

- Android 標準の `Vibrator` / `VibrationEffect` を利用します
- OS バージョンに応じて利用可能な API を切り替えます
- `PlayLoop` は waveform を使った継続再生、または近似動作で実装されます

### Unity Editor

- Editor 上では実機の物理振動は再現しません
- 代わりに Simulation Window で再生状態を視覚的に確認できます

## Editor Simulation

Unity メニューの `Window > Unity Mobile Haptics > Simulation` からシミュレーションウィンドウを開けます。

Runtime API を呼ぶと、このウィンドウに再生状態が反映されます。実機にデプロイする前の挙動確認や、UI 実装中の導線確認に便利です。

<img width="451" height="684" alt="image" src="https://github.com/user-attachments/assets/73425122-9aa3-4c62-9dcf-f7b47e8075d1" />

## Sample

このリポジトリにはサンプルシーンが含まれています。

- Scene: `Assets/Sample/Scenes/sample.unity`
- Script: `Assets/Sample/Scripts/HapticsSampleController.cs`

サンプルでは各 `HapticType` の単発再生、loop 再生、停止をボタンから確認できます。

<img width="513" height="922" alt="image" src="https://github.com/user-attachments/assets/30adc5e3-2413-4127-b359-42561809e6e2" />

## License

This project is licensed under the MIT License. See `LICENSE.md` for details.
