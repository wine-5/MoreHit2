using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace MoreHit.Enemy
{
    /// <summary>
    /// 敵スポーン制御システム
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        [Header("ファクトリー設定")]
        [SerializeField] private EnemyFactory enemyFactory;
        
        [Header("スポーン制御")]
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private float spawnInterval = 2f;
        [SerializeField] private int maxEnemyCount = 10;
#if UNITY_EDITOR
        [SerializeField] private bool autoSpawn = true;
#endif

        private List<EnemyBase> spawnedEnemies = new List<EnemyBase>();
        private float lastSpawnTime;

        private void Start()
        {
            if (enemyFactory == null)
            {
                enemyFactory = GetComponent<EnemyFactory>();
                if (enemyFactory == null)
                {
                    enabled = false;
                    return;
                }
            }
            
            if (spawnPoints == null || spawnPoints.Length == 0)
                spawnPoints = new Transform[] { transform };
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (autoSpawn && CanSpawn())
                SpawnEnemySequentially();
#endif
        }

        private bool CanSpawn()
        {
            return enemyFactory != null &&
                   spawnedEnemies.Count < maxEnemyCount &&
                   Time.time - lastSpawnTime >= spawnInterval;
        }

        private int nextSpawnIndex = 0;

        public EnemyBase SpawnEnemySequentially()
        {
            if (enemyFactory == null || spawnPoints.Length == 0) return null;

            Transform selectedPoint = spawnPoints[nextSpawnIndex];

            Vector3 spawnPosition = selectedPoint.position;
            EnemyBase enemy = enemyFactory.CreateRandomEnemy(spawnPosition);

            if (enemy != null)
                RegisterEnemy(enemy, selectedPoint);

            nextSpawnIndex = (nextSpawnIndex + 1) % spawnPoints.Length;

            return enemy;
        }

        private Vector3 GetRandomSpawnPosition()
        {
            if (spawnPoints.Length == 0)
                return transform.position;

            if (spawnPoints.Length < 2)
                return spawnPoints[0].position;

            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float minY = float.MaxValue;
            float maxY = float.MinValue;

            foreach (Transform spawnPoint in spawnPoints)
            {
                if (spawnPoint == null) continue;

                Vector3 pos = spawnPoint.position;
                minX = Mathf.Min(minX, pos.x);
                maxX = Mathf.Max(maxX, pos.x);
                minY = Mathf.Min(minY, pos.y);
                maxY = Mathf.Max(maxY, pos.y);
            }

            return new Vector3(
                Random.Range(minX, maxX),
                Random.Range(minY, maxY),
                0f
            );
        }

        public EnemyBase SpawnEnemyByIndex(int index, Vector3 position)
        {
            if (enemyFactory == null) return null;

            EnemyBase enemy = enemyFactory.CreateEnemyByIndex(index, position);
            if (enemy != null)
                RegisterEnemy(enemy, transform);

            return enemy;
        }

        private void RegisterEnemy(EnemyBase enemy, Transform spawnPoint)
        {
            spawnedEnemies.Add(enemy);
            enemy.OnEnemyDeath += OnEnemyDeath;
            lastSpawnTime = Time.time;

            EnemySpawnSettings settings = spawnPoint.GetComponent<EnemySpawnSettings>();
            if (settings != null && enemy is ZakoEnemy zako)
                zako.SetPatrolRange(settings.leftRange, settings.rightRange);
        }

        private void OnEnemyDeath(EnemyBase enemy)
        {
            if (spawnedEnemies.Contains(enemy))
            {
                spawnedEnemies.Remove(enemy);
                enemy.OnEnemyDeath -= OnEnemyDeath;
            }
        }

        public void ClearAllEnemies()
        {
            for (int i = spawnedEnemies.Count - 1; i >= 0; i--)
            {
                if (spawnedEnemies[i] != null)
                    spawnedEnemies[i].Die();
            }
            spawnedEnemies.Clear();
        }
    }
}