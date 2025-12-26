using UnityEngine;
using MoreHit.ElapsedTime; // 名前空間を忘れずに指定
using MoreHit.Scene;

/// <summary>
/// ステージ開始時のセットアップ（タイマーの開始指示など）を担当するクラス
/// </summary>
public class StageManager : MonoBehaviour
{

    [SerializeField]
    private SceneName nextScene = SceneName.InGame;
    void Start()
    {
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