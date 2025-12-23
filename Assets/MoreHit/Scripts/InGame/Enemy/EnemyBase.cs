using System.Collections;
using UnityEngine;
using TMPro;

namespace MoreHit.Enemy
{

    public enum EnemyState { Idle, Move, HitStun, Launch }//状態の定義
    /// <summary>
    /// 敵の基底クラス
    /// </summary>
    public abstract class EnemyBase : MonoBehaviour, IDamageable
    {
        protected EnemyState currentState = EnemyState.Move; // 現在の状態

        [Header("UI設定")]
        [SerializeField] protected TextMeshProUGUI stockText;
        [Header("敵設定")]
        [SerializeField] protected EnemyDataSO enemyDataSO;
        [SerializeField] protected EnemyType enemyType = EnemyType.Zako;
        [Header("吹っ飛ばし設定")]
        [SerializeField] protected Vector2 launchVector = new Vector2(1, 1); // インスペクターで設定可能
        [SerializeField] protected float launchPower = 10f;



        protected EnemyData enemyData;
        protected float currentHP;
        protected float currentLaunchTimer = 0f;
        protected int currentStockCount;

        // コンポーネント
        protected Rigidbody2D rb;
        protected Animator animator;
        protected SpriteRenderer spriteRenderer;

        // イベント
        public System.Action<EnemyBase> OnEnemyDeath;

        protected bool canMove = true; // 移動可能フラグ
        protected bool isSmash = false;// 吹っ飛ばされているか

        public void StopMovement(float duration)//敵を止める処理
        {
            StartCoroutine(StopRoutine(duration));
        }

        private IEnumerator StopRoutine(float duration)
        {
            canMove = false;
            // 物理速度を完全にゼロにする（これがないと滑る）
            if (rb != null) rb.linearVelocity = Vector2.zero;

            yield return new WaitForSeconds(duration); // 指定秒数待機

            canMove = true;
        }

        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            spriteRenderer = GetComponent<SpriteRenderer>();

            LoadEnemyData();
            InitializeEnemy();
            UpdateStockText();
        }

        public void AddStock(int amount)
        {
            currentStockCount += amount;
            UpdateStockText();
            StopMovement(1.0f); // ストックが増えた衝撃で1秒止まる、などの演出
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
            // 仕様：ストックが needstock を超えているかチェック
            if (currentStockCount < enemyData.Needstock) return;

            // 仕様：吹っ飛ばし時間の計算
            int extraStocks = currentStockCount - enemyData.Needstock;
            currentLaunchTimer = 5f + Mathf.Floor(extraStocks / 5f);

            // 状態を吹っ飛ばしに切り替え
            currentState = EnemyState.Launch;
            canMove = false;

            // 初速を与える
            rb.linearVelocity = launchVector.normalized * launchPower;
            OnStateChanged(currentState);
        }




        /// <summary>
        /// IDamageable実装：ダメージを受ける処理
        /// </summary>
        public virtual void TakeDamage(float damage)
        {
            if (IsDead) return;

            currentHP -= damage;

            OnDamageReceived(damage);

            // HP が 0 以下になったらストック減少
            if (currentHP <= 0)
            {
                currentStockCount--;
                UpdateStockText();

                if (currentStockCount > 0)
                {
                    // ストックが残っている場合はHP回復
                    currentHP = enemyData.MaxHP;
                    OnStockLost();
                }
                else
                {
                    // ストックがなくなったら死亡
                    Die();
                }
            }
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
        public virtual void Die()
        {
            OnEnemyDeath?.Invoke(this);
            Destroy(gameObject);
        }

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

            // 状態に応じた振る舞いの分岐（ステートマシン）
            switch (currentState)
            {
                case EnemyState.Move:
                    if (canMove) Move();
                    break;
                case EnemyState.HitStun:
                    // 停止中は何もしない
                    break;
                case EnemyState.Launch:
                    ProcessLaunch(); // 吹っ飛ばし中の着地判定（追加すべき機能）
                    break;
            }

            
        }

        // カメラ端での跳ね返り処理
        private void ProcessLaunch()
        {
            // タイマー減少
            currentLaunchTimer -= Time.deltaTime;
            if (currentLaunchTimer <= 0)
            {
                // 仕様変更：時間切れで復帰せず、そのまま撃破（消滅）させる
                Die();
                return;
            }

            // カメラの範囲を取得 (Viewport 0.0〜1.0 をワールド座標に変換)
            Camera cam = Camera.main;
            Vector3 pos = transform.position;
            Vector3 viewportPos = cam.WorldToViewportPoint(pos);

            // 左右の壁で跳ね返り
            if (viewportPos.x < 0 || viewportPos.x > 1)
            {
                rb.linearVelocity = new Vector2(-rb.linearVelocity.x, rb.linearVelocity.y);
                // 画面内に押し戻す補正
                viewportPos.x = Mathf.Clamp(viewportPos.x, 0.01f, 0.99f);
                transform.position = cam.ViewportToWorldPoint(viewportPos);
            }

            // 上下の壁（天井・地面）で跳ね返り
            if (viewportPos.y < 0 || viewportPos.y > 1)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -rb.linearVelocity.y);
                // 画面内に押し戻す補正
                viewportPos.y = Mathf.Clamp(viewportPos.y, 0.01f, 0.99f);
                transform.position = cam.ViewportToWorldPoint(viewportPos);
            }
        }

        // フックメソッド：子クラスで「特定の瞬間」に処理を挟めるようにする
        protected virtual void OnStateChanged(EnemyState newState) { }

        // プロパティ
        public int CurrentStockCount => currentStockCount;
        public float CurrentHP => currentHP;
        public bool IsDead => currentStockCount <= 0;
        public EnemyData EnemyData => enemyData;
    }
}