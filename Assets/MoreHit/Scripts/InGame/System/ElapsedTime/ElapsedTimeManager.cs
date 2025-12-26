using UnityEngine;

namespace MoreHit.ElapsedTime
{
    /// <summary>
    /// �Q�[�����̌o�ߎ��Ԃ��Ǘ�����}�l�[�W���[
    /// </summary>
    // SceneController�Ɠ��������ʂ�Singleton�e�N���X���p��������
    public class ElapsedTimeManager : Singleton<ElapsedTimeManager>
    {
        // �v���W�F�N�g�S�̂Ŏg���u���Ԃ̒萔�v�͂����ɏW�񂷂�
        public const int SECONDS_PER_MINUTE = 60;

        // DontDestroyOnLoad��L���ɂ���iSingleton�e�N���X�̋@�\�𗘗p�j
        protected override bool UseDontDestroyOnLoad => true;

        public float CurrentTime { get; private set; }
        private bool isTimerRunning = false;

        private void Start()
        {
            StartTimer(); // �����̊J�n���\�b�h�������ŌĂ�
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
        /// ���݂̎��Ԃ� "0:00" �`���̕�����Ŏ擾����
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