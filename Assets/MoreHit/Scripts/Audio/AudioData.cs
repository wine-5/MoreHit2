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
    }

    [CreateAssetMenu(fileName = "AudioData", menuName = "Fibonacci/AudioData")]
    public class AudioDataSO : ScriptableObject
    {
        [Header("Audio List")]
        [SerializeField] private AudioData[] audioDataList;

        public AudioData[] AudioDataList => audioDataList;
    }
}
