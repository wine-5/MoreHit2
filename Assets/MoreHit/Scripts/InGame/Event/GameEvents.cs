using System;
using UnityEngine;

namespace MoreHit.Events
{
    /// <summary>
    /// ゲーム全体のイベントを管理する静的クラス
    /// 各システムがここに定義されたイベントを購読・発火する
    /// </summary>
    public static class GameEvents
    {
        /// <summary>
        /// プレイヤーが死亡した時
        /// </summary>
        public static event Action OnPlayerDeath;
        
        /// <summary>
        /// プレイヤーがダメージを受けた時
        /// </summary>
        public static event Action<int, int> OnPlayerDamage;
        
        /// <summary>
        /// プレイヤーの無敵時間が終了した時
        /// </summary>
        public static event Action OnPlayerInvincibilityEnded;
        
        /// <summary>
        /// ストックが満タンになった時
        /// </summary>
        public static event Action<GameObject> OnStockFull;
        
        /// <summary>
        /// ストック数が変更された時
        /// </summary>
        public static event Action<int, int> OnStockChanged;
        
        /// <summary>
        /// ボスが出現した時
        /// </summary>
        public static event Action OnBossAppear;
        
        /// <summary>
        /// ボスが敗北した時
        /// </summary>
        public static event Action OnBossDefeated;
        
        /// <summary>
        /// ボスがダメージを受けた時
        /// </summary>
        public static event Action<int> OnBossDamaged;
        
        /// <summary>
        /// 敵がダメージを受けた時
        /// </summary>
        public static event Action<GameObject, int> OnEnemyDamaged;
        
        /// <summary>
        /// 敵が倒された時
        /// </summary>
        public static event Action<GameObject> OnEnemyDefeated;

        public static void TriggerPlayerDeath()
        {
            OnPlayerDeath?.Invoke();
        }
        
        public static void TriggerPlayerDamage(int damage, int currentHealth)
        {
            OnPlayerDamage?.Invoke(damage, currentHealth);
        }
        
        public static void TriggerPlayerInvincibilityEnded()
        {
            OnPlayerInvincibilityEnded?.Invoke();
        }

        public static void TriggerStockFull(GameObject target)
        {
            OnStockFull?.Invoke(target);
        }
        
        public static void TriggerStockChanged(int currentStock, int maxStock)
        {
            OnStockChanged?.Invoke(currentStock, maxStock);
        }
        
        public static void TriggerBossAppear()
        {
            OnBossAppear?.Invoke();
        }
        
        public static void TriggerBossDefeated()
        {
            OnBossDefeated?.Invoke();
        }
        
        public static void TriggerBossDamaged(int damage)
        {
            OnBossDamaged?.Invoke(damage);
        }
        
        public static void TriggerEnemyDamaged(GameObject enemy, int damage)
        {
            OnEnemyDamaged?.Invoke(enemy, damage);
        }
        
        public static void TriggerEnemyDefeated(GameObject enemy)
        {
            OnEnemyDefeated?.Invoke(enemy);
        }

        /// <summary>
        /// 全てのイベントリスナーをクリアする（シーン切り替え時などに使用）
        /// </summary>
        public static void ClearAllEvents()
        {
            OnPlayerDeath = null;
            OnPlayerDamage = null;
            OnPlayerInvincibilityEnded = null;
            OnStockFull = null;
            OnStockChanged = null;
            OnBossAppear = null;
            OnBossDefeated = null;
            OnBossDamaged = null;
            OnEnemyDamaged = null;
            OnEnemyDefeated = null;
        }
    }
}