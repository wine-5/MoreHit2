using System.Collections;
using UnityEngine;
using MoreHit.Attack;
using MoreHit.Player;
using MoreHit.Events;
using MoreHit.Effect;
using TMPro;

namespace MoreHit.Enemy
{
    public enum EnemyState { Idle, Move, HitStun, ReadyToLaunch, Launch }
    
    /// <summary>
    /// 敵の基底クラス
    /// </summary>
    public abstract class EnemyBase : MonoBehaviour, IDamageable, IStockable
    {
        #region フィールド
        
        protected EnemyState currentState = EnemyState.Move;

        [Header("UI設定")]
        [SerializeField] protected TMP_Text stockText;
        [Header("エフェクト設定")]
        [SerializeField] protected float bounceEffectDuration = 3f;
        [Header("敵設定")]
        [SerializeField] protected EnemyType enemyType = EnemyType.Normal;
        [SerializeField] protected EnemyDataSO enemyDataSO;
        [SerializeField] protected AttackData enemyAttackData;
        [Header("吹っ飛ばし設定")]
        [SerializeField] protected Vector2 launchVector = new Vector2(1, 1);
        [SerializeField] protected float launchPower = 70f;
        [SerializeField] protected float stockMultiplier = 0.2f;
        [Header("ストックリセット設定")]
        [SerializeField] private float stockResetDuration = 5f;
        protected float stockResetTimer = 0f;
        protected bool isStockTimerActive = false;
        [Header("システム定数")]
        [SerializeField] private float baseLaunchDuration = 1f;
        [SerializeField] private float stockBonusThreshold = 5f;
        [SerializeField] private float collisionBounceMultiplier = 0.2f;
        [SerializeField] private float hitStopDuration = 1.0f;

        private const float ViewportMargin = 0.01f;
        private const string TagWall = "Wall";
        private const string TagGround = "Ground";
        private const string LayerEnemy = "Enemy";
        private const string LayerFlyingEnemy = "FlyingEnemy";

        protected EnemyDataSO enemyData;
        protected float currentHP;
        protected bool isDead = false;
        public bool IsDead => isDead;
        protected float currentLaunchTimer = 0f;
        private float currentConstantSpeed;
        protected int currentStockCount;

        protected Rigidbody2D rb;
        protected Animator animator;
        protected SpriteRenderer spriteRenderer;

        public System.Action<EnemyBase> OnEnemyDeath;

        protected bool canMove = true;
        protected bool isSmash = false;

        private GameObject currentFullStockEffect = null;
        
        [Header("ストック満タン直前エフェクト")]
        [SerializeField] private GameObject stockAlmostFullEffect;
        
        #endregion
        
        #region 初期化とライフサイクル

        public void StopMovement(float duration)
        {
            if (!gameObject.activeInHierarchy || isDead) return;
            
            StartCoroutine(StopRoutine(duration));
        }

        private IEnumerator StopRoutine(float duration)
        {
            canMove = false;
            if (rb != null) rb.linearVelocity = Vector2.zero;

            yield return new WaitForSeconds(duration);

            canMove = true;
        }

        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            spriteRenderer = GetComponent<SpriteRenderer>();

            if (rb != null) rb.gravityScale = 1;

            LoadEnemyData();
            InitializeEnemy();
            UpdateStockText();
            
            if (stockAlmostFullEffect != null)
                stockAlmostFullEffect.SetActive(false);
        }
        
        #endregion
        
        #region ストック管理

        public void AddStock(int amount)
        {
            if (isDead || !gameObject.activeInHierarchy) return;
            
            currentStockCount += amount;

            UpdateStockText();
            UpdateStockAlmostFullEffect();

            StopMovement(hitStopDuration);

            stockResetTimer = stockResetDuration;
            isStockTimerActive = true;

            if (enemyData != null && currentStockCount >= enemyData.NeedStock && currentState != EnemyState.ReadyToLaunch && currentState != EnemyState.Launch)
                OnStockReachedRequired();
        }

        /// <summary>
        /// ストックをクリア（IStockableインターフェース実装）
        /// </summary>
        public void ClearStock()
        {
            currentStockCount = 0;
            UpdateStockText();
            if (stockAlmostFullEffect != null)
                stockAlmostFullEffect.SetActive(false);
        }

        protected virtual void OnStockReachedRequired()
        {
            currentState = EnemyState.ReadyToLaunch;
            canMove = false;

            GameEvents.TriggerStockFull(gameObject);

            if (EffectFactory.I != null)
                currentFullStockEffect = EffectFactory.I.CreateEffect(EffectType.FullStockEffect, transform.position);

            OnStateChanged(currentState);
        }

