namespace MoreHit.Enemy
{
    /// <summary>
    /// ダメージを受けることができるオブジェクトのインターフェース
    /// </summary>
    public interface IDamageable
    {
        void TakeDamage(float damage);
    }
}