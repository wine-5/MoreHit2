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
            if (autoInitialize)
                InitializeFactory();
        }
        
        public void InitializeFactory()
        {
            var factory = ProjectileFactory.Instance;
            
            if (factory == null)
            {
                Debug.LogError("ProjectileFactory: インスタンスの取得に失敗");
                return;
            }
            
            if (projectilePool == null && autoInitialize)
                projectilePool = FindFirstObjectByType<ObjectPool>();
        }
        
        public void SetObjectPool(ObjectPool pool)
        {
            projectilePool = pool;
        }
    }
}