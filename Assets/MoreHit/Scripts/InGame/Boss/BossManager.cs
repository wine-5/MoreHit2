using UnityEngine;
using MoreHit.Events;

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
        
        [Header("デバッグ")]
        [SerializeField] private bool showDebugLog = true;
        
        private void OnEnable()
        {
            // ボス出現イベントを購読
            GameEvents.OnBossAppear += OnBossAppear;
            GameEvents.OnBossDefeated += OnBossDefeated;
            
            if (showDebugLog)
                Debug.Log($"✅ [BossManager] イベントリスナーを登録しました");
        }
        
        private void OnDisable()
        {
            // イベント購読を解除
            GameEvents.OnBossAppear -= OnBossAppear;
            GameEvents.OnBossDefeated -= OnBossDefeated;
            
            if (showDebugLog)
                Debug.Log($"🔄 [BossManager] イベントリスナーを解除しました");
        }
        
        private void Start()
        {
            // HPバーは最初は非表示
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
                Debug.LogError($"❌ [BossManager] ボスGameObjectが設定されていません！");
                return;
            }
            
            if (bossGameObject.activeInHierarchy)
            {
                if (showDebugLog)
                    Debug.LogWarning($"⚠️ [BossManager] ボスは既にアクティブです");
                return;
            }
            
            // ボスを有効化
            bossGameObject.SetActive(true);
            
            // HPバーを表示
            if (bossHPBarUI != null)
                bossHPBarUI.SetActive(true);
            
            if (showDebugLog)
                Debug.Log($"🔥 [BossManager] ボス '{bossGameObject.name}' を有効化しました");
        }
        
        /// <summary>
        /// ボス敗北イベント受信
        /// </summary>
        private void OnBossDefeated()
        {
            // HPバーを非表示
            if (bossHPBarUI != null)
                bossHPBarUI.SetActive(false);
                
            if (showDebugLog)
                Debug.Log($"💀 [BossManager] ボス敗北 - HPバーを非表示にしました");
        }
    }
}