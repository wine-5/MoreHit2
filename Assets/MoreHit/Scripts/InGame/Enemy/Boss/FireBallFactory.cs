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
        [SerializeField] private ObjectPool objectPool;
        [SerializeField] private GameObject fireBallPrefab;
        [SerializeField] private float fireBallSpeed = 8.0f;
        [SerializeField] private float fireBallLifeTime = 5.0f;
        
        protected override bool UseDontDestroyOnLoad => false;
        
        protected override void Awake()
        {
            base.Awake();
            
            if (objectPool == null)
                objectPool = FindFirstObjectByType<ObjectPool>();
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
            if (fireBallPrefab == null) return null;
            
            GameObject fireBall = null;
            
            if (objectPool != null)
                fireBall = objectPool.GetObject(fireBallPrefab);
            
            if (fireBall == null)
                fireBall = Instantiate(fireBallPrefab);
            
            InitializeFireBall(fireBall, position, direction, shooter);
            
            return fireBall;
        }
        
        /// <summary>
        /// FireBallの初期化（ダメージ値を指定）
        /// </summary>
        public GameObject CreateFireBall(Vector3 position, Vector2 direction, GameObject shooter, int damage)
        {
            GameObject fireBall = CreateFireBall(position, direction, shooter);
            if (fireBall != null)
            {
                var fireBallDamage = fireBall.GetComponent<MoreHit.Enemy.FireBallDamage>();
                if (fireBallDamage != null)
                {
                    fireBallDamage.SetDamage(damage);
                }
            }
            return fireBall;
        }
        
        /// <summary>
        /// FireBallの初期化
        /// </summary>
        private void InitializeFireBall(GameObject fireBall, Vector3 position, Vector2 direction, GameObject shooter)
        {
            if (fireBall == null) return;
            
            fireBall.transform.position = position;
            fireBall.transform.rotation = Quaternion.identity;
            
            // FireBallDamageに発射者を設定
            var fireBallDamage = fireBall.GetComponent<MoreHit.Enemy.FireBallDamage>();
            if (fireBallDamage != null)
            {
                fireBallDamage.SetShooter(shooter);
            }
            
            Rigidbody2D rb = fireBall.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 velocity = direction.normalized * fireBallSpeed;
                rb.linearVelocity = velocity;
                rb.angularVelocity = 0f;
            }
            
            MonoBehaviour mb = this;
            mb.StartCoroutine(AutoReturnFireBall(fireBall, fireBallLifeTime));
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
            
            Rigidbody2D rb = fireBall.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
            
            if (objectPool != null)
                objectPool.ReturnObject(fireBall);
            else
                Destroy(fireBall);
        }
    }
}