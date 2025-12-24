namespace MoreHit.Attack
{
    /// <summary>
    /// 攻撃クラスの共通インターフェース
    /// </summary>
    public interface IAttack
    {
        /// <summary>攻撃を実行</summary>
        void Execute();
        
        /// <summary>攻撃可能かどうか</summary>
        bool CanExecute();
    }
}
