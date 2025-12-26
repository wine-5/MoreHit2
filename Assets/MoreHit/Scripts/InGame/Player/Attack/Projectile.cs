using UnityEngine;
using MoreHit;
using MoreHit.Pool;
using MoreHit.Effect;

namespace MoreHit.Attack
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Projectile : MonoBehaviour, IPoolable
    {        
        // 初期状態保存用
        private Vector3 originalScale;
        private bool hasOriginalScale = false;
        
        private ProjectileData data;
        private Vector3 direction;
        private Vector3 startPosition;
        private GameObject shooter;
        private Rigidbody2D rb;
        
        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            
            // 初期スケールを保存
            if (!hasOriginalScale)
            {
                originalScale = transform.localScale;
                hasOriginalScale = true;
            }
        }
        
        public void Initialize(ProjectileData projectileData, Vector3 dir, GameObject owner)
        {
            if (projectileData == null)
            {
                return;
            }
            
            data = projectileData;
            direction = dir.normalized;
            shooter = owner;
            startPosition = transform.position;
            
            // 初期速度を設定
            if (rb != null)
                rb.linearVelocity = direction * data.Speed;
            
            // Factoryのプールシステムとの連携のため、一定時間後に自動返却
            Invoke(nameof(ReturnToPool), data.LifeTime);
        }
        
        /// <summary>
        /// プールへ返却する
        /// </summary>
        private void ReturnToPool() => DestroyProjectile(false);
        
        private void Update() => CheckMaxDistance();
        
        private void CheckMaxDistance()
        {
            if (data == null)
                return;
            
            float traveledDistance = Vector3.Distance(startPosition, transform.position);
            
            if (traveledDistance >= data.MaxDistance)
                DestroyProjectile(false);
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other == null || data == null)
                return;
            
            if (ShouldIgnoreCollision(other)) return;
            
            // ヒット処理とエフェクト生成
            var damageable = other.GetComponent<IDamageable>();
            damageable?.TakeDamage(data.Damage);
            
            ApplyStock(other);
            
            // ヒットエフェクト
            if (EffectFactory.Instance != null)
            {
                var effect = EffectFactory.Instance.CreateEffect(EffectType.HitEffect, transform.position);
                if (effect != null)
                    EffectFactory.Instance.ReturnEffectDelayed(effect, 2f);
            }
            
            DestroyProjectile(true);
        }
        
        private bool ShouldIgnoreCollision(Collider2D other)
        {
            if (other == null)
                return true;
                
            if (shooter != null && other.gameObject == shooter)
                return true;
            
            return !HasValidTag(other);
        }
        
        private bool HasValidTag(Collider2D other)
        {
            if (data == null || data.TargetTags == null)
                return false;
                
            foreach (string tag in data.TargetTags)
            {
                if (other.CompareTag(tag))
                    return true;
            }
            
            return false;
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
                enemyBase.TriggerBounceEffect();
        }
        
        private void DestroyProjectile(bool wasHit)
        {
            // 消滅時のエフェクトは不要（シンプルに保つ）
            
            // Invokeで実行中の自動返却をキャンセル
            CancelInvoke(nameof(ReturnToPool));
            
            // Factoryのプールシステムに返却
            if (ProjectileFactory.Instance != null)
                ProjectileFactory.Instance.ReturnProjectile(gameObject);
            else
                Destroy(gameObject); // Factoryが無い場合は直接破棄
        }
        
        #region IPoolable Implementation
        
        /// <summary>
        /// プールから取得された時に呼ばれる初期化処理
        /// </summary>
        public void OnPoolGet()
        {
            // Colliderを有効化
            var collider = GetComponent<Collider2D>();
            if (collider != null)
                collider.enabled = true;
            
            // 元のスケールを復元
            if (hasOriginalScale)
            {
                transform.localScale = originalScale;
            }
            
            // Rigidbodyをリセット
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
            
            // Invoke のクリア
            CancelInvoke();
        }
        
        /// <summary>
        /// プールに返却される時に呼ばれるリセット処理
        /// </summary>
        public void OnPoolReturn()
        {
            // Colliderを無効化して衝突を停止
            var collider = GetComponent<Collider2D>();
            if (collider != null)
                collider.enabled = false;
            
            // データをクリア
            data = null;
            direction = Vector3.zero;
            startPosition = Vector3.zero;
            shooter = null;
            
            // Invoke のクリア
            CancelInvoke();
            
            // Rigidbodyをリセット
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
        }
        
        #endregion
    }
}