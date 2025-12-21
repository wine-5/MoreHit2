using UnityEngine;

namespace MoreHit.Scene
{
    /// <summary>
    /// ButtonのOnClickからシーンを変更するためのラッパークラス
    /// </summary>
    public class SceneChangeButtonHandler : MonoBehaviour
    {
        [SerializeField]
        private SceneName targetScene = SceneName.Title;

        /// <summary>
        /// ButtonのOnClickから呼び出されるメソッド
        /// 指定されたシーンに切り替える
        /// </summary>
        public void ChangeScene()
        {
            if (SceneController.I != null)
                SceneController.I.LoadScene(targetScene);
            else
                Debug.LogWarning("SceneController instance not found!");
        }

        /// <summary>
        /// ButtonのOnClickから呼び出されるメソッド
        /// Titleシーンに切り替える
        /// </summary>
        public void GoToTitle()
        {
            ChangeSceneTo(SceneName.Title);
        }

        /// <summary>
        /// ButtonのOnClickから呼び出されるメソッド
        /// InGameシーンに切り替える
        /// </summary>
        public void GoToInGame()
        {
            ChangeSceneTo(SceneName.InGame);
        }

        /// <summary>
        /// ButtonのOnClickから呼び出されるメソッド
        /// Resultシーンに切り替える
        /// </summary>
        public void GoToResult()
        {
            ChangeSceneTo(SceneName.Result);
        }

        /// <summary>
        /// 指定されたシーンに切り替える
        /// </summary>
        /// <param name="sceneName">遷移先のシーン</param>
        private void ChangeSceneTo(SceneName sceneName)
        {
            if (SceneController.I != null)
                SceneController.I.LoadScene(sceneName);
            else
                Debug.LogWarning("SceneController instance not found!");
        }
    }
}
