using UnityEngine;

namespace MoreHit.UI
{
    /// <summary>
    /// ルールパネルの表示制御と、それに連動したボタンの表示・非表示を管理するクラス
    /// </summary>
    public class RulePanelManager : MonoBehaviour
    {
        [SerializeField]
        private GameObject rulePanel; // ルールパネル本体

        [SerializeField]
        private GameObject openButton; // パネルを開くためのボタン

        [SerializeField]
        private GameObject closeButton; // パネルを開くためのボタン

        private void Start()
        {
            ClosePanel();
            closeButton.SetActive(false);
        }

        /// <summary>
        /// パネルを表示し、開くボタンを隠す
        /// </summary>
        public void OpenPanel()
        {
            if (rulePanel != null && openButton != null)
            {
                rulePanel.SetActive(true);
                openButton.SetActive(false); 
                closeButton.SetActive(true);
            }
        }

        /// <summary>
        /// パネルを隠し、開くボタンを再表示する
        /// </summary>
        public void ClosePanel()
        {
            if (rulePanel != null && openButton != null)
            {
                rulePanel.SetActive(false);
                openButton.SetActive(true);
                closeButton.SetActive(false);
            }
        }
    }
}