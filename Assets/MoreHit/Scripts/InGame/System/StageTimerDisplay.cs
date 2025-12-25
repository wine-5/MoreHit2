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
            // 分:秒.ミリ秒 の形式に整形
            timerText.text = string.Format("{0:00}:{1:00}.{2:00}",
                Mathf.FloorToInt(t / 60),
                Mathf.FloorToInt(t % 60),
                Mathf.FloorToInt((t * 100) % 100));
        }
    }
}