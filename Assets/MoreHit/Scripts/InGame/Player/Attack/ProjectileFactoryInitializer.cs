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
#if UNITY_EDITOR
        [SerializeField] private bool autoInitialize = true;
#endif
        
        [Header("手動設定")]
        [SerializeField] private ObjectPool projectilePool;
        
        private void Start()
        {
#if UNITY_EDITOR
            if (autoInitialize)
                InitializeFactory();
#else
            // リリースビルドでは常に初期化
            InitializeFactory();
#endif
        }
        
        private void InitializeFactory()
        {
            var factory = ProjectileFactory.Instance;
            
            if (factory == null)
            {
                Debug.LogError("ProjectileFactory: インスタンスの取得に失敗");
                return;
            }
            
#if UNITY_EDITOR
            if (projectilePool == null && autoInitialize)
                projectilePool = FindFirstObjectByType<ObjectPool>();
#else
            // リリースビルドでは常にプールを自動検索
            if (projectilePool == null)
                projectilePool = FindFirstObjectByType<ObjectPool>();
#endif
        }
    }
}