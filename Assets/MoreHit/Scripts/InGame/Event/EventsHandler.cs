using UnityEngine;
using MoreHit.Events;
using MoreHit.Scene;

namespace MoreHit.InGame
{
    /// <summary>
    /// ゲームイベントの購読を管理するクラス
    /// GameEventsで発火されたイベントを受け取り、適切な処理を実行する
    /// </summary>
    public class EventsHandler : MonoBehaviour
    {
        private void OnEnable()
        {
            GameEvents.OnPlayerDeath += OnPlayerDeath;
        }

        private void OnDisable()
        {
            GameEvents.OnPlayerDeath -= OnPlayerDeath;
        }

        /// <summary>
        /// プレイヤー死亡時の処理
        /// </summary>
        private void OnPlayerDeath()
        {
            Debug.Log("EventsHandler: プレイヤー死亡を検知 - GameOverシーンへ遷移");
            
            // SceneControllerを使ってGameOverシーンに遷移
            if (SceneController.I != null)
                SceneController.I.LoadScene(SceneName.GameOver);
            else
                Debug.LogError("SceneController instance not found!");
        }
    }
}
