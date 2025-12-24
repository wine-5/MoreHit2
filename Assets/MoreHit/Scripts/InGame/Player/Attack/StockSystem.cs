using UnityEngine;
using MoreHit.Attack;
using MoreHit.Events;

namespace MoreHit.Player
{
    public class StockSystem : MonoBehaviour, IStockable
    {
        [Header("プレイヤーデータ")]
        [SerializeField] private PlayerData playerData;
        
        private int currentStock = 0;
        
        public int CurrentStock => currentStock;
        public int MaxStock => playerData != null ? playerData.MaxStock : 99;
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
        
        public bool CanUseStock(int cost) => currentStock >= cost;
        
        public bool UseStock(int cost)
        {
            if (!CanUseStock(cost)) return false;
            
            currentStock -= cost;
            GameEvents.TriggerStockChanged(currentStock, MaxStock);
            return true;
        }
        
        public void ClearStock()
        {
            if (currentStock == 0) return;
            
            currentStock = 0;
            GameEvents.TriggerStockChanged(currentStock, MaxStock);
        }
    }
}