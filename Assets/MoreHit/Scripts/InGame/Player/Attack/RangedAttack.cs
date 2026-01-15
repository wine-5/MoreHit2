using MoreHit.Audio;
using UnityEngine;

namespace MoreHit.Attack
{
    /// <summary>
    /// プレイヤーの通常遠距離攻撃
    /// </summary>
    public class RangedAttack : ProjectileAttackBase
    {
        protected override void OnAfterProjectileSpawn(GameObject projectileObj)
        {
            base.OnAfterProjectileSpawn(projectileObj);
            
            if (AudioManager.I != null)
                AudioManager.I.PlaySE(SeType.Projectile);
        }
    }
}