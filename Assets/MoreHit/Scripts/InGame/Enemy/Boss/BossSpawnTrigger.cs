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
            if (isOneTimeOnly && hasTriggered) return;
            if (!other.CompareTag(playerTag)) return;
            TriggerBossSpawn();
        }
        
        private void TriggerBossSpawn()
        {
            if (hasTriggered) return;
            
            hasTriggered = true;
            GameEvents.TriggerBossAreaEntered();
            
            if (wallsToActivate != null && wallsToActivate.Length > 0)
                StartCoroutine(ActivateWallsDelayed());
        }
        
        /// <summary>
        /// 遅延して壁を出現させる
        /// </summary>
        private System.Collections.IEnumerator ActivateWallsDelayed()
        {
            yield return new WaitForSeconds(wallActivateDelay);
            
            for (int i = 0; i < wallsToActivate.Length; i++)
            {
                if (wallsToActivate[i] != null)
                {
                    wallsToActivate[i].SetActive(true);
                    if (i < wallsToActivate.Length - 1) yield return new WaitForSeconds(0.1f);
                }
            }
        }
        
        /// <summary>
        /// トリガー状態をリセット（デバッグ用）
        /// </summary>
        [ContextMenu("Reset Trigger")]
        public void ResetTrigger()
        {
            hasTriggered = false;
            
            if (wallsToActivate != null)
            {
                foreach (var wall in wallsToActivate)
                {
                    if (wall != null) wall.SetActive(false);
                }
            }
        }
        
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!showGizmosInEditor) return;
            
            Gizmos.color = hasTriggered ? Color.gray : gizmosColor;
            var collider = GetComponent<Collider2D>();
            
            if (collider != null)
            {
                if (collider is BoxCollider2D boxCollider)
                    Gizmos.DrawWireCube(transform.position + (Vector3)boxCollider.offset, boxCollider.size);
                else if (collider is CircleCollider2D circleCollider)
                    Gizmos.DrawWireSphere(transform.position + (Vector3)circleCollider.offset, circleCollider.radius);
            }
            else
                Gizmos.DrawWireCube(transform.position, Vector3.one);
            
            if (wallsToActivate != null)
            {
                Gizmos.color = wallGizmosColor;
                for (int i = 0; i < wallsToActivate.Length; i++)
                {
                    if (wallsToActivate[i] != null)
                    {
                        Vector3 wallPos = wallsToActivate[i].transform.position;
                        Gizmos.DrawWireCube(wallPos, Vector3.one * 0.5f);
                        UnityEditor.Handles.color = wallGizmosColor;
                        UnityEditor.Handles.Label(wallPos + Vector3.up * 0.7f, $"Wall {i + 1}");
                    }
                }
            }
        }
#endif
    }
}