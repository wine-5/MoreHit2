using UnityEngine;
using UnityEngine.InputSystem;

namespace MoreHit.Player
{
    public class PlayerAnimatorController : MonoBehaviour
    {
        private Animator animator;
        private PlayerController playerController;
        private Rigidbody2D rb;

        // Input System 用
        private InputSystem_Actions inputActions;
        private Vector2 moveInput;

        void Awake()
        {
            animator = GetComponent<Animator>();
            playerController = GetComponent<PlayerController>();
            rb = GetComponent<Rigidbody2D>();

            inputActions = new InputSystem_Actions();
        }

        void OnEnable()
        {
            inputActions.Player.Enable();
            inputActions.Player.Move.performed += OnMove;
            inputActions.Player.Move.canceled += OnMove;
        }

        void OnDisable()
        {
            inputActions.Player.Move.performed -= OnMove;
            inputActions.Player.Move.canceled -= OnMove;
            inputActions.Player.Disable();
        }

        private void OnMove(InputAction.CallbackContext context)
        {
            moveInput = context.ReadValue<Vector2>();
        }

        void Update()
        {
            // 左右移動判定
             bool isWalking = Mathf.Abs(moveInput.x) > 0.1f;
             animator.SetBool("isWalking", isWalking);

            // 接地判定
             animator.SetBool("isGrounded", playerController.isGrounded);

             // Y方向速度
             animator.SetFloat("yVelocity", rb.linearVelocity.y);
        }

        void FixedUpdate()
        {
            animator.SetFloat("yVelocity", rb.linearVelocity.y);
        }
    }
}
