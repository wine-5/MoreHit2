using UnityEngine;

namespace MoreHit.Pool
{
    /// <summary>
    /// プール可能なオブジェクトが実装すべきインターフェース
    /// </summary>
    public interface IPoolable
    {
        /// <summary>
        /// プールから取得された時に呼ばれる初期化処理
        /// </summary>
        void OnPoolGet();
        
        /// <summary>
        /// プールに返却される時に呼ばれるリセット処理
        /// </summary>
        void OnPoolReturn();
    }
}