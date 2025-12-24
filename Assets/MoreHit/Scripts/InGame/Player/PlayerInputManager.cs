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
        [Header("プレイヤーデータ")]
        [SerializeField] private PlayerData playerData;

        [Header("イベント")]
        public UnityEvent<Vector2> onMove;
        public UnityEvent onJumpPerformed;
        public UnityEvent onJumpCanceled;
        public UnityEvent onNormalAttack;
        public UnityEvent onRangedAttack;
        public UnityEvent onRushAttack;
        public UnityEvent onBackStep;
        public UnityEvent onChargeRushAttack;
        public UnityEvent onChargeRangedAttack;

        private PlayerInput playerInput;
        private InputAction moveAction;
        private InputAction jumpAction;
        private InputAction normalAttackAction;
        private InputAction rangedAttackAction;
        private InputAction rushAttackAction;
        private InputAction backStepAction;

        // 溜め攻撃管理
        private float attackHoldTime = 0f;
        private float backstepHoldTime = 0f;
        private bool isChargingAttack = false;
        private bool isChargingBackstep = false;
        private const float chargeAttackThreshold = 1.0f; // 溜め攻撃の閾値

        private void Awake()
        {
            playerInput = GetComponent<PlayerInput>();

            // アクションの取得
            moveAction = playerInput.actions["Move"];
            jumpAction = playerInput.actions["Jump"];
            normalAttackAction = playerInput.actions["NormalAttack"];
            rangedAttackAction = playerInput.actions["RangedAttack"];
            rushAttackAction = playerInput.actions["RushAttack"];
            backStepAction = playerInput.actions["BackStep"];
        }

        private void OnEnable()
        {
            // イベント登録
            moveAction.performed += OnMove;
            moveAction.canceled += OnMove;

            jumpAction.performed += OnJumpPerformed;
            jumpAction.canceled += OnJumpCanceled;

            normalAttackAction.performed += OnNormalAttack;
            normalAttackAction.canceled += OnNormalAttackCanceled;

            rangedAttackAction.performed += OnRangedAttack;
            rushAttackAction.performed += OnRushAttack;
            backStepAction.performed += OnBackStep;
            backStepAction.canceled += OnBackStepCanceled;
        }

        private void OnDisable()
        {
            // イベント解除
            moveAction.performed -= OnMove;
            moveAction.canceled -= OnMove;

            jumpAction.performed -= OnJumpPerformed;
            jumpAction.canceled -= OnJumpCanceled;

            normalAttackAction.performed -= OnNormalAttack;
            normalAttackAction.canceled -= OnNormalAttackCanceled;

            rangedAttackAction.performed -= OnRangedAttack;
            rushAttackAction.performed -= OnRushAttack;
            backStepAction.performed -= OnBackStep;
            backStepAction.canceled -= OnBackStepCanceled;
        }

        private void Update()
        {
            UpdateChargeAttacks();
        }

        /// <summary>
        /// 溜め攻撃の更新処理
        /// </summary>
        private void UpdateChargeAttacks()
        {
            // メイン攻撃の溜め処理
            if (normalAttackAction.IsPressed() && !isChargingAttack)
            {
                attackHoldTime += Time.deltaTime;

                if (attackHoldTime >= chargeAttackThreshold)
                {
                    onChargeRangedAttack?.Invoke();
                    isChargingAttack = true;
                }
            }

            // バックステップの溜め処理
            if (backStepAction.IsPressed() && !isChargingBackstep)
            {
                backstepHoldTime += Time.deltaTime;

                if (backstepHoldTime >= chargeAttackThreshold)
                {
                    onChargeRushAttack?.Invoke();
                    isChargingBackstep = true;
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
            if (!isChargingAttack)
                onNormalAttack?.Invoke();
        }

        private void OnNormalAttackCanceled(InputAction.CallbackContext context)
        {
            attackHoldTime = 0f;
            isChargingAttack = false;
        }

        private void OnRangedAttack(InputAction.CallbackContext context)
        {
            onRangedAttack?.Invoke();
        }

        private void OnRushAttack(InputAction.CallbackContext context)
        {
            onRushAttack?.Invoke();
        }

        private void OnBackStep(InputAction.CallbackContext context)
        {
            if (!isChargingBackstep)
                onBackStep?.Invoke();
        }

        private void OnBackStepCanceled(InputAction.CallbackContext context)
        {
            backstepHoldTime = 0f;
            isChargingBackstep = false;
        }
    }
}
