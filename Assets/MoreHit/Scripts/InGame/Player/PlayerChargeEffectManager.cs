using UnityEngine;
using MoreHit.Effect;

namespace MoreHit.Player
{
    /// <summary>
    /// プレイヤーのチャージエフェクトを管理するクラス
    /// </summary>
    public class PlayerChargeEffectManager : MonoBehaviour
    {
        [Header("エフェクト設定")]
        [SerializeField] private Transform effectSpawnPoint; // エフェクト生成位置
        private PlayerInputManager inputManager; // 入力管理への参照
        [SerializeField] private EffectDataSO effectDataCollection; // エフェクトデータ

        // エフェクト管理
        private GameObject currentChargeEffect;
        private bool isEffectActive = false;

        private void Awake()
        {
            inputManager = GetComponent<PlayerInputManager>();
        }

        private void OnEnable()
        {
            if (inputManager != null)
            {
                inputManager.onChargeStarted.AddListener(StartChargeEffect);
                inputManager.onChargeCanceled.AddListener(StopChargeEffect);
                inputManager.onChargeRangedAttack.AddListener(OnChargeRangedAttack);
            }
        }

        private void OnDisable()
        {
            if (inputManager != null)
            {
                inputManager.onChargeStarted.RemoveListener(StartChargeEffect);
                inputManager.onChargeCanceled.RemoveListener(StopChargeEffect);
                inputManager.onChargeRangedAttack.RemoveListener(OnChargeRangedAttack);
            }
        }

        /// <summary>
        /// チャージエフェクト開始
        /// </summary>
        private void StartChargeEffect()
        {
            if (EffectFactory.I == null || isEffectActive) return;

            // EffectDataからチャージ攻撃エフェクトデータを取得
            if (effectDataCollection == null)
            {
                Debug.LogError("[PlayerChargeEffectManager] EffectDataSOが設定されていません！");
                return;
            }

            EffectData chargeEffectData = effectDataCollection.GetEffectByType(EffectType.ChargeAttackEffect);
            if (chargeEffectData == null)
            {
                Debug.LogError("[PlayerChargeEffectManager] ChargeAttackEffectのデータが見つかりません！");
                return;
            }

            Vector3 spawnPosition = effectSpawnPoint != null ? effectSpawnPoint.position : transform.position;
            currentChargeEffect = EffectFactory.I.CreateEffect(EffectType.ChargeAttackEffect, spawnPosition);
            isEffectActive = true;
        }

        /// <summary>
        /// チャージエフェクト停止
        /// </summary>
        private void StopChargeEffect()
        {
            if (currentChargeEffect != null && EffectFactory.I != null)
            {
                EffectFactory.I.ReturnEffect(currentChargeEffect);
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
            // 念のためエフェクトをクリーンアップ
            StopChargeEffect();
        }
    }
}