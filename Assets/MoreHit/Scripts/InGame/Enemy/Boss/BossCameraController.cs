using UnityEngine;
using System.Collections;

namespace MoreHit.Boss
{
    /// <summary>
    /// ボス演出専用のカメラ制御
    /// Transformを直接操作してカメラ演出を実現
    /// </summary>
    public class BossCameraController : MonoBehaviour
    {
        [Header("カメラ参照")]
        [SerializeField] private UnityEngine.Camera mainCamera;
        [SerializeField] private Transform playerTransform;
        
        [Header("演出設定")]
        [SerializeField] private float panDuration = 2f;
        [SerializeField] private float zoomOutDuration = 1.5f;
        [SerializeField] private float wideViewCameraSize = 12f;
        [SerializeField] private AnimationCurve panCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        private const float CENTER_LERP_RATIO = 0.5f;
        
        private float originalCameraSize;
        private Transform originalCameraParent;
        
        private void Awake()
        {
            if (mainCamera == null) mainCamera = UnityEngine.Camera.main;
            if (playerTransform == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                    playerTransform = player.transform;
            }
            
            if (mainCamera != null)
            {
                originalCameraSize = mainCamera.orthographicSize;
                originalCameraParent = mainCamera.transform.parent;
            }
        }
        
        /// <summary>
        /// ボスへカメラをパン（移動）
        /// </summary>
        public IEnumerator PanToBoss(Transform bossTransform)
        {
            if (mainCamera == null) yield break;
            if (bossTransform == null) yield break;
            
            mainCamera.transform.parent = null;
            
            Vector3 startPos = mainCamera.transform.position;
            Vector3 targetPos = new Vector3(bossTransform.position.x, bossTransform.position.y, startPos.z);
            float elapsed = 0f;
            
            while (elapsed < panDuration)
            {
                elapsed += Time.deltaTime;
                float t = panCurve.Evaluate(elapsed / panDuration);
                mainCamera.transform.position = Vector3.Lerp(startPos, targetPos, t);
                yield return null;
            }
            
            mainCamera.transform.position = targetPos;
        }
        
        /// <summary>
        /// フィールド全体が見えるようにズームアウト
        /// </summary>
        public IEnumerator ZoomOutToFieldView()
        {
            if (mainCamera == null) yield break;
            
            float startSize = mainCamera.orthographicSize;
            float elapsed = 0f;
            
            if (playerTransform != null)
            {
                Vector3 currentPos = mainCamera.transform.position;
                Vector3 centerPos = new Vector3(
                    (playerTransform.position.x + currentPos.x) * CENTER_LERP_RATIO,
                    currentPos.y,
                    currentPos.z
                );
                mainCamera.transform.position = centerPos;
            }
            
            while (elapsed < zoomOutDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / zoomOutDuration;
                mainCamera.orthographicSize = Mathf.Lerp(startSize, wideViewCameraSize, t);
                yield return null;
            }
            
            mainCamera.orthographicSize = wideViewCameraSize;
        }
        
        /// <summary>
        /// 通常のプレイヤー追従カメラに戻す
        /// </summary>
        public void ReturnToNormalCamera()
        {
            if (mainCamera == null) return;
            
            mainCamera.transform.parent = originalCameraParent;
            mainCamera.orthographicSize = originalCameraSize;
        }
    }
}
