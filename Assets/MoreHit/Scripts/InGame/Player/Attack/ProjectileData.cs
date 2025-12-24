using UnityEngine;
using MoreHit.Attack;

namespace MoreHit.Attack
{
    [CreateAssetMenu(fileName = "ProjectileData", menuName = "MoreHit/Attack/ProjectileData")]
    public class ProjectileData : AttackData
    {
        [Header("弾固有設定")]
        [SerializeField] private float speed = 15f;
        [SerializeField] private float maxDistance = 20f;
        [SerializeField] private float lifeTime = 5f;
        [SerializeField] private GameObject destroyEffectPrefab;
        
        public float Speed => speed;
        public float MaxDistance => maxDistance;
        public float LifeTime => lifeTime;
        public GameObject DestroyEffectPrefab => destroyEffectPrefab;
    }
}
