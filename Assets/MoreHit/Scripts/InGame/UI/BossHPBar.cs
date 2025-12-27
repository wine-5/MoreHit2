using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MoreHit.Events;
using MoreHit.Enemy;

namespace MoreHit.UI
{
    /// <summary>
    /// ボスのHPバー表示システム（二つのImage重ね合わせ方式）
    /// </summary>
    public class BossHPBar : MonoBehaviour
    {
        [Header("UI参照")]
        [SerializeField] private GameObject bossHPPanel;
        [SerializeField] private Image hpBackgroundImage; // 背景（空のHPバー）
        [SerializeField] private Image hpForegroundImage; // 前景（現在のHP表示）
        [SerializeField] private TextMeshProUGUI bossNameText;
        
        [Header("設定")]
        [SerializeField] private string bossName = "Boss";
        
        private BossEnemy currentBoss;
                
        private void OnEnable()
        {
            GameEvents.OnBossAppear += OnBossAppear;
            GameEvents.OnBossDefeated += OnBossDefeated;
        }
        
        private void OnDisable()
        {
            GameEvents.OnBossAppear -= OnBossAppear;
            GameEvents.OnBossDefeated -= OnBossDefeated;
        }
        
        private void Update()
        {
            if (currentBoss != null && bossHPPanel.activeInHierarchy)
            {
                UpdateHPBar();
            }
        }
        
        /// <summary>
        /// ボス出現時の処理
        /// </summary>
        private void OnBossAppear()
        {
            // シーン内のBossEnemyを検索
            currentBoss = FindFirstObjectByType<BossEnemy>();
            
            if (currentBoss != null)
            {
                // HPバーを表示
                ShowBossHPBar();
                UpdateHPBar();
            }
        }
        
        /// <summary>
        /// ボス撃破時の処理
        /// </summary>
        private void OnBossDefeated()
        {
            HideBossHPBar();
            currentBoss = null;
        }
        
        /// <summary>
        /// HPバーを表示
        /// </summary>
        private void ShowBossHPBar()
        {
            if (bossHPPanel != null)
            {
                bossHPPanel.SetActive(true);
                
                if (bossNameText != null)
                {
                    bossNameText.text = bossName;
                }
                
                // HPバーを満タンの状態で初期化
                if (hpForegroundImage != null)
                {
                    hpForegroundImage.fillAmount = 1.0f;
                }
            }
        }
        
        /// <summary>
        /// HPバーを非表示
        /// </summary>
        private void HideBossHPBar()
        {
            if (bossHPPanel != null)
            {
                bossHPPanel.SetActive(false);
            }
        }
        
        /// <summary>
        /// HPバーの更新
        /// </summary>
        private void UpdateHPBar()
        {
            if (currentBoss == null) return;
            
            int currentHP = currentBoss.GetCurrentHP();
            int maxHP = currentBoss.GetMaxHP();
            float hpRatio = currentBoss.GetHPRatio();
            
            // Image の fillAmount でHPバーを更新
            if (hpForegroundImage != null)
            {
                hpForegroundImage.fillAmount = hpRatio;
            }
        }
    }
}