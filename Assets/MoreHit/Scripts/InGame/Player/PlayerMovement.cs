using UnityEngine;

namespace MoreHit.Player
{
    /// <summary>
    /// プレイヤーの移動処理を管理するクラス
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("移動設定")]
        [SerializeField] private float moveSpeed = 8f;
        [SerializeField] private float acceleration = 10f;
        [SerializeField] private float deceleration = 10f;
        
        [Header("ジャンプ設定")]
        [SerializeField] private float jumpForce = 15f;
        [SerializeField] private float jumpCutMultiplier = 0.5f;
        [SerializeField] private float fallGravityMultiplier = 2.5f;
        [SerializeField] private int maxJumpCount = 2; // 2段ジャンプ
        
        [Header("バックステップ設定")]
        [SerializeField] private float backstepDistance = 3f;
        [SerializeField] private float backstepDuration = 0.2f;
        [SerializeField] private float backstepCooldown = 0.5f;
        
        [Header("地面判定")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private Vector2 groundCheckSize = new Vector2(0.8f, 0.1f);
        [SerializeField] private LayerMask groundLayer;
        
        // Components
        private Rigidbody2D rb;
        
        // Movement State
        private Vector2 moveInput;
        private float currentSpeed;
        private bool isFacingRight = true;
        
        // Jump State
        private int jumpCount = 0;
        private bool isGrounded;
        private float defaultGravityScale;
        
        // Backstep State
        private bool isBackstepping = false;
        private float backstepTimer = 0f;
        private float backstepCooldownTimer = 0f;
        private Vector2 backstepDirection;
        
        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            defaultGravityScale = rb.gravityScale;
        }
        
        private void Update()
        {
            CheckGroundStatus();
            UpdateBackstepTimer();
        }
        
        private void FixedUpdate()
        {
            if (isBackstepping)
            {
                HandleBackstep();
            }
            else
            {
                HandleMovement();
            }
            
            HandleGravity();
        }
        
        /// <summary>
        /// 移動入力を設定
        /// </summary>
        public void SetMoveInput(Vector2 input)
        {
            moveInput = input;
        }
        
        /// <summary>
        /// 地面判定チェック
        /// </summary>
        private void CheckGroundStatus()
        {
            if (groundCheck == null)
                // groundCheckがない場合は簡易判定
                isGrounded = Mathf.Abs(rb.linearVelocity.y) < 0.01f;
            else
                isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);
            
            // 地面に着地したらジャンプカウントをリセット
            if (isGrounded)
                jumpCount = 0;
        }
        
        /// <summary>
        /// 水平移動処理
        /// </summary>
        private void HandleMovement()
        {
            float targetSpeed = moveInput.x * moveSpeed;
            float speedDiff = targetSpeed - currentSpeed;
            float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
            
            float movement = speedDiff * accelRate * Time.fixedDeltaTime;
            currentSpeed += movement;
            
            rb.linearVelocity = new Vector2(currentSpeed, rb.linearVelocity.y);
            
            // 向き変更
            if (moveInput.x > 0.1f && !isFacingRight)
                Flip();
            else if (moveInput.x < -0.1f && isFacingRight)
                Flip();
        }
        
        /// <summary>
        /// ジャンプ実行
        /// </summary>
        public void Jump()
        {
            if (jumpCount < maxJumpCount)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                jumpCount++;
            }
        }
        
        /// <summary>
        /// ジャンプキャンセル（短押し対応）
        /// </summary>
        public void CancelJump()
        {
            if (rb.linearVelocity.y > 0)
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
        }
        
        /// <summary>
        /// バックステップ実行
        /// </summary>
        public void BackStep()
        {
            if (backstepCooldownTimer <= 0f && !isBackstepping)
            {
                isBackstepping = true;
                backstepTimer = backstepDuration;
                backstepCooldownTimer = backstepCooldown;
                
                // バックステップの方向（プレイヤーの後ろ方向）
                backstepDirection = isFacingRight ? Vector2.left : Vector2.right;
                
                // TODO: 無敵時間の設定
            }
        }
        
        /// <summary>
        /// バックステップ処理
        /// </summary>
        private void HandleBackstep()
        {
            if (backstepTimer > 0)
            {
                float backstepSpeed = backstepDistance / backstepDuration;
                rb.linearVelocity = new Vector2(backstepDirection.x * backstepSpeed, rb.linearVelocity.y);
            }
        }
        
        /// <summary>
        /// バックステップタイマー更新
        /// </summary>
        private void UpdateBackstepTimer()
        {
            if (isBackstepping)
            {
                backstepTimer -= Time.deltaTime;
                if (backstepTimer <= 0)
                {
                    isBackstepping = false;
                    // TODO: 無敵時間の解除
                }
            }
            
            if (backstepCooldownTimer > 0)
                backstepCooldownTimer -= Time.deltaTime;
        }
        
        /// <summary>
        /// 重力調整（落下時の重力を強くする）
        /// </summary>
        private void HandleGravity()
        {
            if (rb.linearVelocity.y < 0)
                rb.gravityScale = defaultGravityScale * fallGravityMultiplier;
            else
                rb.gravityScale = defaultGravityScale;
        }
        
        /// <summary>
        /// プレイヤーの向きを反転
        /// </summary>
        private void Flip()
        {
            isFacingRight = !isFacingRight;
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }
        
        /// <summary>
        /// 移動を停止
        /// </summary>
        public void Stop()
        {
            moveInput = Vector2.zero;
            rb.linearVelocity = Vector2.zero;
        }
        
        /// <summary>
        /// 地面にいるかどうか
        /// </summary>
        public bool IsGrounded() => isGrounded;
        
        /// <summary>
        /// バックステップ中かどうか
        /// </summary>
        public bool IsBackstepping() => isBackstepping;
        
        // デバッグ用：地面判定の可視化
        private void OnDrawGizmosSelected()
        {
            if (groundCheck != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
            }
        }
    }
}
