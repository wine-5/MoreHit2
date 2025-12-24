namespace MoreHit.Enemy
{
    /// <summary>
    /// ダメージを受けることができるオブジェクトのインターフェース
    /// </summary>
    public interface IDamageable
    {
        void TakeDamage(float damage);

        // 追加：ストックを増やす機能の定義
        void AddStock(int amount);
    }
}