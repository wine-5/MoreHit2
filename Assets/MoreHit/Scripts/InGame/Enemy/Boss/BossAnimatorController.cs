using UnityEngine;

namespace MoreHit.Enemy
{
    /// <summary>
    /// Bossのアニメーション制御
    /// Idle、Walk、Attackの状態を管理
    /// </summary>
    public class BossAnimatorController : MonoBehaviour
    {        
        private Animator animator;
        private Rigidbody2D rb;
        
        // Animatorパラメータ名
        private static readonly int VelocityHash = Animator.StringToHash("Velocity");
        private static readonly int IsAttackingHash = Animator.StringToHash("IsAttacking");
        private static readonly int AttackTriggerHash = Animator.StringToHash("Attack");
        
        private void Awake()
        {
            animator = GetComponent<Animator>();
            rb = GetComponent<Rigidbody2D>();
        }
        
        private void Update()
        {
            UpdateMovementAnimation();
        }
        
        private void UpdateMovementAnimation()
        {
            if (animator == null || rb == null)
                return;
            
            float velocity = rb.linearVelocity.magnitude;
            animator.SetFloat(VelocityHash, velocity);
        }
        
        public void PlayAttack()
        {
            if (animator == null)
                return;
            
            animator.SetBool(IsAttackingHash, true);
            animator.SetTrigger(AttackTriggerHash);
        }
        
        public void StopAttack()
        {
            if (animator == null)
                return;
            
            animator.SetBool(IsAttackingHash, false);
        }
        
        public void ResetAnimation()
        {
            if (animator == null)
                return;
            
            animator.SetFloat(VelocityHash, 0f);
            animator.SetBool(IsAttackingHash, false);
        }
    }
}