        /// <summary>
        /// ReadyToLaunch状態で攻撃を受けた時の処理
        /// </summary>
        public void TriggerBounceEffect()
        {
            if (currentState != EnemyState.ReadyToLaunch) return;

            int extraStocks = currentStockCount - enemyData.NeedStock;
            currentLaunchTimer = bounceEffectDuration + Mathf.Floor(extraStocks / stockBonusThreshold);

            currentState = EnemyState.Launch;
            gameObject.layer = LayerMask.NameToLayer(LayerFlyingEnemy);
            canMove = false;
            rb.gravityScale = 0;

            float speedMultiplier = 1f + (extraStocks * stockMultiplier);
            float finalLaunchSpeed = launchPower * speedMultiplier;

            currentConstantSpeed = finalLaunchSpeed;
            rb.linearVelocity = launchVector.normalized * finalLaunchSpeed;

            if (currentFullStockEffect != null)
            {
                EffectFactory.I?.ReturnEffect(currentFullStockEffect);
                currentFullStockEffect = null;
            }

            OnStateChanged(currentState);
        }

        /// <summary>
        /// 吹っ飛び状態かどうかを確認
        /// </summary>
        public bool IsInLaunchState() => currentState == EnemyState.ReadyToLaunch || currentState == EnemyState.Launch;

        /// <summary>
        /// プレイヤーに攻撃を実行（AttackExecutor経由）
        /// </summary>
        public virtual void AttackPlayer()
        {
            if (IsInLaunchState()) return;
            if (AttackExecutor.I == null || enemyAttackData == null) return;

            Vector2 direction = GetDirectionToPlayer();

            AttackExecutor.I.Execute(
                enemyAttackData,
                transform.position,
                direction,
                gameObject
            );
        }

        /// <summary>
        /// プレイヤーへの方向を取得
        /// </summary>
        protected virtual Vector2 GetDirectionToPlayer()
        {
            if (PlayerDataProvider.I == null) return Vector2.right;

            Vector2 direction = (PlayerDataProvider.I.Position - transform.position).normalized;
            return direction;
        }
        
        #endregion
        
        #region 吹っ飛ばし処理

        public void TryLaunch()
        {
            if (currentStockCount < enemyData.NeedStock) return;
            
            if (stockAlmostFullEffect != null)
                stockAlmostFullEffect.SetActive(false);
            
            int extraStocks = currentStockCount - enemyData.NeedStock;
            currentLaunchTimer = baseLaunchDuration + Mathf.Floor(extraStocks / stockBonusThreshold);

            currentState = EnemyState.Launch;
            gameObject.layer = LayerMask.NameToLayer(LayerFlyingEnemy);
            canMove = false;
            rb.gravityScale = 0;

            float speedMultiplier = 1f + (currentStockCount * stockMultiplier);
            float finalLaunchSpeed = launchPower * speedMultiplier;

            currentConstantSpeed = finalLaunchSpeed;
            rb.linearVelocity = launchVector.normalized * finalLaunchSpeed;
            OnStateChanged(currentState);
        }


        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (currentState != EnemyState.Launch) return;

            EnemyBase otherEnemy = collision.gameObject.GetComponent<EnemyBase>();

            if (otherEnemy != null && !otherEnemy.isDead)
            {
                HandleEnemyCollision(collision, otherEnemy);
                return;
            }

