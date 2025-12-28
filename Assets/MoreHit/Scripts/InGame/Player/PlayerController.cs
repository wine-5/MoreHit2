using UnityEngine;

namespace MoreHit.Player
{
    /// <summary>
    /// プレイヤーのメイン制御クラス
    /// プレイヤーの状態管理と入力の振り分けを担当
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        [Header("参照")]
        private PlayerInputManager inputManager;
        private PlayerMovement movement;
        private PlayerHealth health;
        private AttackManager attackManager;
        private PlayerAnimatorController animatorController;

        private void Awake()
        {
            inputManager = GetComponent<PlayerInputManager>();
            movement = GetComponent<PlayerMovement>();
            health = GetComponent<PlayerHealth>();
            attackManager = GetComponent<AttackManager>();
            animatorController = GetComponent<PlayerAnimatorController>();
        }

        private void OnEnable()
        {
            inputManager.onMove.AddListener(OnMoveInput);
            inputManager.onJumpPerformed.AddListener(OnJumpInput);
            inputManager.onJumpCanceled.AddListener(OnJumpCanceled);
            inputManager.onNormalAttack.AddListener(OnNormalAttack);
            inputManager.onRangedAttack.AddListener(OnRangedAttack);
            inputManager.onChargeRangedAttack.AddListener(OnChargeRangedAttack);
        }

        private void OnDisable()
        {
            inputManager.onMove.RemoveListener(OnMoveInput);
            inputManager.onJumpPerformed.RemoveListener(OnJumpInput);
            inputManager.onJumpCanceled.RemoveListener(OnJumpCanceled);
            inputManager.onNormalAttack.RemoveListener(OnNormalAttack);
            inputManager.onRangedAttack.RemoveListener(OnRangedAttack);
            inputManager.onChargeRangedAttack.RemoveListener(OnChargeRangedAttack);
        }

        private void OnMoveInput(Vector2 moveInput)
        {
            if (!health.IsAlive) return;
            
            movement.SetMoveInput(moveInput);
        }

        private void OnJumpInput()
        {
            if (!health.IsAlive) return;
            
            movement.Jump();
        }

        private void OnJumpCanceled()
        {
            if (!health.IsAlive) return;
            
            movement.CancelJump();
        }

        private void OnNormalAttack()
        {
            if (!health.IsAlive) return;
            
            attackManager?.ExecuteNormalAttack();
            animatorController?.PlayAttackAnimation();
        }

        private void OnRangedAttack()
        {
            if (!health.IsAlive) return;
            
            attackManager?.ExecuteRangedAttack();
            animatorController?.PlayAttackAnimation();
        }

        private void OnChargeRangedAttack()
        {
            if (!health.IsAlive) return;
            
            attackManager?.ExecuteChargedAttack();
            animatorController?.PlayAttackAnimation();
        }
    }
}