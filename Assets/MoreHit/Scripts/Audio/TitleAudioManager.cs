using UnityEngine;
using System.Collections;

namespace MoreHit
{
    public class TitleAudioManager : MonoBehaviour
    {
        [SerializeField] private bool waitForUserInput = true;
        
        void Start()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (waitForUserInput)
            {
                StartCoroutine(WaitForUserInputThenPlayBGM());
            }
            else
            {
                PlayTitleBGM();
            }
#else
            PlayTitleBGM();
#endif
        }
        
        private void PlayTitleBGM()
        {
            if (Audio.AudioManager.I != null)
            {
                Audio.AudioManager.I.PlayBGM(Audio.BgmType.Title);
            }
            else
            {
                Debug.LogWarning("[TitleAudioManager] AudioManagerが見つかりません");
            }
        }
        
#if UNITY_WEBGL && !UNITY_EDITOR
        private IEnumerator WaitForUserInputThenPlayBGM()
        {
            Debug.Log("[TitleAudioManager] WebGL: ユーザー入力を待機中...");
            
            // ユーザーの入力（クリック、キー押下、タッチ）を待つ
            yield return new WaitUntil(() => 
                Input.GetMouseButtonDown(0) || 
                Input.anyKeyDown || 
                (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            );
            
            Debug.Log("[TitleAudioManager] ユーザー入力を検出、BGMを再生します");
            PlayTitleBGM();
        }
#endif
    }
}
