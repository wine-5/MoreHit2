using UnityEngine;
using System.Collections;

namespace MoreHit.Enemy
{
    /// <summary>
    /// Boss撃破時のスプライト点滅エフェクト
    /// 点滅速度が徐々に加速し、最終的に消える
    /// </summary>
    public class BossDeathEffect : MonoBehaviour
    {
        #region 定数
        
        private const float INITIAL_BLINK_INTERVAL = 0.3f;
        private const float MIN_BLINK_INTERVAL = 0.05f;
        private const float ACCELERATION_RATE = 0.85f;
        private const float TOTAL_EFFECT_DURATION = 3f;
        
        #endregion
        
        #region フィールド
        
        private SpriteRenderer spriteRenderer;
        private bool isPlaying = false;
        
        #endregion
        
        #region 初期化
        
        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            
            if (spriteRenderer == null)
                Debug.LogError("[BossDeathEffect] SpriteRendererが見つかりません！");
        }
        
        #endregion
        
        #region エフェクト再生
        
        /// <summary>
        /// 点滅エフェクトを開始
        /// </summary>
        public void PlayDeathEffect()
        {
            if (isPlaying)
                return;
            
            StartCoroutine(BlinkAndFadeOut());
        }
        
        /// <summary>
        /// 点滅しながら消えるコルーチン
        /// </summary>
        private IEnumerator BlinkAndFadeOut()
        {
            isPlaying = true;
            float elapsedTime = 0f;
            float currentInterval = INITIAL_BLINK_INTERVAL;
            bool isVisible = true;
            
            while (elapsedTime < TOTAL_EFFECT_DURATION)
            {
                // 表示/非表示を切り替え
                if (spriteRenderer != null)
                {
                    isVisible = !isVisible;
                    spriteRenderer.enabled = isVisible;
                }
                
                // 次の点滅までの待機
                yield return new WaitForSeconds(currentInterval);
                
                // 点滅速度を加速（間隔を短縮）
                currentInterval *= ACCELERATION_RATE;
                currentInterval = Mathf.Max(currentInterval, MIN_BLINK_INTERVAL);
                
                elapsedTime += currentInterval;
            }
            
            // 最終的に非表示にする
            if (spriteRenderer != null)
                spriteRenderer.enabled = false;
            
            isPlaying = false;
        }
        
        #endregion
    }
}
