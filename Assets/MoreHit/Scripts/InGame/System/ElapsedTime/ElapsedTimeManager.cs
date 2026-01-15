using UnityEngine;
using MoreHit.Events;

namespace MoreHit.ElapsedTime
{
    /// <summary>
    /// ゲーム内の経過時間を管理するマネージャー
    /// </summary>
    // SceneControllerと同じ機能のSingleton基クラスを利用する
    public class ElapsedTimeManager : Singleton<ElapsedTimeManager>
    {
        // プロジェクト全体で使う「時間の定数」はここに集約する
        public const int SECONDS_PER_MINUTE = 60;

        // DontDestroyOnLoadを無効にする（Singleton基クラスの機能を利用）
        protected override bool UseDontDestroyOnLoad => true;

        public float CurrentTime { get; private set; }
        private bool isTimerRunning = false;

        private void OnEnable()
        {
            GameEvents.OnGameStart += OnGameStart;
        }
        
        private void OnDisable()
        {
            GameEvents.OnGameStart -= OnGameStart;
        }
        
        private void OnGameStart()
        {
            StartTimer();
        }
        private void Update()
        {
            if (isTimerRunning)
            {
                CurrentTime += Time.deltaTime;
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

        /// <summary>
        /// 現在の時間を "0:00" 形式の文字列で取得する
        /// </summary>
        public string GetFormattedTime()
        {
            int totalSeconds = (int)CurrentTime;
            int minutes = totalSeconds / SECONDS_PER_MINUTE;
            int seconds = totalSeconds % SECONDS_PER_MINUTE;

            return string.Format("時間 {0}:{1:00}", minutes, seconds);
        }
    }
}