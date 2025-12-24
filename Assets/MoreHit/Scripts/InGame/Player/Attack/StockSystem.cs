using UnityEngine;
using UnityEngine.Events;
using MoreHit.Attack;
using MoreHit.Events;

namespace MoreHit.Player
{
    public class StockSystem : MonoBehaviour, IStockable
    {
        [Header("プレイヤーデータ")]
        [SerializeField] private PlayerData playerData;
        
        private int currentStock = 0;
        
        [Header("イベント")]
        public UnityEvent<int> OnStockChanged;
        
        public int CurrentStock => currentStock;
        public int MaxStock => playerData != null ? playerData.MaxStock : 99;
        public bool IsFull => currentStock >= MaxStock;
        
        public void AddStock(int amount)
        {
            if (amount <= 0) return;
            
            int newStock = Mathf.Min(currentStock + amount, MaxStock);
            if (newStock != currentStock)
            {
                currentStock = newStock;
                OnStockChanged?.Invoke(currentStock);
                
                if (currentStock >= MaxStock)
                    GameEvents.TriggerStockFull(gameObject);
            }
        }
        
        public bool CanUseStock(int cost)
        {
            return currentStock >= cost;
        }
        
        public bool UseStock(int cost)
        {
            if (!CanUseStock(cost)) return false;
            
            currentStock -= cost;
            OnStockChanged?.Invoke(currentStock);
            return true;
        }
        
        public void ClearStock()
        {
            if (currentStock > 0)
            {
                currentStock = 0;
                OnStockChanged?.Invoke(currentStock);
            }
        }
    }
}
