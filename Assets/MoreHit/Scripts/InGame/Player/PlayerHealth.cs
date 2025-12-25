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

        private int currentHealth;
        private int maxHealth;

        private bool isInvincible = false;
        private float invincibleTimer = 0f;
        private float invincibleDuration;

        private bool isAlive = true;

        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;
        public bool IsInvincible => isInvincible;
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
        public void TakeDamage(int damage)
        {
            if (isInvincible || !isAlive)
                return;

            currentHealth = Mathf.Max(0, currentHealth - damage);
            
            GameEvents.TriggerPlayerDamage(damage, currentHealth);

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
            if (!isAlive)
                return;

            isAlive = false;
            GameEvents.TriggerPlayerDeath();
        }
    }
}