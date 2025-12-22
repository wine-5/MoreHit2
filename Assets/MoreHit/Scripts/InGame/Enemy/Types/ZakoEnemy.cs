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
        protected override void Move()
        {
            // TODO: 雑魚敵の移動ロジックを実装
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
            // TODO: 雑魚敵固有の初期化処理を実装
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
