using UnityEngine;
using UnityEngine.InputSystem;

namespace MoreHit.Attack
{
    public class RangedAttack : MonoBehaviour, IAttack
    {
        private const float MIN_DIRECTION_MAGNITUDE = 0.1f;
        
        [Header("射撃設定")]
        [SerializeField] private ProjectileData projectileData;
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField, Min(0)] private float cooldownTime = 0.3f;
        
        private Camera playerCamera;
        private float lastFireTime;
        
        private void Awake()
        {
            playerCamera = Camera.main;
        }
        
        public bool CanExecute()
        {
            return projectileData != null && 
                   projectilePrefab != null && 
                   Time.time - lastFireTime >= cooldownTime;
        }
        
        public void Execute()
        {
            if (!CanExecute()) return;
            
            Vector3 direction = CalculateFireDirection();
            FireProjectile(direction);
            
            lastFireTime = Time.time;
        }
        
        private Vector3 CalculateFireDirection()
        {
            Vector3 mouseWorldPos = GetMouseWorldPosition();
            Vector3 firePos = GetFirePosition();
            Vector3 direction = (mouseWorldPos - firePos).normalized;
            
            if (direction.magnitude < MIN_DIRECTION_MAGNITUDE)
                return Vector3.right;
            
            return direction;
        }
        
        private Vector3 GetMouseWorldPosition()
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
        
        private Vector3 GetFirePosition()
        {
            return firePoint != null ? firePoint.position : transform.position;
        }
        
        private void FireProjectile(Vector3 direction)
        {
            Vector3 firePos = GetFirePosition();
            Quaternion rotation = CalculateProjectileRotation(direction);
            
            GameObject projectileObj = Instantiate(projectilePrefab, firePos, rotation);
            InitializeProjectile(projectileObj, direction);
        }
        
        private Quaternion CalculateProjectileRotation(Vector3 direction)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            return Quaternion.Euler(0, 0, angle);
        }
        
        private void InitializeProjectile(GameObject projectileObj, Vector3 direction)
        {
            var projectile = projectileObj.GetComponent<Projectile>();
            projectile?.Initialize(projectileData, direction, gameObject);
        }
    }
}