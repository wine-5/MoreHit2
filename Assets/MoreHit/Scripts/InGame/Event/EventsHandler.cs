using UnityEngine;
using MoreHit.Events;
using MoreHit.Scene;
using MoreHit.Effect;

namespace MoreHit.InGame
{
    /// <summary>
    /// ゲームイベントの購読を管理するクラス
    /// GameEventsで発火されたイベントを受け取り、適切な処理を実行する
    /// </summary>
    public class EventsHandler : MonoBehaviour
    {
        private void OnEnable()
        {
            GameEvents.OnPlayerDeath += OnPlayerDeath;
            GameEvents.OnStockFull += OnStockFull;
        }

        private void OnDisable()
        {
            GameEvents.OnPlayerDeath -= OnPlayerDeath;
            GameEvents.OnStockFull -= OnStockFull;
        }

        /// <summary>
        /// プレイヤー死亡時の処理
        /// </summary>
        private void OnPlayerDeath()
        {
            if (SceneController.I != null)
                SceneController.I.LoadScene(SceneName.GameOver);
            else
                Debug.LogError("EventsHandler: SceneController instance not found!");
        }
        
        /// <summary>
        /// 敵のストックが満タンになった時の処理
        /// </summary>
        private void OnStockFull(GameObject enemy)
        {
            // FullStockEffectを表示
            if (EffectFactory.I != null)
            {
                var effect = EffectFactory.I.CreateEffect(MoreHit.Effect.EffectType.FullStockEffect, enemy.transform.position);
                if (effect != null)
                {
                    // 3秒後にエフェクトを非表示にする
                    EffectFactory.I.ReturnEffectDelayed(effect, 3f);
                }
            }
            else
            {
                Debug.LogError("[EventsHandler] EffectFactory が見つかりません!");
            }
        }
    }
}