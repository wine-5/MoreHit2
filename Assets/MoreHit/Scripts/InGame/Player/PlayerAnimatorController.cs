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
            // 左右移動判定
            animator.SetBool("isWalking", playerMovement.IsWalking);

            // 接地判定
            animator.SetBool("isGrounded", playerMovement.IsGrounded);

            // Y方向速度
            animator.SetFloat("yVelocity", rb.linearVelocity.y);
        }

        void FixedUpdate()
        {
            animator.SetFloat("yVelocity", rb.linearVelocity.y);
        }
    }
}
