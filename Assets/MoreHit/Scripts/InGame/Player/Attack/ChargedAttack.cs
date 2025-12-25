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
        
        protected override void OnBeforeProjectileSpawn(Vector3 position, Quaternion rotation)
        {
            if (chargeEffect != null)
                Instantiate(chargeEffect, position, rotation);
        }
    }
}