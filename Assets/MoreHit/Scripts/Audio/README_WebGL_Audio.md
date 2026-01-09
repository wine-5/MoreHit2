# WebGL Audio System Setup Guide

## 概要
WebGLビルドでScriptableObjectが参照できない問題を解決するため、Static AudioDataStoreシステムを実装しました。

## ディレクトリ構成
AudioClipファイルを以下のResourcesディレクトリに配置してください：

```
Assets/
  Resources/
    Audio/
      BGM/
        TitleBGM.wav (または .mp3/.ogg)
        GameBGM.wav
        BossBGM.wav
      SE/
        ButtonClick.wav
        PlayerJump.wav
        EnemyHit.wav
        PlayerDamage.wav
        BossDamage.wav
        StockFull.wav
```

## ファイル配置手順
1. `Assets/Resources/` フォルダを作成（存在しない場合）
2. `Assets/Resources/Audio/` フォルダを作成
3. `Assets/Resources/Audio/BGM/` フォルダを作成
4. `Assets/Resources/Audio/SE/` フォルダを作成
5. 各AudioClipを対応するフォルダに配置

## AudioDataStoreの編集
新しい音声ファイルを追加する場合は、`AudioDataStore.cs`の以下の部分を編集してください：

```csharp
// 新しい音声定義を追加
public static readonly AudioInfo NEW_SOUND = new AudioInfo("NewSound", "Audio/SE/NewSound", 1.0f);

// BuildAudioInfoCache()メソッドに追加
private static void BuildAudioInfoCache()
{
    audioInfoCache = new Dictionary<string, AudioInfo>
    {
        // 既存の音声...
        { NEW_SOUND.Name, NEW_SOUND }, // 新しい音声を追加
    };
}
```

## 使用方法
コードからの音声再生は従来通り：
```csharp
// BGM再生
AudioManager.I.PlayBGM("TitleBGM");

// SE再生
AudioManager.I.Play("ButtonClick");
```

## WebGL対応の利点
- ScriptableObjectに依存しない
- Resources.Load()による動的ロード
- WebGLビルドで確実に動作
- 従来のScriptableObject方式もフォールバック対応

## 注意事項
- AudioClipファイル名は AudioDataStore の定義と一致させてください
- Resourcesフォルダ内のパスは正確に指定してください
- ファイル拡張子（.wav, .mp3, .ogg）は Resources.Load() で自動認識されます