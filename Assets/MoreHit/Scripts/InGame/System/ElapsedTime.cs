using UnityEngine;

namespace MoreHit
{
    public class ElapsedTime : MonoBehaviour
    {
        public static ElapsedTime Instance { get; private set; }

       
        public float CurrentTime { get; private set; }
        private bool isTimerRunning = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

       
        private void Update()
        {
            if (isTimerRunning)
            {
                CurrentTime += UnityEngine.Time.deltaTime;
            }
        }

        public void StartTimer()
        {
            CurrentTime = 0f;
            isTimerRunning = true;
        }

        public void StopTimer()
        {
            isTimerRunning = false;
        }
    }
}