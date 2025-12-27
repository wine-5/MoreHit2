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
        
        [Header("Boss参照")]
        [SerializeField] private BossEnemy bossEnemy; // Editorでアタッチ
        
        [Header("設定")]
        [SerializeField] private string bossName = "Boss";
        [SerializeField] private float hpAnimationSpeed = 2f; // HPバーアニメーション速度
        
        private BossEnemy currentBoss;
        private Coroutine hpBarAnimationCoroutine; // アニメーション制御用
                
        private void OnEnable()
        {
            Debug.Log("[BossHPBar] イベント登録開始");
            GameEvents.OnBossAppear += OnBossAppear;
            GameEvents.OnBossDefeated += OnBossDefeated;
            GameEvents.OnBossDamaged += OnBossDamaged;
            GameEvents.OnStockFull += OnStockFull;
            Debug.Log("[BossHPBar] イベント登録完了");
        }
        
        private void OnDisable()
        {
            Debug.Log("[BossHPBar] イベント登録解除");
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
        
        /// <summary>
        /// ボス出現時の処理
        /// </summary>
        private void OnBossAppear()
        {
            Debug.Log($"[BossHPBar] OnBossAppear呼び出し - bossEnemy: {bossEnemy?.name}");
            
            // アタッチされたBossEnemyを使用
            currentBoss = bossEnemy;
            
            if (currentBoss != null)
            {
                Debug.Log($"[BossHPBar] Boss設定完了: {currentBoss.name}");
                // HPバーを表示
                ShowBossHPBar();
                UpdateHPBar();
            }
            else
            {
                Debug.LogError("[BossHPBar] BossEnemyがアタッチされていません！EditorでbossEnemyフィールドにアタッチしてください。");
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
            
            Debug.Log($"[BossHPBar] HPバー更新 - {currentHP}/{maxHP} (目標比率: {targetHpRatio:F2})");
            
            // Image の fillAmount でHPバーを更新
            if (hpForegroundImage != null)
            {
                float currentFillAmount = hpForegroundImage.fillAmount;
                
                // 詳細なデバッグ情報を追加
                Debug.Log($"[BossHPBar] fillAmount更新開始: {currentFillAmount:F2} -> {targetHpRatio:F2}");
                Debug.Log($"[BossHPBar] Image設定 - Type: {hpForegroundImage.type}, enabled: {hpForegroundImage.enabled}, color: {hpForegroundImage.color}");
                
                // Filled タイプの詳細設定も確認
                if (hpForegroundImage.type == Image.Type.Filled)
                {
                    Debug.Log($"[BossHPBar] Fill設定 - Method: {hpForegroundImage.fillMethod}, Origin: {hpForegroundImage.fillOrigin}, Amount: {hpForegroundImage.fillAmount}");
                    
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
            float animationDuration = Mathf.Abs(startRatio - targetRatio) / hpAnimationSpeed;
            
            Debug.Log($"[BossHPBar] HPバーアニメーション開始: {startRatio:F2} -> {targetRatio:F2}, 時間: {animationDuration:F2}秒");
            
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
            Debug.Log($"[BossHPBar] HPバーアニメーション完了: {targetRatio:F2}");
            
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