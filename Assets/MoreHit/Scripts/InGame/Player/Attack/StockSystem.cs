using UnityEngine;
using MoreHit.Attack;
using MoreHit.Events;

namespace MoreHit.Player
{
    /// <summary>
    /// ストックシステム
    /// 敵がストックを蓄積し、一定量溜まると倒せるようになる
    /// </summary>
    public class StockSystem : MonoBehaviour, IStockable
    {
        [Header("ストック設定")]
        [Tooltip("最大ストック数")]
        [SerializeField] private int maxStock = 99;
        
        private int currentStock = 0;
        
        public int CurrentStock => currentStock;
        public int MaxStock => maxStock;
        public bool IsFull => currentStock >= MaxStock;
        
        public void AddStock(int amount)
        {
            if (amount <= 0) return;
            
            int previousStock = currentStock;
            currentStock = Mathf.Min(currentStock + amount, MaxStock);
            
            if (currentStock != previousStock)
            {
                GameEvents.TriggerStockChanged(currentStock, MaxStock);
                
                if (currentStock >= MaxStock)
                    GameEvents.TriggerStockFull(gameObject);
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