using MoreHit.Audio;
using UnityEngine;
using UnityEngine.UI;

namespace MoreHit.UI
{
    /// <summary>
    /// 設定画面のUI制御
    /// 音量スライダーとAudioManagerの連携を管理
    /// </summary>
    public class SettingsUI : MonoBehaviour
    {
        [Header("Volume Sliders")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider bgmVolumeSlider;
        [SerializeField] private Slider seVolumeSlider;

        [Header("Volume Labels")]
        [SerializeField] private Text masterVolumeLabel;
        [SerializeField] private Text bgmVolumeLabel;
        [SerializeField] private Text seVolumeLabel;

        void Start()
        {
            InitializeSliders();
            SetupSliderEvents();
        }

        /// <summary>
        /// スライダーの初期値をAudioManagerから取得
        /// </summary>
        private void InitializeSliders()
        {
            if (AudioManager.Instance != null)
            {
                if (masterVolumeSlider != null)
                {
                    masterVolumeSlider.value = AudioManager.Instance.MasterVolume;
                    UpdateVolumeLabel(masterVolumeLabel, AudioManager.Instance.MasterVolume);
                }

                if (bgmVolumeSlider != null)
                {
                    bgmVolumeSlider.value = AudioManager.Instance.BGMVolume;
                    UpdateVolumeLabel(bgmVolumeLabel, AudioManager.Instance.BGMVolume);
                }

                if (seVolumeSlider != null)
                {
                    seVolumeSlider.value = AudioManager.Instance.SEVolume;
                    UpdateVolumeLabel(seVolumeLabel, AudioManager.Instance.SEVolume);
                }
            }
        }

        /// <summary>
        /// スライダーのイベントを設定
        /// </summary>
        private void SetupSliderEvents()
        {
            if (masterVolumeSlider != null)
                masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);

            if (bgmVolumeSlider != null)
                bgmVolumeSlider.onValueChanged.AddListener(OnBGMVolumeChanged);

            if (seVolumeSlider != null)
                seVolumeSlider.onValueChanged.AddListener(OnSEVolumeChanged);
        }

        /// <summary>
        /// マスター音量変更時の処理
        /// </summary>
        private void OnMasterVolumeChanged(float value)
        {
            if (AudioManager.I != null)
            {
                AudioManager.I.SetMasterVolume(value);
                UpdateVolumeLabel(masterVolumeLabel, value);
            }
        }

        /// <summary>
        /// BGM音量変更時の処理
        /// </summary>
        private void OnBGMVolumeChanged(float value)
        {
            if (AudioManager.I != null)
            {
                AudioManager.I.SetBGMVolume(value);
                UpdateVolumeLabel(bgmVolumeLabel, value);
            }
        }

        /// <summary>
        /// SE音量変更時の処理
        /// </summary>
        private void OnSEVolumeChanged(float value)
        {
            if (AudioManager.I != null)
            {
                AudioManager.I.SetSEVolume(value);
                UpdateVolumeLabel(seVolumeLabel, value);
            }
        }

        /// <summary>
        /// 音量ラベルの表示を更新
        /// </summary>
        private void UpdateVolumeLabel(Text label, float volume)
        {
            if (label != null)
                label.text = $"{Mathf.RoundToInt(volume * 100)}%";
        }

        /// <summary>
        /// 設定をデフォルトに戻す
        /// </summary>
        public void ResetToDefault()
        {
            if (AudioManager.I != null)
            {
                AudioManager.I.SetMasterVolume(1f);
                AudioManager.I.SetBGMVolume(1f);
                AudioManager.I.SetSEVolume(1f);
                
                if (masterVolumeSlider != null) masterVolumeSlider.value = 1f;
                if (bgmVolumeSlider != null) bgmVolumeSlider.value = 1f;
                if (seVolumeSlider != null) seVolumeSlider.value = 1f;
            }
        }

        void OnDestroy()
        {
            // イベントの解除
            if (masterVolumeSlider != null)
                masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);

            if (bgmVolumeSlider != null)
                bgmVolumeSlider.onValueChanged.RemoveListener(OnBGMVolumeChanged);

            if (seVolumeSlider != null)
                seVolumeSlider.onValueChanged.RemoveListener(OnSEVolumeChanged);
        }
    }
}
