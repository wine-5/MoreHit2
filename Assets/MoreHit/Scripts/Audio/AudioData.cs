using UnityEngine;

namespace MoreHit.Audio
{
    [System.Serializable]
    public class AudioData
    {
        [SerializeField] private string audioName;
        [SerializeField] private AudioClip audioClip;
        [SerializeField, Range(0f, 2f)] private float volumeMultiplier = 1.0f;

        public string AudioName => audioName;
        public AudioClip AudioClip => audioClip;
        public float VolumeMultiplier => volumeMultiplier;
        
        /// <summary>
        /// AudioDataのコンストラクタ（Static AudioDataStore用）
        /// </summary>
        public AudioData(string name, AudioClip clip, float volume)
        {
            audioName = name;
            audioClip = clip;
            volumeMultiplier = Mathf.Clamp(volume, 0f, 2f);
        }
    }

    [CreateAssetMenu(fileName = "AudioData", menuName = "MoreHit/AudioData")]
    public class AudioDataSO : ScriptableObject
    {
        [Header("Audio List")]
        [SerializeField] private AudioData[] audioDataList;

        public AudioData[] AudioDataList => audioDataList;
    }
}
