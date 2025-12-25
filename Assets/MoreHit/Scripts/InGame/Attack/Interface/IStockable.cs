namespace MoreHit.Attack
{
    /// <summary>
    /// ストック蓄積可能なエンティティ
    /// </summary>
    public interface IStockable
    {
        /// <summary>ストックを追加</summary>
        void AddStock(int amount);
        
        /// <summary>ストックをクリア</summary>
        void ClearStock();
    }
}