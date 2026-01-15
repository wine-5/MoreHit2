using UnityEngine;
namespace MoreHit.Enemy
{
    /// <summary>
    /// 雑魚敵の実装クラス
    /// </summary>
    public class ZakoEnemy : EnemyBase
    {
        [Header("ジャンプ設定")]
        [SerializeField] private float jumpForce = 8f;
        [SerializeField] private float jumpInterval = 3f;
        private float jumpTimer;

        private float leftRange = 3f;
        private float rightRange = 3f;

        private float spawnX;
        private float leftLimit;
        private float rightLimit;
        private int direction = 1;

        public void SetPatrolRange(float left, float right)
        {
            leftRange = left;
            rightRange = right;
            InitializeEnemy();
        }

        protected override void Update()
        {
            HandleJumpTimer();
            base.Update();
        }

        private void HandleJumpTimer()
        {
            jumpTimer -= Time.deltaTime;

            if (jumpTimer <= 0)
            {
                Jump();
                jumpTimer = jumpInterval;
            }
        }

        private void Jump()
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        protected override void Move()
        {
            float currentX = transform.position.x;

            if (direction > 0 && currentX >= rightLimit)
                direction = -1;
            else if (direction < 0 && currentX <= leftLimit)
                direction = 1;

            rb.linearVelocity = new Vector2(direction * enemyData.MoveSpeed, rb.linearVelocity.y);
            spriteRenderer.flipX = direction > 0;
        }

        protected override void Attack()
        {
        }

        protected override void InitializeEnemy()
        {
            base.InitializeEnemy();
            
            jumpTimer = jumpInterval;

            spawnX = transform.position.x;
            leftLimit = spawnX - leftRange;
            rightLimit = spawnX + rightRange;
        }
    }
}
