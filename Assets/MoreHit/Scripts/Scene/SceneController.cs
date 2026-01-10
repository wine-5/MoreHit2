using UnityEngine.SceneManagement;

namespace MoreHit.Scene
{
    /// <summary>
    /// ゲーム内のシーン名を定義するenum
    /// </summary>
    public enum SceneName
    {
        Title,
        InGame,
        Clear,
        GameOver,
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
        /// タイトルシーンに遷移
        /// </summary>
        public void ChangeToTitleScene()
        {
            Audio.AudioManager.I?.PlayBGM(Audio.BgmType.Title);
            LoadScene(SceneName.Title);
        }

        /// <summary>
        /// インゲームシーンに遷移
        /// </summary>
        public void ChangeToInGameScene()
        {
            Audio.AudioManager.I?.PlayBGM(Audio.BgmType.InGame);
            LoadScene(SceneName.InGame);
        }

        /// <summary>
        /// ゲームクリアシーンに遷移
        /// </summary>
        public void ChangeToGameClearScene()
        {
            Audio.AudioManager.I?.PlayBGM(Audio.BgmType.GameClear);
            LoadScene(SceneName.Clear);
        }

        /// <summary>
        /// ゲームオーバーシーンに遷移
        /// </summary>
        public void ChangeToGameOverScene()
        {
            Audio.AudioManager.I?.PlayBGM(Audio.BgmType.GameOver);
            LoadScene(SceneName.GameOver);
        }
    }
}
