using UnityEngine;
using MoreHit.Enemy;
using MoreHit.Attack;

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
        }
        
        private void OnCollisionEnter2D(Collision2D collision)
        {
            // 衝突相手のタグをチェック
            if (collision.gameObject.CompareTag("Player"))
            {
                // プレイヤーへ攻撃を実行（敵が死亡していないか、吹っ飛び状態でないかチェック）
                if (enemyBase != null && !enemyBase.IsDead && !enemyBase.IsInLaunchState())
                    enemyBase.AttackPlayer();
            }
        }
    }
}
