using UnityEngine;
using MoreHit.Effect;

namespace MoreHit.Attack
{
    /// <summary>
    /// プレイヤーの溜め攻撃（強化された遠距離攻撃）
    /// </summary>
    public class ChargedAttack : ProjectileAttackBase
    {
        protected override void OnBeforeProjectileSpawn(Vector3 position, Vector3 direction)
        {
            if (EffectFactory.I != null)
            {
                var effect = EffectFactory.I.CreateEffect(EffectType.ChargeEffect, position);
                if (effect != null)
                {
                    float duration = EffectFactory.I.GetEffectDuration(EffectType.ChargeEffect);
                    EffectFactory.I.ReturnEffectDelayed(effect, duration);
                }
            }
        }
    }
}