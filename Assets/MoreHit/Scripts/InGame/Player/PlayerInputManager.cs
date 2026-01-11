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
        public UnityEvent onChargeStarted;  // チャージ開始イベント（即座に発火）

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
        private bool wasChargingLastFrame = false; // Wキー押下状態の前フレーム記録
        private float chargeCooldownRemaining = 0f; // チャージショットクールタイム


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
            
            // Wキー押下開始時：即座にエフェクトを開始
            if (isChargeButtonPressed && !wasChargingLastFrame)
            {
                chargeHoldTime = 0f;
                isChargeReady = false;
                onChargeStarted?.Invoke(); // 即座にエフェクト開始
            }
            
            if (isChargeButtonPressed)
            {
                // Wキーを押している間、時間を積算
                chargeHoldTime += Time.deltaTime;
                
                // 0.8秒以上押していればチャージ準備完了
                if (chargeHoldTime >= CHARGE_READY_THRESHOLD && !isChargeReady)
                {
                    isChargeReady = true;
                    onChargeStateChanged?.Invoke(true);
                }
            }
            else
            {
                // Wキーを離したらチャージ状態をリセット
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
            // 現在Wキーを押しているかつチャージ準備が完了している場合は溜め射撃
            if (chargeButtonAction.IsPressed() && isChargeReady && chargeCooldownRemaining <= 0f)
            {
                onChargeRangedAttack?.Invoke();
                chargeCooldownRemaining = CHARGE_COOLDOWN; // クールタイム設定
            }
            else
                onRangedAttack?.Invoke(); // それ以外は通常射撃
        }
    }
}