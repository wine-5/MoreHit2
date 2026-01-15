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
            enemyBase = GetComponentInParent<EnemyBase>();
        }
        
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                if (enemyBase != null && !enemyBase.IsDead && !enemyBase.IsInLaunchState())
                {
                    // BossEnemyの場合は、collisionから直接Playerを渡す
                    var bossEnemy = enemyBase as BossEnemy;
                    if (bossEnemy != null)
                    {
                        bossEnemy.AttackPlayer(collision.gameObject);
                    }
                    else
                    {
                        enemyBase.AttackPlayer();
                    }
                }
            }
        }
    }
}
