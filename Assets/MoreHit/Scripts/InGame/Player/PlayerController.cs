using UnityEngine;
using MoreHit.Events;

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
        [Header("Bossの前に出現するための設定")]
        [SerializeField] private GameObject nearBossSpawnPoint;
        [SerializeField] private bool spawnNearBoss;
        
        /// <summary>
        /// 入力ロックフラグ（ボス演出中などに使用）
        /// </summary>
        private bool isInputLocked = false;

        private void Awake()
        {
            inputManager = GetComponent<PlayerInputManager>();
            movement = GetComponent<PlayerMovement>();
            health = GetComponent<PlayerHealth>();
            attackManager = GetComponent<AttackManager>();
            animatorController = GetComponent<PlayerAnimatorController>();
        }

        private void Start()
        {
            if(spawnNearBoss)
                transform.position = nearBossSpawnPoint.transform
                .position;
        }

        private void OnEnable()
        {
            GameEvents.OnInputLockChanged += SetInputLock;
            
            inputManager.onMove.AddListener(OnMoveInput);
            inputManager.onJumpPerformed.AddListener(OnJumpInput);
            inputManager.onJumpCanceled.AddListener(OnJumpCanceled);
            inputManager.onNormalAttack.AddListener(OnNormalAttack);
            inputManager.onRangedAttack.AddListener(OnRangedAttack);
            inputManager.onChargeRangedAttack.AddListener(OnChargeRangedAttack);
        }

        private void OnDisable()
        {
            GameEvents.OnInputLockChanged -= SetInputLock;
            
            inputManager.onMove.RemoveListener(OnMoveInput);
            inputManager.onJumpPerformed.RemoveListener(OnJumpInput);
            inputManager.onJumpCanceled.RemoveListener(OnJumpCanceled);
            inputManager.onNormalAttack.RemoveListener(OnNormalAttack);
            inputManager.onRangedAttack.RemoveListener(OnRangedAttack);
            inputManager.onChargeRangedAttack.RemoveListener(OnChargeRangedAttack);
        }

        private void OnMoveInput(Vector2 moveInput)
        {
            // 入力ロック中は処理をスキップ
            if (isInputLocked || !CanExecuteAction()) return;
            movement.SetMoveInput(moveInput);
        }

        private void OnJumpInput()
        {
            // 入力ロック中は処理をスキップ
            if (isInputLocked || !CanExecuteAction()) return;
            movement.Jump();
        }

        private void OnJumpCanceled()
        {
            // 入力ロック中は処理をスキップ
            if (isInputLocked || !CanExecuteAction()) return;
            movement.CancelJump();
        }

        private void OnNormalAttack()
        {
            // 入力ロック中は処理をスキップ
            if (isInputLocked || !CanExecuteAction()) return;
            attackManager?.ExecuteNormalAttack();
            animatorController?.PlayAttackAnimation();
        }

        private void OnRangedAttack()
        {
            // 入力ロック中は処理をスキップ
            if (isInputLocked || !CanExecuteAction()) return;
            attackManager?.ExecuteRangedAttack();
            animatorController?.PlayAttackAnimation();
        }

        private void OnChargeRangedAttack()
        {
            // 入力ロック中は処理をスキップ
            if (isInputLocked || !CanExecuteAction()) return;
            attackManager?.ExecuteChargedAttack();
            animatorController?.PlayAttackAnimation();
        }

        private bool CanExecuteAction() => health.IsAlive;
        
        /// <summary>
        /// 入力ロックを設定する（外部から呼び出し可能）
        /// </summary>
        /// <param name="isLocked">true: 入力をロック, false: 入力を解除</param>
        public void SetInputLock(bool isLocked)
        {
            isInputLocked = isLocked;
            
            // 入力ロック時に入力値をクリア
            if (isLocked && movement != null)
                movement.SetMoveInput(Vector2.zero);
        }
    }
}