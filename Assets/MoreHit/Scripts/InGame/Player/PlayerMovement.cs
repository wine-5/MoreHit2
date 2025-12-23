using UnityEngine;

namespace MoreHit.Player
{
    /// <summary>
    /// プレイヤーの移動処理を管理するクラス
    /// </summary>
    public class PlayerMovement : MonoBehaviour
    {
        [Header("プレイヤーデータ")]
        [SerializeField] private PlayerData playerData;
        
        [Header("地面判定")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private Vector2 groundCheckSize = new Vector2(0.8f, 0.1f);
        [SerializeField] private LayerMask groundLayer;
        
        // Components
        private Rigidbody2D rb;
        
        // キャッシュされたパラメータ
        private float moveSpeed;
        private float acceleration;
        private float deceleration;
        private float jumpForce;
        private float jumpCutMultiplier;
        private float fallGravityMultiplier;
        private int maxJumpCount;
        private float backstepDistance;
        private float backstepDuration;
        private float backstepCooldown;
        
        // Movement State
        private Vector2 moveInput;
        private float currentSpeed;
        private bool isFacingRight = true;
        
        // 読み取り専用プロパティ（推奨: PlayerDataProvider経由でアクセス）
        public Vector2 Velocity => rb != null ? rb.linearVelocity : Vector2.zero;
        public bool IsFacingRight => isFacingRight;
        
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
            
            // PlayerDataからパラメータをキャッシュ
            if (playerData != null)
            {
                moveSpeed = playerData.MoveSpeed;
                acceleration = playerData.Acceleration;
                deceleration = playerData.Deceleration;
                jumpForce = playerData.JumpForce;
                jumpCutMultiplier = playerData.JumpCutMultiplier;
                fallGravityMultiplier = playerData.FallGravityMultiplier;
                maxJumpCount = playerData.MaxJumpCount;
                backstepDistance = playerData.BackstepDistance;
                backstepDuration = playerData.BackstepDuration;
                backstepCooldown = playerData.BackstepCooldown;
            }
            else
                Debug.LogError("PlayerDataが設定されていません！");
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
        /// 地面にいるかどうか（推奨: PlayerDataProvider経由でアクセス）
        /// </summary>
        public bool IsGrounded => isGrounded;
        
        /// <summary>
        /// バックステップ中かどうか（推奨: PlayerDataProvider経由でアクセス）
        /// </summary>
        public bool IsBackstepping => isBackstepping;
        
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
