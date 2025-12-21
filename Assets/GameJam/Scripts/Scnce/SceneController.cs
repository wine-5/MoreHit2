using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fibonacci.Scene
{
    /// <summary>
    /// ゲーム内のシーン名を定義するenum
    /// </summary>
    public enum SceneName
    {
        Title,
        StageSelect,
        Stage1_1,
        Stage1_2,
        Stage2_1,
        Stage2_2,
        Clear,
        InGameF
    }

    /// <summary>
    /// シーン遷移を管理するSingletonクラス
    /// Titleシーンで一度生成されれば、他のシーンでも利用可能
    /// </summary>
    public class SceneController : Singleton<SceneController>
    {
        protected override bool UseDontDestroyOnLoad => true;

        /// <summary>
        /// 現在のステージ情報
        /// </summary>
        public SceneName CurrentStage { get; private set; } = SceneName.Title;

        /// <summary>
        /// enumで指定されたシーンに切り替え
        /// </summary>
        /// <param name="sceneName">遷移先のシーン</param>
        public void LoadScene(SceneName sceneName)
        {
            CurrentStage = sceneName;
            string sceneNameStr = sceneName.ToString();
            SceneManager.LoadScene(sceneNameStr);
        }

        /// <summary>
        /// 次のステージに進む
        /// </summary>
        public void LoadNextStage()
        {
            SceneName nextStage = GetNextStage(CurrentStage);
            if (nextStage != SceneName.Clear)
            {
                LoadScene(nextStage);
            }
            else
            {
                LoadScene(SceneName.Clear);
            }
        }

        /// <summary>
        /// 指定したステージの次のステージを取得
        /// </summary>
        /// <param name="currentStage">現在のステージ</param>
        /// <returns>次のステージ、最後の場合はClear</returns>
        private SceneName GetNextStage(SceneName currentStage)
        {
            switch (currentStage)
            {
                case SceneName.Stage1_1:
                    return SceneName.Stage1_2;
                case SceneName.Stage1_2:
                    return SceneName.Stage2_1;
                case SceneName.Stage2_1:
                    return SceneName.Stage2_2;
                case SceneName.Stage2_2:
                    return SceneName.Clear;
                default:
                    return SceneName.Clear;
            }
        }

        /// <summary>
        /// 現在のステージがゲームステージかどうかを判定
        /// </summary>
        /// <returns>ゲームステージの場合true</returns>
        public bool IsGameStage()
        {
            return CurrentStage == SceneName.Stage1_1 || 
                   CurrentStage == SceneName.Stage1_2 || 
                   CurrentStage == SceneName.Stage2_1 || 
                   CurrentStage == SceneName.Stage2_2;
        }
    }
}
