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
            // EffectFactoryでチャージ攻撃エフェクトを生成
            if (EffectFactory.Instance != null)
            {
                var effect = EffectFactory.Instance.CreateEffect(EffectType.ChargeAttackEffect, position);
                if (effect != null)
                    EffectFactory.Instance.ReturnEffectDelayed(effect, 2f);
            }
        }
    }
}