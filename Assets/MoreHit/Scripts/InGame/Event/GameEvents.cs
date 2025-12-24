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
        public static event Action<GameObject> OnStockFull;


        public static void TriggerPlayerDeath()
        {
            OnPlayerDeath?.Invoke();
        }

        public static void TriggerStockFull(GameObject target)
        {
            OnStockFull?.Invoke(target);
        }


        /// <summary>
        /// 全てのイベントリスナーをクリアする（シーン切り替え時などに使用）
        /// </summary>
        public static void ClearAllEvents()
        {
            OnPlayerDeath = null;
        }
    }
}
