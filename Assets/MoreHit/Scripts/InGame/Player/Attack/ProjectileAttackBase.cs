using UnityEngine;
using UnityEngine.InputSystem;

namespace MoreHit.Attack
{
    /// <summary>
    /// 弾丸発射系攻撃の基底クラス
    /// RangedAttackとChargedAttackの共通処理をまとめる
    /// </summary>
    public abstract class ProjectileAttackBase : MonoBehaviour, IAttack
    {
        private const float MIN_DIRECTION_MAGNITUDE = 0.1f;
        
        [Header("射撃設定")]
        [SerializeField] protected ProjectileData projectileData;
        [SerializeField] protected GameObject projectilePrefab;
        [SerializeField] protected Transform firePoint;
        [SerializeField, Min(0)] protected float cooldownTime = 0.3f;
        
        protected Camera playerCamera;
        protected float lastFireTime;
        
        protected virtual void Awake()
        {
            playerCamera = Camera.main;
        }
        
        public virtual bool CanExecute()
        {
            return projectileData != null && 
                   projectilePrefab != null && 
                   Time.time - lastFireTime >= cooldownTime;
        }
        
        public virtual void Execute()
        {
            if (!CanExecute())
                return;
            
            Vector3 direction = CalculateFireDirection();
            FireProjectile(direction);
            
            lastFireTime = Time.time;
        }
        
        protected Vector3 CalculateFireDirection()
        {
            Vector3 mouseWorldPos = GetMouseWorldPosition();
            Vector3 firePos = GetFirePosition();
            Vector3 direction = (mouseWorldPos - firePos).normalized;
            
            if (direction.magnitude < MIN_DIRECTION_MAGNITUDE)
                return Vector3.right;
            
            return direction;
        }
        
        protected Vector3 GetMouseWorldPosition()
        {
            if (playerCamera == null || Mouse.current == null)
                return transform.position + Vector3.right;
            
            Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
            Vector3 mouseWorldPos = playerCamera.ScreenToWorldPoint(
                new Vector3(mouseScreenPos.x, mouseScreenPos.y, playerCamera.WorldToScreenPoint(transform.position).z)
            );
            
            mouseWorldPos.z = transform.position.z;
            return mouseWorldPos;
        }
        
        protected Vector3 GetFirePosition()
        {
            return firePoint != null ? firePoint.position : transform.position;
        }
        
        protected virtual void FireProjectile(Vector3 direction)
        {
            Vector3 firePos = GetFirePosition();
            Quaternion rotation = CalculateProjectileRotation(direction);
            
            OnBeforeProjectileSpawn(firePos, rotation);
            
            GameObject projectileObj = Instantiate(projectilePrefab, firePos, rotation);
            InitializeProjectile(projectileObj, direction);
        }
        
        protected Quaternion CalculateProjectileRotation(Vector3 direction)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            return Quaternion.Euler(0, 0, angle);
        }
        
        protected void InitializeProjectile(GameObject projectileObj, Vector3 direction)
        {
            var projectile = projectileObj.GetComponent<Projectile>();
            projectile?.Initialize(projectileData, direction, gameObject);
        }
        
        /// <summary>
        /// 弾丸生成前の処理（エフェクト生成など）をオーバーライド可能
        /// </summary>
        protected virtual void OnBeforeProjectileSpawn(Vector3 position, Quaternion rotation)
        {
        }
    }
}