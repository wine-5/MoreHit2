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
        [Tooltip("プレイヤーの名前")]
        [SerializeField] private string playerName = "Player";

        [Header("移動設定")]
        [Tooltip("最大移動速度")]
        [SerializeField] private float moveSpeed = 8f;
        [Tooltip("加速度（速度が上がるスピード）")]
        [SerializeField] private float acceleration = 10f;
        [Tooltip("減速度（速度が下がるスピード）")]
        [SerializeField] private float deceleration = 10f;

        [Header("ジャンプ設定")]
        [Tooltip("ジャンプの初速度")]
        [SerializeField] private float jumpForce = 15f;
        [Tooltip("ジャンプボタンを離した時の速度減衰率（短押し対応）")]
        [SerializeField] private float jumpCutMultiplier = 0.5f;
        [Tooltip("落下時の重力倍率（落下を速くする）")]
        [SerializeField] private float fallGravityMultiplier = 2.5f;
        [Tooltip("最大ジャンプ回数（2で2段ジャンプ）")]
        [SerializeField] private int maxJumpCount = 2;

        [Header("バックステップ設定")]
        [Tooltip("バックステップで移動する距離")]
        [SerializeField] private float backstepDistance = 3f;
        [Tooltip("バックステップの移動時間（秒）")]
        [SerializeField] private float backstepDuration = 0.2f;
        [Tooltip("バックステップのクールタイム（秒）")]
        [SerializeField] private float backstepCooldown = 0.5f;
        [Tooltip("バックステップ中の無敵時間（秒）")]
        [SerializeField] private float backstepInvincibleTime = 0.15f;

        [Header("HP設定")]
        [Tooltip("プレイヤーの最大HP")]
        [SerializeField] private int maxHealth = 100;
        [Tooltip("ダメージを受けた後の無敵時間（秒）")]
        [SerializeField] private float invincibleTimeAfterDamage = 1.0f;

        [Header("ストック設定")]
        [Tooltip("最大ストック数")]
        [SerializeField] private int maxStock = 99;

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
        
        // ストック設定のプロパティ
        public int MaxStock => maxStock;
    }
}
