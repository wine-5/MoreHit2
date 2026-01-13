using UnityEngine;
using MoreHit.Effect;
using MoreHit.Audio;

namespace MoreHit.Player
{
    /// <summary>
    /// プレイヤーのチャージエフェクトを管理するクラス
    /// </summary>
    public class PlayerChargeEffectManager : MonoBehaviour
    {
        [Header("エフェクト設定")]
        [SerializeField] private Transform effectSpawnPoint;
        private PlayerInputManager inputManager;
        
        private GameObject currentChargeEffect;
        private bool isEffectActive = false;

        private void Awake()
        {
            inputManager = GetComponent<PlayerInputManager>();
        }
        
        private void LateUpdate()
        {
            if (isEffectActive && currentChargeEffect != null)
            {
                Vector3 targetPosition = effectSpawnPoint != null ? effectSpawnPoint.position : transform.position;
                currentChargeEffect.transform.position = targetPosition;
            }
            else if (isEffectActive)
                isEffectActive = false;
        }

        private void OnEnable()
        {
            if (inputManager != null)
            {
                inputManager.onChargeStarted.AddListener(StartChargeEffect);
                inputManager.onChargeStateChanged.AddListener(OnChargeStateChanged);
                inputManager.onChargeRangedAttack.AddListener(OnChargeRangedAttack);
            }
        }

        private void OnDisable()
        {
            if (inputManager != null)
            {
                inputManager.onChargeStarted.RemoveListener(StartChargeEffect);
                inputManager.onChargeStateChanged.RemoveListener(OnChargeStateChanged);
                inputManager.onChargeRangedAttack.RemoveListener(OnChargeRangedAttack);
            }
            
            StopChargeEffect();
        }

        /// <summary>
        /// チャージ状態変化時の処理（チャージ準備完了・失敗時）
        /// </summary>
        private void OnChargeStateChanged(bool isCharging)
        {
            if (!isCharging && (isEffectActive || currentChargeEffect != null))
                StopChargeEffect();
        }

        /// <summary>
        /// チャージエフェクト開始
        /// </summary>
        private void StartChargeEffect()
        {
            if (isEffectActive) StopChargeEffect();
            if (EffectFactory.I == null) return;
            
            if (AudioManager.I != null)
                AudioManager.I.PlaySE(SeType.Charge);

            Vector3 spawnPosition = effectSpawnPoint != null ? effectSpawnPoint.position : transform.position;
            currentChargeEffect = EffectFactory.I.CreateEffect(EffectType.ChargeEffect, spawnPosition);
            
            if (currentChargeEffect != null)
            {
                isEffectActive = true;
                currentChargeEffect.transform.position = spawnPosition;
            }
            else
                isEffectActive = false;
        }

        /// <summary>
        /// チャージエフェクト停止
        /// </summary>
        private void StopChargeEffect()
        {
            if (!isEffectActive && currentChargeEffect == null) return;
            
            if (currentChargeEffect != null)
            {
                if (EffectFactory.I != null)
                    EffectFactory.I.ReturnEffect(currentChargeEffect);
                else
                    Destroy(currentChargeEffect);
                
                currentChargeEffect = null;
            }
            
            isEffectActive = false;
        }

        /// <summary>
        /// チャージ攻撃発動時のエフェクト停止
        /// </summary>
        private void OnChargeRangedAttack()
        {
            StopChargeEffect();
        }

        private void OnDestroy()
        {
            StopChargeEffect();
        }
    }
}