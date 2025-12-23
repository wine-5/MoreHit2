using UnityEngine;

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
        
        [Header("プレイヤー状態")]
        [SerializeField] private bool isAlive = true;

        private void Awake()
        {
            // コンポーネント取得
            inputManager = GetComponent<PlayerInputManager>();
            movement = GetComponent<PlayerMovement>();
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
            Debug.Log("通常攻撃");
            // TODO: 攻撃システム実装後に追加
        }

        private void OnRangedAttack()
        {
            if (!isAlive) return;
            Debug.Log("射撃攻撃");
            // TODO: 攻撃システム実装後に追加
        }

        private void OnRushAttack()
        {
            if (!isAlive) return;
            Debug.Log("突進攻撃");
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
            Debug.Log("溜め突進攻撃");
            // TODO: 攻撃システム実装後に追加
        }

        private void OnChargeRangedAttack()
        {
            if (!isAlive) return;
            Debug.Log("溜め射撃攻撃");
            // TODO: 攻撃システム実装後に追加
        }

        #endregion

        /// <summary>
        /// プレイヤーを死亡状態にする
        /// </summary>
        public void Die()
        {
            isAlive = false;
            movement.Stop();
            Debug.Log("プレイヤー死亡");
            // TODO: 死亡演出
        }

        /// <summary>
        /// プレイヤーの生存状態を取得
        /// </summary>
        public bool IsAlive() => isAlive;
    }
}
