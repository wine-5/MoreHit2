using UnityEngine;
using MoreHit.Effect;

namespace MoreHit.Attack
{
    public class AttackExecutor : Singleton<AttackExecutor>
    {
        
        private const float CONTACT_ATTACK_RANGE_THRESHOLD = 2.0f;
        private const float SMALL_HITBOX_THRESHOLD = 3f;
        private const float CONTACT_ATTACK_HITBOX_SIZE = 5f;
        
        
        protected override bool UseDontDestroyOnLoad => false;
    
#if UNITY_EDITOR
        [Header("デバッグ表示")]
        [SerializeField] private bool showAttackGizmos = true;
        [SerializeField] private Color attackGizmosColor = Color.cyan;
#endif
        
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

            lastAttackData = data;
            lastHitPosition = hitPosition;
            lastAttackTime = Time.time;

            return ProcessHits(hits, data, attacker);
        }

        private Vector3 CalculateHitPosition(Vector3 origin, Vector2 direction, float range)
        {
            if (range <= CONTACT_ATTACK_RANGE_THRESHOLD)
                return origin;
            else
            {
                Vector3 hitPos = origin + (Vector3)direction * range;
                return hitPos;
            }
        }

        private Collider2D[] DetectHits(Vector3 position, Vector2 size)
        {
            Vector2 adjustedSize = size;
            if (size.x <= SMALL_HITBOX_THRESHOLD && size.y <= SMALL_HITBOX_THRESHOLD)
            {
                adjustedSize = new Vector2(CONTACT_ATTACK_HITBOX_SIZE, CONTACT_ATTACK_HITBOX_SIZE);
            }
            
            Collider2D[] results = Physics2D.OverlapBoxAll(position, adjustedSize, 0f);
            
            return results;
        }

        private int ProcessHits(Collider2D[] hits, AttackData data, GameObject attacker)
        {
            int hitCount = 0;

            foreach (var hit in hits)
            {
                bool shouldIgnore = ShouldIgnoreHit(hit, attacker, data.TargetTags);
                
                if (shouldIgnore)
                {
                    continue;
                }

                ApplyStock(hit, data.StockAmount);

                if (ApplyDamage(hit, data.Damage))
                {
                    hitCount++;
                }

                if (EffectFactory.I != null)
                {
                    var effect = EffectFactory.I.CreateEffect(EffectType.HitEffect, hit.transform.position);
                    if (effect != null)
                    {
                        float duration = EffectFactory.I.GetEffectDuration(EffectType.HitEffect);
                        EffectFactory.I.ReturnEffectDelayed(effect, duration);
                    }
                }
            }

            return hitCount;
        }

        private bool ShouldIgnoreHit(Collider2D hit, GameObject attacker, string[] targetTags)
        {
            if (hit.gameObject == attacker)
            {
                return true;
            }

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
            if (stockAmount <= 0)
            {
                return;
            }

            var stockable = hit.GetComponent<IStockable>();
            
            if (stockable != null)
            {
                stockable.AddStock(stockAmount);
            }
            
            var enemyBase = hit.GetComponent<MoreHit.Enemy.EnemyBase>();
            if (enemyBase != null && enemyBase.CurrentState == MoreHit.Enemy.EnemyState.ReadyToLaunch)
            {
                enemyBase.TriggerBounceEffect();
            }
        }
        
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!showAttackGizmos || lastAttackData == null) return;
            
            if (Time.time - lastAttackTime < 1f)
            {
                Gizmos.color = attackGizmosColor;
                Gizmos.DrawWireCube(lastHitPosition, lastAttackData.HitboxSize);
                
                Color fillColor = attackGizmosColor;
                fillColor.a = 0.3f;
                Gizmos.color = fillColor;
                Gizmos.DrawCube(lastHitPosition, lastAttackData.HitboxSize);
            }
        }
#endif
    }
}