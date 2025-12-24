using UnityEngine;
using MoreHit.Enemy;

namespace MoreHit.Attack
{
    public class AttackExecutor : MonoBehaviour
    {
        public static AttackExecutor I { get; private set; }

        [Header("Debug")]
        public bool showDebugGizmos = true;

        void Awake()
        {
            if (I == null)
                I = this;
            else
                Destroy(gameObject);
        }

        public int Execute(AttackData data, Vector3 origin, Vector2 direction, GameObject attacker)
        {
            Vector3 hitPos = origin + (Vector3)direction * data.Range;
            Collider2D[] hits = Physics2D.OverlapBoxAll(hitPos, data.HitboxSize, 0, data.TargetLayers);

            int hitCount = 0;
            foreach (var hit in hits)
            {
                if (hit.gameObject == attacker) continue;

                // ダメージ適用
                var damageable = hit.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage((int)data.Damage);
                    hitCount++;
                }

                // ストック適用 
                var stockable = hit.GetComponent<IStockable>();
                if (stockable != null && data.StockAmount > 0)
                {
                    stockable.AddStock(data.StockAmount);
                }

                // エフェクト生成
                if (data.HitEffectPrefab != null)
                {
                    var effect = Instantiate(data.HitEffectPrefab, hit.transform.position, Quaternion.identity);
                    Destroy(effect, 2f);
                }
            }
            return hitCount;
        }
    }
}