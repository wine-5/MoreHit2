using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace MoreHit.Enemy
{
    /// <summary>
    /// 敵スポーン制御システム（EnemyFactoryのラッパー）
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        [Header("ファクトリー設定")]
        [SerializeField] private EnemyFactory enemyFactory;
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private GameObject[] enemyPrefabs;
        [SerializeField] private float spawnInterval = 2f;
        [SerializeField] private int maxEnemyCount = 10;

        [Header("スポーン制御")]
        [SerializeField] private bool autoSpawn = true;

        [Header("テスト用")]
        [SerializeField] private KeyCode testSpawnKey = KeyCode.Space;
        [SerializeField] private bool enableTestMode = true;

        private List<EnemyBase> spawnedEnemies = new List<EnemyBase>();
        private float lastSpawnTime;


        private void Update()
        {
            // 自動スポーン
            if (autoSpawn && CanSpawn())
            {
                SpawnRandomEnemy();
            }

            // テスト用手動スポーン
            if (enableTestMode && Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                SpawnRandomEnemy();
            }
        }

        /// <summary>
        /// スポーン可能かどうかの判定
        /// </summary>
        private bool CanSpawn()
        {
            return spawnedEnemies.Count < maxEnemyCount &&
                   Time.time - lastSpawnTime >= spawnInterval;
        }

        /// <summary>
        /// ランダムな敵を生成
        /// </summary>
        public EnemyBase SpawnRandomEnemy()
        {
            if (enemyPrefabs.Length == 0)
            {
                Debug.LogWarning("Enemy prefabs not set!");
                return null;
            }

            GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

            return SpawnEnemy(enemyPrefab, spawnPoint.position);
        }

        /// <summary>
        /// 指定した敵を指定位置に生成
        /// </summary>
        public EnemyBase SpawnEnemy(GameObject enemyPrefab, Vector3 position)
        {
            GameObject enemyObject = Instantiate(enemyPrefab, position, Quaternion.identity);
            EnemyBase enemy = enemyObject.GetComponent<EnemyBase>();

            if (enemy != null)
            {
                spawnedEnemies.Add(enemy);
                enemy.OnEnemyDeath += OnEnemyDeath;
                lastSpawnTime = Time.time;

                Debug.Log($"Spawned {enemy.GetType().Name} at {position}");
            }
            else
            {
                Debug.LogError($"Enemy prefab {enemyPrefab.name} doesn't have EnemyBase component!");
                Destroy(enemyObject);
            }

            return enemy;
        }

        /// <summary>
        /// 敵が死亡した時のコールバック
        /// </summary>
        private void OnEnemyDeath(EnemyBase enemy)
        {
            if (spawnedEnemies.Contains(enemy))
            {
                spawnedEnemies.Remove(enemy);
                enemy.OnEnemyDeath -= OnEnemyDeath;
                Debug.Log($"{enemy.GetType().Name} died. Remaining enemies: {spawnedEnemies.Count}");
            }
        }

        /// <summary>
        /// 全ての敵を削除
        /// </summary>
        public void ClearAllEnemies()
        {
            for (int i = spawnedEnemies.Count - 1; i >= 0; i--)
            {
                if (spawnedEnemies[i] != null)
                {
                    spawnedEnemies[i].Die();
                }
            }
            spawnedEnemies.Clear();
        }
    }
}
