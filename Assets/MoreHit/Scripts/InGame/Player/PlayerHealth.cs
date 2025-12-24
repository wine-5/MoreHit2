using UnityEngine;
using MoreHit.Events;
using MoreHit.Attack;

namespace MoreHit.Player
{
    /// <summary>
    /// プレイヤーのHP管理とダメージ処理を行うクラス
    /// </summary>
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        [Header("プレイヤーデータ")]
        [SerializeField] private PlayerData playerData;

        // HP状態
        private int currentHealth;
        private int maxHealth;

        // 無敵状態管理
        private bool isInvincible = false;
        private float invincibleTimer = 0f;
        private float invincibleDuration;

        // 生存状態
        private bool isAlive = true;

        /// <summary>
        /// 現在のHPを取得
        /// </summary>
        public int CurrentHealth => currentHealth;

        /// <summary>
        /// 最大HPを取得
        /// </summary>
        public int MaxHealth => maxHealth;

        /// <summary>
        /// 無敵状態かどうか
        /// </summary>
        public bool IsInvincible => isInvincible;

        /// <summary>
        /// 生存状態かどうか
        /// </summary>
        public bool IsAlive => isAlive;

        private void Awake()
        {
            if (playerData != null)
            {
                maxHealth = playerData.MaxHealth;
                invincibleDuration = playerData.InvincibleTimeAfterDamage;
                currentHealth = maxHealth;
            }
        }

        private void Update()
        {
            UpdateInvincibleState();
        }

        /// <summary>
        /// ダメージを受ける
        /// </summary>
        /// <param name="damage">ダメージ量</param>
        public void TakeDamage(int damage)
        {
            // 無敵状態または既に死亡している場合はダメージを受けない
            if (isInvincible || !isAlive)
                return;

            // ダメージを適用
            currentHealth = Mathf.Max(0, currentHealth - damage);

            // 無敵時間を開始
            StartInvincible();

            if (currentHealth <= 0)
                Die();
        }

        /// <summary>
        /// 無敵状態を開始する
        /// </summary>
        private void StartInvincible()
        {
            isInvincible = true;
            invincibleTimer = invincibleDuration;
        }

        /// <summary>
        /// 無敵状態の更新
        /// </summary>
        private void UpdateInvincibleState()
        {
            if (!isInvincible)
                return;
            
            invincibleTimer -= Time.deltaTime;
            if (invincibleTimer <= 0f)
            {
                isInvincible = false;
                invincibleTimer = 0f;
            }
        }

        /// <summary>
        /// 死亡処理
        /// </summary>
        private void Die()
        {
            if (!isAlive) return;
                

            isAlive = false;
            GameEvents.TriggerPlayerDeath();
        }
    }
}
