using System.Collections;
using UnityEngine;
using TMPro;

namespace MoreHit.Enemy
{

    public enum EnemyState { Idle, Move, HitStun, Launch }
    /// <summary>
    /// 敵の基底クラス
    /// </summary>
    public abstract class EnemyBase : MonoBehaviour, IDamageable
    {
        protected EnemyState currentState = EnemyState.Move; 

        [Header("UI設定")]
        [SerializeField] protected TextMeshProUGUI stockText;
        [Header("敵設定")]
        [SerializeField] protected EnemyDataSO enemyDataSO;
        [SerializeField] protected EnemyType enemyType = EnemyType.Zako;
        [Header("吹っ飛ばし設定")]
        [SerializeField] protected Vector2 launchVector = new Vector2(1, 1);
        [SerializeField] protected float launchPower = 10f;
        [SerializeField] protected float stockMultiplier = 0.1f;
        [Header("ストックリセット設定")]
        [SerializeField] private float stockResetDuration = 5f;
        private float stockResetTimer = 0f;
        private bool isStockTimerActive = false;
        [Header("システム定数")]
        [SerializeField] private float baseLaunchDuration = 5f;
        [SerializeField] private float stockBonusThreshold = 5f;
        [SerializeField] private float collisionBounceMultiplier = 0.2f;
        [SerializeField] private float hitStopDuration = 1.0f;

        private const float ViewportMargin = 0.01f;
        private const string TagWall = "Wall";
        private const string TagGround = "Ground";
        private const string LayerEnemy = "Enemy";
        private const string LayerFlyingEnemy = "FlyingEnemy";



        protected EnemyData enemyData;
        protected float currentHP;
        private bool isDead = false; 
        public bool IsDead => isDead;
        protected float currentLaunchTimer = 0f;
        // 吹っ飛ばし中の固定速度を保持
        private float currentConstantSpeed;
        protected int currentStockCount;

        // コンポーネント
        protected Rigidbody2D rb;
        protected Animator animator;
        protected SpriteRenderer spriteRenderer;

        // イベント
        public System.Action<EnemyBase> OnEnemyDeath;

        protected bool canMove = true; 
        protected bool isSmash = false;

        public void StopMovement(float duration)
        {
            StartCoroutine(StopRoutine(duration));
        }

        private IEnumerator StopRoutine(float duration)
        {
            canMove = false;
            // 物理速度を完全にゼロにする（これがないと滑る）
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
        }



      
        public void AddStock(int amount)
        {
            currentStockCount += amount;
            UpdateStockText();

           
            StopMovement(hitStopDuration);

            stockResetTimer = stockResetDuration;
            isStockTimerActive = true;
        }

        /// <summary>
        /// 敵データをロードする
        /// </summary>
        private void LoadEnemyData()
        {
            int enemyIndex = (int)enemyType;

            if (enemyDataSO != null && enemyDataSO.EnemyDataList != null &&
                enemyIndex >= 0 && enemyIndex < enemyDataSO.EnemyDataList.Length)
            { 
                
                enemyData = enemyDataSO.EnemyDataList[enemyIndex];
                currentHP = enemyData.MaxHP;
                currentStockCount = enemyData.StockCount;
            }
        }

        protected virtual void InitializeEnemy()
        {
            // 子クラスで独自の初期化処理をオーバーライド
        }

        protected void UpdateStockText()
        {
            if (stockText != null)
            {
                stockText.text = currentStockCount.ToString();
            }
        }

