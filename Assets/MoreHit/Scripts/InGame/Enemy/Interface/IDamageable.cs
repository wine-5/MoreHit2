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
        void TakeDamage(float damage);
    }
}
