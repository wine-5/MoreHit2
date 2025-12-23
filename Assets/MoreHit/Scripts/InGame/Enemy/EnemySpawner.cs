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
        [SerializeField] private bool autoSpawn = true;

        private List<EnemyBase> spawnedEnemies = new List<EnemyBase>();
        private float lastSpawnTime;

        private void Start()
        {
            if (enemyFactory == null)
            {
                enemyFactory = GetComponent<EnemyFactory>();
                if (enemyFactory == null)
                {
                    Debug.LogError("[EnemySpawner] EnemyFactory component not found!");
                    enabled = false;
                    return;
                }
            }
            
            if (spawnPoints == null || spawnPoints.Length == 0)
                spawnPoints = new Transform[] { transform };
        }

        private void Update()
        {
            if (autoSpawn && CanSpawn())
                SpawnRandomEnemy();

            // デバッグ用：スペースキーで手動生成
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
                SpawnRandomEnemy();
        }

        private bool CanSpawn()
        {
            return enemyFactory != null &&
                   spawnedEnemies.Count < maxEnemyCount &&
                   Time.time - lastSpawnTime >= spawnInterval;
        }

        public EnemyBase SpawnRandomEnemy()
        {
            if (enemyFactory == null) return null;

            Vector3 spawnPosition = GetRandomSpawnPosition();
            EnemyBase enemy = enemyFactory.CreateRandomEnemy(spawnPosition);

            if (enemy != null)
            {
                RegisterEnemy(enemy);
            }

            return enemy;
        }

        private Vector3 GetRandomSpawnPosition()
        {
            // スポーンポイントが設定されていない場合のフォールバック
            if (spawnPoints.Length == 0)
                return transform.position;

            // 最低2つのスポーンポイントが必要（範囲を定義するため）
            if (spawnPoints.Length < 2)
                return spawnPoints[0].position;

            // 全てのスポーンポイントからX座標とY座標の最小値と最大値を取得
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

            // 完全ランダムな座標を生成
            return new Vector3(
                Random.Range(minX, maxX),    // X座標: 最小X〜最大Xの範囲でランダム
                Random.Range(minY, maxY),    // Y座標: 最小Y〜最大Yの範囲でランダム
                0f                           // Z座標: 2Dなので0
            );
        }

        public EnemyBase SpawnEnemyByIndex(int index, Vector3 position)
        {
            if (enemyFactory == null) return null;

            EnemyBase enemy = enemyFactory.CreateEnemyByIndex(index, position);
            if (enemy != null)
            {
                RegisterEnemy(enemy);
            }

            return enemy;
        }

        private void RegisterEnemy(EnemyBase enemy)
        {
            spawnedEnemies.Add(enemy);
            enemy.OnEnemyDeath += OnEnemyDeath;
            lastSpawnTime = Time.time;
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