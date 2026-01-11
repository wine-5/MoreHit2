using UnityEngine;
using MoreHit.Audio;

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
        public bool IsWalking => Mathf.Abs(moveInput.x) > MIN_WALKING_THRESHOLD;

        private int jumpCount = 0;
        private bool isGrounded;
        public bool IsGrounded => isGrounded;

        // RayCastベースの地面検出用変数
        [Header("地面接触判定（RayCast）")]
        [SerializeField] private float groundCheckDistance = 0.3f; // 短くして精度向上
        [SerializeField] private float groundCheckWidth = 0.6f;   // キャラクター幅に合わせて調整
        [SerializeField] private float groundCheckYOffset = 0f;  // Raycast起点のYオフセット（マイナスで上、プラスで下）
        [SerializeField] private int groundRayCount = 5;           // レイの本数を増やして安定性向上

        // ジャンプ連打防止用
        private float lastJumpTime = 0f;
        private const float MIN_JUMP_INTERVAL = 0.15f;

        // 速度・方向判定用定数
        private const float ASCENDING_VELOCITY_THRESHOLD = 0.2f;
        private const float STABLE_VELOCITY_THRESHOLD = 0.1f;
        private const float MIN_FALLING_VELOCITY = -0.3f;
        private const float MOVEMENT_INPUT_THRESHOLD = 0.1f;
        private const float MIN_TARGET_SPEED_THRESHOLD = 0.01f;
        private const float MIN_WALKING_THRESHOLD = 0.1f;
        private const float SCALE_FLIP_MULTIPLIER = -1f;

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

        /// <summary>
        /// RayCastベース
        /// </summary>
        private void CheckGroundStatus()
        {
            bool previouslyGrounded = isGrounded;

            // RayCastによる地面検出（複数のレイを使用して安定性向上）
            isGrounded = PerformGroundRayCast();

            if (rb.linearVelocity.y > ASCENDING_VELOCITY_THRESHOLD)
                isGrounded = false;

            // ジャンプカウントリセット
            if (isGrounded && rb.linearVelocity.y <= STABLE_VELOCITY_THRESHOLD && jumpCount > 0)
                jumpCount = 0;

            // 着地検出（追加のセーフティ）
            if (!previouslyGrounded && isGrounded)
                jumpCount = 0;
        }

        /// <summary>
        /// 複数のRayCastによる地面検出
        /// </summary>
        private bool PerformGroundRayCast()
        {
            // プレイヤーのColliderから直接計算
            Collider2D playerCollider = GetComponent<Collider2D>();
            if (playerCollider == null)
                return Mathf.Abs(rb.linearVelocity.y) < STABLE_VELOCITY_THRESHOLD && rb.linearVelocity.y <= 0;

            // プレイヤーのColliderの下端を起点とし、Yオフセットを適用
            Bounds bounds = playerCollider.bounds;
            Vector2 startPos = new Vector2(bounds.center.x, bounds.min.y + groundCheckYOffset);
            float halfWidth = groundCheckWidth * 0.5f;

            // 複数のポイントからRayCastを実行
            for (int i = 0; i < groundRayCount; i++)
            {
                float offsetX = 0f;
                if (groundRayCount > 1)
                    offsetX = Mathf.Lerp(-halfWidth, halfWidth, (float)i / (groundRayCount - 1));

                Vector2 rayStart = new Vector2(startPos.x + offsetX, startPos.y);
                RaycastHit2D hit = Physics2D.Raycast(rayStart, Vector2.down, groundCheckDistance, groundLayer);

                if (hit.collider != null)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 水平移動処理
        /// </summary>
        private void HandleMovement()
        {
            float targetSpeed = moveInput.x * moveSpeed;
            float speedDiff = targetSpeed - currentSpeed;
            float accelRate = (Mathf.Abs(targetSpeed) > MIN_TARGET_SPEED_THRESHOLD) ? acceleration : deceleration;

            float movement = speedDiff * accelRate * Time.fixedDeltaTime;
            currentSpeed += movement;

            rb.linearVelocity = new Vector2(currentSpeed, rb.linearVelocity.y);

            if (moveInput.x > MOVEMENT_INPUT_THRESHOLD && !isFacingRight)
                Flip();
            else if (moveInput.x < -MOVEMENT_INPUT_THRESHOLD && isFacingRight)
                Flip();
        }

        /// <summary>
        /// ジャンプ実行
        /// </summary>
        public void Jump()
        {
            float currentTime = Time.time;

            // 連打防止：前回のジャンプから最小間隔をチェック
            if (currentTime - lastJumpTime < MIN_JUMP_INTERVAL)
                return;

            // 最大ジャンプ回数チェック
            if (jumpCount >= maxJumpCount)
                return;

            // 最初のジャンプ：地面に立っている必要がある
            if (jumpCount == 0)
            {
                if (!isGrounded)
                    return;

                // 地面に立っていても上昇中なら拒否
                if (rb.linearVelocity.y > STABLE_VELOCITY_THRESHOLD)
                    return;
            }

            // 2回目以降のジャンプ：地面にいる場合は常に許可、空中なら下降中のみ
            if (jumpCount > 0)
            {
                if (!isGrounded && rb.linearVelocity.y >= MIN_FALLING_VELOCITY) // 空中で十分に下降していない
                    return;

                // 地面にいて上昇中なら拒否
                if (isGrounded && rb.linearVelocity.y > STABLE_VELOCITY_THRESHOLD)
                    return;
            }

            // ジャンプ実行
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpCount++;
            lastJumpTime = currentTime;
            
            // ジャンプSE再生
            if (AudioManager.I != null)
            {
                AudioManager.I.PlaySE(SeType.Jump);
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
            scale.x *= SCALE_FLIP_MULTIPLIER;
            transform.localScale = scale;
        }
    }
}