using UnityEngine;
using System.Collections;
using MoreHit.Events;
using MoreHit.UI;

namespace MoreHit.Boss
{
    /// <summary>
    /// ボス出現演出の全体制御
    /// WARNING → カメラパン → ボススポーン → ズームアウト → Ready → Fight
    /// </summary>
    public class BossAppearController : MonoBehaviour
    {
        [Header("UI参照")]
        [SerializeField] private WarningUI warningUI;
        [SerializeField] private BossBattleStartUI bossBattleStartUI;
        
        [Header("カメラ参照")]
        [SerializeField] private BossCameraController bossCameraController;
        
        [Header("ボス参照")]
        [SerializeField] private Transform bossTransform;
        
        [Header("タイミング設定")]
        [SerializeField] private float delayBeforeZoomOut = 0.5f; // ボススポーン後の待機時間
        

        private bool isPlayingIntro = false;
        
        private void OnEnable()
        {
            GameEvents.OnBossAreaEntered += StartBossIntroduction;
        }
        
        private void OnDisable()
        {
            GameEvents.OnBossAreaEntered -= StartBossIntroduction;
        }
        
        /// <summary>
        /// ボス出現演出の開始
        /// </summary>
        private void StartBossIntroduction()
        {
            if (isPlayingIntro) return;
            
            StartCoroutine(PlayBossIntroductionSequence());
        }
        
        /// <summary>
        /// 演出シーケンス全体の実行
        /// </summary>
        private IEnumerator PlayBossIntroductionSequence()
        {
            isPlayingIntro = true;
            
            LockPlayerInput(true);
            Time.timeScale = 0f;
            
            if (warningUI != null)
                yield return warningUI.PlayWarningAnimation();
            
            Time.timeScale = 1f;
            
            if (bossCameraController != null && bossTransform != null)
                yield return bossCameraController.PanToBoss(bossTransform);
            
            GameEvents.TriggerBossAppear();
            yield return new WaitForSeconds(delayBeforeZoomOut);
            
            if (bossCameraController != null)
                yield return bossCameraController.ZoomOutToFieldView();
            
            if (bossBattleStartUI != null)
            {
                yield return bossBattleStartUI.ShowReady();
                yield return bossBattleStartUI.ShowFight();
            }
            
            LockPlayerInput(false);
            isPlayingIntro = false;
        }
        
        /// <summary>
        /// プレイヤー入力のロック/アンロック
        /// </summary>
        private void LockPlayerInput(bool isLocked)
        {
            // イベントを発火して入力ロックを通知
            GameEvents.TriggerInputLockChanged(isLocked);
        }
        
        /// <summary>
        /// 演出を強制リセット（デバッグ用）
        /// </summary>
        [ContextMenu("Reset Boss Introduction")]
        public void ResetIntroduction()
        {
            StopAllCoroutines();
            isPlayingIntro = false;
            Time.timeScale = 1f;
            LockPlayerInput(false);
            
            if (bossCameraController != null)
                bossCameraController.ReturnToNormalCamera();
        }
    }
}
