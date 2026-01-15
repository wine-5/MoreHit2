using UnityEngine;
using System.Collections;

namespace MoreHit
{
    /// <summary>
    /// タイトル画面のBGM再生を管理
    /// </summary>
    public class TitleAudioManager : MonoBehaviour
    {
        private void Start()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            StartCoroutine(WaitForUserInputThenPlayBGM());
#else
            PlayTitleBGM();
#endif
        }
        
        private void PlayTitleBGM()
        {
            if (Audio.AudioManager.I != null)
                Audio.AudioManager.I.PlayBGM(Audio.BgmType.Title);
            else
                Debug.LogWarning("[TitleAudioManager] AudioManagerが見つかりません");
        }
        
#if UNITY_WEBGL && !UNITY_EDITOR
        private IEnumerator WaitForUserInputThenPlayBGM()
        {
            yield return new WaitUntil(() => 
                Input.GetMouseButtonDown(0) || 
                Input.anyKeyDown || 
                (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            );
            
            PlayTitleBGM();
        }
#endif
    }
}
