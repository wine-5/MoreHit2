using UnityEngine;
using MoreHit.Attack;
using UnityEngine.Events;

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
        
        [Header("攻撃実行イベント")]
        public UnityEvent onNormalAttackExecuted;
        public UnityEvent onRangedAttackExecuted;
        public UnityEvent onChargedAttackExecuted;
        
        /// <summary>
        /// 通常攻撃を実行
        /// </summary>
        public void ExecuteNormalAttack()
        {
            if (normalAttack == null) return;
            normalAttack.Execute();
            onNormalAttackExecuted?.Invoke();
        }
        
        /// <summary>
        /// 遠距離攻撃を実行
        /// </summary>
        public void ExecuteRangedAttack()
        {
            if (rangedAttack == null) return;
            rangedAttack.Execute();
            onRangedAttackExecuted?.Invoke();
        }
        
        /// <summary>
        /// 溜め攻撃を実行
        /// </summary>
        public void ExecuteChargedAttack()
        {
            if (chargedAttack == null) return;
            chargedAttack.Execute();
            onChargedAttackExecuted?.Invoke();
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