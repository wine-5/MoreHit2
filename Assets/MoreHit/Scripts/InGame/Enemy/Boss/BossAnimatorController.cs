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
        
        /// <summary>
        /// 移動アニメーションの更新（Idle ⇔ Walk）
        /// </summary>
        private void UpdateMovementAnimation()
        {
            if (animator == null || rb == null) return;
            
            float velocity = rb.linearVelocity.magnitude;
            animator.SetFloat(VelocityHash, velocity);
        }
        
        /// <summary>
        /// 攻撃アニメーションの再生
        /// </summary>
        public void PlayAttack()
        {
            if (animator == null) return;
            
            animator.SetBool(IsAttackingHash, true);
            animator.SetTrigger(AttackTriggerHash);
        }
        
        /// <summary>
        /// 攻撃アニメーションの終了
        /// </summary>
        public void StopAttack()
        {
            if (animator == null) return;
            
            animator.SetBool(IsAttackingHash, false);
        }
        
        /// <summary>
        /// アニメーション状態の強制リセット
        /// </summary>
        public void ResetAnimation()
        {
            if (animator == null) return;
            
            animator.SetFloat(VelocityHash, 0f);
            animator.SetBool(IsAttackingHash, false);
        }
    }
}
