using UnityEngine;
using System.Collections;
using MoreHit.Events;
using MoreHit.UI;
using MoreHit.Audio;

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
        [SerializeField] private MoreHit.Camera.CameraController mainCameraController;
        
        [Header("ボス参照")]
        [SerializeField] private Transform bossTransform;
        [SerializeField] private GameObject bossGameObject;
        
        [Header("タイミング設定")]
        [SerializeField] private float delayBeforeZoomOut = 10f;

        private bool isPlayingIntro = false;
        
        private void OnEnable()
        {
            GameEvents.OnBossAreaEntered += StartBossIntroduction;
        }
        
        private void OnDisable()
        {
            GameEvents.OnBossAreaEntered -= StartBossIntroduction;
        }
        
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
            
            // メインカメラの追従を停止
            if (mainCameraController != null)
                mainCameraController.SetFollowEnabled(false);
            
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
            
            // BossBGMに切り替え（SOのフェード設定を使用）
            if (AudioManager.I != null)
                AudioManager.I.TransitionToBGM(BgmType.Boss);
            
            if (bossBattleStartUI != null)
            {
                yield return bossBattleStartUI.ShowReady();
                yield return bossBattleStartUI.ShowFight();
            }
            
            // Boss動作開始
            if (bossGameObject != null)
            {
                var bossEnemy = bossGameObject.GetComponent<MoreHit.Enemy.BossEnemy>();
                if (bossEnemy != null)
                    bossEnemy.SetCanMove(true);
            }
            
            // メインカメラの追従を再開
            if (mainCameraController != null)
                mainCameraController.SetFollowEnabled(true);
            
            LockPlayerInput(false);
            isPlayingIntro = false;
        }
        
        private void LockPlayerInput(bool isLocked) => GameEvents.TriggerInputLockChanged(isLocked);
    }
}
