using UnityEngine;

namespace MoreHit.UI
{
    /// <summary>
    /// ルールパネルの表示管理と、他のUI要素の表示・非表示を管理するクラス
    /// </summary>
    public class RulePanelManager : MonoBehaviour
    {
        [Header("パネル設定")]
        [SerializeField]
        private GameObject rulePanel; 

        [SerializeField]
        private GameObject closeButton; // パネル閉じるボタン

        [Header("表示・非表示を切り替えるオブジェクト")]
        [SerializeField]
        private GameObject[] objectsToHide; // パネルが開いている時に隠すオブジェクトのリスト

        private void Start()
        {
            // 初期状態はパネル非表示
            ClosePanel();
        }

        /// <summary>
        /// パネルを表示し、指定されたオブジェクトをすべて隠す
        /// </summary>
        public void OpenPanel()
        {
            if (rulePanel == null) return;

            rulePanel.SetActive(true);
            if (closeButton != null) closeButton.SetActive(true);

            // リストに登録されたオブジェクトをすべて非表示にする
            ToggleObjects(false);
        }

        /// <summary>
        /// パネルを閉じ、指定されたオブジェクトをすべて再表示する
        /// </summary>
        public void ClosePanel()
        {
            if (rulePanel == null) return;

            rulePanel.SetActive(false);
            if (closeButton != null) closeButton.SetActive(false);

            // リストに登録されたオブジェクトをすべて表示にする
            ToggleObjects(true);
        }

        /// <summary>
        /// 配列内のオブジェクトの表示状態を一括で切り替える
        /// </summary>
        private void ToggleObjects(bool state)
        {
            if (objectsToHide == null) return;

            foreach (var obj in objectsToHide)
            {
                if (obj != null)
                {
                    obj.SetActive(state);
                }
            }
        }
    }
}