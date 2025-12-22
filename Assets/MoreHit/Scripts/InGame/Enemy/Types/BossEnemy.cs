namespace MoreHit.Enemy
{
    /// <summary>
    /// ボス敵の実装クラス
    /// </summary>
    public class BossEnemy : EnemyBase
    {
        /// <summary>
        /// ボス敵の移動処理
        /// </summary>
        protected override void Move()
        {
            // TODO: ボス敵の移動ロジックを実装
            // 例: フェーズごとの移動パターン、テレポート、複雑な移動など
        }

        /// <summary>
        /// ボス敵の攻撃処理
        /// </summary>
        protected override void Attack()
        {
            // TODO: ボス敵の攻撃ロジックを実装
            // 例: 複数段階攻撃、範囲攻撃、特殊攻撃パターンなど
        }

        /// <summary>
        /// ボス敵固有の初期化処理
        /// </summary>
        protected override void InitializeEnemy()
        {
            base.InitializeEnemy();
            // TODO: ボス敵固有の初期化処理を実装
            // 例: HPバー表示、フェーズ管理の初期化など
        }

        /// <summary>
        /// ダメージを受けた時のボス敵固有の処理
        /// </summary>
        protected override void OnDamageReceived(float damage)
        {
            base.OnDamageReceived(damage);
            // TODO: ダメージ時のエフェクト、アニメーション、フェーズ変更などを実装
        }

        /// <summary>
        /// ボス敵固有の死亡処理
        /// </summary>
        public override void Die()
        {
            // TODO: ボス撃破時の特別演出、アイテムドロップなどを実装
            base.Die();
        }
    }
}
