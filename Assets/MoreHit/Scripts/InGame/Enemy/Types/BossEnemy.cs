using UnityEngine;
using System.Collections;
using MoreHit.Attack;
using MoreHit.Events;
using MoreHit.Player;

namespace MoreHit.Enemy
{
    /// <summary>
    /// ボスの攻撃パターン
    /// </summary>
    public enum BossAttackPattern
    {
        RotatingAttack,  // くるくる回転攻撃
        SpawnMinions,    // 雑魚敵生成
        FireballBarrage  // ファイヤーボール連射
    }

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
        
        [Header("攻撃パターン設定")]
        [SerializeField] private AttackData fireballAttackData; // ファイヤーボール用の攻撃データ
        [SerializeField] private int fireballCount = 3; // ファイヤーボールの発射数
        [SerializeField] private float fireballInterval = 0.5f; // ファイヤーボール間隔
        [SerializeField] private int minionSpawnCount = 2; // 生成する雑魚敵の数
        
        private float lastAttackTime = 0f;
        private bool canTakeDamage = false; // ストック満タン時のみtrue
        private bool isAttacking = false; // 攻撃実行中フラグ
        
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
        /// テスト用：Startでボスを強制的に動作状態にする
        /// </summary>
        private void Start()
        {
            Debug.Log("[BossEnemy] Start開始 - テスト用の強制動作設定");
            
            // 強制的に動作可能状態にする
            canMove = true;
            isDead = false;
            currentState = EnemyState.Move;
            
            // HPを設定
            currentHP = maxHP;
            
            // ボス出現イベントを発火（テスト用）
            Debug.Log("[BossEnemy] テスト用ボス出現イベント発火");
            GameEvents.TriggerBossAppear();
            
            Debug.Log($"[BossEnemy] Start完了 - canMove: {canMove}, isDead: {isDead}, currentState: {currentState}, HP: {currentHP}/{maxHP}");
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
            if (isDead || !canMove) return;
            
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
            Vector3 currentPosition = transform.position;
            Vector3 direction = (targetPosition - currentPosition).normalized;
            
            // X方向のみ移動（2Dゲーム用）
            if (Mathf.Abs(direction.x) > 0.1f)
            {
                Vector2 newVelocity = new Vector2(direction.x * enemyData.MoveSpeed, rb.linearVelocity.y);
                rb.linearVelocity = newVelocity;
                
                // スプライトの向きを調整
                if (spriteRenderer != null)
                {
                    spriteRenderer.flipX = direction.x < 0;
                }
                else
                {
                    Debug.LogWarning("[BossEnemy] spriteRendererがnullです");
                }
            }
        }

        /// <summary>
        /// ボス敵の攻撃処理（ランダムパターン選択）
        /// </summary>
        protected override void Attack()
        {
            if (Time.time - lastAttackTime < attackCooldown) return;
            if (isAttacking) return; // 攻撃実行中は新しい攻撃をしない
            
            if (PlayerDataProvider.I == null)
            {
                Debug.LogWarning("[BossEnemy] PlayerDataProvider.Iがnullで攻撃できません");
                return;
            }
            
            float distanceToPlayer = Vector3.Distance(transform.position, PlayerDataProvider.I.Position);
            
            if (distanceToPlayer <= attackRange)
            {
                // ランダムに攻撃パターンを選択
                BossAttackPattern selectedPattern = (BossAttackPattern)Random.Range(0, System.Enum.GetValues(typeof(BossAttackPattern)).Length);
                Debug.Log($"[BossEnemy] 攻撃パターン選択: {selectedPattern}");
                
                StartCoroutine(ExecuteAttackPattern(selectedPattern));
                lastAttackTime = Time.time;
            }
        }
        
        /// <summary>
        /// 選択された攻撃パターンを実行
        /// </summary>
        private IEnumerator ExecuteAttackPattern(BossAttackPattern pattern)
        {
            isAttacking = true;
            
            switch (pattern)
            {
                case BossAttackPattern.RotatingAttack:
                    yield return StartCoroutine(ExecuteRotatingAttack());
                    break;
                    
                case BossAttackPattern.SpawnMinions:
                    yield return StartCoroutine(ExecuteSpawnMinions());
                    break;
                    
                case BossAttackPattern.FireballBarrage:
                    yield return StartCoroutine(ExecuteFireballBarrage());
                    break;
            }
            
            isAttacking = false;
        }
        
