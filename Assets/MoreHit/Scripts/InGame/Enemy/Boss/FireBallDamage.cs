using UnityEngine;
using MoreHit.Attack;
using MoreHit.Effect;

namespace MoreHit.Enemy
{
    /// <summary>
    /// FireBallのダメージ判定を行うコンポーネント
    /// IDamageableインターフェースを使って疎結合にダメージを与える
    /// </summary>
    public class FireBallDamage : MonoBehaviour
    {
        private int damage;
        private bool hasHit = false;
        private GameObject shooter;
        
        /// <summary>
        /// ダメージ値を設定する（外部から初期化用）
        /// </summary>
        public void SetDamage(int damageValue)
        {
            damage = damageValue;
        }
        
        /// <summary>
        /// 発射者を設定する
        /// </summary>
        public void SetShooter(GameObject shooterObject)
        {
            shooter = shooterObject;
        }
        
        private void OnTriggerEnter2D(Collider2D collision)
        {
            // 既にヒット済みなら無視
            if (hasHit) return;
            
            // 発射者自身には当たらない
            if (collision.gameObject == shooter)
            {
                return;
            }
            
            // Playerタグのオブジェクトに当たった場合
            if (collision.CompareTag("Player"))
            {
                // IDamageableインターフェースを使って疎結合にダメージを与える
                var damageable = collision.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(damage);
                    hasHit = true;
                    
                    // ヒットエフェクトを生成
                    if (EffectFactory.I != null)
                    {
                        var effect = EffectFactory.I.CreateEffect(EffectType.HitEffect, transform.position);
                        if (effect != null)
                        {
                            float duration = EffectFactory.I.GetEffectDuration(EffectType.HitEffect);
                            EffectFactory.I.ReturnEffectDelayed(effect, duration);
                        }
                    }
                    
                    // FireBallをPoolに戻す
                    if (MoreHit.Attack.FireBallFactory.I != null)
                    {
                        MoreHit.Attack.FireBallFactory.I.ReturnFireBall(gameObject);
                    }
                    else
                    {
                        Destroy(gameObject);
                    }
                }
            }
        }
        
        /// <summary>
        /// FireBallがアクティブになった時にリセット
        /// </summary>
        private void OnEnable()
        {
            hasHit = false;
        }
    }
}
