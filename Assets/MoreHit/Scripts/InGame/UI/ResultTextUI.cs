using UnityEngine;
using TMPro;

namespace MoreHit
{
    public class ResultTextUI : MonoBehaviour
    {
        public TextMeshProUGUI timeText;

        public void Start()
        {
            if (ElapsedTime.Instance == null) return; // Nullチェックは必須

            float finalTime = ElapsedTime.Instance.CurrentTime;

            // 小数点以下は不要なので、最初に整数（秒単位）に変換
            int totalSeconds = (int)finalTime;
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;

            // 分 {0} は桁数制限なし（100分なら100と出る）
            // 秒 {1:00} は2桁固定（5秒なら05と出る）
            timeText.text = string.Format("Clear Time: {0}:{1:00}", minutes, seconds);
        }

    }
}
