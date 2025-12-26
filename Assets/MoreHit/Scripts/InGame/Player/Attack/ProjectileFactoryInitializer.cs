using UnityEngine;
using MoreHit.Pool;

namespace MoreHit
{
    /// <summary>
    /// ProjectileFactoryの初期化を補助するコンポーネント
    /// </summary>
    public class ProjectileFactoryInitializer : MonoBehaviour
    {
        [Header("自動セットアップ")]
        [SerializeField] private bool autoInitialize = true;
        
        [Header("手動設定")]
        [SerializeField] private ObjectPool projectilePool;
        
        private void Start()
        {
            if (autoInitialize)
            {
                InitializeFactory();
            }
        }
        
        /// <summary>
        /// Factoryを初期化する
        /// </summary>
        public void InitializeFactory()
        {
            // ProjectileFactoryのインスタンス生成を確実に実行
            var factory = ProjectileFactory.Instance;
            
            if (factory != null)
            {
                Debug.Log("ProjectileFactory: 初期化完了");
                
                // プールの設定
                if (projectilePool != null)
                {
                    Debug.Log($"ProjectileFactory: ObjectPool設定完了 ({projectilePool.name})");
                }
                else if (autoInitialize)
                {
                    // 自動でプールを検索
                    var foundPool = FindObjectOfType<ObjectPool>();
                    if (foundPool != null)
                    {
                        projectilePool = foundPool;
                        Debug.Log($"ProjectileFactory: ObjectPoolを自動検出 ({foundPool.name})");
                    }
                }
            }
            else
            {
                Debug.LogError("ProjectileFactory: インスタンスの取得に失敗");
            }
        }
        
        /// <summary>
        /// プールを手動で設定する
        /// </summary>
        /// <param name="pool">設定するプール</param>
        public void SetObjectPool(ObjectPool pool)
        {
            projectilePool = pool;
            Debug.Log($"ProjectileFactory: ObjectPool手動設定 ({pool?.name ?? "null"})");
        }
    }
}