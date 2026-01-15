using UnityEngine;
using MoreHit.Player;

namespace MoreHit.Enemy
{
    /// <summary>
    /// Boss専用のAI制御
    /// 距離判定、HP閾値による攻撃パターン選択を管理
    /// </summary>
    public class BossAIController : MonoBehaviour
    {
        [Header("参照")]
        [SerializeField] private BossEnemy bossEnemy;
        [SerializeField] private BossAttackDataSO attackData;
        
        private Transform playerTransform;
        private float lastAttackTime;
        
        private void Start()
        {
            if (bossEnemy == null)
                bossEnemy = GetComponent<BossEnemy>();
            
            if (PlayerDataProvider.I != null)
                playerTransform = PlayerDataProvider.I.transform;
            
            if (attackData == null)
                Debug.LogError("[BossAIController] attackDataがnull！");
        }
        
        /// <summary>
        /// 攻撃可能かチェック
        /// </summary>
        public bool CanAttack()
        {
            if (attackData == null)
            {
                if (Time.frameCount % 300 == 0)
                    Debug.LogWarning("[BossAIController] CanAttack: attackDataがnull");
                return false;
            }
            
            bool canAttack = Time.time - lastAttackTime >= attackData.baseAttackCooldown;
            return canAttack;
        }
        
        /// <summary>
        /// 最適な攻撃パターンを選択
        /// </summary>
        public BossAttackPattern SelectAttackPattern()
        {
            if (attackData == null || playerTransform == null || bossEnemy == null)
                return BossAttackPattern.RotatingAttack;
            
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            float hpRatio = bossEnemy.GetHPRatio();
            
            if (hpRatio <= attackData.groundSlamHPThreshold)
            {
                if (Random.value < 0.3f)
                    return BossAttackPattern.FireballBarrage;
                
                return BossAttackPattern.SpawnMinions;
            }
            
            if (distanceToPlayer <= attackData.meleeAttackRange)
                return BossAttackPattern.RotatingAttack;
            
            if (distanceToPlayer <= attackData.rangedAttackRange)
                return BossAttackPattern.FireballBarrage;
            
            return BossAttackPattern.RotatingAttack;
        }
        
        /// <summary>
        /// 攻撃実行時に呼び出し
        /// </summary>
        public void OnAttackExecuted() => lastAttackTime = Time.time;
        
        /// <summary>
        /// プレイヤーへの方向を取得
        /// </summary>
        public Vector2 GetDirectionToPlayer()
        {
            if (playerTransform == null) return Vector2.right;
            return (playerTransform.position - transform.position).normalized;
        }
        
        /// <summary>
        /// プレイヤーとの距離を取得
        /// </summary>
        public float GetDistanceToPlayer()
        {
            if (playerTransform == null) return float.MaxValue;
            return Vector2.Distance(transform.position, playerTransform.position);
        }
    }
}
