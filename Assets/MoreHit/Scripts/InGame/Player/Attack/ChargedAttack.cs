using MoreHit.Audio;
using UnityEngine;

namespace MoreHit.Attack
{
    /// <summary>
    /// プレイヤーの溜め攻撃（強化された遠距離攻撃）
    /// </summary>
    public class ChargedAttack : ProjectileAttackBase
    {
        protected override void OnAfterProjectileSpawn(GameObject projectileObj)
        {
            base.OnAfterProjectileSpawn(projectileObj);
            
            if (AudioManager.I != null)
                AudioManager.I.PlaySE(SeType.ChargedProjectile);
        }
    }
}