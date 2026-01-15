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
            

            
            if (attackData != null)
            {

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
            
            Debug.Log($"[BossAI] HP比率={hpRatio:F2}, 距離={distanceToPlayer:F2}, 閾値={attackData.groundSlamHPThreshold:F2}");
            
            if (hpRatio <= attackData.groundSlamHPThreshold)
            {
                float randomValue = Random.value;
                Debug.Log($"[BossAI] HP低下モード: ランダム値={randomValue:F2}");
                
                if (randomValue < 0.3f)
                {
                    Debug.Log($"[BossAI] 選択: FireballBarrage (HP低下・30%確率)");
                    return BossAttackPattern.FireballBarrage;
                }

                Debug.Log($"[BossAI] 選択: SpawnMinions/GroundSlam (HP低下・70%確率)");
                return BossAttackPattern.SpawnMinions; // GroundSlam攻撃
            }
            
            BossAttackPattern pattern;
            if (distanceToPlayer <= attackData.meleeAttackRange)
            {
                pattern = BossAttackPattern.RotatingAttack;
                Debug.Log($"[BossAI] 選択: RotatingAttack (近距離={distanceToPlayer:F2} <= {attackData.meleeAttackRange:F2})");
            }
            else if (distanceToPlayer <= attackData.rangedAttackRange)
            {
                pattern = BossAttackPattern.FireballBarrage;
                Debug.Log($"[BossAI] 選択: FireballBarrage (中距離={distanceToPlayer:F2} <= {attackData.rangedAttackRange:F2})");
            }
            else
            {
                pattern = BossAttackPattern.RotatingAttack;
                Debug.Log($"[BossAI] 選択: RotatingAttack (遠距離={distanceToPlayer:F2})");
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
