using UnityEngine;

namespace MoreHit.Player
{
    /// <summary>
    /// プレイヤーのパラメータを管理するScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerData", menuName = "MoreHit/Player/PlayerData")]
    public class PlayerData : ScriptableObject
    {
        [Header("基本情報")]
        [SerializeField] private string playerName = "Player";
        
        [Header("移動設定")]
        [SerializeField] private float moveSpeed = 8f;
        [SerializeField] private float acceleration = 10f;
        [SerializeField] private float deceleration = 10f;
        
        [Header("ジャンプ設定")]
        [SerializeField] private float jumpForce = 15f;
        [SerializeField] private float jumpCutMultiplier = 0.5f;
        [SerializeField] private float fallGravityMultiplier = 2.5f;
        [SerializeField] private int maxJumpCount = 2;
        
        [Header("バックステップ設定")]
        [SerializeField] private float backstepDistance = 3f;
        [SerializeField] private float backstepDuration = 0.2f;
        [SerializeField] private float backstepCooldown = 0.5f;
        [SerializeField] private float backstepInvincibleTime = 0.15f;
        
        [Header("HP設定")]
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private float invincibleTimeAfterDamage = 1.0f;
        
        [Header("通常攻撃設定")]
        [SerializeField] private float normalAttackDamage = 10f;
        [SerializeField] private float normalAttackRange = 1.5f;
        [SerializeField] private float normalAttackCooldown = 0.3f;
        [SerializeField] private int normalAttackComboMax = 3;
        
        [Header("射撃攻撃設定")]
        [SerializeField] private float rangedAttackDamage = 8f;
        [SerializeField] private float rangedAttackSpeed = 15f;
        [SerializeField] private float rangedAttackCooldown = 0.5f;
        
        [Header("突進攻撃設定")]
        [SerializeField] private float rushAttackDamage = 12f;
        [SerializeField] private float rushAttackDistance = 5f;
        [SerializeField] private float rushAttackDuration = 0.3f;
        [SerializeField] private float rushAttackCooldown = 1.0f;
        
        [Header("溜め攻撃設定")]
        [SerializeField] private float chargeAttackThreshold = 1.0f;
        [SerializeField] private float chargeRushAttackDamage = 30f;
        [SerializeField] private float chargeRangedAttackDamage = 25f;
        [SerializeField] private float chargeAttackKnockbackForce = 20f;
        
        // 移動設定のプロパティ
        public float MoveSpeed => moveSpeed;
        public float Acceleration => acceleration;
        public float Deceleration => deceleration;
        
        // ジャンプ設定のプロパティ
        public float JumpForce => jumpForce;
        public float JumpCutMultiplier => jumpCutMultiplier;
        public float FallGravityMultiplier => fallGravityMultiplier;
        public int MaxJumpCount => maxJumpCount;
        
        // バックステップ設定のプロパティ
        public float BackstepDistance => backstepDistance;
        public float BackstepDuration => backstepDuration;
        public float BackstepCooldown => backstepCooldown;
        public float BackstepInvincibleTime => backstepInvincibleTime;
        
        // HP設定のプロパティ
        public int MaxHealth => maxHealth;
        public float InvincibleTimeAfterDamage => invincibleTimeAfterDamage;
        
        // 通常攻撃設定のプロパティ
        public float NormalAttackDamage => normalAttackDamage;
        public float NormalAttackRange => normalAttackRange;
        public float NormalAttackCooldown => normalAttackCooldown;
        public int NormalAttackComboMax => normalAttackComboMax;
        
        // 射撃攻撃設定のプロパティ
        public float RangedAttackDamage => rangedAttackDamage;
        public float RangedAttackSpeed => rangedAttackSpeed;
        public float RangedAttackCooldown => rangedAttackCooldown;
        
        // 突進攻撃設定のプロパティ
        public float RushAttackDamage => rushAttackDamage;
        public float RushAttackDistance => rushAttackDistance;
        public float RushAttackDuration => rushAttackDuration;
        public float RushAttackCooldown => rushAttackCooldown;
        
        // 溜め攻撃設定のプロパティ
        public float ChargeAttackThreshold => chargeAttackThreshold;
        public float ChargeRushAttackDamage => chargeRushAttackDamage;
        public float ChargeRangedAttackDamage => chargeRangedAttackDamage;
        public float ChargeAttackKnockbackForce => chargeAttackKnockbackForce;
    }
}
