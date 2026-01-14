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
        private const float FIREBALL_TRACKING_DURATION = 2f;
        private const float VERTICAL_MOVE_MULTIPLIER = 0.5f;
        private const float GRAVITY_ADDITION_MULTIPLIER = 0.3f;
        private const float HORIZONTAL_DECAY_MULTIPLIER = 0.8f;
        private const float DOWNWARD_MOVE_MULTIPLIER = 0.7f;
        
        #endregion
        
        #region シリアライズフィールド
        
        [Header("ボス専用設定")]
        [SerializeField] private BossAttackDataSO bossAttackData;
        [SerializeField] private BossAIController aiController;
        
        [Header("攻撃パターン設定")]
        [SerializeField] private AttackData rotatingAttackData;
        [SerializeField] private GameObject fireballPrefab;
        [SerializeField] private Transform fireballSpawnPoint;
        
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
            
            if (aiController == null)
                aiController = GetComponent<BossAIController>();
            
            Debug.Log($"[BossEnemy] Awake: aiController={aiController != null}, bossAttackData={bossAttackData != null}, enemyAttackData={enemyAttackData != null}");
            
            if (bossAttackData == null)
                Debug.LogError("[BossEnemy] BossAttackDataSOがアタッチされていません！Inspectorで設定してください。");
            
            if (enemyAttackData == null)
                Debug.LogError("[BossEnemy] enemyAttackDataがアタッチされていません！Inspectorで設定してください。");
        }
        
        private void Start()
        {
            canMove = true;
            isDead = false;
            currentState = EnemyState.Move;
            canTakeDamage = true;
            
            if (enemyData != null)
                currentHP = enemyData.MaxHP;
            else
                currentHP = 300;
            
            Debug.Log($"[BossEnemy] Start: HP={currentHP}, canMove={canMove}, state={currentState}");
        }
        
        protected override void Update()
        {
            if (isDead || !canMove)
            {
                if (Time.frameCount % 300 == 0)
                    Debug.Log($"[BossEnemy] Update: 停止中 - isDead={isDead}, canMove={canMove}");
                return;
            }
            
            if (Time.frameCount % 300 == 0)
                Debug.Log($"[BossEnemy] Update: 動作中 - HP={currentHP}, state={currentState}");
            
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
                currentHP = GetMaxHP();
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
            if (isDead || !gameObject.activeInHierarchy) return;
            
            currentHP = Mathf.Max(0, currentHP - AUTO_DAMAGE);
            GameEvents.TriggerBossDamaged(Mathf.FloorToInt(AUTO_DAMAGE));
            ClearStock();
            
            currentState = EnemyState.Move;
            canMove = true;
            canTakeDamage = true;
            
            if (currentHP <= 0)
                Die();
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
                Vector2 newVelocity = new Vector2(
                    direction.x * enemyData.MoveSpeed * BOSS_SPEED_MULTIPLIER,
                    direction.y * enemyData.MoveSpeed * BOSS_SPEED_MULTIPLIER * VERTICAL_MOVE_MULTIPLIER
                );
                
                if (direction.y < 0)
                    newVelocity.y += rb.gravityScale * Physics2D.gravity.y * GRAVITY_ADDITION_MULTIPLIER;
                
                rb.linearVelocity = newVelocity;
                
                if (spriteRenderer != null)
                    spriteRenderer.flipX = direction.x < 0;
            }
            else if (direction.y < -MIN_MOVE_THRESHOLD)
            {
                Vector2 newVelocity = new Vector2(
                    rb.linearVelocity.x * HORIZONTAL_DECAY_MULTIPLIER,
                    direction.y * enemyData.MoveSpeed * BOSS_SPEED_MULTIPLIER * DOWNWARD_MOVE_MULTIPLIER
                );
                rb.linearVelocity = newVelocity;
            }
        }
        
        #endregion
        
        #region 攻撃システム
        
        protected override void Attack()
        {
            if (isAttacking)
            {
                if (Time.frameCount % 100 == 0)
                    Debug.Log("[BossEnemy] Attack: 攻撃中のためスキップ");
                return;
            }
            
            if (aiController == null)
            {
                Debug.LogWarning("[BossEnemy] Attack: aiControllerがnull");
                return;
            }
            
            if (!aiController.CanAttack())
            {
                if (Time.frameCount % 100 == 0)
                    Debug.Log("[BossEnemy] Attack: クールダウン中");
                return;
            }
            
            if (PlayerDataProvider.I == null)
            {
                Debug.LogWarning("[BossEnemy] Attack: PlayerDataProviderがnull");
                return;
            }
            
            BossAttackPattern selectedPattern = aiController.SelectAttackPattern();
            Debug.Log($"[BossEnemy] Attack: 攻撃開始 - pattern={selectedPattern}");
            StartCoroutine(ExecuteAttackPattern(selectedPattern));
        }
        
        private IEnumerator ExecuteAttackPattern(BossAttackPattern pattern)
        {
            isAttacking = true;
            Debug.Log($"[BossEnemy] ExecuteAttackPattern: 開始 - {pattern}");
            
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
            
            if (aiController != null)
                aiController.OnAttackExecuted();
            
            Debug.Log($"[BossEnemy] ExecuteAttackPattern: 完了 - {pattern}");
            isAttacking = false;
        }
        
        private IEnumerator ExecuteRotatingAttack()
        {
            Debug.Log("[BossEnemy] ExecuteRotatingAttack: 開始");
            Vector3 targetPosition = PlayerDataProvider.I.Position;
            Vector2 direction = (targetPosition - transform.position).normalized;
            
            if (AttackExecutor.I != null && enemyAttackData != null)
            {
                Debug.Log($"[BossEnemy] ExecuteRotatingAttack: 攻撃実行 - direction={direction}");
                AttackExecutor.I.Execute(
                    enemyAttackData,
                    transform.position,
                    direction,
                    gameObject
                );
            }
            else
            {
                Debug.LogWarning($"[BossEnemy] ExecuteRotatingAttack: 攻撃不可 - AttackExecutor={AttackExecutor.I != null}, enemyAttackData={enemyAttackData != null}");
            }
            
            yield return new WaitForSeconds(ROTATION_ATTACK_COOLDOWN);
        }
        
        private IEnumerator ExecuteSpawnMinions()
        {
            Debug.Log("[BossEnemy] ExecuteSpawnMinions: 開始");
            
            if (EnemyFactory.I == null)
            {
                Debug.LogWarning("[BossEnemy] ExecuteSpawnMinions: EnemyFactoryがnull");
                yield break;
            }
            
            int spawnCount = bossAttackData?.groundSlam.projectileCount ?? 2;
            Vector3 playerPosition = PlayerDataProvider.I?.Position ?? Vector3.zero;
            
            Debug.Log($"[BossEnemy] ExecuteSpawnMinions: {spawnCount}体のミニオンを生成");
            
            for (int i = 0; i < spawnCount; i++)
            {
                Vector2 directionToPlayer = (playerPosition - transform.position).normalized;
                Vector3 spawnPosition = transform.position + (Vector3)(directionToPlayer * 2f);
                
                if (i > 0)
                {
                    Vector3 offset = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f);
                    spawnPosition += offset;
                }
                
                EnemyBase minion = EnemyFactory.I.CreateEnemyByType(EnemyType.Normal, spawnPosition);
                
                if (minion != null)
                {
                    Rigidbody2D minionRb = minion.GetComponent<Rigidbody2D>();
                    if (minionRb != null)
                    {
                        minionRb.gravityScale = 1f;
                        minionRb.linearVelocity = Vector2.zero;
                    }
                }
                
                yield return new WaitForSeconds(MINION_SPAWN_INTERVAL);
            }
        }
        
        private IEnumerator ExecuteFireballBarrage()
        {
            if (bossAttackData == null || bossAttackData.fireBall == null)
            {
                Debug.LogWarning("[BossEnemy] ExecuteFireballBarrage: bossAttackDataまたはfireBallがnull");
                yield break;
            }
            
            Debug.Log("[BossEnemy] ExecuteFireballBarrage: 開始");
            var fireBallData = bossAttackData.fireBall;
            Vector3 targetPosition = PlayerDataProvider.I?.Position ?? Vector3.zero;
            
            if (fireballPrefab != null && FireBallFactory.I != null)
            {
                Debug.Log($"[BossEnemy] ExecuteFireballBarrage: {fireBallData.projectileCount}個の火球を発射");
                for (int i = 0; i < fireBallData.projectileCount; i++)
                {
                    Vector3 spawnPos = fireballSpawnPoint != null ? fireballSpawnPoint.position : transform.position;
                    Vector2 direction = (targetPosition - spawnPos).normalized;
                    
                    float spreadAngle = (i - fireBallData.projectileCount / 2f) * 15f;
                    direction = Quaternion.Euler(0, 0, spreadAngle) * direction;
                    
                    FireBallFactory.I.CreateFireBall(spawnPos, direction, gameObject);
                    yield return new WaitForSeconds(fireBallData.projectileInterval);
                }
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
            if (isDead) return;
            
            currentHP = Mathf.Max(0, currentHP - damage);
            
            if (currentHP <= 0)
            {
                Die();
                return;
            }
        }
        
        public override void AttackPlayer()
        {
            if (enemyAttackData == null) return;
            
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
            int maxHP = GetMaxHP();
            return maxHP > 0 ? (float)currentHP / maxHP : 0f;
        }
        
        public int GetMaxHP()
        {
            return Mathf.FloorToInt(enemyData?.MaxHP ?? 300f);
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
            if (bossAttackData != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, bossAttackData.meleeAttackRange);
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, bossAttackData.rangedAttackRange);
            }
        }
#endif
        
        #endregion
    }
}