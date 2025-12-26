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

        private Rigidbody2D rb;

        private float moveSpeed;
        private float acceleration;
        private float deceleration;
        private float jumpForce;
        private float jumpCutMultiplier;
        private float fallGravityMultiplier;
        private int maxJumpCount;

        private Vector2 moveInput;
        private float currentSpeed;
        private bool isFacingRight = true;

        public Vector2 Velocity => rb != null ? rb.linearVelocity : Vector2.zero;
        public bool IsFacingRight => isFacingRight;
        public bool IsWalking => Mathf.Abs(moveInput.x) > 0.1f;

        private int jumpCount = 0;
        private bool isGrounded;
        public bool IsGrounded => isGrounded;

        private float defaultGravityScale;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            defaultGravityScale = rb.gravityScale;

            if (playerData != null)
                CachePlayerParameters();
        }

        /// <summary>
        /// PlayerDataからパラメータをキャッシュ
        /// </summary>
        private void CachePlayerParameters()
        {
            moveSpeed = playerData.MoveSpeed;
            acceleration = playerData.Acceleration;
            deceleration = playerData.Deceleration;
            jumpForce = playerData.JumpForce;
            jumpCutMultiplier = playerData.JumpCutMultiplier;
            fallGravityMultiplier = playerData.FallGravityMultiplier;
            maxJumpCount = playerData.MaxJumpCount;
        }

        private void Update()
        {
            CheckGroundStatus();
        }

        private void FixedUpdate()
        {
            HandleMovement();
            HandleGravity();
        }

        /// <summary>
        /// 移動入力を設定
        /// </summary>
        public void SetMoveInput(Vector2 input)
        {
            moveInput = input;
        }

        private void CheckGroundStatus()
        {
            bool previouslyGrounded = isGrounded;
            
            isGrounded = groundCheck == null
                ? Mathf.Abs(rb.linearVelocity.y) < 0.01f
                : Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);

            // 地面に着地した瞬間のみジャンプカウントをリセット
            if (isGrounded && !previouslyGrounded)
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
            if (jumpCount >= maxJumpCount)
                return;

            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpCount++;
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
    }
}