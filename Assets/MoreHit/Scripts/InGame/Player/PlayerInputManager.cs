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
        public UnityEvent onChargeStarted;

        private PlayerInput playerInput;
        private InputAction moveAction;
        private InputAction jumpAction;
        private InputAction normalAttackAction;
        private InputAction rangedAttackAction;
        private InputAction chargeButtonAction;

        private float chargeHoldTime = 0f;
        private bool isChargeReady = false;
        private const float CHARGE_READY_THRESHOLD = 0.8f;
        private const float CHARGE_COOLDOWN = 0.01f;
        private bool wasChargingLastFrame = false;
        private float chargeCooldownRemaining = 0f;

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
        }

        private void OnDisable()
        {
            moveAction.performed -= OnMove;
            moveAction.canceled -= OnMove;

            jumpAction.performed -= OnJumpPerformed;
            jumpAction.canceled -= OnJumpCanceled;

            normalAttackAction.performed -= OnNormalAttack;

            rangedAttackAction.performed -= OnRangedAttack;
        }

        private void Update()
        {
            UpdateChargeAttacks();
            UpdateChargeCooldown();
        }

        /// <summary>
        /// チャージ管理の更新処理
        /// </summary>
        private void UpdateChargeAttacks()
        {
            bool isChargeButtonPressed = chargeButtonAction.IsPressed();
            
            if (isChargeButtonPressed && !wasChargingLastFrame)
            {
                chargeHoldTime = 0f;
                isChargeReady = false;
                onChargeStarted?.Invoke();
            }
            
            if (isChargeButtonPressed)
            {
                chargeHoldTime += Time.deltaTime;
                
                if (chargeHoldTime >= CHARGE_READY_THRESHOLD && !isChargeReady)
                {
                    isChargeReady = true;
                    onChargeStateChanged?.Invoke(true);
                }
            }
            else
            {
                if (chargeHoldTime > 0f || isChargeReady)
                {
                    chargeHoldTime = 0f;
                    isChargeReady = false;
                    onChargeStateChanged?.Invoke(false);
                }
            }
            
            wasChargingLastFrame = isChargeButtonPressed;
        }

        /// <summary>
        /// チャージショットのクールタイム管理
        /// </summary>
        private void UpdateChargeCooldown()
        {
            if (chargeCooldownRemaining > 0f)
                chargeCooldownRemaining -= Time.deltaTime;
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
            if (chargeButtonAction.IsPressed() && isChargeReady && chargeCooldownRemaining <= 0f)
            {
                onChargeRangedAttack?.Invoke();
                chargeCooldownRemaining = CHARGE_COOLDOWN;
            }
            else
                onRangedAttack?.Invoke();
        }
    }
}