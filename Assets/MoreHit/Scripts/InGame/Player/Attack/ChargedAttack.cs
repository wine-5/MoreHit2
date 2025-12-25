using UnityEngine;

namespace MoreHit.Attack
{
    /// <summary>
    /// プレイヤーの溜め攻撃（強化された遠距離攻撃）
    /// </summary>
    public class ChargedAttack : ProjectileAttackBase
    {
        [Header("溜め攻撃エフェクト")]
        [SerializeField] private GameObject chargeEffect;
        
        protected override void OnBeforeProjectileSpawn(Vector3 position, Vector3 direction)
        {
            // エフェクトは後で別クラスで実装する
            // TODO: EffectFactoryで処理する
        }
    }
}