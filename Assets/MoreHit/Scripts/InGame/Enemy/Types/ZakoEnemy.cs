using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
namespace MoreHit.Enemy
{
    /// <summary>
    /// 雑魚敵の実装クラス
    /// </summary>
    public class ZakoEnemy : EnemyBase
    {
        /// <summary>
        /// 雑魚敵の移動処理
        /// </summary>
        /// 

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

            if (direction > 0 && currentX >= rightLimit) direction = -1;
            else if (direction < 0 && currentX <= leftLimit) direction = 1;

            rb.linearVelocity = new Vector2(direction * enemyData.MoveSpeed, rb.linearVelocity.y);
            spriteRenderer.flipX = (direction < 0);


        }

       

        /// <summary>
        /// 雑魚敵の攻撃処理
        /// </summary>
        protected override void Attack()
        {
            // TODO: 雑魚敵の攻撃ロジックを実装
            // 例: 接触ダメージ、弾の発射など
        }

        /// <summary>
        /// 雑魚敵固有の初期化処理
        /// </summary>
        protected override void InitializeEnemy()
        {
            base.InitializeEnemy();

            
            jumpTimer = jumpInterval;

         
            spawnX = transform.position.x;
            leftLimit = spawnX - leftRange;
            rightLimit = spawnX + rightRange;
        }

        /// <summary>
        /// ダメージを受けた時の雑魚敵固有の処理
        /// </summary>
        protected override void OnDamageReceived(int damage)
        {
            base.OnDamageReceived(damage);
         
           
           
          
        }

       

    }
}