        /// <summary>
        /// パターン1: くるくる回転攻撃（位置固定版）
        /// </summary>
        private IEnumerator ExecuteRotatingAttack()
        {
            Debug.Log("[BossEnemy] 回転攻撃実行");
            
            // プレイヤーの現在位置を一回だけ取得（固定ターゲット）
            Vector3 targetPosition = PlayerDataProvider.I.Position;
            Vector2 direction = (targetPosition - transform.position).normalized;
            
            Debug.Log($"[BossEnemy] 固定ターゲット位置: {targetPosition}, 方向: {direction}");
            
            if (AttackExecutor.I != null && enemyAttackData != null)
            {
                AttackExecutor.I.Execute(
                    enemyAttackData,
                    transform.position,
                    direction, // 固定された方向
                    gameObject
                );
            }
            
            yield return new WaitForSeconds(1f); // 攻撃のクールダウン
        }
        
        /// <summary>
        /// パターン2: 雑魚敵生成
        /// </summary>
        private IEnumerator ExecuteSpawnMinions()
        {
            Debug.Log("[BossEnemy] 雑魚敵生成攻撃実行");
            
            // TODO: EnemyFactoryとPoolの連携実装
            // 現在は仮実装
            for (int i = 0; i < minionSpawnCount; i++)
            {
                Debug.Log($"[BossEnemy] 雑魚敵 {i + 1} 生成");
                // ここでEnemyFactory.I.SpawnEnemy()などを呼び出す予定
                yield return new WaitForSeconds(0.5f);
            }
        }
        
        /// <summary>
        /// パターン3: ファイヤーボール連射
        /// </summary>
        private IEnumerator ExecuteFireballBarrage()
        {
            Debug.Log("[BossEnemy] ファイヤーボール攻撃実行");
            
            if (fireballAttackData == null)
            {
                Debug.LogWarning("[BossEnemy] fireballAttackDataがnullです");
                yield break;
            }
            
            for (int i = 0; i < fireballCount; i++)
            {
                // 各ファイヤーボールごとにプレイヤーの現在位置に向ける
                Vector3 currentTargetPosition = PlayerDataProvider.I.Position;
                Vector2 fireballDirection = (currentTargetPosition - transform.position).normalized;
                
                Debug.Log($"[BossEnemy] ファイヤーボール {i + 1} 発射 - 方向: {fireballDirection}");
                
                if (AttackExecutor.I != null)
                {
                    AttackExecutor.I.Execute(
                        fireballAttackData,
                        transform.position,
                        fireballDirection,
                        gameObject
                    );
                }
                
                yield return new WaitForSeconds(fireballInterval);
            }
        }

        /// <summary>
        /// ボス専用ダメージ処理
        /// 通常はストックのみ増加、ストック満タンで自動的にHPが減る
        /// </summary>
        public override void TakeDamage(int damage)
        {
            Debug.Log($"[BossEnemy] TakeDamage呼び出し - damage: {damage}, isDead: {isDead}, currentState: {currentState}");
            
            if (isDead) 
            {
                Debug.Log("[BossEnemy] 死亡状態のためダメージ処理をスキップ");
                return;
            }
            
            // 通常の攻撃ではストックのみ増加
            // ストックが満タンになったら自動的にOnStockReachedRequiredが呼ばれてHPが減る
            Debug.Log($"[BossEnemy] 通常状態でダメージ受信 - ストック追加: +1 (現在: {currentStockCount})");
            AddStock(1); // 攻撃を受ける度にストック+1
        }
        
        /// <summary>
        /// ストック満タン時の処理をオーバーライド
        /// </summary>
        protected override void OnStockReachedRequired()
        {
            Debug.Log("[BossEnemy] ストック満タン！HP減少処理開始");
            
            base.OnStockReachedRequired();
            
            // Boss専用：ダメージを受けられる状態に
            canTakeDamage = true;
            
            // ストック満タン時に自動的にHPを25減らす
            const int autoDamage = 25;
            currentHP -= autoDamage;
            
            Debug.Log($"[BossEnemy] ストック満タンによるHP減少 - -{autoDamage}, 残りHP: {currentHP}/{maxHP}");
            
            // ダメージエフェクト発火
            GameEvents.TriggerEnemyDamaged(gameObject, autoDamage);
            
            // ストッククリア
            ClearStock();
            canTakeDamage = false;
            currentState = EnemyState.Move;
            
            Debug.Log($"[BossEnemy] ストック処理完了 - HP: {currentHP}, ストック: {currentStockCount}, 状態: {currentState}");
            
            // 死亡判定
            if (currentHP <= 0)
            {
                Debug.Log("[BossEnemy] HP0以下で死亡処理実行");
                Die();
            }
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
