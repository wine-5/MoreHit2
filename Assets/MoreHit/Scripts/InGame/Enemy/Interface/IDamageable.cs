using UnityEngine;

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
        /// <param name="attackDirection">攻撃を受けた方向</param>
        void TakeDamage(float damage, Vector2 attackDirection);
        
        /// <summary>
        /// 死亡処理
        /// </summary>
        void Die();
        
        /// <summary>
        /// 現在のHP
        /// </summary>
        float CurrentHP { get; }
        
        /// <summary>
        /// 最大HP
        /// </summary>
        float MaxHP { get; }
        
        /// <summary>
        /// 死亡しているかどうか
        /// </summary>
        bool IsDead { get; }
    }
}
