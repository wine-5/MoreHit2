using UnityEngine;
using MoreHit.Pool;

namespace MoreHit.Enemy
{
    /// <summary>
    /// 敵を生成するクラス（Poolのラッパー）
    /// </summary>
    public class EnemyFactory : Singleton<EnemyFactory>
    {
        protected override bool UseDontDestroyOnLoad => false;

        [Header("生成する敵Prefab設定")]
        [SerializeField] private GameObject[] enemyPrefabs;

        [Header("Pool連携")]
        [SerializeField] private ObjectPool objectPool; // Pool参照

        /// <summary>
        /// ランダムな敵を生成（Pool使用）
        /// </summary>
        public EnemyBase CreateRandomEnemy(Vector3 position)
        {
            if (enemyPrefabs == null || enemyPrefabs.Length == 0)
            {
                Debug.LogError("[EnemyFactory] Enemy prefabs not configured!");
                return null;
            }

            GameObject randomPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            return CreateEnemyFromPool(randomPrefab, position);
        }

        /// <summary>
        /// 指定Prefabから敵を生成（Pool使用）
        /// </summary>
        public EnemyBase CreateEnemy(GameObject enemyPrefab, Vector3 position)
        {
            return CreateEnemyFromPool(enemyPrefab, position);
        }

        /// <summary>
        /// インデックスで敵を生成（Pool使用）
        /// </summary>
        public EnemyBase CreateEnemyByIndex(int index, Vector3 position)
        {
            if (enemyPrefabs == null || index < 0 || index >= enemyPrefabs.Length)
            {
                Debug.LogError($"[EnemyFactory] Invalid enemy index: {index}");
                return null;
            }

            return CreateEnemyFromPool(enemyPrefabs[index], position);
        }

        /// <summary>
        /// EnemyTypeで敵を生成
        /// </summary>
        public EnemyBase CreateEnemyByType(EnemyType enemyType, Vector3 position)
        {
            int index = (int)enemyType;
            return CreateEnemyByIndex(index, position);
        }

        /// <summary>
        /// Poolから敵オブジェクトを取得して配置
        /// </summary>
        private EnemyBase CreateEnemyFromPool(GameObject enemyPrefab, Vector3 position)
        {
            if (enemyPrefab == null)
            {
                Debug.LogError("[EnemyFactory] Enemy prefab is null!");
                return null;
            }

            GameObject enemyObject = null;

            // Poolが利用可能な場合はPoolから取得
            if (objectPool != null)
            {
                enemyObject = objectPool.GetObject(enemyPrefab);
                if (enemyObject != null)
                {
                    enemyObject.transform.position = position;
                    enemyObject.transform.rotation = Quaternion.identity;
                    enemyObject.SetActive(true);
                }
            }

            // Poolが利用不可またはオブジェクトが取得できない場合は直接生成
            if (enemyObject == null)
            {
                Debug.LogWarning("[EnemyFactory] Pool利用不可、直接生成します");
                enemyObject = Instantiate(enemyPrefab, position, Quaternion.identity);
            }

            EnemyBase enemy = enemyObject.GetComponent<EnemyBase>();

            if (enemy == null)
            {
                Debug.LogError($"[EnemyFactory] Prefab {enemyPrefab.name} missing EnemyBase component!");

                // Poolに返すかDestroy
                if (objectPool != null)
                    objectPool.ReturnObject(enemyObject);
                else
                    Destroy(enemyObject);

                return null;
            }

            return enemy;
        }
    }
}