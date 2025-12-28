using UnityEngine;
using System.Collections.Generic;

namespace MoreHit.Audio
{
    /// <summary>
    /// WebGL対応のため、AudioClipをResourcesから動的にロードするStaticデータストア
    /// ScriptableObjectの代替として使用
    /// </summary>
    public static class AudioDataStore
    {
        #region 音声データ定義

        // BGM定義（実際のファイル名と用途に合わせて修正）
        public static readonly AudioInfo TITLE_BGM = new AudioInfo("TitleBGM", "Audio/BGM/kaisyun", 0.05f);
        public static readonly AudioInfo INGAME_BGM = new AudioInfo("InGameBGM", "Audio/BGM/maou_bgm_8bit04", 0.05f);
        public static readonly AudioInfo GAMEOVER_BGM = new AudioInfo("GameOverBGM", "Audio/BGM/maou_bgm_8bit20", 0.05f);
        public static readonly AudioInfo CLEAR_BGM = new AudioInfo("ClearBGM", "Audio/BGM/maou_game_jingle05", 0.05f);

        // SE定義
        public static readonly AudioInfo SE_TAKE_DAMAGE = new AudioInfo("Se_TakeDamage", "Audio/SE/Se_TakeDamage", 0.15f);
        public static readonly AudioInfo SE_PROJECTILE = new AudioInfo("Se_Projectile", "Audio/SE/Se_Projectile", 0.12f);
        public static readonly AudioInfo SE_NORMAL_ATTACK = new AudioInfo("Se_NormalAttack", "Audio/SE/Se_NormalAttack", 0.18f);
        public static readonly AudioInfo SE_JUMP = new AudioInfo("Se_Jump", "Audio/SE/Se_Jump", 0.1f);
        public static readonly AudioInfo SE_ENEMY_DEFEAT = new AudioInfo("Se_EnemyDefeat", "Audio/SE/Se_EnemyDefeat", 0.16f);
        public static readonly AudioInfo SE_CHARGE = new AudioInfo("Se_Charge", "Audio/SE/Se_Charge", 0.14f);
        public static readonly AudioInfo SE_BUTTON = new AudioInfo("Se_Button", "Audio/SE/Se_Button", 0.1f);
        public static readonly AudioInfo SE_BOSS_DEFEAT = new AudioInfo("Se_BossDefeat", "Audio/SE/Se_BossDefeat", 0.2f);

        #endregion

        #region 静的データアクセス

        private static Dictionary<string, AudioInfo> audioInfoCache;

        /// <summary>
        /// 全音声データの辞書を取得（初回時にキャッシュを構築）
        /// </summary>
        public static Dictionary<string, AudioInfo> GetAudioInfoDictionary()
        {
            if (audioInfoCache == null)
            {
                BuildAudioInfoCache();
            }
            return audioInfoCache;
        }

        /// <summary>
        /// 指定した名前の音声情報を取得
        /// </summary>
        public static AudioInfo GetAudioInfo(string audioName)
        {
            var dictionary = GetAudioInfoDictionary();
            dictionary.TryGetValue(audioName, out AudioInfo info);
            return info; // 見つからない場合はnull
        }

        /// <summary>
        /// 音声データキャッシュを構築
        /// </summary>
        private static void BuildAudioInfoCache()
        {
            audioInfoCache = new Dictionary<string, AudioInfo>
            {
                // BGM
                { TITLE_BGM.Name, TITLE_BGM },
                { INGAME_BGM.Name, INGAME_BGM },
                { GAMEOVER_BGM.Name, GAMEOVER_BGM },
                { CLEAR_BGM.Name, CLEAR_BGM },
                // SE
                { SE_TAKE_DAMAGE.Name, SE_TAKE_DAMAGE },
                { SE_PROJECTILE.Name, SE_PROJECTILE },
                { SE_NORMAL_ATTACK.Name, SE_NORMAL_ATTACK },
                { SE_JUMP.Name, SE_JUMP },
                { SE_ENEMY_DEFEAT.Name, SE_ENEMY_DEFEAT },
                { SE_CHARGE.Name, SE_CHARGE },
                { SE_BUTTON.Name, SE_BUTTON },
                { SE_BOSS_DEFEAT.Name, SE_BOSS_DEFEAT }
            };

            Debug.Log($"[AudioDataStore] 音声データキャッシュを構築しました。登録件数: {audioInfoCache.Count}");
        }

        /// <summary>
        /// 登録されている全音声名を取得（デバッグ用）
        /// </summary>
        public static string[] GetAllAudioNames()
        {
            var dictionary = GetAudioInfoDictionary();
            var names = new string[dictionary.Count];
            dictionary.Keys.CopyTo(names, 0);
            return names;
        }

        #endregion

        #region 音声情報クラス

        /// <summary>
        /// 音声データの情報を保持するクラス
        /// </summary>
        [System.Serializable]
        public class AudioInfo
        {
            public string Name { get; private set; }
            public string ResourcePath { get; private set; }
            public float VolumeMultiplier { get; private set; }
            
            private AudioClip _cachedClip;

            public AudioInfo(string name, string resourcePath, float volumeMultiplier = 1.0f)
            {
                Name = name;
                ResourcePath = resourcePath;
                VolumeMultiplier = Mathf.Clamp(volumeMultiplier, 0f, 2f);
            }

            /// <summary>
            /// AudioClipを取得（初回時にResourcesからロード）
            /// </summary>
            public AudioClip AudioClip
            {
                get
                {
                    if (_cachedClip == null)
                    {
                        Debug.Log($"[AudioDataStore] AudioClipをロード試行中: {Name} -> {ResourcePath}");
                        _cachedClip = Resources.Load<AudioClip>(ResourcePath);
                        if (_cachedClip == null)
                        {
                            Debug.LogError($"[AudioDataStore] AudioClipがResourcesに見つかりません: {ResourcePath}");
                            
                            // WebGL診断: Resourcesフォルダの内容を確認
                            var allAudios = Resources.LoadAll<AudioClip>("Audio");
                            Debug.Log($"[AudioDataStore] Audioフォルダ内のファイル数: {allAudios.Length}");
                            foreach (var audio in allAudios)
                            {
                                Debug.Log($"  - 発見: {audio.name}");
                            }
                        }
                        else
                        {
                            Debug.Log($"[AudioDataStore] AudioClipをロード成功: {Name} -> {ResourcePath}");
                        }
                    }
                    return _cachedClip;
                }
            }

            /// <summary>
            /// キャッシュをクリア（メモリ節約用）
            /// </summary>
            public void ClearCache()
            {
                if (_cachedClip != null)
                {
                    Resources.UnloadAsset(_cachedClip);
                    _cachedClip = null;
                }
            }
        }

        #endregion

        #region ユーティリティ

        /// <summary>
        /// 全音声キャッシュをクリア（メモリ節約用）
        /// </summary>
        public static void ClearAllCache()
        {
            if (audioInfoCache != null)
            {
                foreach (var audioInfo in audioInfoCache.Values)
                {
                    audioInfo?.ClearCache();
                }
            }
            Debug.Log("[AudioDataStore] 全音声キャッシュをクリアしました");
        }

        #endregion
    }
}