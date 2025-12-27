using UnityEngine;
using System.Collections;
using MoreHit.Attack;
using MoreHit.Events;
using MoreHit.Player;

namespace MoreHit.Enemy
{
    /// <summary>
    /// ボス敵の実装クラス
    /// ストックを上限まで溜めて攻撃することで初めてHPを減らせる特殊な敵
    /// </summary>
    public class BossEnemy : EnemyBase
    {
        [Header("ボス専用設定")]
        [SerializeField] private float attackCooldown = 3f;
        [SerializeField] private float attackRange = 5f;
        [SerializeField] private int maxHP = 300;
        
        private float lastAttackTime = 0f;
        private bool canTakeDamage = false; // ストック満タン時のみtrue
        
        protected override void Awake()
        {
            Debug.Log("[BossEnemy] Awake開始");
            
            // Boss専用の初期化
            enemyType = EnemyType.Boss;
            Debug.Log($"[BossEnemy] EnemyType設定: {enemyType}");
            
            base.Awake();
            
            Debug.Log("[BossEnemy] Awake完了");
        }
        
        /// <summary>
        /// ボス敵固有の初期化処理
        /// </summary>
        protected override void InitializeEnemy()
        {
            Debug.Log("[BossEnemy] InitializeEnemy開始");
            base.InitializeEnemy();
            
            // Boss専用の初期化処理
            if (enemyData != null)
            {
                currentHP = maxHP; // Boss用の高いHPを設定
                Debug.Log($"[BossEnemy] HP初期化: {currentHP}/{maxHP}");
            }
            else
            {
                Debug.LogError("[BossEnemy] enemyDataがnullです!");
            }
            
            // Boss出現イベントを発火
            Debug.Log("[BossEnemy] Boss出現イベント発火");
            GameEvents.TriggerBossAppear();
        }
        
        protected override void Update()
        {
            if (isDead)
            {
                Debug.Log("[BossEnemy] 死亡状態でUpdate処理をスキップ");
                return;
            }
            
            if (!canMove)
            {
                Debug.Log("[BossEnemy] 移動不可状態でUpdate処理をスキップ");
                return;
            }
            
            Debug.Log($"[BossEnemy] Update処理 - isDead: {isDead}, canMove: {canMove}, currentState: {currentState}");
            
            // ストックタイマー更新
            UpdateStockTimer();
            
            // プレイヤー追跡
            Move();
            
            // 攻撃判定
            Attack();
        }
        
        /// <summary>
        /// ストックタイマーの更新（親クラスの処理を流用）
        /// </summary>
        private void UpdateStockTimer()
        {
            if (isStockTimerActive)
            {
                stockResetTimer -= Time.deltaTime;
                if (stockResetTimer <= 0f)
                {
                    ClearStock();
                    isStockTimerActive = false;
                }
            }
        }
        
        /// <summary>
        /// ボス敵の移動処理
        /// </summary>
        protected override void Move()
        {
            if (PlayerDataProvider.I == null)
            {
                Debug.LogWarning("[BossEnemy] PlayerDataProvider.Iがnullです");
                return;
            }
            
            if (enemyData == null)
            {
                Debug.LogWarning("[BossEnemy] enemyDataがnullです");
                return;
            }
            
            if (rb == null)
            {
                Debug.LogWarning("[BossEnemy] Rigidbody2Dがnullです");
                return;
            }
            
            Vector3 targetPosition = PlayerDataProvider.I.Position;
            Vector3 direction = (targetPosition - transform.position).normalized;
            
            Debug.Log($"[BossEnemy] 移動処理 - Target: {targetPosition}, Current: {transform.position}, Direction: {direction}");
            
            // X方向のみ移動（2Dゲーム用）
            if (Mathf.Abs(direction.x) > 0.1f)
            {
                Vector2 newVelocity = new Vector2(direction.x * enemyData.MoveSpeed, rb.linearVelocity.y);
                rb.linearVelocity = newVelocity;
                
                Debug.Log($"[BossEnemy] 新しいvelocity: {newVelocity}, MoveSpeed: {enemyData.MoveSpeed}");
                
                // スプライトの向きを調整
                if (spriteRenderer != null)
                {
                    spriteRenderer.flipX = direction.x < 0;
                }
            }
            else
            {
                Debug.Log("[BossEnemy] 方向の差が小さいため移動しません");
            }
        }

