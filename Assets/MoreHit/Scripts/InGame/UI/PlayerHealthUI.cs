using UnityEngine;
using UnityEngine.UI;
using MoreHit.Events;
using MoreHit.Player;

namespace MoreHit.UI
{
    /// <summary>
    /// プレイヤーのHP表示を管理するUIクラス
    /// </summary>
    public class PlayerHealthUI : MonoBehaviour
    {
        [Header("UI設定")]
        [SerializeField] private Image backgroundImage; // 白色（減った部分）
        [SerializeField] private Image healthImage;     // 緑色（現在のHP）
#if UNITY_EDITOR
        [SerializeField] private bool animateChanges = true;
        [SerializeField] private float animationSpeed = 2f;
#endif
        
        private int maxHealth;
        private int currentHealth;
        private float targetFillAmount;
        private bool needsAnimation;
        private bool isInitialized = false;
        
        private void Start()
        {
            InitializeHealth();
        }
        
        private void OnEnable()
        {
            GameEvents.OnPlayerDamage += OnPlayerDamageReceived;
        }
        
        private void OnDisable()
        {
            GameEvents.OnPlayerDamage -= OnPlayerDamageReceived;
        }
        
        private void Update()
        {
#if UNITY_EDITOR
            if (needsAnimation && animateChanges)
            {
                AnimateHealthImage();
            }
#else
            if (needsAnimation)
            {
                AnimateHealthImage();
            }
#endif
        }
        
        /// <summary>
        /// 初期化処理：プレイヤーの最大HPを取得してImageを設定
        /// </summary>
        private void InitializeHealth()
        {
            var playerDataProvider = PlayerDataProvider.I;
            if (playerDataProvider != null)
            {
                maxHealth = playerDataProvider.MaxHealth;
                currentHealth = playerDataProvider.CurrentHealth;
                isInitialized = true;
            }
            else
            {
                // PlayerDataProviderがまだ準備されていない場合は後で再試行
                Debug.LogWarning("PlayerHealthUI: PlayerDataProviderが見つかりません。後で再初期化します。");
                StartCoroutine(RetryInitialization());
                return;
            }
            
            if (healthImage != null)
            {
                // Image Type を Filled に設定し、Fill Method を Horizontal に
                healthImage.type = Image.Type.Filled;
                healthImage.fillMethod = Image.FillMethod.Horizontal;
                
                float fillRatio = maxHealth > 0 ? (float)currentHealth / maxHealth : 1f;
                healthImage.fillAmount = fillRatio;
                targetFillAmount = fillRatio;
            }
            else
            {
                Debug.LogError("PlayerHealthUI: healthImageが設定されていません");
            }
            
            if (backgroundImage != null)
            {
                // 背景は常に満タン表示
                backgroundImage.type = Image.Type.Simple;
            }
        }
        
        /// <summary>
        /// PlayerDataProviderの初期化を待つ
        /// </summary>
        private System.Collections.IEnumerator RetryInitialization()
        {
            int maxRetries = 10;
            int retries = 0;
            
            while (retries < maxRetries)
            {
                yield return new WaitForSeconds(0.1f);
                
                var playerDataProvider = PlayerDataProvider.I;
                if (playerDataProvider != null)
                {
                    InitializeHealth();
                    yield break;
                }
                
                retries++;
            }
            
            // 最終的に見つからない場合はデフォルト値を使用
            Debug.LogError("PlayerHealthUI: PlayerDataProviderが見つかりません。デフォルト値を使用します。");
            maxHealth = 100;
            currentHealth = 100;
            isInitialized = true;
            
            if (healthImage != null)
            {
                healthImage.type = Image.Type.Filled;
                healthImage.fillMethod = Image.FillMethod.Horizontal;
                healthImage.fillAmount = 1f;
                targetFillAmount = 1f;
            }
        }
        
        /// <summary>
        /// プレイヤーがダメージを受けた時の処理
        /// </summary>
        private void OnPlayerDamageReceived(int damage, int newCurrentHealth)
        {
            // 初期化されていない場合はmaxHealthを更新
            if (!isInitialized || maxHealth <= 0)
            {
                var playerDataProvider = PlayerDataProvider.I;
                if (playerDataProvider != null)
                {
                    maxHealth = playerDataProvider.MaxHealth;
                    isInitialized = true;
                }
            }
            
            currentHealth = newCurrentHealth;
            float fillRatio = maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
            targetFillAmount = Mathf.Clamp01(fillRatio); // 0-1の範囲にクランプ
            
#if UNITY_EDITOR
            if (animateChanges)
            {
                needsAnimation = true;
            }
            else
            {
                UpdateImageImmediately();
            }
#else
            needsAnimation = true;
#endif

        }
        
        /// <summary>
        /// Imageの値を即座に更新
        /// </summary>
        private void UpdateImageImmediately()
        {
            if (healthImage != null)
            {
                healthImage.fillAmount = targetFillAmount;
            }
            needsAnimation = false;
        }
        
        /// <summary>
        /// Imageの値をアニメーション付きで更新
        /// </summary>
        private void AnimateHealthImage()
        {
            if (healthImage == null) return;
            
            float difference = Mathf.Abs(healthImage.fillAmount - targetFillAmount);
            if (difference < 0.01f)
            {
                healthImage.fillAmount = targetFillAmount;
                needsAnimation = false;
            }
            else
            {
#if UNITY_EDITOR
                healthImage.fillAmount = Mathf.Lerp(healthImage.fillAmount, targetFillAmount, animationSpeed * Time.deltaTime);
#else
                healthImage.fillAmount = Mathf.Lerp(healthImage.fillAmount, targetFillAmount, 2f * Time.deltaTime);
#endif
            }
        }
    }
}
