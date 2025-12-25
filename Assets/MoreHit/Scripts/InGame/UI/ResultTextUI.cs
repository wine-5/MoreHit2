using UnityEngine;
using TMPro;

namespace MoreHit
{
    public class ResultTextUI : MonoBehaviour
    {
        public TextMeshProUGUI timeText;

        public void Start()
        {
            float finalTime = ElapsedTime.Instance.CurrentTime;

            int minutes = Mathf.FloorToInt(finalTime / 60F);
            int seconds = Mathf.FloorToInt(finalTime % 60F);
            int milliseconds = Mathf.FloorToInt((finalTime * 100F) % 100F);

            timeText.text = string.Format("Clear Time: {0:00}:{1:00}.{2:00}", minutes, seconds, milliseconds);


        }

    }
}
