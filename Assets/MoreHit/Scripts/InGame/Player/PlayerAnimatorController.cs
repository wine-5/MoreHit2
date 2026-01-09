using UnityEngine;

namespace MoreHit.Player
{
    public class PlayerAnimatorController : MonoBehaviour
    {
        private Animator animator;
        private PlayerController playerController;
        private Rigidbody2D rb;
        private PlayerMovement playerMovement;

        void Awake()
        {
            animator = GetComponent<Animator>();
            playerController = GetComponent<PlayerController>();
            rb = GetComponent<Rigidbody2D>();
            playerMovement = GetComponent<PlayerMovement>();
        }

        void Update()
        {
            // Animatorコンポーネントの存在チェック
            if (animator == null) return;

            // 左右移動判定
            animator.SetBool("isWalking", playerMovement.IsWalking);

            // 接地判定
            animator.SetBool("isGrounded", playerMovement.IsGrounded);

            // Y方向速度
            animator.SetFloat("yVelocity", rb.linearVelocity.y);
        }

        /// <summary>
        /// 攻撃アニメーション再生（全攻撃共通）
        /// </summary>
        public void PlayAttackAnimation()
        {
            if (animator != null)
                animator.SetTrigger("NormalAttack");
        }

        /// <summary>
        /// 攻撃アニメーションが再生中かどうか
        /// </summary>
        public bool IsAttacking()
        {
            if (animator == null) return false;
            
            AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);
            return currentState.IsName("PlayerAttack");
        }

        /// <summary>
        /// 現在の攻撃アニメーションの進行状況を取得（0-1）
        /// </summary>
        public float GetAttackAnimationProgress()
        {
            if (animator == null || !IsAttacking()) return 0f;
            
            AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);
            return currentState.normalizedTime;
        }
    }
}
