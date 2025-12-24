using UnityEngine;
using System.Collections;
using MoreHit.Attack;

namespace MoreHit
{
    /// <summary>
    /// プレイヤーの通常攻撃（3段コンボ）
    /// </summary>
    public class NormalAttack : MonoBehaviour, IAttack
    {
        [Header("コンボ攻撃設定")]
        [SerializeField] private AttackData[] comboAttacks = new AttackData[3]; // 3段分
        [SerializeField] private float comboResetTime = 1f; // コンボリセット時間
        [SerializeField] private float attackDuration = 0.3f; // 攻撃硬直時間
        
        // 状態管理
        private int comboIndex = 0;
        private float lastAttackTime;
        private bool isAttacking = false;
        
        /// <summary>
        /// 攻撃可能かチェック
        /// </summary>
        public bool CanExecute() => !isAttacking && AttackExecutor.I != null;
        
        /// <summary>
        /// 攻撃実行
        /// </summary>
        public void Execute()
        {
            if (!CanExecute()) return;
            
            StartCoroutine(AttackRoutine());
        }
        
        /// <summary>
        /// 攻撃処理のコルーチン
        /// </summary>
        private IEnumerator AttackRoutine()
        {
            isAttacking = true;
            
            // コンボリセット判定
            if (Time.time - lastAttackTime > comboResetTime)
                comboIndex = 0;
            
            // 攻撃データ取得
            AttackData attackData = GetCurrentAttackData();
            if (attackData == null)
            {
                Debug.LogWarning("攻撃データが設定されていません！");
                isAttacking = false;
                yield break;
            }
            
            // 攻撃方向取得
            Vector2 direction = GetAttackDirection();
            
            // 攻撃実行
            int hitCount = AttackExecutor.I.Execute(
                attackData,
                transform.position,
                direction,
                gameObject
            );
            
            // ログ出力
            Debug.Log($"通常攻撃 {comboIndex + 1}段目: {hitCount}体の敵にヒット");
            
            // コンボ進行
            comboIndex = (comboIndex + 1) % comboAttacks.Length;
            lastAttackTime = Time.time;
            
            // 硬直時間待機
            yield return new WaitForSeconds(attackDuration);
            
            isAttacking = false;
        }
        
        /// <summary>
        /// 現在のコンボ段階の攻撃データを取得
        /// </summary>
        private AttackData GetCurrentAttackData()
        {
            if (comboAttacks == null || comboAttacks.Length == 0)
                return null;
                
            int index = Mathf.Clamp(comboIndex, 0, comboAttacks.Length - 1);
            return comboAttacks[index];
        }
        
        /// <summary>
        /// 攻撃方向を取得
        /// </summary>
        private Vector2 GetAttackDirection()
        {
            // プレイヤーの向きを基準に方向決定
            float scaleX = transform.localScale.x;
            return scaleX > 0 ? Vector2.right : Vector2.left;
        }
        
        /// <summary>
        /// コンボをリセット
        /// </summary>
        public void ResetCombo() => comboIndex = 0;
    }
}
