using UnityEngine;

namespace MoreHit
{
    public class TitleAudioManager : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            Audio.AudioManager.I.PlayBGM("TitleBGM");
        
        }

        
    }
}
