using UnityEngine;
using MoreHit;

namespace MoreHit.Attack
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Projectile : MonoBehaviour
    {
        private const float EFFECT_LIFETIME = 2f;
        
        private ProjectileData data;
        private Vector3 direction;
        private Vector3 startPosition;
        private GameObject shooter;
        private Rigidbody2D rb;
        
        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }
        
        public void Initialize(ProjectileData projectileData, Vector3 dir, GameObject owner)
        {
            data = projectileData;
            direction = dir.normalized;
            shooter = owner;
            startPosition = transform.position;
            
            SetInitialVelocity();
            
            // Factoryのプールシステムとの連携のため、一定時間後に自動返却
            Invoke(nameof(ReturnToPool), data.LifeTime);
        }
        
        /// <summary>
        /// プールへ返却する
        /// </summary>
        private void ReturnToPool()
        {
            DestroyProjectile(false);
        }
        
        private void SetInitialVelocity()
        {
            if (rb != null)
                rb.linearVelocity = direction * data.Speed;
        }
        
        private void Update()
        {
            CheckMaxDistance();
        }
        
        private void CheckMaxDistance()
        {
            float traveledDistance = Vector3.Distance(startPosition, transform.position);
            
            if (traveledDistance >= data.MaxDistance)
                DestroyProjectile(false);
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (ShouldIgnoreCollision(other))
                return;
            
            ProcessHit(other);
            DestroyProjectile(true);
        }
        
        private bool ShouldIgnoreCollision(Collider2D other)
        {
            if (other.gameObject == shooter)
                return true;
            
            return !HasValidTag(other);
        }
        
        private bool HasValidTag(Collider2D other)
        {
            foreach (string tag in data.TargetTags)
            {
                if (other.CompareTag(tag))
                    return true;
            }
            return false;
        }
        
        private void ProcessHit(Collider2D other)
        {
            ApplyDamage(other);
            ApplyStock(other);
            SpawnHitEffect();
        }
        
        private void ApplyDamage(Collider2D other)
        {
            var damageable = other.GetComponent<IDamageable>();
            damageable?.TakeDamage(data.Damage);
        }
        
        private void ApplyStock(Collider2D other)
        {
            if (data.StockAmount <= 0) return;
            
            // 修正: otherにストックを付与（敵がストックを蓄積）
            var stockable = other.GetComponent<IStockable>();
            stockable?.AddStock(data.StockAmount);
            
            // ReadyToLaunch状態の敵なら反射効果を発動
            var enemyBase = other.GetComponent<MoreHit.Enemy.EnemyBase>();
            if (enemyBase != null && enemyBase.CurrentState == MoreHit.Enemy.EnemyState.ReadyToLaunch)
            {
                enemyBase.TriggerBounceEffect();
            }
        }
        
        private void SpawnHitEffect()
        {
            if (data.HitEffectPrefab == null) return;
            
            // エフェクトは後で別クラスで実装する
            // TODO: EffectFactoryで処理する
        }
        
        private void DestroyProjectile(bool wasHit)
        {
            if (!wasHit)
                SpawnDestroyEffect();
            
            // Invokeで実行中の自動返却をキャンセル
            CancelInvoke(nameof(ReturnToPool));
            
            // Factoryのプールシステムに返却
            if (ProjectileFactory.Instance != null)
            {
                ProjectileFactory.Instance.ReturnProjectile(gameObject);
            }
            else
            {
                // Factoryが無い場合は直接破棄
                Destroy(gameObject);
            }
        }
        
        private void SpawnDestroyEffect()
        {
            if (data.DestroyEffectPrefab == null) return;
            
            // エフェクトは後で別クラスで実装する
            // TODO: EffectFactoryで処理する
        }
    }
}