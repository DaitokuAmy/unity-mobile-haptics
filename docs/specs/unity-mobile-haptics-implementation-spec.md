# Unity Mobile Haptics 実装仕様

## 目的

- Unity からモバイル端末の振動を扱うためのライブラリ仕様を定義する
- 実装時の判断基準をそろえる

## 対象

- Unity Package Manager パッケージ `com.daitokuamy.unitymobilehaptics`
- iOS / Android 向けの振動制御

## 実装方針

- 呼び出し側はプラットフォーム差異を意識しない
- 未対応環境では安全に no-op とする
- 公開 API とプラットフォーム実装を分離する

## 公開 API

```csharp
namespace UnityMobileHaptics {
    public static class MobileHaptics {
        public static bool IsSupported { get; }

        public static void Play(HapticType type);

        public static void PlayLoop(ImpactHapticType type);

        public static void Stop();
    }
}
```

- `Play` は単発振動を再生する
- `PlayLoop` は `ImpactHapticType` で指定した衝撃系振動のみを停止されるまで継続再生する
- `Stop` は単発、Loop を問わず再生中の振動を停止する

## HapticType

- `Selection`
- `Success`
- `Warning`
- `Error`
- `LightImpact`
- `MediumImpact`
- `HeavyImpact`

## ImpactHapticType

- `Light`
- `Medium`
- `Heavy`

## プラットフォーム方針

### Android

- Android 標準 API を利用する
- OS バージョンに応じて利用機能を切り替える
- ネイティブ実装は `.aar` としてパッケージに含める
- Loop は `ImpactHapticType` のみを対象に、Android の実現方法に合わせて継続再生または近似動作で実装する

### iOS

- iOS 標準のハプティクス API を利用する
- `HapticType` をネイティブ機能へマッピングする
- Loop は `ImpactHapticType` のみを対象に、iOS の制約に応じた近似再生で実装する

### Unsupported

- 非対応環境では再生しない
- `IsSupported` は `false` を返す

## Editor サポート

- 開発中の確認用途として専用の `EditorWindow` を提供する
- `EditorWindow` を開いている間は、振動の再生状態をエディタ上でシミュレーション表示できる
- 実機での物理的な振動そのものは再現せず、再生中、停止、強弱、パターン進行などを視覚的に確認できるものとする
- Runtime API からの呼び出し内容を、Editor 向けのシミュレーション表示へ反映できる構成にする
- 本機能は開発支援用であり、実機挙動の完全再現は目的としない

## 未確定事項

- `HapticType` の最終セット
- パターン再生を初期版に含めるか
- `PlayLoop` の API をこの構成で確定とするか
- `PlayLoop` の停止条件とアプリ非アクティブ時の扱い
- iOS 実装方式の詳細
- Android 実装方式の詳細
- `EditorWindow` の見た目とシミュレーション表現の詳細
