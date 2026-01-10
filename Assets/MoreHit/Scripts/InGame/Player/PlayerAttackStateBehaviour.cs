using UnityEngine;

namespace MoreHit.Player
{
    /// <summary>攻撃アニメーション状態を管理するStateMachineBehaviour。アニメーション終了時にisAttackingをfalseに戻す</summary>
    public class PlayerAttackStateBehaviour : StateMachineBehaviour
    {
        /// <summary>
        /// アニメーション状態に入った時
        /// </summary>
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            Debug.Log("[PlayerAttack] アニメーション開始");
        }
        
        /// <summary>
        /// アニメーション更新中に毎フレーム呼ばれる
        /// </summary>
        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            // アニメーションが90%以上再生されたらisAttackingをfalseに
            if (stateInfo.normalizedTime >= 0.9f)
            {
                animator.SetBool("isAttacking", false);
                Debug.Log($"[PlayerAttack] アニメーション進行度: {stateInfo.normalizedTime:F2} - isAttackingをfalseに設定");
            }
        }
        
        /// <summary>
        /// アニメーション状態から抜ける時に呼ばれる
        /// </summary>
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            // 念のため確実にfalseに戻す
            animator.SetBool("isAttacking", false);
            Debug.Log("[PlayerAttack] アニメーション終了 - isAttackingをfalseに設定");
        }
    }
}
