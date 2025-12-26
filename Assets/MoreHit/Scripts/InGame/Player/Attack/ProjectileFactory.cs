using UnityEngine;
using MoreHit.Attack;
using MoreHit.Pool;

namespace MoreHit
{
    /// <summary>
    /// プロジェクタイル生成を管理するFactory
    /// </summary>
    public class ProjectileFactory : Singleton<ProjectileFactory>
    {
        [Header("Pool設定")]
        [SerializeField] private ObjectPool projectilePool;
        protected override bool UseDontDestroyOnLoad => false;
        
        protected override void Awake()
        {
            base.Awake();
            
            // プールが未設定の場合は自動で検索
            if (projectilePool == null)
            {
                projectilePool = FindFirstObjectByType<ObjectPool>();
                if (projectilePool == null)
                {
                    Debug.LogError("ProjectileFactory: ObjectPoolが見つかりません。プールなしでは動作できません！");
                    return;
                }
            } 
        }
        
        /// <summary>
        /// プロジェクタイルを生成する
        /// </summary>
        /// <param name="prefab">プロジェクタイルのPrefab</param>
        /// <param name="position">生成位置</param>
        /// <param name="rotation">生成時の回転</param>
        /// <param name="projectileData">プロジェクタイルデータ</param>
        /// <param name="direction">発射方向</param>
        /// <param name="owner">発射者</param>
        /// <returns>生成されたプロジェクタイル</returns>
        public GameObject CreateProjectile(GameObject prefab, Vector3 position, Quaternion rotation, ProjectileData projectileData, Vector3 direction, GameObject owner)
        {
            if (prefab == null)
            {
                Debug.LogError("ProjectileFactory: プレハブがnullです。");
                return null;
            }
            
            if (projectilePool == null)
            {
                Debug.LogError("ProjectileFactory: ObjectPoolが無いため、プロジェクタイルを生成できません！");
                return null;
            }
            
            // プールからプロジェクタイル取得（必須）
            GameObject projectileObj = projectilePool.GetObject(prefab, position, rotation);
            
            // プロジェクタイルコンポーネントを初期化
            var projectile = projectileObj.GetComponent<Projectile>();
            if (projectile != null)
            {
                projectile.Initialize(projectileData, direction, owner);
            }
            else
            {
                Debug.LogWarning($"ProjectileFactory: {prefab.name}にProjectileコンポーネントが見つかりません。");
            }
            
            return projectileObj;
        }
        
        /// <summary>
        /// 簡易版：基本パラメータでプロジェクタイル生成
        /// </summary>
        /// <param name="prefab">プロジェクタイルのPrefab</param>
        /// <param name="position">生成位置</param>
        /// <param name="direction">発射方向</param>
        /// <param name="projectileData">プロジェクタイルデータ</param>
        /// <param name="owner">発射者</param>
        /// <returns>生成されたプロジェクタイル</returns>
        public GameObject CreateProjectile(GameObject prefab, Vector3 position, Vector3 direction, ProjectileData projectileData, GameObject owner)
        {
            Quaternion rotation = CalculateProjectileRotation(direction);
            return CreateProjectile(prefab, position, rotation, projectileData, direction, owner);
        }
        
        /// <summary>
        /// 方向ベクトルから回転を計算
        /// </summary>
        /// <param name="direction">方向ベクトル</param>
        /// <returns>回転</returns>
        private Quaternion CalculateProjectileRotation(Vector3 direction)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            return Quaternion.Euler(0, 0, angle);
        }
        
        /// <summary>
        /// プロジェクタイルをプールに返却
        /// </summary>
        /// <param name="projectile">返却するプロジェクタイル</param>
        public void ReturnProjectile(GameObject projectile)
        {
            if (projectilePool == null)
            {
                Debug.LogError("ProjectileFactory: ObjectPoolが無いため、プロジェクタイルを返却できません！");
                return;
            }
            
            projectilePool.ReturnObject(projectile);
        }
    }
}
