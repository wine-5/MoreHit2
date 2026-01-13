using UnityEngine;
using System.Collections.Generic;

namespace MoreHit.Audio
{
    public class AudioManager : Singleton<AudioManager>
    {
        protected override bool UseDontDestroyOnLoad => true;

        [Header("Audio Data")]
        [SerializeField] private AudioDataSO audioDataSO;
        [SerializeField] private int maxAudioSources = 10;

        [Header("Volume Settings")]
        [SerializeField, Range(0f, 1f)] private float masterVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float bgmVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float seVolume = 1f;
        
        private Dictionary<SeType, SeAudioData> seAudioDictionary;
        private Dictionary<BgmType, BgmAudioData> bgmAudioDictionary;
        private List<AudioSource> audioSourcePool;
        private Queue<AudioSource> availableAudioSources;
        private AudioSource currentBgmSource;

        // 音量設定のPlayerPrefsキー
        private const string MASTER_VOLUME_KEY = "MasterVolume";
        private const string BGM_VOLUME_KEY = "BGMVolume";
        private const string SE_VOLUME_KEY = "SEVolume";

        public float MasterVolume => masterVolume;
        public float BGMVolume => bgmVolume;
        public float SEVolume => seVolume;

        protected override void Awake()
        {
            base.Awake();
            LoadVolumeSettings();
            InitializeAudioDictionaries();
            SetupAudioSourcePool();
        }

        /// <summary>
        /// PlayerPrefsから音量設定を読み込み
        /// </summary>
        private void LoadVolumeSettings()
        {
            masterVolume = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, 1f);
            bgmVolume = PlayerPrefs.GetFloat(BGM_VOLUME_KEY, 1f);
            seVolume = PlayerPrefs.GetFloat(SE_VOLUME_KEY, 1f);
        }

        private void InitializeAudioDictionaries()
        {
            seAudioDictionary = new Dictionary<SeType, SeAudioData>();
            bgmAudioDictionary = new Dictionary<BgmType, BgmAudioData>();
            
            if (audioDataSO == null)
            {
                Debug.LogWarning("AudioManager: AudioDataSOが設定されていません");
                return;
            }
            
            // SEデータを辞書に登録
            if (audioDataSO.SeAudioDataList != null)
            {
                foreach (var seData in audioDataSO.SeAudioDataList)
                {
                    if (seData != null && seData.AudioClip != null)
                    {
                        seAudioDictionary[seData.SeType] = seData;
                    }
                }
            }
            
            // BGMデータを辞書に登録
            if (audioDataSO.BgmAudioDataList != null)
            {
                foreach (var bgmData in audioDataSO.BgmAudioDataList)
                {
                    if (bgmData != null && bgmData.AudioClip != null)
                    {
                        bgmAudioDictionary[bgmData.BgmType] = bgmData;
                    }
                }
            }
        }

        private void SetupAudioSourcePool()
        {
            audioSourcePool = new List<AudioSource>();
            availableAudioSources = new Queue<AudioSource>();

            for (int i = 0; i < maxAudioSources; i++)
            {
                AudioSource audioSource = gameObject.AddComponent<AudioSource>();
                audioSourcePool.Add(audioSource);
                availableAudioSources.Enqueue(audioSource);
            }
        }

        /// <summary>
        /// SEを再生
        /// </summary>
        public void PlaySE(SeType seType)
        {
            if (seType == SeType.None) return;
            
            if (seAudioDictionary.TryGetValue(seType, out SeAudioData seData))
            {
                if (seData.AudioClip != null)
                {
                    AudioSource audioSource = GetAvailableAudioSource();
                    if (audioSource != null)
                    {
                        audioSource.clip = seData.AudioClip;
                        audioSource.volume = seData.VolumeMultiplier * seVolume * masterVolume;
                        audioSource.loop = false;
                        audioSource.Play();
                        
                        StartCoroutine(ReturnAudioSourceWhenFinished(audioSource));
                    }
                }
            }
            else
            {
                Debug.LogWarning($"AudioManager: SE '{seType}' が見つかりません");
            }
        }

        /// <summary>
        /// BGMをループ再生
        /// </summary>
        public void PlayBGM(BgmType bgmType)
        {
            if (bgmType == BgmType.None) return;
            
            if (bgmAudioDictionary.TryGetValue(bgmType, out BgmAudioData bgmData))
            {
                if (bgmData.AudioClip != null)
                {
                    // 既存のBGMを停止
                    StopBGM();
                    
                    AudioSource audioSource = GetAvailableAudioSource();
                    if (audioSource != null)
                    {
                        audioSource.clip = bgmData.AudioClip;
                        audioSource.volume = bgmData.VolumeMultiplier * bgmVolume * masterVolume;
                        audioSource.loop = bgmData.Loop;
                        audioSource.Play();
                        
                        currentBgmSource = audioSource;
                    }
                }
            }
            else
            {
                Debug.LogWarning($"AudioManager: BGM '{bgmType}' が見つかりません");
            }
        }

        /// <summary>
        /// 再生中のBGMを停止
        /// </summary>
        public void StopBGM()
        {
            if (currentBgmSource != null && currentBgmSource.isPlaying)
            {
                currentBgmSource.Stop();
                currentBgmSource.loop = false;
                availableAudioSources.Enqueue(currentBgmSource);
                currentBgmSource = null;
            }
        }

        private AudioSource GetAvailableAudioSource()
        {
            // 利用可能なAudioSourceがあればそれを返す
            if (availableAudioSources.Count > 0)
                return availableAudioSources.Dequeue();

            // なければ再生していないAudioSourceを探す
            foreach (var audioSource in audioSourcePool)
            {
                if (!audioSource.isPlaying && audioSource != currentBgmSource)
                    return audioSource;
            }

            return null;
        }

        private System.Collections.IEnumerator ReturnAudioSourceWhenFinished(AudioSource audioSource)
        {
            while (audioSource.isPlaying)
            {
                yield return null;
            }

            if (audioSource != currentBgmSource)
            {
                availableAudioSources.Enqueue(audioSource);
            }
        }

        /// <summary>
        /// マスター音量を設定
        /// </summary>
        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat(MASTER_VOLUME_KEY, masterVolume);
            UpdateAllVolumes();
        }

        /// <summary>
        /// BGM音量を設定
        /// </summary>
        public void SetBGMVolume(float volume)
        {
            bgmVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat(BGM_VOLUME_KEY, bgmVolume);
            UpdateBGMVolume();
        }

        /// <summary>
        /// SE音量を設定
        /// </summary>
        public void SetSEVolume(float volume)
        {
            seVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat(SE_VOLUME_KEY, seVolume);
        }

        /// <summary>
        /// 再生中の全AudioSourceの音量を更新
        /// </summary>
        private void UpdateAllVolumes()
        {
            UpdateBGMVolume();
        }

        /// <summary>
        /// BGMの音量を更新
        /// </summary>
        private void UpdateBGMVolume()
        {
            if (currentBgmSource != null && currentBgmSource.isPlaying)
            {
                // 現在のBGMタイプを特定
                foreach (var kvp in bgmAudioDictionary)
                {
                    if (kvp.Value.AudioClip == currentBgmSource.clip)
                    {
                        currentBgmSource.volume = kvp.Value.VolumeMultiplier * bgmVolume * masterVolume;
                        break;
                    }
                }
            }
        }
    }
}
