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
        [Tooltip("Boss専用の全データ（HP、速度、攻撃パターンなど）")]
        [SerializeField] private BossAttackDataSO bossAttackData;
        [SerializeField] private BossAIController aiController;
        
        [Header("攻撃パターン設定")]
        [Tooltip("近接回転攻撃用のAttackData")]
        [SerializeField] private AttackData rotatingAttackData;
        [SerializeField] private GameObject fireballPrefab;
        [SerializeField] private Transform fireballSpawnPoint;
        
        // Note: EnemyDataSOとenemyAttackDataはEnemyBaseから継承されますが、Bossでは使用しません（BossAttackDataSOを使用）
        
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
            // Boss専用の攻撃データを使用するため、enemyAttackDataは基本nullだが
            // EnemyColliderからの接触ダメージのため、完全にnullにはしない
            base.Awake();
            
            if (aiController == null)
                aiController = GetComponent<BossAIController>();
            

            
            if (bossAttackData == null)
            {
                Debug.LogError("[BossEnemy] BossAttackDataSOがアタッチされていません！Inspectorで設定してください。");
            }
        }
        
        private void Start()
        {
            canMove = true;
            isDead = false;
            currentState = EnemyState.Move;
            canTakeDamage = true;
            
            if (enemyData != null)
            {
                currentHP = enemyData.MaxHP;
                // EnemyDataSOにはSizeScaleがないので、2倍固定
                transform.localScale = Vector3.one * 2f;

            }
            else
            {
                currentHP = 100;
                transform.localScale = Vector3.one * 2f;
                Debug.LogWarning("[BossEnemy] EnemyDataがnull、デフォルト値を使用");
            }
            
            // Rigidbody2Dの状態確認
            if (rb != null)
            {

            }
            else
            {
                Debug.LogError("[BossEnemy] Rigidbody2Dがnull！");
            }
        }
        
        protected override void Update()
        {
            if (isDead || !canMove)
            {
                return;
            }
            
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
                #region 攻撃処理（オーバーライド）
        
        /// <summary>
        /// プレイヤーに攻撃を実行（Boss専用実装）
        /// BossはBossAttackDataSOを使用するため、enemyAttackDataは使わず直接ダメージ
        /// </summary>
        public override void AttackPlayer()
        {
            // 基底クラスとの互換性のため残す（EnemyColliderからはAttackPlayer(GameObject)が呼ばれる）
        }
        
        /// <summary>
        /// プレイヤーに攻撃を実行（Collision経由、疎結合）
        /// </summary>
        public void AttackPlayer(GameObject playerObject)
        {
            if (IsInLaunchState()) return;
            if (playerObject == null) return;
            
            // IDamageableインターフェースを使って疎結合にダメージを与える
            var damageable = playerObject.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(20);
            }
        }
        
        #endregion
                #region 移動
        
        protected override void Move()
        {
            if (PlayerDataProvider.I == null || rb == null)
            {
                return;
            }
            
            Vector3 targetPosition = PlayerDataProvider.I.Position;
            Vector3 currentPosition = transform.position;
            Vector3 direction = (targetPosition - currentPosition).normalized;
            
            float speed = enemyData != null ? enemyData.MoveSpeed : 3f;
            
            if (Mathf.Abs(direction.x) > MIN_MOVE_THRESHOLD)
            {
                Vector2 newVelocity = new Vector2(
                    direction.x * speed * BOSS_SPEED_MULTIPLIER,
                    direction.y * speed * BOSS_SPEED_MULTIPLIER * VERTICAL_MOVE_MULTIPLIER
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
                    direction.y * speed * BOSS_SPEED_MULTIPLIER * DOWNWARD_MOVE_MULTIPLIER
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
                return;
            }
            
            if (aiController == null)
            {
                return;
            }
            
            if (!aiController.CanAttack())
            {
                return;
            }
            
            if (PlayerDataProvider.I == null)
            {
                return;
            }
            
            BossAttackPattern selectedPattern = aiController.SelectAttackPattern();
            StartCoroutine(ExecuteAttackPattern(selectedPattern));
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
            
            if (aiController != null)
                aiController.OnAttackExecuted();
            
            isAttacking = false;
        }
        
        private IEnumerator ExecuteRotatingAttack()
        {

            Vector3 targetPosition = PlayerDataProvider.I.Position;
            Vector2 direction = (targetPosition - transform.position).normalized;
            
            if (AttackExecutor.I != null && rotatingAttackData != null)
            {

                AttackExecutor.I.Execute(
                    rotatingAttackData,
                    transform.position,
                    direction,
                    gameObject
                );
            }
            else
            {
                Debug.LogWarning($"[BossEnemy] ExecuteRotatingAttack: 攻撃不可 - AttackExecutor={AttackExecutor.I != null}, rotatingAttackData={rotatingAttackData != null}");
            }
            
            yield return new WaitForSeconds(ROTATION_ATTACK_COOLDOWN);
        }
        
        /// <summary>
        /// 地面叩きつけ攻撃（16方向範囲攻撃）
        /// </summary>
        private IEnumerator ExecuteSpawnMinions()
        {
            if (bossAttackData == null || bossAttackData.groundSlam == null)
            {
                yield break;
            }
            
            if (AttackExecutor.I == null || rotatingAttackData == null)
            {
                yield break;
            }
            
            var slamData = bossAttackData.groundSlam;
            
            // 16方向に攻撃判定を生成
            int directionCount = 16;
            float angleStep = 360f / directionCount;
            
            for (int i = 0; i < directionCount; i++)
            {
                float angle = i * angleStep;
                Vector2 direction = new Vector2(
                    Mathf.Cos(angle * Mathf.Deg2Rad),
                    Mathf.Sin(angle * Mathf.Deg2Rad)
                );
                
                AttackExecutor.I.Execute(
                    rotatingAttackData,
                    transform.position,
                    direction,
                    gameObject
                );
            }
            
            yield return new WaitForSeconds(slamData.animationDuration);
        }
        
        private IEnumerator ExecuteFireballBarrage()
        {
            if (bossAttackData == null || bossAttackData.fireBall == null)
            {
                yield break;
            }
            
            var fireBallData = bossAttackData.fireBall;
            Vector3 targetPosition = PlayerDataProvider.I?.Position ?? Vector3.zero;
            
            if (fireballPrefab != null && FireBallFactory.I != null)
            {
                for (int i = 0; i < fireBallData.projectileCount; i++)
                {
                    Vector3 spawnPos = fireballSpawnPoint != null ? fireballSpawnPoint.position : transform.position;
                    Vector2 direction = (targetPosition - spawnPos).normalized;
                    
                    float spreadAngle = (i - fireBallData.projectileCount / 2f) * 15f;
                    direction = Quaternion.Euler(0, 0, spreadAngle) * direction;
                    
                    // BossAttackDataSOで設定されたダメージ値を使用
                    int damage = Mathf.FloorToInt(fireBallData.damage);
                    GameObject fireball = FireBallFactory.I.CreateFireBall(spawnPos, direction, gameObject, damage);
                    
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
            return Mathf.FloorToInt(enemyData?.MaxHP ?? 100f);
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