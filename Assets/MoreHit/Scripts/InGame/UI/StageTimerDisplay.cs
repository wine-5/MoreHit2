using UnityEngine;
using TMPro; // TextMeshProを使用するために必要
using MoreHit.ElapsedTime;

/// <summary>
/// プレイ中の経過時間をマネージャーから取得し、UIテキストへリアルタイムに反映する表示専用クラス
/// </summary>
namespace MoreHit.UI // UI関連であることを明示
{
    public class StageTimerDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI timerText; // インスペクターでUIをドラッグ＆ドロップ

        void Update()
        {
            if (ElapsedTimeManager.I != null) // Instanceを I と定義している場合
            {
                // 複雑な計算はマネージャーに任せ、UIは「表示するだけ」に専念する
                timerText.text = ElapsedTimeManager.I.GetFormattedTime();
            }
        }
    }
}