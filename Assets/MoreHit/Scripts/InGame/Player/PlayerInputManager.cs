using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

namespace MoreHit.Player
{
    /// <summary>
    /// プレイヤーの入力を管理するクラス
    /// </summary>
    public class PlayerInputManager : MonoBehaviour
    {
        [Header("イベント")]
        public UnityEvent<Vector2> onMove;
        public UnityEvent onJumpPerformed;
        public UnityEvent onJumpCanceled;
        public UnityEvent onNormalAttack;
        public UnityEvent onRangedAttack;
        public UnityEvent onChargeRangedAttack;
        public UnityEvent<bool> onChargeStateChanged;

        private PlayerInput playerInput;
        private InputAction moveAction;
        private InputAction jumpAction;
        private InputAction normalAttackAction;
        private InputAction rangedAttackAction;
        private InputAction chargeButtonAction;

        private float chargeHoldTime = 0f;
        private bool isChargeReady = false;
        private const float CHARGE_READY_THRESHOLD = 1.0f;

        private void Awake()
        {
            playerInput = GetComponent<PlayerInput>();

            moveAction = playerInput.actions["Move"];
            jumpAction = playerInput.actions["Jump"];
            normalAttackAction = playerInput.actions["NormalAttack"];
            rangedAttackAction = playerInput.actions["RangedAttack"];
            chargeButtonAction = playerInput.actions["ChargeButton"];
        }

        private void OnEnable()
        {
            moveAction.performed += OnMove;
            moveAction.canceled += OnMove;

            jumpAction.performed += OnJumpPerformed;
            jumpAction.canceled += OnJumpCanceled;

            normalAttackAction.performed += OnNormalAttack;

            rangedAttackAction.performed += OnRangedAttack;
            
            chargeButtonAction.canceled += OnChargeButtonCanceled;
        }

        private void OnDisable()
        {
            moveAction.performed -= OnMove;
            moveAction.canceled -= OnMove;

            jumpAction.performed -= OnJumpPerformed;
            jumpAction.canceled -= OnJumpCanceled;

            normalAttackAction.performed -= OnNormalAttack;

            rangedAttackAction.performed -= OnRangedAttack;
            
            chargeButtonAction.canceled -= OnChargeButtonCanceled;
        }

        private void Update()
        {
            UpdateChargeAttacks();
        }

        /// <summary>
        /// チャージ管理の更新処理
        /// </summary>
        private void UpdateChargeAttacks()
        {
            if (chargeButtonAction.IsPressed() && !isChargeReady)
            {
                chargeHoldTime += Time.deltaTime;

                if (chargeHoldTime >= CHARGE_READY_THRESHOLD)
                {
                    isChargeReady = true;
                    onChargeStateChanged?.Invoke(true);
                }
            }
        }

        private void OnMove(InputAction.CallbackContext context)
        {
            onMove?.Invoke(context.ReadValue<Vector2>());
        }

        private void OnJumpPerformed(InputAction.CallbackContext context)
        {
            onJumpPerformed?.Invoke();
        }

        private void OnJumpCanceled(InputAction.CallbackContext context)
        {
            onJumpCanceled?.Invoke();
        }

        private void OnNormalAttack(InputAction.CallbackContext context)
        {
            onNormalAttack?.Invoke();
        }

        private void OnRangedAttack(InputAction.CallbackContext context)
        {
            if (isChargeReady)
            {
                onChargeRangedAttack?.Invoke();
                ResetChargeState();
            }
            else
                onRangedAttack?.Invoke();
        }
        
        private void OnChargeButtonCanceled(InputAction.CallbackContext context)
        {
            ResetChargeState();
        }
        
        private void ResetChargeState()
        {
            chargeHoldTime = 0f;
            isChargeReady = false;
            onChargeStateChanged?.Invoke(false);
        }
    }
}