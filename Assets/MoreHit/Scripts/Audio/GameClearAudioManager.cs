using UnityEngine;

namespace MoreHit
{
    public class GameClearAudioManager : MonoBehaviour
    {
        void Start()
        {
            Audio.AudioManager.I.PlayBGM("GameClearBGM");
        }
    }
}
