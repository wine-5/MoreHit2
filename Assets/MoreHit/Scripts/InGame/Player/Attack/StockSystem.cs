
using UnityEngine;
using MoreHit.Attack;
using MoreHit.Events;

namespace MoreHit.Player
{
    /// <summary>
    /// ストックシステム
    /// 敵がストックを蓄積し、一定量溜まると倒せるようになる
    /// </summary>
    public class StockSystem : IStockable
    {
        private readonly int maxStock;
        private readonly GameObject ownerObject; // StockFullイベント用
        private int currentStock = 0;
        
        public int CurrentStock => currentStock;
        public int MaxStock => maxStock;
        public bool IsFull => currentStock >= MaxStock;
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="maxStock">最大ストック数</param>
        /// <param name="ownerObject">所有者のGameObject（イベント通知用）</param>
        public StockSystem(int maxStock, GameObject ownerObject)
        {
            this.maxStock = maxStock;
            this.ownerObject = ownerObject;
        }
        
        public void AddStock(int amount)
        {
            if (amount <= 0) return;
            
            int previousStock = currentStock;
            currentStock = Mathf.Min(currentStock + amount, MaxStock);
            
            if (currentStock != previousStock)
            {
                GameEvents.TriggerStockChanged(currentStock, MaxStock);
                
                if (currentStock >= MaxStock)
                    GameEvents.TriggerStockFull(ownerObject);
            }
        }
        
        public void ClearStock()
        {
            if (currentStock == 0) return;
            
            currentStock = 0;
            GameEvents.TriggerStockChanged(currentStock, MaxStock);
        }
        
        public bool CanUseStock(int amount)
        {
            return currentStock >= amount;
        }
    }
}