        /// <summary>
        /// ボス敵の攻撃処理
        /// </summary>
        protected override void Attack()
        {
            if (Time.time - lastAttackTime < attackCooldown)
            {
                Debug.Log($"[BossEnemy] 攻撃クールダウン中 - 残り時間: {attackCooldown - (Time.time - lastAttackTime)}秒");
                return;
            }
            
            if (PlayerDataProvider.I == null)
            {
                Debug.LogWarning("[BossEnemy] PlayerDataProvider.Iがnullで攻撃できません");
                return;
            }
            
            float distanceToPlayer = Vector3.Distance(transform.position, PlayerDataProvider.I.Position);
            Debug.Log($"[BossEnemy] プレイヤーとの距離: {distanceToPlayer}, 攻撃範囲: {attackRange}");
            
            if (distanceToPlayer <= attackRange)
            {
                Debug.Log("[BossEnemy] プレイヤーを攻撃！");
                AttackPlayer();
                lastAttackTime = Time.time;
            }
            else
            {
                Debug.Log("[BossEnemy] プレイヤーが攻撃範囲外");
            }
        }

        /// <summary>
        /// ボス専用ダメージ処理
        /// ストック満タン状態でのみHPが減る
        /// </summary>
        public override void TakeDamage(int damage)
        {
            if (isDead) return;
            
            // 通常の攻撃ではストックのみ増加
            if (currentState != EnemyState.ReadyToLaunch)
            {
                // 通常状態：ストックのみ増加、HPは減らない
                AddStock(1); // 攻撃を受ける度にストック+1
                return;
            }
            
            // ReadyToLaunch状態（ストック満タン）：HPを減らす
            if (canTakeDamage)
            {
                currentHP -= damage;
                
                // ダメージエフェクト
                GameEvents.TriggerEnemyDamaged(gameObject, damage);
                
                // ストッククリア
                ClearStock();
                canTakeDamage = false;
                currentState = EnemyState.Move;
                
                // 死亡判定
                if (currentHP <= 0)
                {
                    Die();
                }
            }
        }
        
        /// <summary>
        /// ストック満タン時の処理をオーバーライド
        /// </summary>
        protected override void OnStockReachedRequired()
        {
            base.OnStockReachedRequired();
            
            // Boss専用：ダメージを受けられる状態に
            canTakeDamage = true;
        }

        /// <summary>
        /// ボス敵固有の死亡処理
        /// </summary>
        public override void Die()
        {
            if (isDead) return;
            
            isDead = true;
            canMove = false;
            
            // Boss撃破イベント発火
            GameEvents.TriggerBossDefeated();
            
            // 撃破エフェクト
            GameEvents.TriggerEnemyDefeated(gameObject);
            
            // オブジェクト非アクティブ化
            gameObject.SetActive(false);
        }
        
        /// <summary>
        /// 現在のHP率を取得（HPバー表示用）
        /// </summary>
        public float GetHPRatio()
        {
            return maxHP > 0 ? (float)currentHP / maxHP : 0f;
        }
        
        /// <summary>
        /// 最大HPを取得
        /// </summary>
        public int GetMaxHP()
        {
            return maxHP;
        }
        
        /// <summary>
        /// 現在のHPを取得
        /// </summary>
        public int GetCurrentHP()
        {
            return Mathf.FloorToInt(currentHP);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            // 攻撃範囲の可視化
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
#endif
    }
}
