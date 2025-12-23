using UnityEngine;
using MoreHit.Events;
using MoreHit.Enemy;

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
            // PlayerDataからパラメータを取得
            if (playerData != null)
            {
                maxHealth = playerData.MaxHealth;
                invincibleDuration = playerData.InvincibleTimeAfterDamage;
                currentHealth = maxHealth;
            }
            else
            {
                Debug.LogError("PlayerDataが設定されていません！");
                maxHealth = 100;
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
        public void TakeDamage(float damage)
        {
            // 無敵状態または既に死亡している場合はダメージを受けない
            if (isInvincible || !isAlive)
                return;

            // ダメージを適用
            int damageAmount = Mathf.CeilToInt(damage);
            currentHealth = Mathf.Max(0, currentHealth - damageAmount);

            // 無敵時間を開始
            StartInvincible();

            // HP0で死亡処理
            if (currentHealth <= 0)
                Die();

            Debug.Log($"プレイヤーがダメージを受けた: {damage} (残りHP: {currentHealth}/{maxHealth})");
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
            if (isInvincible)
            {
                invincibleTimer -= Time.deltaTime;
                if (invincibleTimer <= 0f)
                {
                    isInvincible = false;
                    invincibleTimer = 0f;
                }
            }
        }

        /// <summary>
        /// 死亡処理
        /// </summary>
        private void Die()
        {
            if (!isAlive) return;
                

            isAlive = false;
            Debug.Log("プレイヤーが死亡しました");

            // GameEventsを通じて死亡イベント発火
            GameEvents.TriggerPlayerDeath();
        }
    }
}
