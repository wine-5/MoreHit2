using UnityEngine;
using UnityEngine.InputSystem;
using MoreHit.Attack;

namespace MoreHit.Player
{
    /// <summary>
    /// プレイヤーの溜め攻撃（強化された遠距離攻撃）
    /// </summary>
    public class ChargedAttack : MonoBehaviour, IAttack
    {
        private const float MIN_DIRECTION_MAGNITUDE = 0.1f;
        
        [Header("射撃設定")]
        [SerializeField] private ProjectileData projectileData;  // 弾丸の設定データ
        [SerializeField] private GameObject projectilePrefab;     // 発射する弾丸Prefab  
        [SerializeField] private Transform firePoint;
        [SerializeField, Min(0)] private float cooldownTime = 0.5f;
        
        [Header("溜め攻撃エフェクト")]
        [SerializeField] private GameObject chargeEffect;         // 溜め攻撃発射時のエフェクト
        
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
            
            // 溜め攻撃エフェクトを生成
            if (chargeEffect != null)
            {
                Instantiate(chargeEffect, firePos, rotation);
            }
            
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
