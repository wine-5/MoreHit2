using UnityEngine;
using System.Collections.Generic;

namespace MoreHit.Audio
{
    public class AudioManager : Singleton<AudioManager>
    {
        protected override bool UseDontDestroyOnLoad => true;

        [Header("Audio Data")]
        [SerializeField] private AudioDataSO[] audioDataArray;
        [SerializeField] private int maxAudioSources = 10;

        [Header("Volume Settings")]
        [SerializeField, Range(0f, 1f)] private float masterVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float bgmVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float seVolume = 1f;
        
        private Dictionary<string, AudioData> audioDictionary;
        private List<AudioSource> audioSourcePool;
        private Queue<AudioSource> availableAudioSources;

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
            InitializeAudioDictionary();
            SetupAudioSourcePool();
        }

        private void Start()
        {
            // ゲーム開始時にBGMを再生
            // PlayBGM("GameBGM");
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

        private void InitializeAudioDictionary()
        {
            audioDictionary = new Dictionary<string, AudioData>();
            
            // ScriptableObjectから音声データを取得
            if (audioDataArray != null)
            {
                foreach (var audioDataSO in audioDataArray)
                {
                    if (audioDataSO != null && audioDataSO.AudioDataList != null)
                    {
                        foreach (var audioData in audioDataSO.AudioDataList)
                        {
                            if (audioData != null && !string.IsNullOrEmpty(audioData.AudioName))
                            {
                                audioDictionary[audioData.AudioName] = audioData;
                            }
                        }
                    }
                }
            }
            
            Debug.Log($"[AudioManager] 音声辞書の初期化完了。総登録数: {audioDictionary.Count}");
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
        /// 指定した名前の音声を再生します
        /// </summary>
        /// <param name="audioName">音声データの名前</param>
        public void Play(string audioName)
        {
            if (audioDictionary.TryGetValue(audioName, out AudioData audioData))
            {
                if (audioData.AudioClip != null)
                {
                    AudioSource audioSource = GetAvailableAudioSource();
                    if (audioSource != null)
                    {
                        audioSource.clip = audioData.AudioClip;
                        // BGMかSEかを判定（名前にbgmやmusicが含まれていればBGM）
                        bool isBGM = audioName.ToLower().Contains("bgm") || audioName.ToLower().Contains("music");
                        float volumeMultiplier = isBGM ? bgmVolume : seVolume;
                        audioSource.volume = audioData.VolumeMultiplier * volumeMultiplier * masterVolume;
                        audioSource.loop = false; // SEはループしない
                        audioSource.Play();
                        
                        StartCoroutine(ReturnAudioSourceWhenFinished(audioSource));
                    }
                    else
                        Debug.LogError("No available AudioSource to play the audio");
                }
                else
                    Debug.LogError($"AudioClip is null for audio: {audioName}");
            }
            else
                Debug.LogError($"Audio not found: {audioName}");
        }

        /// <summary>
        /// BGMをループ再生します
        /// </summary>
        /// <param name="bgmName">BGMの名前</param>
        public void PlayBGM(string bgmName)
        {
            if (audioDictionary.TryGetValue(bgmName, out AudioData audioData))
            {
                if (audioData.AudioClip != null)
                {
                    // 既存のBGMを停止
                    StopBGM();
                    
                    AudioSource audioSource = GetAvailableAudioSource();
                    if (audioSource != null)
                    {
                        audioSource.clip = audioData.AudioClip;
                        audioSource.volume = audioData.VolumeMultiplier * bgmVolume * masterVolume;
                        audioSource.loop = true; // BGMはループ再生
                        audioSource.Play();
                        
                        Debug.Log($"[AudioManager] BGM再生開始: {bgmName}");
                        // BGM用AudioSourceは回収しない（ループし続けるため）
                    }
                    else
                        Debug.LogError("[AudioManager] No available AudioSource to play BGM");
                }
                else
                {
                    Debug.LogError($"[AudioManager] AudioClip is null for BGM: {bgmName}");
                }
            }
            else
            {
                Debug.LogError($"[AudioManager] BGM not found: {bgmName}");
                PrintRegisteredAudioNames();
            }
        }

        /// <summary>
        /// 再生中のBGMを停止します
        /// </summary>
        public void StopBGM()
        {
            foreach (var audioSource in audioSourcePool)
            {
                if (audioSource.isPlaying && audioSource.loop)
                {
                    audioSource.Stop();
                    audioSource.loop = false;
                    availableAudioSources.Enqueue(audioSource);
                }
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
                if (!audioSource.isPlaying)
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

            availableAudioSources.Enqueue(audioSource);
        }

        /// <summary>
        /// すべての音声を停止します
        /// </summary>
        public void StopAll()
        {
            foreach (var audioSource in audioSourcePool)
            {
                if (audioSource.isPlaying)
                {
                    audioSource.Stop();
                    availableAudioSources.Enqueue(audioSource);
                }
            }
        }

        /// <summary>
        /// 指定した名前の音声をすべて停止します
        /// </summary>
        /// <param name="audioName">停止する音声の名前</param>
        public void Stop(string audioName)
        {
            if (audioDictionary.TryGetValue(audioName, out AudioData audioData))
            {
                foreach (var audioSource in audioSourcePool)
                {
                    if (audioSource.isPlaying && audioSource.clip == audioData.AudioClip)
                    {
                        audioSource.Stop();
                        availableAudioSources.Enqueue(audioSource);
                    }
                }
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
            UpdateAllVolumes();
        }

        /// <summary>
        /// SE音量を設定
        /// </summary>
        public void SetSEVolume(float volume)
        {
            seVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat(SE_VOLUME_KEY, seVolume);
            UpdateAllVolumes();
        }

        /// <summary>
        /// 再生中の全AudioSourceの音量を更新
        /// </summary>
        private void UpdateAllVolumes()
        {
            foreach (var audioSource in audioSourcePool)
            {
                if (audioSource.isPlaying && audioSource.clip != null)
                {
                    string audioName = GetAudioNameFromClip(audioSource.clip);
                    if (!string.IsNullOrEmpty(audioName) && audioDictionary.TryGetValue(audioName, out AudioData audioData))
                    {
                        bool isBGM = audioName.ToLower().Contains("bgm") || audioName.ToLower().Contains("music");
                        float volumeMultiplier = isBGM ? bgmVolume : seVolume;
                        audioSource.volume = audioData.VolumeMultiplier * volumeMultiplier * masterVolume;
                    }
                }
            }
        }

        /// <summary>
        /// AudioClipから音声名を取得（逆引き）
        /// </summary>
        private string GetAudioNameFromClip(AudioClip clip)
        {
            foreach (var kvp in audioDictionary)
            {
                if (kvp.Value.AudioClip == clip)
                    return kvp.Key;
            }
            return string.Empty;
        }
        
        /// <summary>
        /// 登録されている全音声名を表示（デバッグ用）
        /// </summary>
        public void PrintRegisteredAudioNames()
        {
            Debug.Log($"[AudioManager] 登録音声数: {audioDictionary.Count}");
            foreach (var kvp in audioDictionary)
            {
                bool hasClip = kvp.Value.AudioClip != null;
                Debug.Log($"  - {kvp.Key} (AudioClip: {(hasClip ? "✓" : "✗")})");
            }
        }
    }
}
