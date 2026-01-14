using UnityEngine;
using MoreHit.Events;
using MoreHit.UI;

namespace MoreHit.Boss
{
    /// <summary>
    /// ボス出現とHPバー表示を管理するマネージャー
    /// </summary>
    public class BossManager : MonoBehaviour
    {
        [Header("ボス設定")]
        [SerializeField] private GameObject bossGameObject; // ボスのGameObject

        [Header("UI設定")]
        [SerializeField] private GameObject bossHPBarUI; // ボスHPバーのUI
        [SerializeField] private BossHPBar bossHPBarScript; // BossHPBarコンポーネント（直接参照）

        [Header("デバッグ")]
        [SerializeField] private bool showDebugLog = true;

        private void OnEnable()
        {
            // ボス出現イベントを購読
            GameEvents.OnBossAppear += OnBossAppear;
            GameEvents.OnBossDefeated += OnBossDefeated;
            GameEvents.OnBossDamaged += OnBossDamaged; // 直接ダメージイベントも購読   
        }

        private void OnDisable()
        {
            // イベント購読を解除
            GameEvents.OnBossAppear -= OnBossAppear;
            GameEvents.OnBossDefeated -= OnBossDefeated;
            GameEvents.OnBossDamaged -= OnBossDamaged;
        }

        private void Start()
        {
            if (bossHPBarUI != null)
                bossHPBarUI.SetActive(false);
        }

        /// <summary>
        /// ボス出現イベント受信
        /// </summary>
        private void OnBossAppear()
        {
            if (bossGameObject == null)
            {
                Debug.LogError("[BossManager] ボスGameObjectが設定されていません！");
                return;
            }

            bossGameObject.SetActive(true);

            if (bossHPBarUI != null)
                bossHPBarUI.SetActive(true);

            // BossHPBarスクリプトに直接ボスを設定
            if (bossHPBarScript != null)
            {
                var bossEnemy = bossGameObject.GetComponent<MoreHit.Enemy.BossEnemy>();
                if (bossEnemy != null)
                {
                    bossHPBarScript.SetCurrentBoss(bossEnemy);
                }
            }
        }

        private void OnBossDamaged(int damage)
        {
            if (bossHPBarScript != null)
                bossHPBarScript.ForceUpdateHPBar();
        }

        private void OnBossDefeated()
        {
            if (bossHPBarUI != null)
                bossHPBarUI.SetActive(false);
        }
    }
}