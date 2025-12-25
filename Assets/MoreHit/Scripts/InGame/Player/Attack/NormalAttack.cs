using UnityEngine;
using System.Collections;
using MoreHit.Player;

namespace MoreHit.Attack
{
    /// <summary>
    /// プレイヤーの通常攻撃（3段コンボ）
    /// </summary>
    public class NormalAttack : MonoBehaviour, IAttack
    {
        [Header("コンボ攻撃設定")]
        [SerializeField] private AttackData[] comboAttacks = new AttackData[3];
        [SerializeField, Min(0)] private float comboResetTime = 1f;
        [SerializeField, Min(0)] private float attackDuration = 0.3f;
        
        [Header("デバッグ表示")]
        [SerializeField] private bool showHitBox = true;
        [SerializeField] private Color hitBoxColor = Color.red;
        
        private PlayerMovement playerMovement;
        
        private int comboIndex = 0;
        private float lastAttackTime;
        private bool isAttacking = false;
        
        public bool CanExecute() => !isAttacking && AttackExecutor.I != null;
        
        private void Awake()
        {
            playerMovement = GetComponentInParent<PlayerMovement>();
        }
        
        public void Execute()
        {
            if (!CanExecute())
                return;
            
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
            if (playerMovement != null)
                return playerMovement.IsFacingRight ? Vector2.right : Vector2.left;
            
            return transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        }
        
        private void AdvanceCombo()
        {
            comboIndex = (comboIndex + 1) % comboAttacks.Length;
            lastAttackTime = Time.time;
        }
        
        public void ResetCombo() => comboIndex = 0;
        
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!showHitBox)
                return;
            
            AttackData currentAttack = GetCurrentAttackData();
            if (currentAttack == null)
                return;
            
            DrawHitBox(currentAttack, hitBoxColor);
        }
        
        private void OnDrawGizmosSelected()
        {
            if (!showHitBox)
                return;
            
            if (comboAttacks != null)
            {
                for (int i = 0; i < comboAttacks.Length; i++)
                {
                    if (comboAttacks[i] == null)
                        continue;
                    
                    Color gizmoColor = Color.Lerp(Color.yellow, Color.red, (float)i / (comboAttacks.Length - 1));
                    gizmoColor.a = 0.3f;
                    DrawHitBox(comboAttacks[i], gizmoColor);
                }
            }
        }
        
        private void DrawHitBox(AttackData attackData, Color color)
        {
            Vector2 direction = GetAttackDirection();
            Vector3 hitPosition = transform.position + (Vector3)direction * attackData.Range;
            
            Gizmos.color = color;
            Gizmos.DrawWireCube(hitPosition, attackData.HitboxSize);
            
            color.a = 0.2f;
            Gizmos.color = color;
            Gizmos.DrawCube(hitPosition, attackData.HitboxSize);
            
            Gizmos.color = Color.white;
            Gizmos.DrawLine(transform.position, hitPosition);
        }
#endif
    }
}