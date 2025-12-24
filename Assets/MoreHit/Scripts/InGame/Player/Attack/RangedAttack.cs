using UnityEngine;
using UnityEngine.InputSystem;
using MoreHit.Attack;

namespace MoreHit
{
    public class RangedAttack : MonoBehaviour, IAttack
    {
        [Header("射撃設定")]
        [SerializeField] private ProjectileData projectileData;
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private float cooldownTime = 0.3f;
        
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
            
            Vector3 mouseWorldPos = GetMouseWorldPosition();
            Vector3 firePos = GetFirePosition();
            Vector3 direction = (mouseWorldPos - firePos).normalized;
            
            // 無効な方向をチェック
            if (direction.magnitude < 0.1f)
            {
                Debug.LogWarning("Invalid direction calculated. Using right direction as fallback.");
                direction = Vector3.right;
            }
            
            Debug.Log($"Mouse World: {mouseWorldPos}, Fire Pos: {firePos}, Direction: {direction}");
            
            FireProjectile(direction);
            lastFireTime = Time.time;
        }
        
        private Vector3 GetMouseWorldPosition()
        {
            if (playerCamera == null || Mouse.current == null)
                return transform.position + Vector3.right;
            
            Vector3 mouseScreenPos = Mouse.current.position.ReadValue();
            
            // 2Dゲームの場合、プレイヤーと同じZ平面にマウス位置を投影
            Vector3 playerScreenPos = playerCamera.WorldToScreenPoint(transform.position);
            mouseScreenPos.z = playerScreenPos.z;
            
            Vector3 mouseWorldPos = playerCamera.ScreenToWorldPoint(mouseScreenPos);
            
            // 2Dの場合、Z座標はプレイヤーと同じにする
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
            GameObject projectileObj = Instantiate(projectilePrefab, firePos, Quaternion.identity);
            
            var projectile = projectileObj.GetComponent<Projectile>();
            if (projectile != null)
                projectile.Initialize(projectileData, direction, gameObject);
            
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            projectileObj.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }
}
