using UnityEngine;
using System.Collections;
using MoreHit.Attack;
using MoreHit.Events;
using MoreHit.Player;

namespace MoreHit.Enemy
{
    /// <summary>
    /// ボス敵クラス
    /// ストックを上限まで溜めて攻撃することで初めてHPを減らせる特殊な敵
    /// </summary>
    public class Boss : EnemyBase
    {
        [Header("ボス専用設定")]
        [SerializeField] private float attackCooldown = 3f;
        [SerializeField] private float attackRange = 5f;
        [SerializeField] private int maxHP = 300;
        
        private float lastAttackTime = 0f;
        private bool canTakeDamage = false; // ストック満タン時のみtrue
        
        protected override void Awake()
        {
            // Boss専用の初期化
            enemyType = EnemyType.Boss;
            base.Awake();
        }
        
        protected override void InitializeEnemy()
        {
            base.InitializeEnemy();
            
            // Boss専用の初期化処理
            if (enemyData != null)
            {
                currentHP = maxHP; // Boss用の高いHPを設定
            }
            
            // Boss出現イベントを発火
            GameEvents.TriggerBossAppear();
        }
        
        protected override void Update()
        {
            if (isDead || !canMove) return;
            
            // ストックタイマー更新
            UpdateStockTimer();
            
            // プレイヤー追跡
            TrackPlayer();
            
            // 攻撃判定
            TryAttack();
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
        /// プレイヤーを追跡する移動AI
        /// </summary>
        private void TrackPlayer()
        {
            if (PlayerDataProvider.I == null) return;
            
            Vector3 targetPosition = PlayerDataProvider.I.Position;
            Vector3 direction = (targetPosition - transform.position).normalized;
            
            // X方向のみ移動（2Dゲーム用）
            if (Mathf.Abs(direction.x) > 0.1f)
            {
                rb.linearVelocity = new Vector2(direction.x * enemyData.MoveSpeed, rb.linearVelocity.y);
                
                // スプライトの向きを調整
                if (spriteRenderer != null)
                {
                    spriteRenderer.flipX = direction.x < 0;
                }
            }
        }
        
        /// <summary>
        /// 攻撃を試行
        /// </summary>
        private void TryAttack()
        {
            if (Time.time - lastAttackTime < attackCooldown) return;
            
            if (PlayerDataProvider.I == null) return;
            
            float distanceToPlayer = Vector3.Distance(transform.position, PlayerDataProvider.I.Position);
            
            if (distanceToPlayer <= attackRange)
            {
                AttackPlayer();
                lastAttackTime = Time.time;
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
        /// Boss撃破処理
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
        
        /// <summary>
        /// 移動処理（抽象メソッドの実装）
        /// </summary>
        protected override void Move()
        {
            // Bossの移動処理はTrackPlayer()で実装済み
            // このメソッドは空のままにするか、必要に応じて追加処理を実装
        }
        
        /// <summary>
        /// 攻撃処理（拽象メソッドの実装）
        /// </summary>
        protected override void Attack()
        {
            // Bossの攻撃処理はTryAttack()で実装済み
            // このメソッドは空のままにするか、必要に応じて追加処理を実装
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