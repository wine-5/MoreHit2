using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using MoreHit.Events;
using MoreHit.Enemy;
using MoreHit.Audio;

namespace MoreHit.UI
{
    public class BossHPBar : MonoBehaviour
    {
        [Header("UI参照")]
        [SerializeField] private GameObject bossHPPanel;
        [SerializeField] private Image hpForegroundImage;
        [SerializeField] private TextMeshProUGUI bossNameText;
        
        [Header("Boss参照")]
        [SerializeField] private BossEnemy bossEnemy;
        
        [Header("設定")]
        [SerializeField] private string bossName = "Boss";
        [SerializeField] private float hpAnimationSpeed = 2f;
        
        private const float RETRY_DELAY = 0.1f;
        private const float FLASH_DURATION = 0.2f;
        private const int FLASH_COUNT = 3;
        private const float EASE_OUT_POWER = 2f;
        private const float FULL_HP_RATIO = 1f;
        
        private BossEnemy currentBoss;
        private Coroutine hpBarAnimationCoroutine;
                
        private void OnEnable()
        {
            GameEvents.OnBossAppear += OnBossAppear;
            GameEvents.OnBossDefeated += OnBossDefeated;
            GameEvents.OnBossDamaged += OnBossDamaged;
            GameEvents.OnStockFull += OnStockFull;
            GameEvents.OnPlayerDamage += OnPlayerDamage;
            GameEvents.OnEnemyDefeated += OnEnemyDefeated;
        }
        
        private void OnDisable()
        {
            GameEvents.OnBossAppear -= OnBossAppear;
            GameEvents.OnBossDefeated -= OnBossDefeated;
            GameEvents.OnBossDamaged -= OnBossDamaged;
            GameEvents.OnStockFull -= OnStockFull;
            GameEvents.OnPlayerDamage -= OnPlayerDamage;
            GameEvents.OnEnemyDefeated -= OnEnemyDefeated;
        }
        
        public void SetCurrentBoss(BossEnemy boss)
        {
            currentBoss = boss;
            if (currentBoss == null) return;
            ShowBossHPBar();
            UpdateHPBar();
        }
        
        public void ForceUpdateHPBar()
        {
            if (currentBoss != null)
                UpdateHPBar();
        }
        
        private void OnBossAppear()
        {
            currentBoss = FindActiveBoss();
            
            if (currentBoss != null)
            {
                ShowBossHPBar();
                UpdateHPBar();
            }
            else
            {
                StartCoroutine(RetryFindBoss());
            }
        }
        
        private IEnumerator RetryFindBoss()
        {
            yield return new WaitForSeconds(RETRY_DELAY);
            
            currentBoss = FindActiveBoss();
            if (currentBoss == null) yield break;
            ShowBossHPBar();
            UpdateHPBar();
        }
        
        private BossEnemy FindActiveBoss()
        {
            if (bossEnemy != null && bossEnemy.gameObject.activeInHierarchy) return bossEnemy;
            
            BossEnemy[] allBosses = FindObjectsByType<BossEnemy>(FindObjectsSortMode.None);
            
            foreach (BossEnemy boss in allBosses)
                if (boss != null && boss.gameObject.activeInHierarchy) return boss;
            
            return null;
        }
        
        private void OnBossDefeated()
        {
            if (AudioManager.I != null)
                AudioManager.I.PlaySE(SeType.BossDefeat);
            HideBossHPBar();
            currentBoss = null;
        }
        
        private void OnBossDamaged(int damage)
        {
            if (currentBoss != null)
                UpdateHPBar();
        }

        private void OnPlayerDamage(int damage, int currentHealth)
        {
            if (AudioManager.I != null)
                AudioManager.I.PlaySE(SeType.TakeDamage);
        }

        private void OnEnemyDefeated(GameObject enemy)
        {
            if (AudioManager.I != null)
                AudioManager.I.PlaySE(SeType.EnemyDefeat);
        }
        
        private void ShowBossHPBar()
        {
            if (bossHPPanel == null) return;
            bossHPPanel.SetActive(true);
            if (bossNameText != null)
                bossNameText.text = bossName;
            if (hpForegroundImage != null)
                hpForegroundImage.fillAmount = FULL_HP_RATIO;
        }
        
        private void HideBossHPBar()
        {
            if (bossHPPanel != null)
                bossHPPanel.SetActive(false);
        }
        
        private void UpdateHPBar()
        {
            if (currentBoss == null) return;
            if (hpForegroundImage == null) return;
            
            float targetHpRatio = currentBoss.GetHPRatio();
            StartHPBarAnimation(targetHpRatio);
            Canvas.ForceUpdateCanvases();
        }
        
        private void StartHPBarAnimation(float targetRatio)
        {
            if (hpBarAnimationCoroutine != null)
                StopCoroutine(hpBarAnimationCoroutine);
            hpBarAnimationCoroutine = StartCoroutine(AnimateHPBar(targetRatio));
        }
        
        private IEnumerator AnimateHPBar(float targetRatio)
        {
            float startRatio = hpForegroundImage.fillAmount;
            float elapsedTime = 0f;
            float animationDuration = Mathf.Abs(startRatio - targetRatio) / hpAnimationSpeed;
            
            while (elapsedTime < animationDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / animationDuration;
                float easedProgress = 1f - Mathf.Pow(1f - progress, EASE_OUT_POWER);
                float currentRatio = Mathf.Lerp(startRatio, targetRatio, easedProgress);
                
                hpForegroundImage.fillAmount = currentRatio;
                yield return null;
            }
            
            hpForegroundImage.fillAmount = targetRatio;
            hpBarAnimationCoroutine = null;
        }
        
        private void OnStockFull(GameObject target)
        {
            if (target != null && target.GetComponent<BossEnemy>() != null)
            {
                StartCoroutine(StockFullEffect());
            }
        }
        
        private IEnumerator StockFullEffect()
        {
            if (hpForegroundImage == null) yield break;
            
            Color originalColor = hpForegroundImage.color;
            Color flashColor = Color.red;
            
            for (int i = 0; i < FLASH_COUNT; i++)
            {
                hpForegroundImage.color = flashColor;
                yield return new WaitForSeconds(FLASH_DURATION);
                hpForegroundImage.color = originalColor;
                yield return new WaitForSeconds(FLASH_DURATION);
            }
        }
    }
}