            if (collision.gameObject.CompareTag(TagWall) || collision.gameObject.CompareTag(TagGround))
            {
                Vector2 normal = collision.contacts[0].normal;
                Vector2 incomingDir = -collision.relativeVelocity.normalized;
                rb.linearVelocity = Vector2.Reflect(incomingDir, normal) * currentConstantSpeed;
            }
        }

        private void HandleEnemyCollision(Collision2D collision, EnemyBase otherEnemy)
        {
            if (otherEnemy.currentState != EnemyState.Launch)
            {
                Vector2 normal = collision.contacts[0].normal;

                Vector2 incomingDir = -collision.relativeVelocity.normalized;
                rb.linearVelocity = Vector2.Reflect(incomingDir, normal) * currentConstantSpeed;

                Vector2 impactDir = (otherEnemy.transform.position - transform.position).normalized;
                int effectiveStock = Mathf.Max(this.currentStockCount, enemyData.NeedStock);
                float attackerStockMultiplier = 1f + (effectiveStock * collisionBounceMultiplier);
                float finalLaunchSpeed = launchPower * attackerStockMultiplier;

                otherEnemy.ForceLaunch(impactDir * finalLaunchSpeed);
            }
        }
        
        public void ForceLaunch(Vector2 initialVelocity)
        {
            if (isDead) return;


            currentLaunchTimer = baseLaunchDuration;
            currentState = EnemyState.Launch;
            canMove = false;

            rb.gravityScale = 0;
            currentConstantSpeed = initialVelocity.magnitude;
            rb.linearVelocity = initialVelocity;
            OnStateChanged(currentState);
        }
        
        #endregion
        
        #region ダメージと死亡処理

        public virtual void TakeDamage(int damage)
        {
            if (isDead) return;

            if (currentState == EnemyState.ReadyToLaunch) return;

            currentHP -= damage;

            if (currentHP <= 0)
            {
                if (currentStockCount > 0)
                {
                    currentStockCount--;
                    UpdateStockText();
                    currentHP = enemyData.MaxHP;
                    OnStockLost();
                }
                else
                {
                    Die();
                }
            }
        }

        public virtual void Die()
        {
            if (isDead) return;
            isDead = true;

            OnEnemyDeath?.Invoke(this);
            Destroy(gameObject);
        }

        protected virtual void OnStockLost()
        {
        }

        protected virtual void OnDamageReceived(int damage)
        {
        }
        
        #endregion
        
        #region 抽象メソッド

        protected abstract void Move();

        protected abstract void Attack();
        
        #endregion
        
        #region Updateと状態管理

        protected virtual void Update()
        {
            if (IsDead) return;

            switch (currentState)
            {
                case EnemyState.Move:
                    if (rb != null && rb.gravityScale != 1)
                        rb.gravityScale = 1;

                    if (canMove) Move();
                    break;

                case EnemyState.ReadyToLaunch:
                    break;

                case EnemyState.Launch:
                    ProcessLaunch();
                    break;
            }
        }

        private void ProcessLaunch()
        {
            currentLaunchTimer -= Time.deltaTime;
            if (currentLaunchTimer <= 0)
            {
                Die();
                return;
            }

            if (rb.linearVelocity.sqrMagnitude > 0)
                rb.linearVelocity = rb.linearVelocity.normalized * currentConstantSpeed;

            UnityEngine.Camera cam = UnityEngine.Camera.main;
            Vector3 pos = transform.position;
            Vector3 viewportPos = cam.WorldToViewportPoint(pos);

            if (viewportPos.x < 0 || viewportPos.x > 1)
            {
                rb.linearVelocity = new Vector2(-rb.linearVelocity.x, rb.linearVelocity.y);

                viewportPos.x = Mathf.Clamp(viewportPos.x, ViewportMargin, 1f - ViewportMargin);
                transform.position = cam.ViewportToWorldPoint(viewportPos);
            }

            if (viewportPos.y > 1)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -rb.linearVelocity.y);

                viewportPos.y = 1f - ViewportMargin;
                transform.position = cam.ViewportToWorldPoint(viewportPos);
            }
        }
        
        #endregion
        
        #region プライベートメソッド
        
        private void LoadEnemyData()
        {
            enemyData = enemyDataSO;
            
            if (enemyData != null)
            {
                currentHP = enemyData.MaxHP;
                currentStockCount = enemyData.StockCount;
            }
        }
        
        protected virtual void InitializeEnemy()
        {
        }

        protected void UpdateStockText()
        {
            if (stockText != null)
                stockText.text = currentStockCount.ToString();
        }

        private void UpdateStockAlmostFullEffect()
        {
            if (stockAlmostFullEffect == null || enemyData == null) return;

            bool shouldShow = currentStockCount == enemyData.NeedStock - 1;
            stockAlmostFullEffect.SetActive(shouldShow);
        }

        private void ResetStock()
        {
            isStockTimerActive = false;
            currentStockCount = enemyData.StockCount;
            UpdateStockText();
            
            if (stockAlmostFullEffect != null)
                stockAlmostFullEffect.SetActive(false);

            if (stockText != null) stockText.color = Color.white;
        }

        protected virtual void OnStateChanged(EnemyState newState) { }
        
        #endregion
        
        #region プロパティ

        public int CurrentStockCount => currentStockCount;
        public float CurrentHP => currentHP;
        public EnemyState CurrentState => currentState;
        public EnemyDataSO EnemyData => enemyData;
        
        #endregion
    }
}