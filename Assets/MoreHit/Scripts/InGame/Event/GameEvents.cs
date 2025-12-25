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
        /// ストックが満タンになった時
        /// </summary>
        public static event Action<GameObject> OnStockFull;
        
        /// <summary>
        /// ストック数が変更された時
        /// </summary>
        public static event Action<int, int> OnStockChanged;

        public static void TriggerPlayerDeath()
        {
            OnPlayerDeath?.Invoke();
        }
        
        public static void TriggerPlayerDamage(int damage, int currentHealth)
        {
            OnPlayerDamage?.Invoke(damage, currentHealth);
        }

        public static void TriggerStockFull(GameObject target)
        {
            OnStockFull?.Invoke(target);
        }
        
        public static void TriggerStockChanged(int currentStock, int maxStock)
        {
            OnStockChanged?.Invoke(currentStock, maxStock);
        }

        /// <summary>
        /// 全てのイベントリスナーをクリアする（シーン切り替え時などに使用）
        /// </summary>
        public static void ClearAllEvents()
        {
            OnPlayerDeath = null;
            OnPlayerDamage = null;
            OnStockFull = null;
            OnStockChanged = null;
        }
    }
}