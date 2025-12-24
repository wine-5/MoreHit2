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
        [SerializeField] protected float stockMultiplier = 0.1f;
        [Header("ストックリセット設定")]
        [SerializeField] private float stockResetDuration = 5f; // リセットまでの時間
        private float stockResetTimer = 0f;
        private bool isStockTimerActive = false;



        protected EnemyData enemyData;
        protected float currentHP;
        private bool isDead = false; // 死亡フラグを追加
        public bool IsDead => isDead; // プロパティをフラグに変更
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

        // EnemyBase.cs 内

        // インターフェースの実装として明示
        public void AddStock(int amount)
        {
            currentStockCount += amount;
            UpdateStockText();

            // 演出：移動を一定時間止める
            StopMovement(1.0f);

            // ストックリセットタイマーの開始
            stockResetTimer = stockResetDuration;
            isStockTimerActive = true;

            Debug.Log($"{gameObject.name} にストックが {amount} 追加されました。現在：{currentStockCount}");
        }

        /// <summary>
        /// 敵データをロードする
        /// </summary>
        private void LoadEnemyData()//enemyDataSOからHPや初期ストック数を読み取る
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
        public void TryLaunch()//吹っ飛ばし始動（ストックが足りていたら、重力を０にして指定方向に勢いよく射出）
        {
            if (currentStockCount < enemyData.Needstock) return;

            int extraStocks = currentStockCount - enemyData.Needstock;
            currentLaunchTimer = 5f + Mathf.Floor(extraStocks / 5f);

            currentState = EnemyState.Launch;
            canMove = false;

            // ★重要：重力を切ることで放物線にならず、勢いを維持する
            rb.gravityScale = 0;

            float speedMultiplier = 1f + (currentStockCount * stockMultiplier);
            float finalLaunchSpeed = launchPower * speedMultiplier;

            // ★重要：この時のスピードを保存
            currentConstantSpeed = finalLaunchSpeed;

            rb.linearVelocity = launchVector.normalized * finalLaunchSpeed;
            OnStateChanged(currentState);
        }


        private void OnCollisionEnter2D(Collision2D collision)  //吹っ飛び中にどこにぶつかったかを判定する
        {
            // 自分が吹っ飛び中（Launch）でなければ何もしない
            if (currentState != EnemyState.Launch) return;

            EnemyBase otherEnemy = collision.gameObject.GetComponent<EnemyBase>();

            // --- パターンA：衝突相手が「敵」だった場合（既存の連鎖ロジック） ---
            if (otherEnemy != null && !otherEnemy.isDead)
            {
                HandleEnemyCollision(collision, otherEnemy);
                return;
            }

            // --- パターンB：衝突相手が「壁」または「地面」だった場合（新規追加） ---
            if (collision.gameObject.CompareTag("Wall") || collision.gameObject.CompareTag("Ground"))
            {
                // 勢いを維持して反射させる
                float mySpeed = Mathf.Max(rb.linearVelocity.magnitude, launchPower);
                Vector2 normal = collision.contacts[0].normal;

                // ベクトルを反射させて速度を上書き
                rb.linearVelocity = Vector2.Reflect(rb.linearVelocity.normalized, normal) * mySpeed;

                Debug.Log($"<color=yellow>【環境反射】</color> {collision.gameObject.tag} に当たって跳ね返りました");
            }
        }

        // コードを整理するために敵同士の衝突は別メソッドに分けます
        private void HandleEnemyCollision(Collision2D collision, EnemyBase otherEnemy)
        {
            // this : 吹っ飛んでいる側の敵（加害者）
            // otherEnemy : ぶつかられた側の敵（被害者）

            if (otherEnemy.currentState == EnemyState.Launch)
            {
                // お互い吹っ飛んでいる場合の反射処理（省略）
                Vector2 awayDirection = (transform.position - otherEnemy.transform.position).normalized;
                rb.linearVelocity = awayDirection * currentConstantSpeed;
            }
            else
            {
                // 1. 自分の反射処理（省略）
                Vector2 normal = collision.contacts[0].normal;
                rb.linearVelocity = Vector2.Reflect(rb.linearVelocity.normalized, normal) * currentConstantSpeed;

                // 2. 相手を弾き飛ばす処理
                Vector2 impactDir = (otherEnemy.transform.position - transform.position).normalized;

                // ★【ここを修正】
                // 計算に使うストック数を「自分のストック」と「最低必要数(Needstock)」の大きい方にする
                int effectiveStock = Mathf.Max(this.currentStockCount, enemyData.Needstock);

                // 決定した effectiveStock を使って倍率を計算
                float attackerStockMultiplier = 1f + (effectiveStock * 0.2f);
                float finalLaunchSpeed = launchPower * attackerStockMultiplier;

                otherEnemy.ForceLaunch(impactDir * finalLaunchSpeed);

                Debug.Log($"<color=cyan>【連鎖】</color> 最低保証({effectiveStock})の威力で相手を飛ばしました");
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
                                 // ★重要：受け取ったベクトルの大きさを保存
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
        private void ProcessLaunch()//吹っ飛びタイマーを減らし画面外に出そうになったら跳ね返す処理
        {
            // タイマー減少
            currentLaunchTimer -= Time.deltaTime;
            if (currentLaunchTimer <= 0)
            {
                // 仕様変更：時間切れで復帰せず、そのまま撃破（消滅）させる
                Die();
                return;
            }

            // ★追加：速度を常に一定に保つ処理
            // 現在の向き（normalized）に対して、保存しておいたスピードを掛け合わせる
            if (rb.linearVelocity.sqrMagnitude > 0) // 停止（ゼロ除算）防止
            {
                rb.linearVelocity = rb.linearVelocity.normalized * currentConstantSpeed;
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

            // ★修正：下方向（地面側）へのワープ処理を無効化、または条件を絞る
            // 天井（viewportPos.y > 1）だけで跳ね返すようにする
            if (viewportPos.y > 1)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -rb.linearVelocity.y);
                viewportPos.y = 0.99f; // 天井に張り付かないように押し戻す
                transform.position = cam.ViewportToWorldPoint(viewportPos);
            }
        }

        private void ResetStock()//ストックをリセットさせる処理
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