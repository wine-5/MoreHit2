using UnityEngine;
using MoreHit.Pool;
using MoreHit.Effect;

namespace MoreHit.Attack
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Projectile : MonoBehaviour, IPoolable
    {
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

            if (!hasOriginalScale)
            {
                originalScale = transform.localScale;
                hasOriginalScale = true;
            }
        }

        public void Initialize(ProjectileData projectileData, Vector3 dir, GameObject owner)
        {
            if (projectileData == null) return;

            data = projectileData;
            direction = dir.normalized;
            shooter = owner;
            startPosition = transform.position;

            if (rb != null)
                rb.linearVelocity = direction * data.Speed;

            Invoke(nameof(ReturnToPool), data.LifeTime);
        }

        private void ReturnToPool() => DestroyProjectile(false);

        private void Update() => CheckMaxDistance();

        private void CheckMaxDistance()
        {
            if (data == null) return;

            float traveledDistance = Vector3.Distance(startPosition, transform.position);
            if (traveledDistance >= data.MaxDistance)
                DestroyProjectile(false);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other == null || data == null) return;

            if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                DestroyProjectile(true);
                return;
            }

            bool isEnemy = other.CompareTag("Enemy");
            bool shouldProcess = isEnemy || !ShouldIgnoreCollision(other);

            if (shouldProcess)
            {
                var damageable = other.GetComponent<IDamageable>();
                damageable?.TakeDamage(data.Damage);

                ApplyStock(other);

                if (EffectFactory.I != null)
                {
                    var effect = EffectFactory.I.CreateEffect(EffectType.HitEffect, transform.position);
                    if (effect != null)
                    {
                        float duration = EffectFactory.I.GetEffectDuration(EffectType.HitEffect);
                        EffectFactory.I.ReturnEffectDelayed(effect, duration);
                    }
                }

                DestroyProjectile(true);
            }
        }

        private bool ShouldIgnoreCollision(Collider2D other)
        {
            if (other == null) return true;
            if (shooter != null && other.gameObject == shooter) return true;
            return !HasValidTag(other);
        }

        private bool HasValidTag(Collider2D other)
        {
            if (data == null || data.TargetTags == null) return false;

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

            var stockable = other.GetComponent<IStockable>();
            stockable?.AddStock(data.StockAmount);

            var enemyBase = other.GetComponent<MoreHit.Enemy.EnemyBase>();
            if (enemyBase != null && enemyBase.CurrentState == MoreHit.Enemy.EnemyState.ReadyToLaunch)
                enemyBase.TriggerBounceEffect();
        }

        private void DestroyProjectile(bool wasHit)
        {
            CancelInvoke(nameof(ReturnToPool));

            if (ProjectileFactory.Instance != null)
                ProjectileFactory.Instance.ReturnProjectile(gameObject);
            else
                Destroy(gameObject);
        }

        #region IPoolable の実装

        public void OnPoolGet()
        {
            var collider = GetComponent<Collider2D>();
            if (collider != null)
                collider.enabled = true;

            if (hasOriginalScale)
                transform.localScale = originalScale;

            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }

            CancelInvoke();
        }

        public void OnPoolReturn()
        {
            var collider = GetComponent<Collider2D>();
            if (collider != null)
                collider.enabled = false;

            data = null;
            direction = Vector3.zero;
            startPosition = Vector3.zero;
            shooter = null;

            CancelInvoke();

            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
        }

        #endregion
    }
}