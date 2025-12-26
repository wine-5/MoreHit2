using UnityEngine;
using MoreHit; // 名前空間を忘れずに指定

public class StageManager : MonoBehaviour
{
    void Start()
    {
        // シングルトンなので Instance 経由で直接呼ぶ
        if (ElapsedTime.Instance != null)
        {
            ElapsedTime.Instance.StartTimer();
        }
        else
        {
            Debug.LogError("ElapsedTime がシーンに存在しません。");
        }
    }
}