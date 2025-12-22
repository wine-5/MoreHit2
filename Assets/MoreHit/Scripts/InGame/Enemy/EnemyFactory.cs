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
        
        /// <summary>
        /// ランダムな敵を生成
        /// </summary>
        public EnemyBase CreateRandomEnemy(Vector3 position)
        {
            if (enemyPrefabs.Length == 0)
            {
                Debug.LogWarning("Enemy prefabs not set in EnemyFactory!");
                return null;
            }
            
            GameObject randomPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            return CreateEnemy(randomPrefab, position);
        }
        
        /// <summary>
        /// 指定した敵を生成
        /// </summary>
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
                Debug.LogError($"Enemy prefab {enemyPrefab.name} doesn't have EnemyBase component!");
                Destroy(enemyObject);
                return null;
            }
            
            
            return enemy;
        }
        
        /// <summary>
        /// 指定インデックスの敵を生成
        /// </summary>
        public EnemyBase CreateEnemyByIndex(int index, Vector3 position)
        {
            if (index < 0 || index >= enemyPrefabs.Length)
            {
                Debug.LogError($"Enemy index {index} is out of range! Available: 0-{enemyPrefabs.Length-1}");
                return null;
            }
            
            return CreateEnemy(enemyPrefabs[index], position);
        }
        
        /// <summary>
        /// 利用可能な敵の種類数
        /// </summary>
        public int AvailableEnemyCount => enemyPrefabs.Length;
    }
}
