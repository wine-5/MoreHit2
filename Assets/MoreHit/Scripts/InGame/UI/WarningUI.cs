using UnityEngine;
using TMPro;
using System.Collections;
using MoreHit.Audio;

namespace MoreHit.UI
{
    /// <summary>
    /// WARNING テキストのスクロール表示
    /// 右から左へスクロールしながらフェードアウト
    /// </summary>
    public class WarningUI : MonoBehaviour
    {
        [Header("UI参照")]
        [SerializeField] private TextMeshProUGUI warningText;
        
        [Header("アニメーション設定")]
        [SerializeField] private float scrollSpeed = 500f;
        [SerializeField] private float displayDuration = 2f;
        [SerializeField] private float blinkSpeed = 5f;
        
        private RectTransform canvasRect;
        
        /// <summary>
        /// WARNING アニメーション実行
        /// </summary>
        public IEnumerator PlayWarningAnimation()
        {
            if (warningText == null) yield break;
            
            // Warning音を再生
            if (AudioManager.I != null)
                AudioManager.I.PlaySE(SeType.Warning);
            
            if (canvasRect == null)
                canvasRect = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
            
            warningText.gameObject.SetActive(true);
            
            RectTransform textRect = warningText.rectTransform;
            float canvasWidth = canvasRect.rect.width;
            float startX = canvasWidth / 2f + textRect.rect.width / 2f;
            float originalY = textRect.anchoredPosition.y;
            
            textRect.anchoredPosition = new Vector2(startX, originalY);
            
            Color originalColor = warningText.color;
            float elapsedTime = 0f;
            
            while (elapsedTime < displayDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float newX = startX - (scrollSpeed * elapsedTime);
                textRect.anchoredPosition = new Vector2(newX, originalY);
                
                float blinkAlpha = Mathf.Abs(Mathf.Sin(elapsedTime * blinkSpeed));
                warningText.color = new Color(originalColor.r, originalColor.g, originalColor.b, blinkAlpha);
                
                yield return null;
            }
            
            warningText.gameObject.SetActive(false);
            warningText.color = originalColor;
        }
    }
}
