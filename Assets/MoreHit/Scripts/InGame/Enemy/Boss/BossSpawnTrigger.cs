using UnityEngine;
using MoreHit.Events;

namespace MoreHit.Boss
{
    /// <summary>
    /// プレイヤーがトリガーエリアに入ったときにボス出現イベントを発火する
    /// スーパーマリオ風にプレイヤーの後ろに壁を出現させる機能付き
    /// </summary>
    public class BossSpawnTrigger : MonoBehaviour
    {
        [Header("設定")]
        [SerializeField] private bool isOneTimeOnly = true; // 一度だけトリガーするか
        [SerializeField] private string playerTag = "Player"; // プレイヤーのタグ
        
        [Header("壁設定")]
        [SerializeField] private GameObject[] wallsToActivate; // 出現させる壁のリスト
        [SerializeField] private float wallActivateDelay = 0.5f; // ボス出現後、壁が出現するまでの遅延時間
        [SerializeField] private bool showWallActivationLog = true; // 壁出現のログを表示するか
        
        [Header("デバッグ表示")]
        [SerializeField] private bool showGizmosInEditor = true;
        [SerializeField] private Color gizmosColor = Color.yellow;
        [SerializeField] private Color wallGizmosColor = Color.red;
        
        private bool hasTriggered = false;
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            // 一度だけトリガーする設定で、既にトリガー済みの場合は無視
            if (isOneTimeOnly && hasTriggered)
                return;
                
            // プレイヤータグチェック
            if (!other.CompareTag(playerTag))
                return;
            
            // ボス出現イベントを発火
            TriggerBossSpawn();
        }
        
        private void TriggerBossSpawn()
        {
            // 既にトリガー済みならイベント発火を阻止
            if (hasTriggered)
            {
                Debug.LogWarning($"⚠️ [BossSpawnTrigger] 既にトリガー済みのため、重複イベント発火を阻止しました");
                return;
            }
            
            Debug.Log($"🔥 [BossSpawnTrigger] プレイヤーがボス出現エリアに侵入！ボス演出を開始します");
            
            // フラグを先に立てる（重複防止）
            hasTriggered = true;
            
            // GameEventsでボス出現エリア侵入を通知（演出開始）
            GameEvents.TriggerBossAreaEntered();
            
            // 壁を遅延して出現させる
            if (wallsToActivate != null && wallsToActivate.Length > 0)
            {
                StartCoroutine(ActivateWallsDelayed());
            }
            
            // 一度だけの設定の場合、フラグを立てる（既に上で実行済み）
            if (isOneTimeOnly)
            {
                Debug.Log($"✅ [BossSpawnTrigger] 一度だけトリガー完了、今後は無効");
            }
        }
        
        /// <summary>
        /// 遅延して壁を出現させる
        /// </summary>
        private System.Collections.IEnumerator ActivateWallsDelayed()
        {
            // 指定した時間だけ待機
            yield return new WaitForSeconds(wallActivateDelay);
            
            // 壁を順番に出現させる
            for (int i = 0; i < wallsToActivate.Length; i++)
            {
                if (wallsToActivate[i] != null)
                {
                    wallsToActivate[i].SetActive(true);
                    
                    if (showWallActivationLog)
                    {
                        Debug.Log($"🧱 [BossSpawnTrigger] 壁 '{wallsToActivate[i].name}' を出現させました ({i + 1}/{wallsToActivate.Length})");
                    }
                    
                    // 壁と壁の間に少し間隔を空ける（演出効果）
                    if (i < wallsToActivate.Length - 1)
                    {
                        yield return new WaitForSeconds(0.1f);
                    }
                }
                else
                {
                    Debug.LogWarning($"⚠️ [BossSpawnTrigger] 壁のインデックス {i} がnullです");
                }
            }
            
            if (showWallActivationLog)
            {
                Debug.Log($"✅ [BossSpawnTrigger] 全ての壁の出現が完了しました");
            }
        }
        
        /// <summary>
        /// トリガー状態をリセット（デバッグ用）
        /// </summary>
        [ContextMenu("Reset Trigger")]
        public void ResetTrigger()
        {
            hasTriggered = false;
            
            // 壁も非表示に戻す
            if (wallsToActivate != null)
            {
                foreach (var wall in wallsToActivate)
                {
                    if (wall != null)
                        wall.SetActive(false);
                }
            }
            
            Debug.Log($"🔄 [BossSpawnTrigger] トリガーと壁をリセットしました");
        }
        
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!showGizmosInEditor) return;
            
            // トリガーエリアを描画
            Gizmos.color = hasTriggered ? Color.gray : gizmosColor;
            
            // Collider2Dがある場合はその範囲を描画
            var collider = GetComponent<Collider2D>();
            if (collider != null)
            {
                if (collider is BoxCollider2D boxCollider)
                {
                    Gizmos.DrawWireCube(transform.position + (Vector3)boxCollider.offset, boxCollider.size);
                }
                else if (collider is CircleCollider2D circleCollider)
                {
                    Gizmos.DrawWireSphere(transform.position + (Vector3)circleCollider.offset, circleCollider.radius);
                }
            }
            else
            {
                // デフォルトで1x1のキューブを描画
                Gizmos.DrawWireCube(transform.position, Vector3.one);
            }
            
            // 壁の位置も描画
            if (wallsToActivate != null)
            {
                Gizmos.color = wallGizmosColor;
                for (int i = 0; i < wallsToActivate.Length; i++)
                {
                    if (wallsToActivate[i] != null)
                    {
                        Vector3 wallPos = wallsToActivate[i].transform.position;
                        Gizmos.DrawWireCube(wallPos, Vector3.one * 0.5f);
                        
                        // 壁の番号を表示
                        UnityEditor.Handles.color = wallGizmosColor;
                        UnityEditor.Handles.Label(wallPos + Vector3.up * 0.7f, $"Wall {i + 1}");
                    }
                }
            }
        }
#endif
    }
}