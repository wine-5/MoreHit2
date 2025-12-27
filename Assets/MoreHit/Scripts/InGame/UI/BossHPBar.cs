using UnityEngine;
using UnityEngine.UI;
using MoreHit.Events;
using MoreHit.Enemy;

namespace MoreHit.UI
{
    /// <summary>
    /// ボスのHPバー表示システム
    /// </summary>
    public class BossHPBar : MonoBehaviour
    {
        [Header("UI参照")]
        [SerializeField] private GameObject bossHPPanel;
        [SerializeField] private Slider hpSlider;
        [SerializeField] private Text bossNameText;
        [SerializeField] private Text hpValueText;
        
        [Header("設定")]
        [SerializeField] private string bossName = "Boss";
        
        private Boss currentBoss;
        
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
            // シーン内のBossを検索
            currentBoss = FindFirstObjectByType<Boss>();
            
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
            
            // スライダー更新
            if (hpSlider != null)
            {
                hpSlider.value = hpRatio;
            }
            
            // HP数値表示
            if (hpValueText != null)
            {
                hpValueText.text = $"{currentHP} / {maxHP}";
            }
        }
    }
}