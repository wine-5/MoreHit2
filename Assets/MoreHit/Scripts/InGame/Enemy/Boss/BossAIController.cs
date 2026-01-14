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
            
            Debug.Log($"[BossAIController] Start: bossEnemy={bossEnemy != null}, attackData={attackData != null}, player={playerTransform != null}");
            
            if (attackData != null)
            {
                Debug.Log($"[BossAIController] attackData値: meleeRange={attackData.meleeAttackRange}, rangedRange={attackData.rangedAttackRange}, cooldown={attackData.baseAttackCooldown}");
            }
            else
            {
                Debug.LogError("[BossAIController] attackDataがnull！");
            }
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
            if (Time.frameCount % 300 == 0)
                Debug.Log($"[BossAIController] CanAttack: {canAttack} (cooldown={Time.time - lastAttackTime}/{attackData.baseAttackCooldown})");
            
            return canAttack;
        }
        
        /// <summary>
        /// 最適な攻撃パターンを選択
        /// </summary>
        public BossAttackPattern SelectAttackPattern()
        {
            if (attackData == null || playerTransform == null || bossEnemy == null)
            {
                Debug.LogWarning($"[BossAIController] SelectAttackPattern: 不正な状態 - attackData={attackData != null}, player={playerTransform != null}, bossEnemy={bossEnemy != null}");
                return BossAttackPattern.RotatingAttack;
            }
            
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            float hpRatio = bossEnemy.GetHPRatio();
            
            Debug.Log($"[BossAIController] SelectAttackPattern: distance={distanceToPlayer:F2}, HP割合={hpRatio:F2}");
            
            if (hpRatio <= attackData.groundSlamHPThreshold)
            {
                Debug.Log($"[BossAIController] HPが低い ({hpRatio:F2} <= {attackData.groundSlamHPThreshold})");
                if (Random.value < 0.3f)
                {
                    Debug.Log("[BossAIController] 選択: FireballBarrage");
                    return BossAttackPattern.FireballBarrage;
                }
            }
            
            BossAttackPattern pattern;
            if (distanceToPlayer <= attackData.meleeAttackRange)
            {
                pattern = BossAttackPattern.RotatingAttack;
                Debug.Log($"[BossAIController] 近距離 ({distanceToPlayer:F2} <= {attackData.meleeAttackRange}) - 選択: {pattern}");
            }
            else if (distanceToPlayer <= attackData.rangedAttackRange)
            {
                pattern = BossAttackPattern.FireballBarrage;
                Debug.Log($"[BossAIController] 中距離 ({distanceToPlayer:F2} <= {attackData.rangedAttackRange}) - 選択: {pattern}");
            }
            else
            {
                pattern = BossAttackPattern.RotatingAttack;
                Debug.Log($"[BossAIController] 遠距離 ({distanceToPlayer:F2} > {attackData.rangedAttackRange}) - デフォルト: {pattern}");
            }
            
            return pattern;
        }
        
        /// <summary>
        /// 攻撃実行時に呼び出し
        /// </summary>
        public void OnAttackExecuted()
        {
            lastAttackTime = Time.time;
        }
        
        /// <summary>
        /// プレイヤーへの方向を取得
        /// </summary>
        public Vector2 GetDirectionToPlayer()
        {
            if (playerTransform == null)
                return Vector2.right;
            
            return (playerTransform.position - transform.position).normalized;
        }
        
        /// <summary>
        /// プレイヤーとの距離を取得
        /// </summary>
        public float GetDistanceToPlayer()
        {
            if (playerTransform == null)
                return float.MaxValue;
            
            return Vector2.Distance(transform.position, playerTransform.position);
        }
    }
}
