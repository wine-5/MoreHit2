using UnityEngine;
using MoreHit.Events;

namespace MoreHit.Attack
{
    /// <summary>
    /// ストックシステム
    /// 敵がストックを蓄積し、一定量溜まると倒せるようになる
    /// </summary>
    public class StockSystem : IStockable
    {
        private readonly int maxStock;
        private readonly GameObject ownerObject;
        private int currentStock = 0;
        
        public int CurrentStock => currentStock;
        public int MaxStock => maxStock;
        public bool IsFull => currentStock >= MaxStock;
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
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