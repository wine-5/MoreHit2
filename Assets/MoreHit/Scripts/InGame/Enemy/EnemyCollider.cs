using UnityEngine;

namespace MoreHit
{
    public class EnemyCollider : MonoBehaviour
    {
        
        private void OnCollisionEnter2D(Collision2D collision)
        {
            // 衝突相手のタグをチェック
            // 「== "Player"」ではなく「CompareTag」を使うのがプロの推奨です
            if (collision.gameObject.CompareTag("Player"))
            {
                Debug.Log($" プレイヤーに当たりました！相手：{collision.gameObject.name}");
            }
        }

        private void Attack()
        {
            //ここに攻撃処理
        }


    }
}
