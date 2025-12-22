namespace MoreHit.Enemy
{
    /// <summary>
    /// ダメージを受けることができるオブジェクトのインターフェース
    /// </summary>
    public interface IDamageable
    {
        /// <summary>
        /// ダメージを受ける処理
        /// </summary>
        /// <param name="damage">ダメージ量</param>
        void TakeDamage(float damage);
    }
}
