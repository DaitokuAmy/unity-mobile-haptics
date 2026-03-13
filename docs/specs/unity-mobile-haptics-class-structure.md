# Unity Mobile Haptics クラス構成案

## 目的

- Runtime / Editor / ネイティブ連携の責務分割を整理する
- 実装時に追加するクラスの叩き台を用意する

## 想定ディレクトリ構成

```text
Packages/com.daitokuamy.unitymobilehaptics/
  Scripts/
    Runtime/
      MobileHaptics.cs
      HapticType.cs
      HapticPlaybackHandle.cs
      IMobileHapticsPlatform.cs
      Internal/
        MobileHapticsPlatformFactory.cs
        MobileHapticsEditorBridge.cs
      Platforms/
        AndroidMobileHapticsPlatform.cs
        IosMobileHapticsPlatform.cs
        UnsupportedMobileHapticsPlatform.cs
    Editor/
      MobileHapticsSimulationWindow.cs
      MobileHapticsSimulationState.cs
      MobileHapticsSimulationView.cs
  Plugins/
    Android/
      UnityMobileHapticsAndroidBridge.aar
    iOS/
      UnityMobileHapticsiOSBridge.h
      UnityMobileHapticsiOSBridge.mm
```

## Runtime

### `MobileHaptics`

- ライブラリの公開エントリポイント
- `IsSupported`、`Play`、`PlayPulse`、`Stop` を提供する
- `PlayPulse` は `loop` 引数で単発再生と反復再生を切り替える
- 実際の再生処理はプラットフォーム実装へ委譲する

### `HapticType`

- 振動種別を表す enum
- ネイティブ定義済みの定数的な振動のみを扱う
- 呼び出し側はネイティブ API ではなくこの型を使う

### `HapticPlaybackHandle`

- `PlayPulse` が返す停止用ハンドル
- 現在の再生インスタンスにのみ紐づき、上書き再生後は無効化される
- `IsValid` と `Stop()` を提供する

### `IMobileHapticsPlatform`

- プラットフォーム実装の共通インターフェース
- `IsSupported`、`Play`、`PlayPulse`、`Stop` を定義する

## Runtime Internal

### `MobileHapticsPlatformFactory`

- 実行環境に応じて使用するプラットフォーム実装を生成する
- `Android`、`iOS`、`Unsupported` の分岐を集約する

### `MobileHapticsEditorBridge`

- Editor 上での再生要求をシミュレーション側へ通知する
- Runtime と EditorWindow の橋渡しを担当する
- Player 実行時には実機再生、Editor 上ではシミュレーション反映に使う

## Runtime Platforms

### `AndroidMobileHapticsPlatform`

- Android 向けの振動処理を実装する
- OS バージョン差分を吸収する
- `Play` はネイティブ定義済み振動を扱う
- `PlayPulse` は強度と時間を指定する可変制御振動を扱う
- `loop` が `true` の場合は Android 側で反復再生を継続し、停止要求で解除する

### `IosMobileHapticsPlatform`

- iOS 向けの振動処理を実装する
- `HapticType` を iOS ネイティブ機能へ変換する
- `PlayPulse` は iOS 側の可変制御ハプティクス表現へ変換する
- `loop` が `true` の場合は iOS 側で再スケジュールまたは近似再生を行う

### `UnsupportedMobileHapticsPlatform`

- 非対応環境向けの no-op 実装
- `IsSupported` は `false` を返す
- `PlayPulse` は無効ハンドルを返す

## Editor

### `MobileHapticsSimulationWindow`

- 開発中の振動状態を確認する専用 `EditorWindow`
- 現在の再生状態、再生種別、強度、再生時間、ループ状態を表示する

### `MobileHapticsSimulationState`

- EditorWindow で表示するシミュレーション状態を保持する
- 最後に再生された `HapticType`、可変制御振動の強度と時間、ループ状態、再生中フラグ、ハンドル状態などを持つ

### `MobileHapticsSimulationView`

- `EditorWindow` 内の描画責務を分けるための補助クラス
- 表示ロジックが増えた場合に UI 描画を分離する

## Native Plugins

### `UnityMobileHapticsAndroidBridge.aar`

- Android ネイティブ API を呼び出すブリッジクラスを含む AAR
- C# から `AndroidJavaClass` で内部クラスを呼び出す想定
- 単発振動、可変制御振動、停止を提供する

### `UnityMobileHapticsiOSBridge.h`

- iOS ネイティブブリッジの公開宣言を配置する
- C# から `DllImport("__Internal")` で呼び出す関数を定義する

### `UnityMobileHapticsiOSBridge.mm`

- iOS の実処理を実装する
- `HapticType` に応じたネイティブハプティクス呼び出しを担当する
- 単発振動、可変制御振動、停止を提供する

## メモ

- 最初はクラス数を絞り、複雑化した時点で分割してもよい
- `MobileHapticsSimulationView` は不要なら後から省略できる
- Android は `.aar` 配布を基本とし、ソースは別ディレクトリで管理する
- iOS は実装都合に応じて `.m` または追加ファイルへ分割してよい
