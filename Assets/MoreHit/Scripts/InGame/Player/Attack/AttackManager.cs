using UnityEngine;
using MoreHit.Attack;
using MoreHit.Events;

namespace MoreHit.Player
{
    /// <summary>
    /// プレイヤーの攻撃システム統括クラス
    /// </summary>
    public class AttackManager : MonoBehaviour
    {
        [Header("攻撃コンポーネント")]
        [SerializeField] private NormalAttack normalAttack;
        [SerializeField] private RangedAttack rangedAttack;
        [SerializeField] private StockSystem stockSystem;
        
        // プロパティ
        public int CurrentStock => stockSystem?.CurrentStock ?? 0;
        public int MaxStock => stockSystem?.MaxStock ?? 0;
        public bool IsStockFull => stockSystem?.IsFull ?? false;
        
        /// <summary>
        /// 通常攻撃を実行
        /// </summary>
        public void ExecuteNormalAttack()
        {
            normalAttack?.Execute();
        }
        
        /// <summary>
        /// 遠距離攻撃を実行
        /// </summary>
        public void ExecuteRangedAttack()
        {
            rangedAttack?.Execute();
        }
        
        /// <summary>
        /// ストックが使用可能かチェック
        /// </summary>
        public bool CanUseStock(int amount)
        {
            return stockSystem?.CanUseStock(amount) ?? false;
        }
        
        /// <summary>
        /// ストックを使用
        /// </summary>
        public bool UseStock(int amount)
        {
            return stockSystem?.UseStock(amount) ?? false;
        }
        
        /// <summary>
        /// ストックを追加
        /// </summary>
        public void AddStock(int amount)
        {
            stockSystem?.AddStock(amount);
        }
        
        /// <summary>
        /// ストックをクリア
        /// </summary>
        public void ClearStock()
        {
            stockSystem?.ClearStock();
        }
        
        /// <summary>
        /// 通常攻撃のコンボリセット
        /// </summary>
        public void ResetNormalAttackCombo()
        {
            normalAttack?.ResetCombo();
        }
    }
}
