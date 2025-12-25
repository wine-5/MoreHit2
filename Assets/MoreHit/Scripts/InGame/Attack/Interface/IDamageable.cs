namespace MoreHit.Attack
{
    /// <summary>
    /// ダメージを受けることができるオブジェクトのインターフェース
    /// </summary>
    public interface IDamageable
    {
        void TakeDamage(int damage);
    }
}