using UnityEngine;

namespace MoreHit
{
    /// <summary>
    /// プレイヤーのアニメーション制御クラス
    /// </summary>
    public class PlayerAnimatorController : MonoBehaviour
    {
        /*（参考程度に）
         * TODO: PlayerAnimatorController 実装タスク
         * 
         * 【アニメーション制御】
         * - 待機アニメーション制御
         * - ジャンプアニメーション制御
         * 
         * 【状態判定】
         * - PlayerMovementから状態を参照（同じGameObjectなのでProvider不要）
         * - ジャンプ判定: IsGrounded の逆で判定
         * 
         * 【実装方針】
         * - Update()でジャンプ状態をチェック
         * - animator.SetBool("IsJumping", isJumping); でアニメーション切り替え
         */
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
