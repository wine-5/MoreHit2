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
        [SerializeField] private GameObject bossGameObject;

        [Header("UI設定")]
        [SerializeField] private GameObject bossHPBarUI;
        [SerializeField] private BossHPBar bossHPBarScript;

        private void OnEnable()
        {
            GameEvents.OnBossAppear += OnBossAppear;
            GameEvents.OnBossDefeated += OnBossDefeated;
            GameEvents.OnBossDamaged += OnBossDamaged;
        }

        private void OnDisable()
        {
            GameEvents.OnBossAppear -= OnBossAppear;
            GameEvents.OnBossDefeated -= OnBossDefeated;
            GameEvents.OnBossDamaged -= OnBossDamaged;
        }

        private void Start()
        {
            if (bossHPBarUI != null)
                bossHPBarUI.SetActive(false);
        }

        private void OnBossAppear()
        {
            if (bossGameObject == null) return;

            bossGameObject.SetActive(true);

            if (bossHPBarUI != null)
                bossHPBarUI.SetActive(true);

            if (bossHPBarScript != null)
            {
                var bossEnemy = bossGameObject.GetComponent<MoreHit.Enemy.BossEnemy>();
                if (bossEnemy != null)
                    bossHPBarScript.SetCurrentBoss(bossEnemy);
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