        // 吹っ飛ばし開始処理
        public void TryLaunch()
        {
            if (currentStockCount < enemyData.Needstock) return;
            // 必要数を超えた分のストックを計算（ボーナス時間の算出用）
            int extraStocks = currentStockCount - enemyData.Needstock;
            // 基本時間に、余剰ストックに応じた追加時間を加算して「飛んでいる時間」を決める
            currentLaunchTimer = baseLaunchDuration + Mathf.Floor(extraStocks / stockBonusThreshold);

            currentState = EnemyState.Launch;
            // レイヤーを変更して、吹っ飛ばし中の敵同士の衝突を無視させる
            gameObject.layer = LayerMask.NameToLayer(LayerFlyingEnemy);
            canMove = false;
            rb.gravityScale = 0;

            float speedMultiplier = 1f + (currentStockCount * stockMultiplier);
            float finalLaunchSpeed = launchPower * speedMultiplier;

            currentConstantSpeed = finalLaunchSpeed;// 減速しないよう、現在の速度を固定値として保持
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

                // 【修正点】物理演算で書き換わる前の「入射ベクトル」を collision.relativeVelocity から取得する
                // staticな壁への衝突時、-relativeVelocity は衝突直前の自分の速度を指す
                Vector2 incomingDir = -collision.relativeVelocity.normalized;

                // 【修正点】速度は現在の実測値ではなく、保持している「currentConstantSpeed」をそのまま使う
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

                // 相手を吹っ飛ばす処理
                Vector2 impactDir = (otherEnemy.transform.position - transform.position).normalized;
                int effectiveStock = Mathf.Max(this.currentStockCount, enemyData.Needstock);
                float attackerStockMultiplier = 1f + (effectiveStock * collisionBounceMultiplier);
                float finalLaunchSpeed = launchPower * attackerStockMultiplier;

                otherEnemy.ForceLaunch(impactDir * finalLaunchSpeed);
            }
        }
        /// <summary>
        /// 他の敵からの衝突などによって、強制的に吹っ飛ばし状態にする
        /// </summary>
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



        /// <summary>
        /// IDamageable実装：ダメージを受ける処理
        /// </summary>
        public virtual void TakeDamage(float damage)
        {
            if (currentHP <= 0)
            {
                // ストックが 1 以上の場合は、ストックを消費して復活
                if (currentStockCount > 0)
                {
                    currentStockCount--;
                    UpdateStockText();
                    currentHP = enemyData.MaxHP;
                    OnStockLost();
                }
                else
                {
                    // ストックが 0 の状態でさらにHPが 0 になったら死亡
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

        /// <summary>
        /// ストックを失った時の処理（復活演出など）
        /// </summary>
        protected virtual void OnStockLost()
        {
            // 子クラスでオーバーライド
        }

        /// <summary>
        /// 死亡処理
        /// </summary>


        /// <summary>
        /// ダメージを受けた時の処理
        /// </summary>
        protected virtual void OnDamageReceived(float damage)
        {
            // 子クラスでオーバーライド
        }

        /// <summary>
        /// 移動処理
        /// </summary>
        protected abstract void Move();

        /// <summary>
        /// 攻撃処理
        /// </summary>
        protected abstract void Attack();



        
        protected virtual void Update()
        {
            if (IsDead) return;

          

            switch (currentState)
            {
                case EnemyState.Move:
                    //通常移動時はジャンプ動作のため重力を 1 に戻す
                    if (rb != null && rb.gravityScale != 1)
                    {
                        rb.gravityScale = 1;
                    }

                    if (canMove) Move();
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
                Die();// 時間切れで消滅
                return;
            }

            // 勝手に減速しないよう、常に固定速度を維持させる
            if (rb.linearVelocity.sqrMagnitude > 0)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * currentConstantSpeed;
            }

            Camera cam = Camera.main;
            Vector3 pos = transform.position;
            Vector3 viewportPos = cam.WorldToViewportPoint(pos);

            if (viewportPos.x < 0 || viewportPos.x > 1)
            {
                rb.linearVelocity = new Vector2(-rb.linearVelocity.x, rb.linearVelocity.y);

                // 画面内に押し戻して、壁にめり込むのを防ぐ
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
        private void ResetStock()
        {
            isStockTimerActive = false;
            currentStockCount = enemyData.StockCount;
            UpdateStockText();

            if (stockText != null) stockText.color = Color.white;

        }

      
        protected virtual void OnStateChanged(EnemyState newState) { }

        // プロパティ
        public int CurrentStockCount => currentStockCount;
        public float CurrentHP => currentHP;
       
        public EnemyData EnemyData => enemyData;
    }
}