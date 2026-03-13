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
- ネイティブ定義済みの振動と、強度・時間を指定する可変制御振動を明確に分離する
- 同時に保持できる可変制御振動は 1 件のみとし、後から開始した再生が前の再生を上書きする

## 公開 API

```csharp
namespace UnityMobileHaptics {
    public static class MobileHaptics {
        public static bool IsSupported { get; }

        public static void Play(HapticType type);

        public static HapticPlaybackHandle PlayPulse(float intensity, float durationSeconds, bool loop = false);

        public static void Stop();
    }

    public readonly struct HapticPlaybackHandle {
        public bool IsValid { get; }

        public void Stop();
    }
}
```

- `Play` は `HapticType` で指定したネイティブ定義済み振動を単発再生する
- `PlayPulse` は強度と時間を指定する可変制御振動を再生し、停止用ハンドルを返す
- `PlayPulse` の `loop` が `false` の場合は 1 回のみ再生し、必要に応じて再生途中でハンドルから停止できる
- `PlayPulse` の `loop` が `true` の場合は停止されるまで同一パラメータで繰り返し再生する
- `Stop` は現在再生中の可変制御振動を停止する
- ネイティブ定義済み振動の `Play` はハンドルを返さない

## HapticType

- `Selection`
- `Success`
- `Warning`
- `Error`

- `HapticType` には、各プラットフォームがネイティブに提供する定数的な振動のみを含める
- 旧 `Impact` 系のような強度差分付き振動は `HapticType` から切り離し、`PlayPulse` で表現する

## 可変制御振動

- 可変制御振動は `intensity` と `durationSeconds` を指定して再生する
- `intensity` は `0.0f` から `1.0f` の範囲を基本とし、範囲外入力は clamp する
- `durationSeconds` は 0 より大きい値を受け付ける
- `loop` が `false` の場合は指定された 1 パルスのみを再生する
- `loop` が `true` の場合は指定された 1 パルスを繰り返し再生する無限ループ扱いとする
- 再生中の停止は `HapticPlaybackHandle.Stop()` または `MobileHaptics.Stop()` で行う

## HapticPlaybackHandle

- `PlayPulse` の呼び出しごとに、その再生を識別するハンドルを返す
- ハンドルは発行時点の再生インスタンスにのみ紐づく
- 新しい `PlayPulse` が呼ばれて再生が上書きされた場合、それ以前のハンドルは無効化される
- 無効化されたハンドルに対して `Stop()` を呼んでも、現在の再生は停止しない
- 再生完了後または停止後のハンドルは無効扱いとする
- `IsValid` は、そのハンドルが現在の再生を停止できる状態かを示す

## 再生競合ルール

- `PlayPulse` 呼び出し時に既存の可変制御振動がある場合は、既存再生を停止して新しい再生へ切り替える
- `Play` 呼び出しはネイティブ定義済み振動の単発再生として扱い、可変制御振動とは別系統で扱う
- ただしプラットフォーム制約により共存できない場合は、実装側で安全な優先順位を決めて破綻しない動作を優先する
- `Stop` は可変制御振動の現在再生分のみを対象とし、過去ハンドルの有効性は復活させない

## API 命名方針

- 旧 `Impact` は、プリセット種別名としては使用しない
- 強度と時間を与える可変制御振動は `Pulse` を基本用語として扱う
- 可変制御振動は `PlayPulse` の 1 API に集約し、`durationSeconds` は 1 回の再生長、`loop` は反復再生の有無を表す

## プラットフォーム方針

### Android

- Android 標準 API を利用する
- OS バージョンに応じて利用機能を切り替える
- ネイティブ実装は `.aar` としてパッケージに含める
- `HapticType` は Android の定数的なハプティクス表現または近似動作へマッピングする
- `PlayPulse` は Android の振動時間・強度制御 API を優先利用し、必要に応じて近似動作へフォールバックする
- `loop` が `true` の場合は waveform またはタイマー制御で反復再生を実現し、停止時に確実にキャンセルする

### iOS

- iOS 標準のハプティクス API を利用する
- `HapticType` は iOS のネイティブ定義済みフィードバックへマッピングする
- `PlayPulse` は Core Haptics などの可変制御 API を優先利用し、利用不可環境では安全にフォールバックまたは no-op とする
- `loop` が `true` の場合は iOS の制約に応じて再スケジュールまたは近似再生で実装する

### Unsupported

- 非対応環境では再生しない
- `IsSupported` は `false` を返す
- `PlayPulse` は無効ハンドルを返す

## Editor サポート

- 開発中の確認用途として専用の `EditorWindow` を提供する
- `EditorWindow` を開いている間は、振動の再生状態をエディタ上でシミュレーション表示できる
- 実機での物理的な振動そのものは再現せず、再生中、停止、強弱、パターン進行などを視覚的に確認できるものとする
- Runtime API からの呼び出し内容を、Editor 向けのシミュレーション表示へ反映できる構成にする
- `PlayPulse` の強度、時間、ループ状態、ハンドル有効 / 無効も確認できるようにする
- 本機能は開発支援用であり、実機挙動の完全再現は目的としない

## 未確定事項

- `HapticType` の最終セット
- `PlayPulse` の引数型をプリミティブのままにするか、専用パラメータ型を導入するか
- `Play` と `PlayPulse` の競合時の最終優先順位
- アプリ非アクティブ時やサスペンド時の自動停止方針
- iOS 実装方式の詳細
- Android 実装方式の詳細
- `EditorWindow` の見た目とシミュレーション表現の詳細
