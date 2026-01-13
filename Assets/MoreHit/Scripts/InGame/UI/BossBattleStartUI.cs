using UnityEngine;
using TMPro;
using System.Collections;

namespace MoreHit.UI
{
    /// <summary>
    /// ボス戦開始時の「Ready...」→「Fight!」表示
    /// </summary>
    public class BossBattleStartUI : MonoBehaviour
    {
        [Header("UI参照")]
        [SerializeField] private GameObject readyPanel;
        [SerializeField] private GameObject fightPanel;
        [SerializeField] private TextMeshProUGUI readyText;
        [SerializeField] private TextMeshProUGUI fightText;
        
        [Header("アニメーション設定")]
        [SerializeField] private float readyDisplayDuration = 1.5f; // Ready表示時間
        [SerializeField] private float fightDisplayDuration = 1f; // Fight表示時間
        [SerializeField] private float scaleAnimationDuration = 0.3f; // スケールアニメーション時間
        [SerializeField] private float minScale = 0.5f; // 開始時のスケール
        [SerializeField] private float maxScale = 1f; // 最終スケール
        
        private void Start()
        {
            if (readyPanel != null)
                readyPanel.SetActive(false);
            if (fightPanel != null)
                fightPanel.SetActive(false);
        }
        
        /// <summary>
        /// "Ready..." 表示
        /// </summary>
        public IEnumerator ShowReady()
        {
            if (readyPanel == null || readyText == null)
            {
                Debug.LogError("[BossBattleStartUI] Ready UI参照が設定されていません！");
                yield break;
            }
            
            readyPanel.SetActive(true);
            yield return ScaleAnimation(readyText.transform);
            yield return new WaitForSeconds(readyDisplayDuration);
            readyPanel.SetActive(false);
        }
        
        /// <summary>
        /// "Fight!" 表示
        /// </summary>
        public IEnumerator ShowFight()
        {
            if (fightPanel == null || fightText == null)
            {
                Debug.LogError("[BossBattleStartUI] Fight UI参照が設定されていません！");
                yield break;
            }
            
            fightPanel.SetActive(true);
            yield return ScaleAnimation(fightText.transform);
            yield return new WaitForSeconds(fightDisplayDuration);
            fightPanel.SetActive(false);
        }
        
        /// <summary>
        /// スケールアニメーション（小→大）
        /// </summary>
        private IEnumerator ScaleAnimation(Transform target)
        {
            float elapsed = 0f;
            
            while (elapsed < scaleAnimationDuration)
            {
                elapsed += Time.deltaTime;
                float scale = Mathf.Lerp(minScale, maxScale, elapsed / scaleAnimationDuration);
                target.localScale = Vector3.one * scale;
                
                yield return null;
            }
            
            // 最終スケールを確実に設定
            target.localScale = Vector3.one * maxScale;
        }
    }
}
