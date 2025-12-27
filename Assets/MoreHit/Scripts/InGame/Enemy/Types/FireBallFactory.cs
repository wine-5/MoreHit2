using UnityEngine;
using MoreHit.Pool;
using MoreHit.Attack;

namespace MoreHit.Attack
{
    /// <summary>
    /// FireBall生成ファクトリ（EnemyFactoryと同様の仕組み）
    /// ObjectPoolを参照してFireBallの生成・管理を行う
    /// </summary>
    public class FireBallFactory : Singleton<FireBallFactory>
    {
        [Header("Pool設定")]
        [SerializeField] private ObjectPool objectPool; // メインのObjectPool参照
        [SerializeField] private GameObject fireBallPrefab; // FireBall用Prefab
        [SerializeField] private float fireBallSpeed = 8.0f; // FireBallの移動速度
        [SerializeField] private float fireBallLifeTime = 5.0f; // FireBallの生存時間
        
        protected override bool UseDontDestroyOnLoad => false;
        
        protected override void Awake()
        {
            base.Awake();
            
            if (objectPool == null)
            {
                objectPool = FindFirstObjectByType<ObjectPool>();
                if (objectPool == null)
                {
                    Debug.LogWarning("[FireBallFactory] ObjectPoolが見つかりません");
                }
            }
        }
        
        /// <summary>
        /// FireBallを生成する
        /// </summary>
        /// <param name="position">生成位置</param>
        /// <param name="direction">発射方向</param>
        /// <param name="shooter">発射者</param>
        /// <returns>生成されたFireBall GameObject</returns>
        public GameObject CreateFireBall(Vector3 position, Vector2 direction, GameObject shooter = null)
        {
            if (fireBallPrefab == null)
            {
                Debug.LogWarning("[FireBallFactory] FireBallPrefabが設定されていません");
                return null;
            }
            
            GameObject fireBall = null;
            
            // Poolから取得を試行
            if (objectPool != null)
            {
                fireBall = objectPool.GetObject(fireBallPrefab);
                if (fireBall != null)
                {
                    Debug.Log("[FireBallFactory] PoolからFireBall取得成功");
                }
            }
            
            // Poolから取得失敗時のフォールバック
            if (fireBall == null)
            {
                Debug.LogWarning("[FireBallFactory] Poolから取得失敗、Instantiateを実行");
                fireBall = Instantiate(fireBallPrefab);
            }
            
            // FireBallの初期化
            InitializeFireBall(fireBall, position, direction, shooter);
            
            return fireBall;
        }
        
        /// <summary>
        /// FireBallの初期化
        /// </summary>
        private void InitializeFireBall(GameObject fireBall, Vector3 position, Vector2 direction, GameObject shooter)
        {
            if (fireBall == null) return;
            
            // 位置と回転の設定
            fireBall.transform.position = position;
            fireBall.transform.rotation = Quaternion.identity;
            
            // Rigidbody2Dで速度を設定
            Rigidbody2D rb = fireBall.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = direction.normalized * fireBallSpeed;
                rb.angularVelocity = 0f;
            }
            
            // Projectileコンポーネントがあれば初期化
            Projectile projectileComponent = fireBall.GetComponent<Projectile>();
            if (projectileComponent != null)
            {
                // ProjectileDataが必要な場合はここで設定
                // projectileComponent.Initialize(projectileData, direction, shooter);
            }
            
            // 一定時間後に自動的にPoolに戻すか破棄
            MonoBehaviour mb = this;
            mb.StartCoroutine(AutoReturnFireBall(fireBall, fireBallLifeTime));
            
            Debug.Log($"[FireBallFactory] FireBall初期化完了 - Position: {position}, Direction: {direction}, Speed: {fireBallSpeed}");
        }
        
        /// <summary>
        /// 一定時間後にFireBallをPoolに戻す
        /// </summary>
        private System.Collections.IEnumerator AutoReturnFireBall(GameObject fireBall, float lifeTime)
        {
            yield return new WaitForSeconds(lifeTime);
            
            if (fireBall != null)
            {
                ReturnFireBall(fireBall);
            }
        }
        
        /// <summary>
        /// FireBallをPoolに戻す
        /// </summary>
        /// <param name="fireBall">返却するFireBall</param>
        public void ReturnFireBall(GameObject fireBall)
        {
            if (fireBall == null) return;
            
            // 速度をリセット
            Rigidbody2D rb = fireBall.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
            
            // Poolに戻すか破棄
            if (objectPool != null)
            {
                objectPool.ReturnObject(fireBall);
                Debug.Log("[FireBallFactory] FireBallをPoolに返却");
            }
            else
            {
                Destroy(fireBall);
                Debug.Log("[FireBallFactory] FireBallを破棄（Pool未設定）");
            }
        }
    }
}