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
        
        [Header("2Dスポーン設定")]
        [SerializeField] private bool use2DRandomSpawn = true;
        [SerializeField] private float spawnOffsetFromEdge = 1f; // 画面端からのオフセット
        
        // 2Dスポーン用の位置定義
        private enum SpawnCorner
        {
            TopLeft,     // 左上
            BottomRight  // 右下
        }

        [Header("テスト用")]
        [SerializeField] private bool enableTestMode = true;

        private List<EnemyBase> spawnedEnemies = new List<EnemyBase>();
        private float lastSpawnTime;

        private void Start()
        {
            if (enemyFactory == null)
            {
                enemyFactory = GetComponent<EnemyFactory>();
                if (enemyFactory == null)
                {
                    Debug.LogError("EnemyFactory not found! Attach EnemyFactory component.");
                    enabled = false;
                    return;
                }
            }
            
            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                spawnPoints = new Transform[] { transform };
            }
        }

        private void Update()
        {
            if (autoSpawn && CanSpawn())
            {
                SpawnRandomEnemy();
            }

            if (enableTestMode && Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                SpawnRandomEnemy();
            }
        }

        private bool CanSpawn()
        {
            return enemyFactory != null &&
                   spawnedEnemies.Count < maxEnemyCount &&
                   Time.time - lastSpawnTime >= spawnInterval;
        }

        /// <summary>
        /// ランダムな敵を生成（Factoryを使用）
        /// </summary>
        public EnemyBase SpawnRandomEnemy()
        {
            if (enemyFactory == null)
            {
                Debug.LogWarning("EnemyFactory is not set!");
                return null;
            }

            Vector3 spawnPosition = GetRandomSpawnPosition();
            EnemyBase enemy = enemyFactory.CreateRandomEnemy(spawnPosition);

            if (enemy != null)
            {
                RegisterEnemy(enemy);
            }

            return enemy;
        }

        /// <summary>
        /// ランダムなスポーン位置を取得（2D対応）
        /// </summary>
        private Vector3 GetRandomSpawnPosition()
        {
            if (use2DRandomSpawn)
            {
                return Get2DRandomSpawnPosition();
            }
            else
            {
                // 従来のスポーンポイント方式
                if (spawnPoints.Length > 0)
                {
                    Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
                    return spawnPoint.position;
                }
                return transform.position;
            }
        }

        /// <summary>
        /// 2Dゲーム用のランダムスポーン位置計算
        /// </summary>
        private Vector3 Get2DRandomSpawnPosition()
        {
            // カメラの画面境界を取得
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogWarning("Main Camera not found! Using default position.");
                return transform.position;
            }

            // 画面の境界を世界座標で取得
            Vector3 bottomLeft = mainCamera.ScreenToWorldPoint(new Vector3(0, 0, mainCamera.nearClipPlane));
            Vector3 topRight = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, mainCamera.nearClipPlane));

            // スポーンコーナーをランダムに選択
            SpawnCorner corner = (SpawnCorner)Random.Range(0, 2);

            Vector3 spawnPosition = corner switch
            {
                SpawnCorner.TopLeft => new Vector3(
                    bottomLeft.x - spawnOffsetFromEdge,     // 左端の外側
                    topRight.y + spawnOffsetFromEdge,       // 上端の外側
                    0f
                ),
                SpawnCorner.BottomRight => new Vector3(
                    topRight.x + spawnOffsetFromEdge,       // 右端の外側
                    bottomLeft.y - spawnOffsetFromEdge,     // 下端の外側
                    0f
                ),
                _ => Vector3.zero
            };

            Debug.Log($"[EnemySpawner] Spawning at {corner} corner: {spawnPosition}");
            return spawnPosition;
        }

        /// <summary>
        /// 指定インデックスの敵を生成
        /// </summary>
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

        /// <summary>
        /// 敵を登録してイベントを設定
        /// </summary>
        private void RegisterEnemy(EnemyBase enemy)
        {
            spawnedEnemies.Add(enemy);
            enemy.OnEnemyDeath += OnEnemyDeath;
            lastSpawnTime = Time.time;

            Debug.Log($"Spawned {enemy.GetType().Name} - Total: {spawnedEnemies.Count}");
        }

        private void OnEnemyDeath(EnemyBase enemy)
        {
            if (spawnedEnemies.Contains(enemy))
            {
                spawnedEnemies.Remove(enemy);
                enemy.OnEnemyDeath -= OnEnemyDeath;
                Debug.Log($"Enemy died - Remaining: {spawnedEnemies.Count}");
            }
        }

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