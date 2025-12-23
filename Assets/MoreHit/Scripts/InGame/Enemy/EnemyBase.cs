using UnityEngine;

namespace MoreHit.Enemy
{
    /// <summary>
    /// 敵の基底クラス
    /// </summary>
    public abstract class EnemyBase : MonoBehaviour, IDamageable
    {
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

        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            spriteRenderer = GetComponent<SpriteRenderer>();

            LoadEnemyData();
            InitializeEnemy();
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