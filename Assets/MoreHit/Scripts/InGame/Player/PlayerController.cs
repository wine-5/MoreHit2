using UnityEngine;
using UnityEngine.InputSystem;
using MoreHit.Attack;

namespace MoreHit.Player
{
    /// <summary>
    /// プレイヤーのメイン制御クラス
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        [Header("プレイヤーデータ")]
        [SerializeField] private PlayerData playerData;
        
        [Header("参照")]
        private PlayerInputManager inputManager;
        private PlayerMovement movement;
        private NormalAttack normalAttack;
        private RangedAttack rangedAttack;
        private StockSystem stockSystem;
        
        [Header("プレイヤー状態")]
        [SerializeField] private bool isAlive = true;

        private void Awake()
        {
            // コンポーネント取得
            inputManager = GetComponent<PlayerInputManager>();
            movement = GetComponent<PlayerMovement>();
            normalAttack = GetComponent<NormalAttack>();
            rangedAttack = GetComponent<RangedAttack>();
            stockSystem = GetComponent<StockSystem>();
        }

        private void OnEnable()
        {
            // 入力イベントのバインド
            inputManager.onMove.AddListener(OnMoveInput);
            inputManager.onJumpPerformed.AddListener(OnJumpInput);
            inputManager.onJumpCanceled.AddListener(OnJumpCanceled);
            inputManager.onNormalAttack.AddListener(OnNormalAttack);
            inputManager.onRangedAttack.AddListener(OnRangedAttack);
            inputManager.onRushAttack.AddListener(OnRushAttack);
            inputManager.onBackStep.AddListener(OnBackStep);
            inputManager.onChargeRushAttack.AddListener(OnChargeRushAttack);
            inputManager.onChargeRangedAttack.AddListener(OnChargeRangedAttack);
        }

        private void OnDisable()
        {
            // 入力イベントの解除
            inputManager.onMove.RemoveListener(OnMoveInput);
            inputManager.onJumpPerformed.RemoveListener(OnJumpInput);
            inputManager.onJumpCanceled.RemoveListener(OnJumpCanceled);
            inputManager.onNormalAttack.RemoveListener(OnNormalAttack);
            inputManager.onRangedAttack.RemoveListener(OnRangedAttack);
            inputManager.onRushAttack.RemoveListener(OnRushAttack);
            inputManager.onBackStep.RemoveListener(OnBackStep);
            inputManager.onChargeRushAttack.RemoveListener(OnChargeRushAttack);
            inputManager.onChargeRangedAttack.RemoveListener(OnChargeRangedAttack);
        }

        #region 入力ハンドラー

        private void OnMoveInput(Vector2 moveInput)
        {
            if (!isAlive) return;
            movement.SetMoveInput(moveInput);
        }

        private void OnJumpInput()
        {
            if (!isAlive) return;
            movement.Jump();
        }

        private void OnJumpCanceled()
        {
            if (!isAlive) return;
            movement.CancelJump();
        }

        private void OnNormalAttack()
        {
            if (!isAlive) return;
            normalAttack?.Execute();
        }

        private void OnRangedAttack()
        {
            if (!isAlive) return;
            rangedAttack?.Execute();
        }
        private void OnRushAttack()
        {
            if (!isAlive) return;
            // TODO: 攻撃システム実装後に追加
        }

        private void OnBackStep()
        {
            if (!isAlive) return;
            movement.BackStep();
        }

        private void OnChargeRushAttack()
        {
            if (!isAlive) return;
            // TODO: 攻撃システム実装後に追加
        }

        private void OnChargeRangedAttack()
        {
            if (!isAlive) return;
            // TODO: 攻撃システム実装後に追加
        }

        #endregion


    }
}
