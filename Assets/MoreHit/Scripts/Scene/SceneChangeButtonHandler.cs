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
    }
}