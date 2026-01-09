using UnityEngine;

namespace MoreHit.Player
{
    /// <summary>
    /// プレイヤーの各種情報を提供するラッパークラス
    /// 他のシステム（敵AIなど）がプレイヤー情報にアクセスする際の唯一の窓口
    /// </summary>
    public class PlayerDataProvider : Singleton<PlayerDataProvider>
    {
        [Header("コンポーネント参照")]
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private PlayerMovement playerMovement;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private Transform playerTransform;

        protected override bool UseDontDestroyOnLoad => false;

        public Vector3 Position => playerTransform.position;
        public Quaternion Rotation => playerTransform.rotation;
        
        public int CurrentHealth => playerHealth.CurrentHealth;
        public int MaxHealth => playerHealth.MaxHealth;
        public float HealthRatio => MaxHealth > 0 ? (float)CurrentHealth / MaxHealth : 0f;
        public bool IsAlive => playerHealth.IsAlive;
        public bool IsInvincible => playerHealth.IsInvincible;
        
        public bool IsGrounded => playerMovement.IsGrounded;
        public Vector2 Velocity => playerMovement.Velocity;
        public bool IsFacingRight => playerMovement.IsFacingRight;
        
        public Transform Transform => playerTransform;

        protected override void Awake()
        {
            base.Awake();
            
            Debug.Log("PlayerDataProvider: 初期化を開始します...");
            
            if (playerHealth == null)
                playerHealth = GetComponent<PlayerHealth>();
            if (playerMovement == null)
                playerMovement = GetComponent<PlayerMovement>();
            if (playerController == null)
                playerController = GetComponent<PlayerController>();
            if (playerTransform == null)
                playerTransform = transform;
                
            Debug.Log("PlayerDataProvider: 初期化が完了しました。");
        }

        private void Start()
        {
            // Startで確実に初期化完了を報告
            Debug.Log("PlayerDataProvider: Start で初期化確認完了。");
        }
    }
}