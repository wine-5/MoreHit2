using UnityEngine;
using TMPro;
using MoreHit.ElapsedTime;

/// <summary>
/// プレイ中の経過時間をマネージャーから取得し、UIテキストにリアルタイムに反映する表示用クラス
/// </summary>
namespace MoreHit.UI
{
    public class StageTimerDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI timerText;

        void Update()
        {
            if (ElapsedTimeManager.I != null)
            {
                timerText.text = ElapsedTimeManager.I.GetFormattedTime();
            }
        }
    }
}