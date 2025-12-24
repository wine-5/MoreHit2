using UnityEngine;
using System.Collections;

namespace MoreHit.Attack
{
    /// <summary>
    /// プレイヤーの通常攻撃（3段コンボ）
    /// </summary>
    public class NormalAttack : MonoBehaviour, IAttack
    {
        private const int COMBO_COUNT = 3;
        
        [Header("コンボ攻撃設定")]
        [SerializeField] private AttackData[] comboAttacks = new AttackData[COMBO_COUNT];
        [SerializeField, Min(0)] private float comboResetTime = 1f;
        [SerializeField, Min(0)] private float attackDuration = 0.3f;
        
        private int comboIndex = 0;
        private float lastAttackTime;
        private bool isAttacking = false;
        
        public bool CanExecute() => !isAttacking && AttackExecutor.I != null;
        
        public void Execute()
        {
            if (!CanExecute()) return;
            StartCoroutine(AttackRoutine());
        }
        
        private IEnumerator AttackRoutine()
        {
            isAttacking = true;
            
            UpdateComboState();
            
            AttackData attackData = GetCurrentAttackData();
            if (attackData == null)
            {
                isAttacking = false;
                yield break;
            }
            
            ExecuteCurrentAttack(attackData);
            AdvanceCombo();
            
            yield return new WaitForSeconds(attackDuration);
            
            isAttacking = false;
        }
        
        private void UpdateComboState()
        {
            if (Time.time - lastAttackTime > comboResetTime)
                comboIndex = 0;
        }
        
        private AttackData GetCurrentAttackData()
        {
            if (comboAttacks == null || comboAttacks.Length == 0)
                return null;
                
            int index = Mathf.Clamp(comboIndex, 0, comboAttacks.Length - 1);
            return comboAttacks[index];
        }
        
        private void ExecuteCurrentAttack(AttackData attackData)
        {
            Vector2 direction = GetAttackDirection();
            
            AttackExecutor.I.Execute(
                attackData,
                transform.position,
                direction,
                gameObject
            );
        }
        
        private Vector2 GetAttackDirection()
        {
            return transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        }
        
        private void AdvanceCombo()
        {
            comboIndex = (comboIndex + 1) % comboAttacks.Length;
            lastAttackTime = Time.time;
        }
        
        public void ResetCombo() => comboIndex = 0;
    }
}