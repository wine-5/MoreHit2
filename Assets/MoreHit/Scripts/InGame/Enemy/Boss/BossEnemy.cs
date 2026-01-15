using UnityEngine;
using System.Collections;
using MoreHit.Attack;
using MoreHit.Events;
using MoreHit.Player;

namespace MoreHit.Enemy
{
    #region 列挙型
    
    /// <summary>
    /// ボスの攻撃パターン
    /// </summary>
    public enum BossAttackPattern
    {
        RotatingAttack,
        SpawnMinions,
        FireballBarrage
    }
    
    #endregion

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
        [SerializeField] private BossAnimatorController bossAnimatorController;
        
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
        private bool isInputLocked = false;
        
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
            
            if (bossAnimatorController == null)
                bossAnimatorController = GetComponent<BossAnimatorController>();
            
            // InputLockイベント購読
            GameEvents.OnInputLockChanged += SetInputLock;
            
            if (bossAttackData == null)
                Debug.LogError("[BossEnemy] BossAttackDataSOがアタッチされていません！Inspectorで設定してください。");
        }
        
        private void Start()
        {
            // Bossは演出が終わるまで動かない
            canMove = false;
            isDead = false;
            currentState = EnemyState.Move;
            canTakeDamage = true;
            
            if (enemyData == null)
            {
                Debug.LogError("[BossEnemy] EnemyDataがnull！BossAttackDataSOとは別にEnemyDataSOが必要です。");
                currentHP = 100;
                transform.localScale = Vector3.one * 2f;
            }
            else
            {
                currentHP = enemyData.MaxHP;
                // EnemyDataSOにはSizeScaleがないので、2倍固定
                transform.localScale = Vector3.one * 2f;
            }
            
            if (rb == null)
                Debug.LogError("[BossEnemy] Rigidbody2Dがnull！");
        }
        
        protected override void Update()
        {
            if (isDead || !canMove || isInputLocked)
                return;
            
            UpdateStockTimer();
            Move();
            Attack();
        }
        
        private void OnDestroy()
        {
            GameEvents.OnInputLockChanged -= SetInputLock;
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
            if (isDead || !gameObject.activeInHierarchy)
                return;
            
            // エフェクト生成（基底クラスの処理を実行）
            base.OnStockReachedRequired();
            
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
        
        public override void AttackPlayer()
        {
            // 基底クラスとの互換性のため残す（EnemyColliderからはAttackPlayer(GameObject)が呼ばれる）
        }
        
        public void AttackPlayer(GameObject playerObject)
        {
            if (IsInLaunchState())
                return;
            if (playerObject == null)
                return;
            
            // IDamageableインターフェースを使って疎結合にダメージを与える
            var damageable = playerObject.GetComponent<IDamageable>();
            if (damageable != null)
                damageable.TakeDamage(20);
        }
        
        #endregion
                #region 移動
        
        protected override void Move()
        {
            if (PlayerDataProvider.I == null || rb == null)
                return;
            
            Vector3 targetPosition = PlayerDataProvider.I.Position;
            Vector3 currentPosition = transform.position;
            Vector3 direction = (targetPosition - currentPosition).normalized;
            
            // HP比率に応じて速度を変更
            float speed = enemyData != null ? enemyData.MoveSpeed : 3f;
            float hpRatio = GetHPRatio();
            
            if (bossAttackData != null)
            {
                if (hpRatio <= bossAttackData.hpThreshold25)
                    speed = bossAttackData.moveSpeed25;
                else if (hpRatio <= bossAttackData.hpThreshold50)
                    speed = bossAttackData.moveSpeed50;
            }
            
            float speedMultiplier = BOSS_SPEED_MULTIPLIER;
            
            if (Mathf.Abs(direction.x) > MIN_MOVE_THRESHOLD)
            {
                Vector2 newVelocity = new Vector2(
                    direction.x * speed * speedMultiplier,
                    direction.y * speed * speedMultiplier * VERTICAL_MOVE_MULTIPLIER
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
                    direction.y * speed * speedMultiplier * DOWNWARD_MOVE_MULTIPLIER
                );
                rb.linearVelocity = newVelocity;
            }
        }
        
        #endregion
        
        #region 攻撃システム
        
        protected override void Attack()
        {
            if (isAttacking || bossAttackData == null)
                return;
            
            if (PlayerDataProvider.I == null)
                return;
            
            // クールダウンチェック
            if (Time.time - lastAttackTime < bossAttackData.baseAttackCooldown)
                return;
            
            StartCoroutine(ExecuteFireballBarrage());
        }
        

        
        private IEnumerator ExecuteFireballBarrage()
        {
            isAttacking = true;
            
            // 攻撃アニメーション開始
            if (bossAnimatorController != null)
                bossAnimatorController.PlayAttack();
            
            if (bossAttackData == null || bossAttackData.fireBall == null)
            {
                isAttacking = false;
                if (bossAnimatorController != null)
                    bossAnimatorController.StopAttack();
                yield break;
            }
            
            var fireBallData = bossAttackData.fireBall;
            Vector3 targetPosition = PlayerDataProvider.I?.Position ?? Vector3.zero;
            
            // HP比率に応じて弾の数を変更
            float hpRatio = GetHPRatio();
            int projectileMultiplier = 1;
            
            if (hpRatio <= bossAttackData.hpThreshold25)
                projectileMultiplier = Mathf.RoundToInt(bossAttackData.projectileMultiplier25);
            else if (hpRatio <= bossAttackData.hpThreshold50)
                projectileMultiplier = Mathf.RoundToInt(bossAttackData.projectileMultiplier50);
            
            int totalProjectiles = fireBallData.projectileCount * projectileMultiplier;
            
            if (fireballPrefab != null && FireBallFactory.I != null)
            {
                // 扇状の弾を発射
                for (int i = 0; i < totalProjectiles; i++)
                {
                    Vector3 spawnPos = fireballSpawnPoint != null ? fireballSpawnPoint.position : transform.position;
                    // プレイヤーの方向を基準に計算
                    Vector2 baseDirection = (targetPosition - spawnPos).normalized;
                    
                    // 扇状に広げる（FireDataのspreadAngleを使用）
                    float angleOffset = (i - (totalProjectiles - 1) / 2f) * fireBallData.spreadAngle;
                    Vector2 direction = Quaternion.Euler(0, 0, angleOffset) * baseDirection;
                    
                    // BossAttackDataSOで設定されたダメージ値を使用
                    int damage = Mathf.FloorToInt(fireBallData.damage);
                    GameObject fireball = FireBallFactory.I.CreateFireBall(spawnPos, direction, gameObject, damage);
                    
                    yield return new WaitForSeconds(fireBallData.projectileInterval);
                }
                
                // HP50%以下の場合、プレイヤーに直接向かう弾を追加
                if (hpRatio <= bossAttackData.hpThreshold50)
                {
                    int directShotCount = hpRatio <= bossAttackData.hpThreshold25 
                        ? bossAttackData.directShotCount25 
                        : bossAttackData.directShotCount50;
                    
                    for (int i = 0; i < directShotCount; i++)
                    {
                        // プレイヤーの最新位置を取得
                        targetPosition = PlayerDataProvider.I?.Position ?? Vector3.zero;
                        Vector3 spawnPos = fireballSpawnPoint != null ? fireballSpawnPoint.position : transform.position;
                        Vector2 direction = (targetPosition - spawnPos).normalized;
                        
                        int damage = Mathf.FloorToInt(fireBallData.damage);
                        GameObject fireball = FireBallFactory.I.CreateFireBall(spawnPos, direction, gameObject, damage);
                        
                        yield return new WaitForSeconds(fireBallData.projectileInterval * bossAttackData.directShotIntervalMultiplier);
                    }
                }
            }
            
            lastAttackTime = Time.time;
            
            // 攻撃アニメーション終了
            if (bossAnimatorController != null)
                bossAnimatorController.StopAttack();
            
            isAttacking = false;
        }
        
        /// <summary>
        /// FireBallの方向をプレイヤー位置に向けて継続的に更新
        /// </summary>
        private IEnumerator UpdateFireballDirection(GameObject fireball, Transform playerTransform)
        {
            if (fireball == null || playerTransform == null)
                yield break;
            
            Rigidbody2D rb = fireball.GetComponent<Rigidbody2D>();
            if (rb == null)
                yield break;
            
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
            if (isDead)
                return;
            
            currentHP = Mathf.Max(0, currentHP - damage);
            
            if (currentHP <= 0)
            {
                Die();
                return;
            }
        }

        public override void Die()
        {
            if (isDead)
                return;
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
        
        public int GetMaxHP() => Mathf.FloorToInt(enemyData?.MaxHP ?? 100f);
        
        public int GetCurrentHP() => Mathf.FloorToInt(currentHP);
        
        #endregion
        
        #region 入力制御
        
        private void SetInputLock(bool isLocked) => isInputLocked = isLocked;
        
        public void SetCanMove(bool canMove) => this.canMove = canMove;
        
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