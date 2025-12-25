using UnityEngine;

namespace MoreHit.Attack
{
    public class AttackExecutor : Singleton<AttackExecutor>
    {
        private const float EFFECT_LIFETIME = 2f;
        protected override bool UseDontDestroyOnLoad => true;
    
        [Header("デバッグ表示")]
        [SerializeField] private bool showAttackGizmos = true;
        [SerializeField] private Color attackGizmosColor = Color.cyan;
        
        // デバッグ用の最後の攻撃データ
        private AttackData lastAttackData;
        private Vector3 lastHitPosition;
        private float lastAttackTime;

        /// <summary>
        /// 攻撃を実行し、ヒット数を返す
        /// </summary>
        public int Execute(AttackData data, Vector3 origin, Vector2 direction, GameObject attacker)
        {
            if (data == null || attacker == null) return 0;

            Vector3 hitPosition = CalculateHitPosition(origin, direction, data.Range);
            Collider2D[] hits = DetectHits(hitPosition, data.HitboxSize);

            // デバッグ情報を記録
            lastAttackData = data;
            lastHitPosition = hitPosition;
            lastAttackTime = Time.time;

            return ProcessHits(hits, data, attacker);
        }

        private Vector3 CalculateHitPosition(Vector3 origin, Vector2 direction, float range)
        {
            return origin + (Vector3)direction * range;
        }

        private Collider2D[] DetectHits(Vector3 position, Vector2 size)
        {
            return Physics2D.OverlapBoxAll(position, size, 0f);
        }

        private int ProcessHits(Collider2D[] hits, AttackData data, GameObject attacker)
        {
            int hitCount = 0;

            foreach (var hit in hits)
            {
                if (ShouldIgnoreHit(hit, attacker, data.TargetTags))
                    continue;

                if (ApplyDamage(hit, data.Damage))
                    hitCount++;

                ApplyStock(hit, data.StockAmount);
                SpawnHitEffect(hit.transform.position, data.HitEffectPrefab);
            }

            return hitCount;
        }

        private bool ShouldIgnoreHit(Collider2D hit, GameObject attacker, string[] targetTags)
        {
            if (hit.gameObject == attacker)
                return true;

            return !HasValidTag(hit, targetTags);
        }

        private bool HasValidTag(Collider2D hit, string[] targetTags)
        {
            foreach (string tag in targetTags)
            {
                if (hit.CompareTag(tag))
                    return true;
            }
            return false;
        }

        private bool ApplyDamage(Collider2D hit, int damage)
        {
            var damageable = hit.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
                return true;
            }
            return false;
        }

        private void ApplyStock(Collider2D hit, int stockAmount)
        {
            if (stockAmount <= 0) return;

            var stockable = hit.GetComponent<IStockable>();
            stockable?.AddStock(stockAmount);
        }

        private void SpawnHitEffect(Vector3 position, GameObject effectPrefab)
        {
            if (effectPrefab == null) return;

            GameObject effect = Instantiate(effectPrefab, position, Quaternion.identity);
            Destroy(effect, EFFECT_LIFETIME);
        }
        
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!showAttackGizmos || lastAttackData == null) return;
            
            // 最近の攻撃から1秒以内なら描画
            if (Time.time - lastAttackTime < 1f)
            {
                Gizmos.color = attackGizmosColor;
                Gizmos.DrawWireCube(lastHitPosition, lastAttackData.HitboxSize);
                
                // 半透明で塗りつぶし
                Color fillColor = attackGizmosColor;
                fillColor.a = 0.3f;
                Gizmos.color = fillColor;
                Gizmos.DrawCube(lastHitPosition, lastAttackData.HitboxSize);
            }
        }
#endif
    }
}