using UnityEngine;

namespace MoreHit.Enemy
{
    /// <summary>
    /// 敵の基底クラス
    /// </summary>
    public abstract class EnemyBase : MonoBehaviour, IDamageable
    {
        [Header("敵設定")]
        [SerializeField] protected string enemyDataName;
        
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
            // 必要なコンポーネントを取得
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
            // TODO: EnemyManagerから敵データを取得（後で実装）
            // enemyData = EnemyManager.Instance.GetEnemyData(enemyDataName);
            
            if (enemyData != null)
            {
                currentHP = enemyData.MaxHP;
                currentStockCount = enemyData.StockCount;
            }
        }

        /// <summary>
        /// 敵の初期化処理（子クラスでオーバーライド可能）
        /// </summary>
        protected virtual void InitializeEnemy()
        {
            // 子クラスで独自の初期化処理をオーバーライド
        }

        /// <summary>
        /// IDamageable実装：ダメージを受ける処理
        /// </summary>
        public virtual void TakeDamage(float damage)
        {
            if (currentStockCount <= 0) return; // 既に死亡している場合は処理しない

            currentStockCount--;
            
            // ダメージを受けた時の処理
            OnDamageReceived(damage);
            
            // ストック判定
            if (currentStockCount <= 0)
            {
                Die();
            }
        }

        /// <summary>
        /// 死亡処理
        /// </summary>
        public virtual void Die()
        {
            // 死亡エフェクト・サウンドは後で実装
            
            // イベント発火
            OnEnemyDeath?.Invoke(this);
            
            // オブジェクト破棄
            Destroy(gameObject);
        }

        /// <summary>
        /// ダメージを受けた時の処理（子クラスでオーバーライド）
        /// </summary>
        protected virtual void OnDamageReceived(float damage)
        {
            // 子クラスでオーバーライド
            // アニメーション、一時的な無敵時間、ノックバックなど
        }

        /// <summary>
        /// 移動処理（子クラスで必ず実装）
        /// </summary>
        protected abstract void Move();

        /// <summary>
        /// 攻撃処理（子クラスで必ず実装）
        /// </summary>
        protected abstract void Attack();

        protected virtual void Update()
        {
            if (currentStockCount <= 0) return; // 死亡している場合は処理しない
            
            Move();
        }

        // デバッグ用プロパティ
        public int CurrentStockCount => currentStockCount;
        public float CurrentHP => currentHP;
        public bool IsDead => currentStockCount <= 0;
    }
}
