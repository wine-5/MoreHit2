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
        
        private float leftRange = 3f;  // 左側に動ける距離
        private float rightRange = 3f; // 右側に動ける距離

        private float spawnX;      // 生まれた場所のX座標
        private float leftLimit;   // 左の限界点
        private float rightLimit;  // 右の限界点
        private int direction = 1; // 1: 右, -1: 左


        // スポーン時に値をセットするための関数
        public void SetPatrolRange(float left, float right)
        {
            leftRange = left;
            rightRange = right;
            // 範囲が変わったので初期化をやり直す
            InitializeEnemy();
        }

        protected override void Move()
        {
            // TODO: 雑魚敵の移動ロジックを実装
            float currentX = transform.position.x;

            // 限界を超えたら反転
            if (direction > 0 && currentX >= rightLimit)
            {
                direction = -1;
            }
            else if (direction < 0 && currentX <= leftLimit)
            {
                direction = 1;
            }

            // 移動処理（Rigidbody2Dを使用）
            rb.linearVelocity = new Vector2(direction * enemyData.MoveSpeed, rb.linearVelocity.y);

            // 向きに合わせてスプライトを反転
            spriteRenderer.flipX = (direction < 0);
            // 例: プレイヤーに向かって移動、パトロールなど
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

            // 生成された時のX座標を記録
            spawnX = transform.position.x;

            // 限界座標を計算
            leftLimit = spawnX - leftRange;
            rightLimit = spawnX + rightRange;
        }

        protected override void Update()
        {
            base.Update(); // 親クラスのUpdate（Moveなど）を呼ぶ

            // 仮実装：左Shiftキーが押された瞬間にストックを1増やす
            if (Keyboard.current.leftShiftKey.wasPressedThisFrame)
            {
                AddStock(1);
            }
        }

        /// <summary>
        /// ダメージを受けた時の雑魚敵固有の処理
        /// </summary>
        protected override void OnDamageReceived(float damage)
        {
            base.OnDamageReceived(damage);
            // TODO: ダメージ時のエフェクト、アニメーション、ノックバックなどを実装
        }
    }
}
