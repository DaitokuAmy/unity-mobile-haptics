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
      ImpactHapticType.cs
      HapticPlayMode.cs
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
- `IsSupported`、`Play`、`PlayLoop`、`Stop` を提供する
- 実際の再生処理はプラットフォーム実装へ委譲する

### `HapticType`

- 振動種別を表す enum
- 呼び出し側はネイティブ API ではなくこの型を使う

### `ImpactHapticType`

- Loop 再生可能な衝撃系振動種別を表す enum
- `PlayLoop` の引数として利用する

### `HapticPlayMode`

- 単発再生か Loop 再生かを表す enum
- Runtime と Editor の間で再生状態を共通表現として扱うために利用する

### `IMobileHapticsPlatform`

- プラットフォーム実装の共通インターフェース
- `IsSupported`、`Play`、`PlayLoop`、`Stop` を定義する

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
- `Play` は通知系と衝撃系の両方を扱い、`PlayLoop` は `ImpactHapticType` のみを扱う

### `IosMobileHapticsPlatform`

- iOS 向けの振動処理を実装する
- `HapticType` を iOS ネイティブ機能へ変換する
- `PlayLoop` は `ImpactHapticType` を iOS 側の impact 表現へ変換する

### `UnsupportedMobileHapticsPlatform`

- 非対応環境向けの no-op 実装
- `IsSupported` は `false` を返す

## Editor

### `MobileHapticsSimulationWindow`

- 開発中の振動状態を確認する専用 `EditorWindow`
- 現在の再生状態、再生種別、Loop 状態を表示する

### `MobileHapticsSimulationState`

- EditorWindow で表示するシミュレーション状態を保持する
- 最後に再生された `HapticType`、再生中フラグ、Loop 状態などを持つ

### `MobileHapticsSimulationView`

- `EditorWindow` 内の描画責務を分けるための補助クラス
- 表示ロジックが増えた場合に UI 描画を分離する

## Native Plugins

### `UnityMobileHapticsAndroidBridge.aar`

- Android ネイティブ API を呼び出すブリッジクラスを含む AAR
- C# から `AndroidJavaClass` で内部クラスを呼び出す想定
- 単発振動、Loop 振動、停止を提供する

### `UnityMobileHapticsiOSBridge.h`

- iOS ネイティブブリッジの公開宣言を配置する
- C# から `DllImport("__Internal")` で呼び出す関数を定義する

### `UnityMobileHapticsiOSBridge.mm`

- iOS の実処理を実装する
- `HapticType` に応じたネイティブハプティクス呼び出しを担当する
- 単発振動、Loop 相当処理、停止を提供する

## メモ

- 最初はクラス数を絞り、複雑化した時点で分割してもよい
- `HapticPlayMode` や `MobileHapticsSimulationView` は不要なら後から省略できる
- Android は `.aar` 配布を基本とし、ソースは別ディレクトリで管理する
- iOS は実装都合に応じて `.m` または追加ファイルへ分割してよい
