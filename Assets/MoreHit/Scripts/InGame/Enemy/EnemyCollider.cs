using UnityEngine;
using MoreHit.Enemy;

namespace MoreHit
{
    public class EnemyCollider : MonoBehaviour
    {
        private EnemyBase enemyBase;
        
        private void Awake()
        {
            // 親オブジェクトからEnemyBaseを取得
            enemyBase = GetComponentInParent<EnemyBase>();
            if (enemyBase == null)
            {
                Debug.LogWarning($"{gameObject.name}: EnemyBaseコンポーネントが見つかりません");
            }
            else
            {
                Debug.Log($"{gameObject.name}: EnemyBase取得成功 - {enemyBase.gameObject.name}");
            }
        }
        
        private void OnCollisionEnter2D(Collision2D collision)
        {
            Debug.Log($"[EnemyCollider] 衝突検出: {collision.gameObject.name} (タグ: {collision.gameObject.tag})");
            
            // 衝突相手のタグをチェック
            if (collision.gameObject.CompareTag("Player"))
            {
                Debug.Log($"プレイヤーに当たりました！敵名：{enemyBase?.gameObject.name}");
                
                // プレイヤーへ攻撃を実行
                if (enemyBase != null && !enemyBase.IsDead)
                {
                    Debug.Log($"AttackPlayer()を呼び出します - Enemy: {enemyBase.gameObject.name}");
                    enemyBase.AttackPlayer();
                }
                else
                {
                    Debug.LogWarning($"攻撃できません - enemyBase: {enemyBase}, IsDead: {enemyBase?.IsDead}");
                }
            }
            else
            {
                Debug.Log($"プレイヤー以外との衝突: {collision.gameObject.name}");
            }
        }
    }
}
