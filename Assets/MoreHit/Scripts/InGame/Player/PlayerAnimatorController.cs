using UnityEngine;

namespace MoreHit.Player
{
    /// <summary>
    /// プレイヤーのアニメーション制御を管理するコンポーネント
    /// </summary>
    public class PlayerAnimatorController : MonoBehaviour
    {
        private static readonly int IsWalkingHash = Animator.StringToHash("isWalking");
        private static readonly int IsGroundedHash = Animator.StringToHash("isGrounded");
        private static readonly int YVelocityHash = Animator.StringToHash("yVelocity");
        private static readonly int IsAttackingHash = Animator.StringToHash("isAttacking");

        private const float VELOCITY_THRESHOLD = 0.01f;
        private const string ATTACK_STATE_NAME = "PlayerAttack";

        private Animator animator;
        private Rigidbody2D rb;
        private PlayerMovement playerMovement;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            rb = GetComponent<Rigidbody2D>();
            playerMovement = GetComponent<PlayerMovement>();
        }

        private void Update()
        {
            if (animator == null) return;

            UpdateMovementAnimation();
            UpdateAttackAnimation();
        }

        /// <summary>
        /// 移動関連のアニメーションパラメータを更新
        /// </summary>
        private void UpdateMovementAnimation()
        {
            animator.SetBool(IsWalkingHash, playerMovement.IsWalking);
            animator.SetBool(IsGroundedHash, playerMovement.IsGrounded);

            // Y方向速度（微小な値はゼロとして扱う）
            float yVelocity = Mathf.Abs(rb.linearVelocity.y) < VELOCITY_THRESHOLD 
                ? 0f 
                : rb.linearVelocity.y;
            animator.SetFloat(YVelocityHash, yVelocity);
        }

        /// <summary>
        /// 攻撃アニメーションの状態を更新
        /// </summary>
        private void UpdateAttackAnimation()
        {
            // アニメーションが完全に終了したら攻撃フラグをリセット
            if (IsAttacking() && GetAttackAnimationProgress() >= 1.0f)
                StopAttackAnimation();
        }

        /// <summary>
        /// 攻撃アニメーションを再生
        /// </summary>
        public void PlayAttackAnimation()
        {
            if (animator != null)
                animator.SetBool(IsAttackingHash, true);
        }

        /// <summary>
        /// 攻撃アニメーションを終了
        /// </summary>
        private void StopAttackAnimation()
        {
            if (animator != null)
                animator.SetBool(IsAttackingHash, false);
        }

        /// <summary>
        /// 攻撃アニメーションが再生中かどうか
        /// </summary>
        private bool IsAttacking()
        {
            if (animator == null) return false;

            AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);
            return currentState.IsName(ATTACK_STATE_NAME);
        }

        /// <summary>
        /// 現在の攻撃アニメーションの進行状況を取得（0-1）
        /// </summary>
        private float GetAttackAnimationProgress()
        {
            if (animator == null || !IsAttacking()) return 0f;

            AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);
            return currentState.normalizedTime;
        }
    }
}
