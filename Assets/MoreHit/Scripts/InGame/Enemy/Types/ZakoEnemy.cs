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

        protected override void Update()
        {
            // 1. 親クラスのUpdateを呼び、移動制限（canMove）や死亡判定を適用する
            base.Update();

            // 2. 【仮実装】左Shiftキーを常に監視する
            // Updateの中に書くことで、いつでも反応するようになります
            if (Keyboard.current.leftShiftKey.wasPressedThisFrame)
            {
                AddStock(1);
            }
        }

        protected override void Move()
        {
            // TODO: 雑魚敵の移動ロジックを実装


            // 親クラスのUpdateから呼ばれる移動ロジック
            // ここに「canMove」のチェックを入れるとより確実です
            if (!canMove)//canMaveがfalseの時、計算処理をスキップ（動かなくなる）
            {
                rb.linearVelocity = Vector2.zero;
                return;
            }
            //巡回範囲の判定
            float currentX = transform.position.x;
            if (direction > 0 && currentX >= rightLimit) direction = -1;
            else if (direction < 0 && currentX <= leftLimit) direction = 1;

            //決まった方向と設定された速度を掛け合わせて与える
            rb.linearVelocity = new Vector2(direction * enemyData.MoveSpeed, rb.linearVelocity.y);
            spriteRenderer.flipX = (direction < 0); //スプライトを反転
        

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


        /// <summary>
        /// ダメージを受けた時の雑魚敵固有の処理
        /// </summary>
        protected override void OnDamageReceived(float damage)
        {
            base.OnDamageReceived(damage);
            //ストックを増やす処理
            base.Update(); // 親クラスのUpdate（Moveなど）を呼ぶ

            // 仮実装：左Shiftキーが押された瞬間にストックを1増やす
            if (Keyboard.current.leftShiftKey.wasPressedThisFrame)
            {
                AddStock(1);
            }
            // TODO: ダメージ時のエフェクト、アニメーション、ノックバックなどを実装
        }
    }
}
