using UnityEngine;

namespace MoreHit.Enemy
{
    /// <summary>
    /// Boss専用の攻撃パターン設定
    /// </summary>
    [CreateAssetMenu(fileName = "BossAttackData", menuName = "MoreHit/Boss/BossAttackData")]
    public class BossAttackDataSO : ScriptableObject
    {
        [Header("近接コンボ攻撃")]
        public BossAttackPatternData meleeCombo;
        
        [Header("火球攻撃")]
        public BossAttackPatternData fireBall;
        
        [Header("地面叩きつけ攻撃")]
        public BossAttackPatternData groundSlam;
        
        [Header("AI設定")]
        [Tooltip("攻撃の基本クールダウン時間")]
        public float baseAttackCooldown = 2f;
        
        [Tooltip("近接攻撃を使用する距離")]
        public float meleeAttackRange = 300f;
        
        [Tooltip("遠距離攻撃を使用する距離")]
        public float rangedAttackRange = 3000f;
        
        [Tooltip("GroundSlamを解禁するHP割合（0-1）")]
        [Range(0f, 1f)]
        public float groundSlamHPThreshold = 0.5f;
    }
    
    /// <summary>
    /// 各攻撃パターンのデータ
    /// </summary>
    [System.Serializable]
    public class BossAttackPatternData
    {
        [Header("基本設定")]
        [Tooltip("攻撃のダメージ量")]
        public float damage = 10f;
        
        [Tooltip("攻撃範囲")]
        public float attackRange = 5f;
        
        [Tooltip("攻撃の持続時間")]
        public float attackDuration = 1f;
        
        [Tooltip("アニメーション時間")]
        public float animationDuration = 1.5f;
        
        [Header("発射物設定（FireBall用）")]
        [Tooltip("発射する弾の数")]
        public int projectileCount = 3;
        
        [Tooltip("発射間隔")]
        public float projectileInterval = 0.3f;
        
        [Tooltip("弾速")]
        public float projectileSpeed = 10f;
    }
}
