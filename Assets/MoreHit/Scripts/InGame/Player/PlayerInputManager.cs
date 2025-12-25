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
        public UnityEvent onChargeRangedAttack;

        private PlayerInput playerInput;
        private InputAction moveAction;
        private InputAction jumpAction;
        private InputAction normalAttackAction;
        private InputAction rangedAttackAction;

        // 溜め攻撃管理
        private float attackHoldTime = 0f;
        private float rangedHoldTime = 0f;  // 遠距離攻撃の長押し時間
        private bool isChargingAttack = false;
        private bool isChargingRanged = false;  // 遠距離攻撃の長押し状態
        private const float chargeAttackThreshold = 1.0f; // 溜め攻撃の閾値

        private void Awake()
        {
            playerInput = GetComponent<PlayerInput>();

            // アクションの取得
            moveAction = playerInput.actions["Move"];
            jumpAction = playerInput.actions["Jump"];
            normalAttackAction = playerInput.actions["NormalAttack"];
            rangedAttackAction = playerInput.actions["RangedAttack"];
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
            rangedAttackAction.canceled += OnRangedAttackCanceled;
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
            rangedAttackAction.canceled -= OnRangedAttackCanceled;
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

            // 遠距離攻撃（右クリック）の溜め処理
            if (rangedAttackAction.IsPressed() && !isChargingRanged)
            {
                rangedHoldTime += Time.deltaTime;

                if (rangedHoldTime >= chargeAttackThreshold)
                {
                    onChargeRangedAttack?.Invoke();  // チャージ攻撃を発動
                    isChargingRanged = true;
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
            // 長押しでチャージ中でなければ、通常の遠距離攻撃を実行
            if (!isChargingRanged)
                onRangedAttack?.Invoke();
        }

        private void OnRangedAttackCanceled(InputAction.CallbackContext context)
        {
            rangedHoldTime = 0f;
            isChargingRanged = false;
        }
    }
}
