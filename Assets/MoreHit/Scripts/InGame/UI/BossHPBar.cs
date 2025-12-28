using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
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
        
        [Header("Boss参照")]
        [SerializeField] private BossEnemy bossEnemy; // Editorでアタッチ
        
        [Header("設定")]
        [SerializeField] private string bossName = "Boss";
        [SerializeField] private float hpAnimationSpeed = 2f; // HPバーアニメーション速度
        
        private BossEnemy currentBoss;
        private Coroutine hpBarAnimationCoroutine; // アニメーション制御用
                
        private void OnEnable()
        {
            GameEvents.OnBossAppear += OnBossAppear;
            GameEvents.OnBossDefeated += OnBossDefeated;
            GameEvents.OnBossDamaged += OnBossDamaged;
            GameEvents.OnStockFull += OnStockFull;
        }
        
        private void OnDisable()
        {
            GameEvents.OnBossAppear -= OnBossAppear;
            GameEvents.OnBossDefeated -= OnBossDefeated;
            GameEvents.OnBossDamaged -= OnBossDamaged;
            GameEvents.OnStockFull -= OnStockFull;
        }
        
        private void Update()
        {
            // Updateでの定期更新は削除（イベント駆動に変更）
            // HPバーはOnBossDamagedイベント時のみアニメーション更新
        }
        
        /// <summary>        /// 外部からボスを直接設定（BossManager用）
        /// </summary>
        public void SetCurrentBoss(BossEnemy boss)
        {
            currentBoss = boss;
            Debug.Log($"[BossHPBar] 外部からボスを設定: {boss?.name}");
            
            if (currentBoss != null)
            {
                ShowBossHPBar();
                UpdateHPBar();
            }
        }
        
        /// <summary>
        /// 外部からHPバー強制更新（BossManager用）
        /// </summary>
        public void ForceUpdateHPBar()
        {
            if (currentBoss != null)
            {
                Debug.Log("[BossHPBar] 外部からHPバー強制更新");
                UpdateHPBar();
            }
            else
            {
                Debug.LogWarning("[BossHPBar] currentBossがnullのため、HPバー強制更新をスキップ");
            }
        }
        
        /// <summary>        /// ボス出現時の処理
        /// </summary>
        private void OnBossAppear()
        {
            Debug.Log("[BossHPBar] OnBossAppear呼び出し - ボス検索開始");
            
            // 動的にアクティブなBossEnemyを探す
            currentBoss = FindActiveBoss();
            
            if (currentBoss != null)
            {
                Debug.Log($"[BossHPBar] アクティブなボスを発見: {currentBoss.name}");
                // HPバーを表示
                ShowBossHPBar();
                UpdateHPBar();
            }
            else
            {
                Debug.LogError("[BossHPBar] アクティブなBossEnemyが見つかりません！少し待ってから再試行します");
                // 少し待ってから再試行
                StartCoroutine(RetryFindBoss());
            }
        }
        
        /// <summary>
        /// ボス検索を少し遅らせて再試行
        /// </summary>
        private System.Collections.IEnumerator RetryFindBoss()
        {
            yield return new WaitForSeconds(0.1f);
            
            Debug.Log("[BossHPBar] ボス再検索開始");
            currentBoss = FindActiveBoss();
            
            if (currentBoss != null)
            {
                Debug.Log($"[BossHPBar] 再試行でアクティブなボスを発見: {currentBoss.name}");
                ShowBossHPBar();
                UpdateHPBar();
            }
            else
            {
                Debug.LogError("[BossHPBar] 再試行でもアクティブなBossEnemyが見つかりませんでした");
            }
        }
        
        /// <summary>
        /// アクティブなBossEnemyを動的に探す
        /// </summary>
        private BossEnemy FindActiveBoss()
        {
            Debug.Log("[BossHPBar] FindActiveBoss開始");
            
            // まずアタッチされたbossEnemyをチェック
            if (bossEnemy != null && bossEnemy.gameObject.activeInHierarchy)
            {
                Debug.Log($"[BossHPBar] アタッチされたボスを使用: {bossEnemy.name}");
                return bossEnemy;
            }
            else
            {
                Debug.Log($"[BossHPBar] アタッチされたボス無効 - bossEnemy={(bossEnemy != null ? bossEnemy.name : "null")}, active={bossEnemy?.gameObject.activeInHierarchy}");
            }
            
            // アタッチされていない、または非アクティブな場合は動的に探す
            BossEnemy[] allBosses = FindObjectsByType<BossEnemy>(FindObjectsSortMode.None);
            Debug.Log($"[BossHPBar] シーン内のBossEnemy数: {allBosses.Length}");
            
            foreach (BossEnemy boss in allBosses)
            {
                Debug.Log($"[BossHPBar] ボス検査: {boss?.name}, active={boss?.gameObject.activeInHierarchy}");
                
                if (boss != null && boss.gameObject.activeInHierarchy)
                {
                    Debug.Log($"[BossHPBar] 動的にアクティブなボスを発見: {boss.name}");
                    return boss;
                }
            }
            
            Debug.Log("[BossHPBar] アクティブなボスが見つかりませんでした");
            return null;
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
        /// ボスダメージ時の処理
        /// </summary>
        private void OnBossDamaged(int damage)
        {
            Debug.Log($"[BossHPBar] OnBossDamaged呼び出し - damage: {damage}");
            
            if (currentBoss != null)
            {
                Debug.Log($"[BossHPBar] ボスダメージ検出 - 即座にHPバー更新");
                UpdateHPBar();
            }
            else
            {
                Debug.Log($"[BossHPBar] currentBossがnull - HPバー更新スキップ");
            }
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
            if (currentBoss == null) 
            {
                Debug.Log("[BossHPBar] UpdateHPBar - currentBossがnull");
                return;
            }
            
            int currentHP = currentBoss.GetCurrentHP();
            int maxHP = currentBoss.GetMaxHP();
            float targetHpRatio = currentBoss.GetHPRatio();
            
            // Image の fillAmount でHPバーを更新
            if (hpForegroundImage != null)
            {
                float currentFillAmount = hpForegroundImage.fillAmount;
                
                // Filled タイプの詳細設定も確認
                if (hpForegroundImage.type == Image.Type.Filled)
                {
                    // 右から左に減るかチェック
                    if (hpForegroundImage.fillMethod != Image.FillMethod.Horizontal)
                    {
                        Debug.LogWarning("[BossHPBar] 横向きHPバーにするには、UnityEditorでFill Methodを'Horizontal'に設定してください！");
                    }
                    else if (hpForegroundImage.fillOrigin != 1)
                    {
                        Debug.LogWarning("[BossHPBar] 右から左に減らすには、UnityEditorでFill Originを'Right(1)'に設定してください！");
                    }
                    
                    // アニメーション開始
                    StartHPBarAnimation(targetHpRatio);
                }
                else
                {
                    Debug.LogWarning($"[BossHPBar] 警告: Image TypeがSimpleです！fillAmountは無効です。UnityEditorでFilledに変更してください。");
                }
                
                // 強制的にUIを更新
                Canvas.ForceUpdateCanvases();
            }
            else
            {
                Debug.LogError("[BossHPBar] hpForegroundImageがnull");
            }
        }
        
        /// <summary>
        /// HPバーのアニメーション開始
        /// </summary>
        private void StartHPBarAnimation(float targetRatio)
        {
            // 既存のアニメーションを停止
            if (hpBarAnimationCoroutine != null)
            {
                StopCoroutine(hpBarAnimationCoroutine);
            }
            
            // 新しいアニメーション開始
            hpBarAnimationCoroutine = StartCoroutine(AnimateHPBar(targetRatio));
        }
        
        /// <summary>
        /// HPバーアニメーション実行
        /// </summary>
        private System.Collections.IEnumerator AnimateHPBar(float targetRatio)
        {
            float startRatio = hpForegroundImage.fillAmount;
            float elapsedTime = 0f;
#if UNITY_EDITOR
            float animationDuration = Mathf.Abs(startRatio - targetRatio) / hpAnimationSpeed;
#else
            float animationDuration = Mathf.Abs(startRatio - targetRatio) / 2f; // リリースビルド用固定値
#endif
            
            while (elapsedTime < animationDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / animationDuration;
                
                // スムーズなアニメーション（EaseOut）
                float easedProgress = 1f - Mathf.Pow(1f - progress, 2f);
                float currentRatio = Mathf.Lerp(startRatio, targetRatio, easedProgress);
                
                hpForegroundImage.fillAmount = currentRatio;
                
                yield return null;
            }
            
            // 最終値を確実に設定
            hpForegroundImage.fillAmount = targetRatio;
            
            hpBarAnimationCoroutine = null;
        }
        
        /// <summary>
        /// ストック満タン時の処理
        /// </summary>
        private void OnStockFull(GameObject target)
        {
            // ボスのストックが満タンになった場合のみ演出
            if (target != null && target.GetComponent<BossEnemy>() != null)
            {
                StartCoroutine(StockFullEffect());
            }
        }
        
        /// <summary>
        /// ストック満タン演出（HPバーの点滅）
        /// </summary>
        private System.Collections.IEnumerator StockFullEffect()
        {
            if (hpForegroundImage == null) yield break;
            
            Color originalColor = hpForegroundImage.color;
            Color flashColor = Color.red;
            
            // 3回点滅
            for (int i = 0; i < 3; i++)
            {
                hpForegroundImage.color = flashColor;
                yield return new WaitForSeconds(0.2f);
                hpForegroundImage.color = originalColor;
                yield return new WaitForSeconds(0.2f);
            }
        }
    }
}