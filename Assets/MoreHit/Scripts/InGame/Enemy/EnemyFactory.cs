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
                Debug.LogWarning("Enemy prefabs not set in EnemyFactory!");
                return null;
            }
            
            GameObject randomPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            return CreateEnemy(randomPrefab, position);
        }
        
        public EnemyBase CreateEnemy(GameObject enemyPrefab, Vector3 position)
        {
            if (enemyPrefab == null)
            {
                Debug.LogError("Enemy prefab is null!");
                return null;
            }
            
            GameObject enemyObject = Instantiate(enemyPrefab, position, Quaternion.identity);
            EnemyBase enemy = enemyObject.GetComponent<EnemyBase>();
            
            if (enemy == null)
            {
                Debug.LogError($"Prefab {enemyPrefab.name} doesn't have EnemyBase component!");
                Destroy(enemyObject);
                return null;
            }
            
            return enemy;
        }
        
        public EnemyBase CreateEnemyByIndex(int index, Vector3 position)
        {
            if (enemyPrefabs == null || index < 0 || index >= enemyPrefabs.Length)
            {
                Debug.LogError($"Enemy index {index} is out of range! Available: 0-{enemyPrefabs?.Length - 1 ?? -1}");
                return null;
            }
            
            return CreateEnemy(enemyPrefabs[index], position);
        }
    }
}