using UnityEngine;

namespace MoreHit.Player
{
    /// <summary>
    /// プレイヤーの当たり判定処理を管理するクラス
    /// </summary>
    public class PlayerCollider : MonoBehaviour
    {
        /// <summary>
        /// コリジョン接触時の処理
        /// </summary>
        private void OnCollisionEnter2D(Collision2D collision)
        {
            // if (collision.collider.CompareTag("Enemy"))
            // {
            //     Debug.Log($"敵との接触: {collision.collider.name}");
            // }
        }
    }
}
