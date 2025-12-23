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
        [Header("ストックリセット設定")]
        [SerializeField] private float stockResetDuration = 5f; // リセットまでの時間
        private float stockResetTimer = 0f;
        private bool isStockTimerActive = false;



        protected EnemyData enemyData;
        protected float currentHP;
        private bool isDead = false; // 死亡フラグを追加
        public bool IsDead => isDead; // プロパティをフラグに変更
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

        protected virtual void Awake()//コンポーネントの取得・データの初期化
        {
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            spriteRenderer = GetComponent<SpriteRenderer>();

            LoadEnemyData();
            InitializeEnemy();
            UpdateStockText();
        }

        public void AddStock(int amount)//ストックが増える処理
        {
            currentStockCount += amount;
            UpdateStockText();
            StopMovement(1.0f); // ストックが増えた衝撃で1秒止まる、などの演出
                                // ★追加：ストックが増えたらタイマーをリセットして開始
            stockResetTimer = stockResetDuration;
            isStockTimerActive = true;
        }

        /// <summary>
        /// 敵データをロードする
        /// </summary>
        private void LoadEnemyData()
        {
            int enemyIndex = (int)enemyType;//列挙型を整数に変換

            if (enemyDataSO != null && enemyDataSO.EnemyDataList != null &&
                enemyIndex >= 0 && enemyIndex < enemyDataSO.EnemyDataList.Length)//データを読み取る条件
            { 
                //自分のデータをゲーム中に増減する変数に変換
                enemyData = enemyDataSO.EnemyDataList[enemyIndex];
                currentHP = enemyData.MaxHP;
                currentStockCount = enemyData.StockCount;
            }
        }

        protected virtual void InitializeEnemy()
        {
            // 子クラスで独自の初期化処理をオーバーライド
        }

        protected void UpdateStockText()//ストックUIを変えてる
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
        /// 他の敵からの衝突などによって、強制的に吹っ飛ばし状態にする
        /// </summary>
        public void ForceLaunch(Vector2 initialVelocity)
        {
            if (isDead) return;
            currentLaunchTimer = 5f;
            currentState = EnemyState.Launch;
            canMove = false;

            rb.gravityScale = 0; // ★追加：重力をゼロにして下に落ちないようにする
            rb.linearVelocity = initialVelocity;
            OnStateChanged(currentState);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            // 自分が吹っ飛び中（Launch）でなければ何もしない
            if (currentState != EnemyState.Launch) return;

            EnemyBase otherEnemy = collision.gameObject.GetComponent<EnemyBase>();
            if (otherEnemy == null || otherEnemy.isDead) return;

            // 吹っ飛んでいない敵に当たった場合のみ連鎖させる
            if (otherEnemy.currentState != EnemyState.Launch)
            {
                // --- 1. 自分自身の跳ね返り処理 ---
                // ぶつかる直前のスピードを維持する
                float mySpeed = Mathf.Max(rb.linearVelocity.magnitude, launchPower);
                // 衝突した面の「向き」を取得
                Vector2 normal = collision.contacts[0].normal;
                // ベクトルを鏡のように反射させる（ビリヤードの動き）
                rb.linearVelocity = Vector2.Reflect(rb.linearVelocity.normalized, normal) * mySpeed;

                // --- 2. 相手を弾き飛ばす処理 ---
                // 自分から相手への方向を計算
                Vector2 impactDir = (otherEnemy.transform.position - transform.position).normalized;
                // 相手には「設定された威力」で初速を与える
                otherEnemy.ForceLaunch(impactDir * launchPower);

                Debug.Log($"{gameObject.name} が反射し、{otherEnemy.name} を弾き飛ばしました");
            }
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
            if (isDead) return; // 二重に死ぬのを防ぐ
            isDead = true; // ここでフラグを立てる

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

            // ★追加：ストックリセットタイマーの処理
            if (isStockTimerActive && currentState == EnemyState.Move)
            {
                stockResetTimer -= Time.deltaTime;

                // 演出：残り1秒を切ったらテキストを赤くするなどのフィードバックがあると親切
                if (stockText != null)
                {
                    stockText.color = (stockResetTimer < 1.0f) ? Color.red : Color.white;
                }

                if (stockResetTimer <= 0)
                {
                    ResetStock();
                }
            }

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

        private void ResetStock()
        {
            isStockTimerActive = false;
            currentStockCount = enemyData.StockCount; // 初期値（名簿の数値）に戻す
            UpdateStockText();

            if (stockText != null) stockText.color = Color.white;

            // デバッグ用ログ
            Debug.Log($"{gameObject.name} のストックがリセットされました");
        }

        // フックメソッド：子クラスで「特定の瞬間」に処理を挟めるようにする
        protected virtual void OnStateChanged(EnemyState newState) { }

        // プロパティ
        public int CurrentStockCount => currentStockCount;
        public float CurrentHP => currentHP;
       
        public EnemyData EnemyData => enemyData;
    }
}