using UnityEngine;
using MoreHit.Attack;

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
        [SerializeField] private ChargedAttack chargedAttack;
        
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
        /// 溜め攻撃を実行
        /// </summary>
        public void ExecuteChargedAttack()
        {
            chargedAttack?.Execute();
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