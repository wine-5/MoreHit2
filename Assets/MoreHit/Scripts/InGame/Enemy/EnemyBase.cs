using System.Collections;
using UnityEngine;
using TMPro;

namespace MoreHit.Enemy
{
    /// <summary>
    /// 敵の基底クラス
    /// </summary>
    public abstract class EnemyBase : MonoBehaviour, IDamageable
    {
        [Header("UI設定")]
        [SerializeField] protected TextMeshProUGUI stockText;
        [Header("敵設定")]
        [SerializeField] protected EnemyDataSO enemyDataSO;
        [SerializeField] protected EnemyType enemyType = EnemyType.Zako;

        protected EnemyData enemyData;
        protected float currentHP;
        protected int currentStockCount;

        // コンポーネント
        protected Rigidbody2D rb;
        protected Animator animator;
        protected SpriteRenderer spriteRenderer;

        // イベント
        public System.Action<EnemyBase> OnEnemyDeath;

        protected bool canMove = true; // 移動可能フラグ
        protected bool isSmash = false;// 吹っ飛ばされているか

        public void StopMovement(float duration)
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
            Move();
        }

        // プロパティ
        public int CurrentStockCount => currentStockCount;
        public float CurrentHP => currentHP;
        public bool IsDead => currentStockCount <= 0;
        public EnemyData EnemyData => enemyData;
    }
}