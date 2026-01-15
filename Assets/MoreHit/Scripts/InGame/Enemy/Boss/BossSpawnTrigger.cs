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
        private const float WALL_ACTIVATION_INTERVAL = 0.1f;
        
        [Header("設定")]
        [SerializeField] private bool isOneTimeOnly = true;
        [SerializeField] private string playerTag = "Player";
        
        [Header("壁設定")]
        [SerializeField] private GameObject[] wallsToActivate;
        [SerializeField] private float wallActivateDelay = 0.5f;
        
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
        
        private System.Collections.IEnumerator ActivateWallsDelayed()
        {
            yield return new WaitForSeconds(wallActivateDelay);
            
            for (int i = 0; i < wallsToActivate.Length; i++)
            {
                if (wallsToActivate[i] != null)
                {
                    wallsToActivate[i].SetActive(true);
                    if (i < wallsToActivate.Length - 1)
                        yield return new WaitForSeconds(WALL_ACTIVATION_INTERVAL);
                }
            }
        }
        
        [ContextMenu("Reset Trigger")]
        public void ResetTrigger()
        {
            hasTriggered = false;
            
            if (wallsToActivate != null)
            {
                foreach (var wall in wallsToActivate)
                {
                    if (wall != null)
                        wall.SetActive(false);
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