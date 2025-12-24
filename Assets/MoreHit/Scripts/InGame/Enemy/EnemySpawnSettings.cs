using UnityEngine;

namespace MoreHit.Enemy
{
    // スポーン地点に付けて、その場所から出る敵の個別の設定を決めるためのクラス
    public class EnemySpawnSettings : MonoBehaviour
    {
        [Header("この地点から出る敵の巡回範囲")]
        public float leftRange = 3f;
        public float rightRange = 3f;
    }
}