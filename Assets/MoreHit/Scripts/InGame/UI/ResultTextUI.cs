using UnityEngine;
using TMPro;
using MoreHit.ElapsedTime;

namespace MoreHit.UI
{
    /// <summary>
    /// リザルト画面において、最終的な確定タイムを読み取り、整形して表示するクラス
    /// </summary>
    public class ResultTextUI : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI timeText;

        public void Start()
        {
            // インスタンスの存在確認
            if (ElapsedTimeManager.I == null)
            {
                Debug.LogWarning("ElapsedTimeManagerが見つかりません。");
                return;
            }

            // マネージャー側で整形済みの文字列を取得し、接頭辞を付けるだけ
            string timeString = ElapsedTimeManager.I.GetFormattedTime();
            timeText.text = $"Clear Time: {timeString}";
        }
    }
}