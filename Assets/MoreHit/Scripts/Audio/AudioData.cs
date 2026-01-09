using UnityEngine;

namespace MoreHit.Audio
{
    /// <summary>
    /// SE用のオーディオデータ
    /// </summary>
    [System.Serializable]
    public class SeAudioData
    {
        [SerializeField] private SeType seType;
        [SerializeField] private AudioClip audioClip;
        [SerializeField, Range(0f, 2f)] private float volumeMultiplier = 1.0f;

        public SeType SeType => seType;
        public AudioClip AudioClip => audioClip;
        public float VolumeMultiplier => volumeMultiplier;
    }

    /// <summary>
    /// BGM用のオーディオデータ
    /// </summary>
    [System.Serializable]
    public class BgmAudioData
    {
        [SerializeField] private BgmType bgmType;
        [SerializeField] private AudioClip audioClip;
        [SerializeField, Range(0f, 2f)] private float volumeMultiplier = 1.0f;
        [SerializeField] private bool loop = true;

        public BgmType BgmType => bgmType;
        public AudioClip AudioClip => audioClip;
        public float VolumeMultiplier => volumeMultiplier;
        public bool Loop => loop;
    }

    /// <summary>
    /// オーディオデータを管理するScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "AudioData", menuName = "MoreHit/AudioData")]
    public class AudioDataSO : ScriptableObject
    {
        [Header("SE List")]
        [SerializeField] private SeAudioData[] seAudioDataList;
        
        [Header("BGM List")]
        [SerializeField] private BgmAudioData[] bgmAudioDataList;

        public SeAudioData[] SeAudioDataList => seAudioDataList;
        public BgmAudioData[] BgmAudioDataList => bgmAudioDataList;
    }
}
