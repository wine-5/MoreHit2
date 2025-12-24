using UnityEngine;
using MoreHit.Attack;

namespace MoreHit
{
    public class Projectile : MonoBehaviour
    {
        private ProjectileData data;
        private Vector3 direction;
        private Vector3 startPosition;
        private GameObject shooter;
        private float traveledDistance;
        
        public void Initialize(ProjectileData projectileData, Vector3 dir, GameObject owner)
        {
            data = projectileData;
            direction = dir.normalized;
            shooter = owner;
            startPosition = transform.position;
            
            Destroy(gameObject, data.LifeTime);
        }
        
        private void Update()
        {
            MoveProjectile();
            CheckMaxDistance();
        }
        
        private void MoveProjectile()
        {
            Vector3 movement = direction * data.Speed * Time.deltaTime;
            transform.Translate(movement, Space.World);
            traveledDistance += movement.magnitude;
        }
        
        private void CheckMaxDistance()
        {
            if (traveledDistance >= data.MaxDistance)
                DestroyProjectile(false);
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject == shooter) return;
            
            bool isValidTarget = false;
            foreach (string targetTag in data.TargetTags)
            {
                if (other.CompareTag(targetTag))
                {
                    isValidTarget = true;
                    break;
                }
            }
            
            if (!isValidTarget) return;
            
            var damageable = other.GetComponent<IDamageable>();
            if (damageable != null)
                damageable.TakeDamage((int)data.Damage);
            
            var stockable = shooter.GetComponent<IStockable>();
            if (stockable != null && data.StockAmount > 0)
                stockable.AddStock(data.StockAmount);
            
            if (data.HitEffectPrefab != null)
            {
                var effect = Instantiate(data.HitEffectPrefab, transform.position, Quaternion.identity);
                Destroy(effect, 2f);
            }
            
            DestroyProjectile(true);
        }
        
        private void DestroyProjectile(bool wasHit)
        {
            if (!wasHit && data.DestroyEffectPrefab != null)
            {
                var effect = Instantiate(data.DestroyEffectPrefab, transform.position, Quaternion.identity);
                Destroy(effect, 2f);
            }
            
            Destroy(gameObject);
        }
    }
}
