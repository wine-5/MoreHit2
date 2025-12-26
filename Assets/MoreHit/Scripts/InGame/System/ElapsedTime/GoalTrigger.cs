using UnityEngine;
using MoreHit.ElapsedTime;
using MoreHit.Scene;

/// <summary>
/// プレイヤーのゴール到達を検知し、時間計測の停止およびシーン遷移の実行を制御するクラス
/// </summary>

    public class GoalTrigger : MonoBehaviour
    {
        [SerializeField]
        private SceneName nextScene = SceneName.Clear;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                // 計測停止
                if (ElapsedTimeManager.Instance != null)
                {
                    ElapsedTimeManager.Instance.StopTimer();
                }

                if (SceneController.I != null)
                {
                    SceneController.I.LoadScene(nextScene);
                }
                else
                {

                    Debug.LogError("SceneControllerが存在しません。");
                }
            }
        }
    }
