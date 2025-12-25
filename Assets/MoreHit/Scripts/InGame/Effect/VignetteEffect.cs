using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using MoreHit.Events;

namespace MoreHit.Effects
{
    /// <summary>
    /// プレイヤーダメージ時のVignette演出を管理するクラス
    /// </summary>
    public class VignetteEffect : MonoBehaviour
    {
        [Header("設定")]
        [SerializeField, Range(0f, 1f)] private float maxIntensity = 0.5f;
        [SerializeField, Min(0)] private float duration = 1f;
        [SerializeField] private AnimationCurve intensityCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
        
        [Header("参照")]
        [SerializeField] private Volume globalVolume;
        
        private Vignette vignette;
        private Coroutine currentEffect;
        
        private void Awake()
        {
            // Global Volumeから自動取得
            if (globalVolume == null)
                globalVolume = FindAnyObjectByType<Volume>();
                
            // Vignetteコンポーネント取得
            if (globalVolume != null && globalVolume.profile.TryGet(out vignette))
            {
                vignette.active = false; // 初期状態は無効
            }
            else
            {
                Debug.LogWarning("VignetteEffect: Global VolumeまたはVignetteが見つかりません");
            }
        }
        
        private void OnEnable()
        {
            GameEvents.OnPlayerDamage += OnPlayerDamage;
        }
        
        private void OnDisable()
        {
            GameEvents.OnPlayerDamage -= OnPlayerDamage;
        }
        
        /// <summary>
        /// プレイヤーダメージイベントのハンドラー
        /// </summary>
        private void OnPlayerDamage(int damage, int currentHealth)
        {
            PlayVignetteEffect();
        }
        
        /// <summary>
        /// Vignette演出を再生
        /// </summary>
        public void PlayVignetteEffect()
        {
            if (vignette == null) return;
            
            // 既存のエフェクトがあれば停止
            if (currentEffect != null)
                StopCoroutine(currentEffect);
                
            currentEffect = StartCoroutine(VignetteCoroutine());
        }
        
        private IEnumerator VignetteCoroutine()
        {
            vignette.active = true;
            
            float elapsedTime = 0f;
            
            while (elapsedTime < duration)
            {
                float progress = elapsedTime / duration;
                float curveValue = intensityCurve.Evaluate(progress);
                
                vignette.intensity.value = curveValue * maxIntensity;
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            // 演出終了
            vignette.intensity.value = 0f;
            vignette.active = false;
            currentEffect = null;
        }
        
        /// <summary>
        /// エフェクトを即座に停止
        /// </summary>
        public void StopEffect()
        {
            if (currentEffect != null)
            {
                StopCoroutine(currentEffect);
                currentEffect = null;
            }
            
            if (vignette != null)
            {
                vignette.intensity.value = 0f;
                vignette.active = false;
            }
        }
    }
}