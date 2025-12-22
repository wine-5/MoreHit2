using UnityEngine;

namespace MoreHit.Enemy
{
    /// <summary>
    /// 敵生成専門クラス
    /// </summary>
    public class EnemyFactory : MonoBehaviour
    {
        [Header("敵Prefab設定")]
        [SerializeField] private GameObject[] enemyPrefabs;
        
        public EnemyBase CreateRandomEnemy(Vector3 position)
        {
            if (enemyPrefabs == null || enemyPrefabs.Length == 0)
            {
                Debug.LogError("[EnemyFactory] Enemy prefabs not configured!");
                return null;
            }
            
            GameObject randomPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            return CreateEnemy(randomPrefab, position);
        }
        
        public EnemyBase CreateEnemy(GameObject enemyPrefab, Vector3 position)
        {
            if (enemyPrefab == null)
            {
                Debug.LogError("[EnemyFactory] Enemy prefab is null!");
                return null;
            }
            
            GameObject enemyObject = Instantiate(enemyPrefab, position, Quaternion.identity);
            EnemyBase enemy = enemyObject.GetComponent<EnemyBase>();
            
            if (enemy == null)
            {
                Debug.LogError($"[EnemyFactory] Prefab {enemyPrefab.name} missing EnemyBase component!");
                Destroy(enemyObject);
                return null;
            }
            
            return enemy;
        }
        
        public EnemyBase CreateEnemyByIndex(int index, Vector3 position)
        {
            if (enemyPrefabs == null || index < 0 || index >= enemyPrefabs.Length)
            {
                Debug.LogError($"[EnemyFactory] Invalid enemy index: {index}");
                return null;
            }
            
            return CreateEnemy(enemyPrefabs[index], position);
        }
    }
}