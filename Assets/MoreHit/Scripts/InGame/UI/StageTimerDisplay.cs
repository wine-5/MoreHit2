using UnityEngine;
using TMPro; // TextMeshProを使用するために必要
using MoreHit;

public class StageTimerDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText; // インスペクターでUIをドラッグ＆ドロップ

    void Update()
    {
        if (ElapsedTime.Instance != null)
        {
            float t = ElapsedTime.Instance.CurrentTime;

            // 整数にキャストして計算（計算負荷の軽減）
            int totalSeconds = (int)t;
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;

            // {0} は桁数制限なし、{1:00} は常に2桁（05秒などを表現）
            timerText.text = string.Format("{0}:{1:00}", minutes, seconds);
        }
    }
}