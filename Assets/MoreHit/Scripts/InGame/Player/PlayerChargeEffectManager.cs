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
        [SerializeField] private Transform effectSpawnPoint; // エフェクト生成位置
        private PlayerInputManager inputManager; // 入力管理への参照
        
        // 定数
        private const float POSITION_TOLERANCE = 0.01f;

        // エフェクト管理
        private GameObject currentChargeEffect;
        private bool isEffectActive = false;

        private void Awake()
        {
            inputManager = GetComponent<PlayerInputManager>();
        }
        
        private void LateUpdate()
        {
            // エフェクトがアクティブな場合、毎フレーム位置を更新
            // LateUpdateを使用することでプレイヤーの移動処理の後に実行される
            if (isEffectActive && currentChargeEffect != null)
            {
                Vector3 targetPosition = effectSpawnPoint != null ? effectSpawnPoint.position : transform.position;
                Transform effectTransform = currentChargeEffect.transform;
                
                // 毎フレーム親をチェックして解除（必要に応じて）
                if (effectTransform.parent != null)
                    effectTransform.SetParent(null, false);
                
                // 位置を強制的に設定
                effectTransform.position = targetPosition;
            }
            else if (isEffectActive)
                isEffectActive = false; // 状態を修正
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
            
            // コンポーネントが無効化される時はエフェクトを確実に停止
            StopChargeEffect();
        }

        /// <summary>
        /// チャージ状態変化時の処理（チャージ準備完了・失敗時）
        /// </summary>
        /// <param name="isCharging">チャージ中かどうか</param>
        private void OnChargeStateChanged(bool isCharging)
        {
            // チャージが終了した時のみエフェクトを停止
            // ただし、既にエフェクトが停止している場合は処理をスキップ
            if (!isCharging && (isEffectActive || currentChargeEffect != null))
                StopChargeEffect();
        }

        /// <summary>
        /// チャージエフェクト開始
        /// </summary>
        private void StartChargeEffect()
        {
            // 既にエフェクトがアクティブの場合は一度停止
            if (isEffectActive) StopChargeEffect();
            
            if (EffectFactory.I == null) return;
            
            // チャージSE再生
            if (AudioManager.I != null)
            {
                AudioManager.I.PlaySE(SeType.Charge);
            }

            Vector3 spawnPosition = effectSpawnPoint != null ? effectSpawnPoint.position : transform.position;
            currentChargeEffect = EffectFactory.I.CreateEffect(EffectType.ChargeAttackEffect, spawnPosition);
            
            // エフェクト生成が成功した場合のみフラグを設定
            if (currentChargeEffect != null)
            {
                isEffectActive = true;
                Transform effectTransform = currentChargeEffect.transform;
                
                // 生成直後に親を確実に解除し、位置を正しく設定
                if (effectTransform.parent != null)
                    effectTransform.SetParent(null, false);
                
                // 現在のプレイヤー位置に強制設定
                Vector3 currentPlayerPos = effectSpawnPoint != null ? effectSpawnPoint.position : transform.position;
                effectTransform.position = currentPlayerPos;
                
                // 位置設定の即座確認と再設定（確実な同期のため）
                if (Vector3.Distance(effectTransform.position, currentPlayerPos) > POSITION_TOLERANCE)
                    effectTransform.position = currentPlayerPos;
            }
            else
                isEffectActive = false;
        }

        /// <summary>
        /// チャージエフェクト停止
        /// </summary>
        private void StopChargeEffect()
        {
            // 既に停止している場合は何もしない
            if (!isEffectActive && currentChargeEffect == null) return;
            
            if (currentChargeEffect != null)
            {
                if (EffectFactory.I != null)
                {
                    EffectFactory.I.ReturnEffect(currentChargeEffect);
                }
                else
                {
                    // EffectFactoryがない場合は直接破棄
                    Destroy(currentChargeEffect);
                }
                currentChargeEffect = null;
            }
            
            // エフェクトの有無に関わらず状態をリセット
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