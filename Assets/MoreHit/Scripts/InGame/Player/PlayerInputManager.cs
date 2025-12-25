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
        public UnityEvent<bool> onChargeStateChanged; // チャージ状態変更

        private PlayerInput playerInput;
        private InputAction moveAction;
        private InputAction jumpAction;
        private InputAction normalAttackAction;
        private InputAction rangedAttackAction;
        private InputAction chargeButtonAction; // Wキー

        // チャージ管理
        private float chargeHoldTime = 0f;  // Wキーの長押し時間
        private bool isChargeReady = false; // チャージ準備状態
        private const float chargeReadyThreshold = 1.0f; // チャージ準備の闾値

        private void Awake()
        {
            playerInput = GetComponent<PlayerInput>();

            // アクションの取得
            moveAction = playerInput.actions["Move"];
            jumpAction = playerInput.actions["Jump"];
            normalAttackAction = playerInput.actions["NormalAttack"];
            rangedAttackAction = playerInput.actions["RangedAttack"];
            chargeButtonAction = playerInput.actions["ChargeButton"];
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
            
            chargeButtonAction.performed += OnChargeButtonPressed;
            chargeButtonAction.canceled += OnChargeButtonCanceled;
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
            
            chargeButtonAction.performed -= OnChargeButtonPressed;
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
            // Wキーの長押し処理
            if (chargeButtonAction.IsPressed() && !isChargeReady)
            {
                chargeHoldTime += Time.deltaTime;

                if (chargeHoldTime >= chargeReadyThreshold)
                {
                    isChargeReady = true;
                    onChargeStateChanged?.Invoke(true); // チャージ状態を通知
                    Debug.Log("チャージ準備完了！右クリックでチャージ攻撃！");
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

        private void OnNormalAttackCanceled(InputAction.CallbackContext context)
        {
            // 特に処理なし
        }

        private void OnRangedAttack(InputAction.CallbackContext context)
        {
            if (isChargeReady)
            {
                // チャージ状態で右クリック → チャージ攻撃
                onChargeRangedAttack?.Invoke();
                ResetChargeState(); // チャージ状態をリセット
            }
            else
            {
                // 通常状態で右クリック → 通常の遠距離攻撃
                onRangedAttack?.Invoke();
            }
        }

        private void OnRangedAttackCanceled(InputAction.CallbackContext context)
        {
            // 特に処理なし
        }
        
        private void OnChargeButtonPressed(InputAction.CallbackContext context)
        {
            // Wキーが押された時の処理（特になし）
        }
        
        private void OnChargeButtonCanceled(InputAction.CallbackContext context)
        {
            // Wキーが離された時の処理
            ResetChargeState();
        }
        
        private void ResetChargeState()
        {
            chargeHoldTime = 0f;
            isChargeReady = false;
            onChargeStateChanged?.Invoke(false);
            Debug.Log("チャージ状態をリセット");
        }
    }
}
