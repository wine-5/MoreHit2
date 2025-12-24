using System.Collections;
using UnityEngine;
using MoreHit.Attack;
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

            if (rb != null) rb.gravityScale = 1; // 開始時は重力ありにする

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
        // 1. TryLaunch に重力カットを追加
        public void TryLaunch()
        {
            if (currentStockCount < enemyData.Needstock) return;

            int extraStocks = currentStockCount - enemyData.Needstock;
            currentLaunchTimer = 5f + Mathf.Floor(extraStocks / 5f);

            currentState = EnemyState.Launch;
            canMove = false;

            // ★重要：重力を切ることで放物線にならず、勢いを維持する
            rb.gravityScale = 0;

            float speedMultiplier = 1f + (currentStockCount * 0.2f);
            float finalLaunchSpeed = launchPower * speedMultiplier;

            rb.linearVelocity = launchVector.normalized * finalLaunchSpeed;
            OnStateChanged(currentState);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (currentState != EnemyState.Launch) return;

            EnemyBase otherEnemy = collision.gameObject.GetComponent<EnemyBase>();
            if (otherEnemy == null || otherEnemy.isDead) return;

            // --- 吹っ飛び状態の敵同士が当たった場合の処理 ---
            if (otherEnemy.currentState == EnemyState.Launch)
            {
                // 1. お互いの中心点からの方向を計算（ビリヤードの球が離れる方向）
                Vector2 awayDirection = (transform.position - otherEnemy.transform.position).normalized;

                // 2. めり込み防止：お互いを少しだけ外側に強制移動させる
                // これがないと次のフレームでも「衝突中」と判定されてグルグル回る
                transform.position += (Vector3)awayDirection * 0.1f;

                // 3. 速度の再計算（自分のストックに応じた勢いで弾き飛ぶ）
                float speedMultiplier = 1f + (currentStockCount * 0.2f);
                float billiardSpeed = launchPower * speedMultiplier;

                // 進行方向を完全に「相手から離れる方向」に上書き
                rb.linearVelocity = awayDirection * billiardSpeed;

                Debug.Log($"{gameObject.name} が吹っ飛び同士で弾け飛びました");
                return; // 吹っ飛び同士の場合はここで終了
            }

            // --- 以下は吹っ飛んでいない敵（巡回中など）に当たった時の既存処理 ---
            if (otherEnemy.currentState != EnemyState.Launch)
            {
                float mySpeed = Mathf.Max(rb.linearVelocity.magnitude, launchPower);
                Vector2 normal = collision.contacts[0].normal;
                rb.linearVelocity = Vector2.Reflect(rb.linearVelocity.normalized, normal) * mySpeed;

                Vector2 impactDir = (otherEnemy.transform.position - transform.position).normalized;
                float otherSpeedMultiplier = 1f + (otherEnemy.currentStockCount * 0.2f);
                float otherLaunchSpeed = launchPower * otherSpeedMultiplier;

                otherEnemy.ForceLaunch(impactDir * otherLaunchSpeed);
            }
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

        


        /// <summary>
        /// IDamageable実装：ダメージを受ける処理
        /// </summary>
        public virtual void TakeDamage(int damage)
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
        protected virtual void OnDamageReceived(int damage)
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



        // EnemyBase.cs の Update 内を修正
        protected virtual void Update()
        {
            if (IsDead) return;

            // (ストックリセットの処理などはそのまま)

            switch (currentState)
            {
                case EnemyState.Move:
                    // ★重要：通常移動時は重力を 1 に戻す
                    if (rb != null && rb.gravityScale != 1)
                    {
                        rb.gravityScale = 1;
                    }

                    if (canMove) Move();
                    break;

                case EnemyState.Launch:
                    // 吹っ飛ばし中は重力 0（ProcessLaunch内でタイマー処理）
                    ProcessLaunch();
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