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
        RotatingAttack,
        SpawnMinions,
        FireballBarrage
    }

    /// <summary>
    /// ボス敵の実装クラス
    /// ストックを上限まで溜めて攻撃することで初めてHPを減らせる特殊な敵
    /// </summary>
    public class BossEnemy : EnemyBase
    {
        #region 定数
        
        private const float AUTO_DAMAGE = 25f;
        private const float ROTATION_ATTACK_COOLDOWN = 1f;
        private const float MINION_SPAWN_INTERVAL = 0.5f;
        private const float MIN_MOVE_THRESHOLD = 0.1f;
        private const float BOSS_SPEED_MULTIPLIER = 0.3f;
        private const float FIREBALL_DEFAULT_SPEED = 10f;
        private const float FIREBALL_TRACKING_DURATION = 2f; // FireBallの追従時間
        
        #endregion
        
        #region シリアライズフィールド
        
        [Header("ボス専用設定")]
        [SerializeField] private float attackCooldown = 0.5f; // 攻撃間隔を大幅に短縮
        [SerializeField] private float attackRange = 5f;
        // maxHPはEnemyDataから取得するため削除
        
        [Header("攻撃パターン設定")]
        [SerializeField] private AttackData fireballAttackData;
        [SerializeField] private GameObject fireballPrefab;
        [SerializeField] private Transform fireballSpawnPoint;
        [SerializeField] private int fireballCount = 3;
        [SerializeField] private float fireballInterval = 0.5f;
        [SerializeField] private int minionSpawnCount = 2;
        
        #endregion
        
        #region プライベートフィールド
        
        private float lastAttackTime = 0f;
        private bool canTakeDamage = false;
        private bool isAttacking = false;
        
        #endregion
        
        #region Unityライフサイクル
        
        protected override void Awake()
        {
            enemyType = EnemyType.Boss;
            base.Awake();
        }
        
        private void Start()
        {
            canMove = true;
            isDead = false;
            currentState = EnemyState.Move;
            
            // EnemyDataからHPを取得（最優先）
            if (enemyData != null)
            {
                currentHP = enemyData.MaxHP; // 正しいプロパティ名はMaxHP
            }
            else
            {
                Debug.LogError("[BossEnemy] enemyDataがnullです！HPをデフォルト値に設定します。");
                currentHP = 300; // フォールバック値
            }
            
            // ボス出現イベントを発火
            GameEvents.TriggerBossAppear();
        }
        
        protected override void Update()
        {
            if (isDead || !canMove) return;
            
            UpdateStockTimer();
            Move();
            Attack();
        }
        
        #endregion
        
        #region 初期化
        
        protected override void InitializeEnemy()
        {
            base.InitializeEnemy();
            
            if (enemyData != null)
            {
                currentHP = GetMaxHP(); // EnemyDataから取得
                GameEvents.TriggerBossAppear();
            }
            else
            {
                Debug.LogError("[BossEnemy] enemyDataがnullです!");
            }
        }
        
        #endregion
        
        #region ストック管理
        
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
        
        protected override void OnStockReachedRequired()
        {
            base.OnStockReachedRequired();
            
            canTakeDamage = true;
            
            float previousHP = currentHP;
            currentHP = Mathf.Max(0, currentHP - AUTO_DAMAGE);
            
            Debug.Log($"[BossEnemy] ストック満タンでHP減少: {previousHP} -> {currentHP}, イベント発火");
            GameEvents.TriggerBossDamaged(Mathf.FloorToInt(AUTO_DAMAGE));
            
            ClearStock();
            canTakeDamage = false;
            currentState = EnemyState.Move;
            
            if (currentHP <= 0) Die();
        }
        
        #endregion
        
        #region 移動
        
        protected override void Move()
        {
            if (PlayerDataProvider.I == null || enemyData == null || rb == null) return;
            
            Vector3 targetPosition = PlayerDataProvider.I.Position;
            Vector3 currentPosition = transform.position;
            Vector3 direction = (targetPosition - currentPosition).normalized;
            
            if (Mathf.Abs(direction.x) > MIN_MOVE_THRESHOLD)
            {
                Vector2 newVelocity = new Vector2(direction.x * enemyData.MoveSpeed * BOSS_SPEED_MULTIPLIER, rb.linearVelocity.y);
                rb.linearVelocity = newVelocity;
                
                if (spriteRenderer != null)
                    spriteRenderer.flipX = direction.x < 0;
            }
        }
        
        #endregion
        
        #region 攻撃システム
        
        protected override void Attack()
        {
            if (Time.time - lastAttackTime < attackCooldown) return;
            if (isAttacking) return;
            if (PlayerDataProvider.I == null) return;
            
            float distanceToPlayer = Vector3.Distance(transform.position, PlayerDataProvider.I.Position);
            
            if (distanceToPlayer <= attackRange)
            {
                BossAttackPattern selectedPattern = (BossAttackPattern)Random.Range(0, System.Enum.GetValues(typeof(BossAttackPattern)).Length);
                StartCoroutine(ExecuteAttackPattern(selectedPattern));
                lastAttackTime = Time.time;
            }
        }
        
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
        
        private IEnumerator ExecuteRotatingAttack()
        {
            Vector3 targetPosition = PlayerDataProvider.I.Position;
            Vector2 direction = (targetPosition - transform.position).normalized;
            
            if (AttackExecutor.I != null && enemyAttackData != null)
            {
                AttackExecutor.I.Execute(
                    enemyAttackData,
                    transform.position,
                    direction,
                    gameObject
                );
            }
            
            yield return new WaitForSeconds(ROTATION_ATTACK_COOLDOWN);
        }
        
        private IEnumerator ExecuteSpawnMinions()
        {
            if (EnemyFactory.I == null) yield break;
            
            // プレイヤーの位置を取得
            Vector3 playerPosition = PlayerDataProvider.I?.Position ?? Vector3.zero;
            
            for (int i = 0; i < minionSpawnCount; i++)
            {
                // プレイヤーがいる方向を計算
                Vector2 directionToPlayer = (playerPosition - transform.position).normalized;
                
                // ボスの目の前（プレイヤー方向）に生成位置を設定
                Vector3 spawnPosition = transform.position + (Vector3)(directionToPlayer * 2f); // 2ユニット前
                
                // 複数生成時は少しずつ位置をずらす
                if (i > 0)
                {
                    Vector3 offset = new Vector3(
                        Random.Range(-1f, 1f), 
                        Random.Range(-1f, 1f), 
                        0f
                    );
                    spawnPosition += offset;
                }
                
                EnemyBase minion = EnemyFactory.I.CreateEnemyByType(EnemyType.Normal, spawnPosition);
                
                if (minion != null)
                {
                    Debug.Log($"[BossEnemy] 雑魚敵をボスの目の前に生成: {spawnPosition}");
                    
                    // 通常の敵として動作させる（投げる処理は削除）
                    Rigidbody2D minionRb = minion.GetComponent<Rigidbody2D>();
                    if (minionRb != null)
                    {
                        // 重力を通常値に設定
                        minionRb.gravityScale = 1f;
                        minionRb.linearVelocity = Vector2.zero; // 初期速度なし
                    }
                }
                
                yield return new WaitForSeconds(MINION_SPAWN_INTERVAL);
            }
        }
        
        private IEnumerator ExecuteFireballBarrage()
        {
            // プレイヤーの位置を攻撃開始時に一度だけ取得（この位置を追従ターゲットとして使用）
            Vector3 targetPosition = PlayerDataProvider.I?.Position ?? Vector3.zero;
            Transform playerTransform = PlayerDataProvider.I?.Transform;
            
            // AttackDataが設定されている場合はAttackExecutorを使用（Pool対応）
            if (fireballAttackData != null && AttackExecutor.I != null)
            {
                Debug.Log("[BossEnemy] AttackExecutorを使用してFireBall攻撃（Pool対応）");
                
                for (int i = 0; i < fireballCount; i++)
                {
                    Vector3 spawnPos = fireballSpawnPoint != null ? fireballSpawnPoint.position : transform.position;
                    Vector2 fireballDirection = (targetPosition - spawnPos).normalized;
                    
                    Debug.Log($"[BossEnemy] FireBall AttackData {i + 1} 発射 - 方向: {fireballDirection}");
                    
                    // AttackExecutor経由で攻撃（Poolが自動的に使用される）
                    AttackExecutor.I.Execute(
                        fireballAttackData,
                        spawnPos,
                        fireballDirection,
                        gameObject
                    );
                    
                    yield return new WaitForSeconds(fireballInterval);
                }
            }
            // Prefabが設定されている場合は直接生成（フォールバック）
            else if (fireballPrefab != null)
            {
                Debug.LogWarning("[BossEnemy] fireballAttackDataが未設定のため、Instantiateを使用します（Pool未使用）");
                
                for (int i = 0; i < fireballCount; i++)
                {
                    Vector3 spawnPos = fireballSpawnPoint != null ? fireballSpawnPoint.position : transform.position;
                    
                    GameObject fireball = Instantiate(fireballPrefab, spawnPos, Quaternion.identity);
                    
                    // FireBallに追従機能を追加
                    StartCoroutine(UpdateFireballDirection(fireball, playerTransform));
                    
                    yield return new WaitForSeconds(fireballInterval);
                }
            }
            else
            {
                Debug.LogWarning("[BossEnemy] fireballAttackDataもfireballPrefabも設定されていません");
            }
        }
        
        /// <summary>
        /// FireBallの方向をプレイヤー位置に向けて継続的に更新
        /// </summary>
        private IEnumerator UpdateFireballDirection(GameObject fireball, Transform playerTransform)
        {
            if (fireball == null || playerTransform == null) yield break;
            
            Rigidbody2D rb = fireball.GetComponent<Rigidbody2D>();
            if (rb == null) yield break;
            
            float elapsedTime = 0f;
            
            while (fireball != null && elapsedTime < FIREBALL_TRACKING_DURATION)
            {
                if (playerTransform != null)
                {
                    // プレイヤーの現在位置に向かう方向を計算
                    Vector2 directionToPlayer = (playerTransform.position - fireball.transform.position).normalized;
                    rb.linearVelocity = directionToPlayer * FIREBALL_DEFAULT_SPEED;
                }
                
                elapsedTime += Time.deltaTime;
                yield return new WaitForFixedUpdate();
            }
        }
        
        #endregion
        
        #region ダメージと死亡処理
        
        public override void TakeDamage(int damage)
        {
            if (isDead) 
                return;
            
            // 実際のダメージ処理を追加
            currentHP = Mathf.Max(0, currentHP - damage);
            
            // 死亡判定
            if (currentHP <= 0)
            {
                Die();
                return;
            }
            
            // 注意：ストックの追加はAttackExecutorで既に行われているため、ここでは行わない
            // ストックが満タンになったら自動的にOnStockReachedRequiredが呼ばれてHPが減る
        }
        
        /// <summary>
        /// プレイヤーへの攻撃
        /// </summary>
        public override void AttackPlayer()
        {
            if (enemyAttackData == null)
            {
                Debug.LogError("[BossEnemy] enemyAttackDataが設定されていません！Unityエディタでアタッチしてください。");
                return;
            }
            
            // 基底クラスのAttackPlayer実行
            base.AttackPlayer();
        }

        public override void Die()
        {
            if (isDead) return;
            
            isDead = true;
            canMove = false;
            
            GameEvents.TriggerBossDefeated();
            GameEvents.TriggerEnemyDefeated(gameObject);
            
            gameObject.SetActive(false);
        }
        
        #endregion
        
        #region HP情報取得
        
        public float GetHPRatio()
        {
            int maxHP = GetMaxHP(); // EnemyDataから取得
            return maxHP > 0 ? (float)currentHP / maxHP : 0f;
        }
        
        public int GetMaxHP()
        {
            // EnemyDataを最優先に使用
            return Mathf.FloorToInt(enemyData?.MaxHP ?? 300f); // 正しいプロパティ名はMaxHP
        }
        
        public int GetCurrentHP()
        {
            return Mathf.FloorToInt(currentHP);
        }
        
        #endregion
        
        #region エディタ
        
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
#endif
        
        #endregion
    }
}