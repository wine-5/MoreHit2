using UnityEngine;
using MoreHit.Events;

namespace MoreHit.Camera
{
    public class CameraController : MonoBehaviour
    {
        [Header("プレイヤー追従設定")]
        [SerializeField] private Transform target; // 追従対象（Player）
        [SerializeField] private Vector3 offset = new Vector3(-2f, 2f, -10f); // オフセット（左にズラす、少し上に）
        [SerializeField] private float followSpeed = 5f; // 追従速度
        [SerializeField] private float verticalFollowSpeed = 3f; // Y軸追従速度
        [SerializeField] private float groundOffset = 1.5f; // 地面からの最低オフセット
        
        [Header("カメラ制限")]
        [SerializeField] private bool useBounds = false; // カメラの移動範囲を制限するか
        [SerializeField] private Vector2 minBounds = new Vector2(-10f, -5f);
        [SerializeField] private Vector2 maxBounds = new Vector2(10f, 5f);
        
        [Header("シェイク設定")]
        [SerializeField] private float shakeIntensity = 0.3f; // シェイクの強度
        [SerializeField] private float shakeDuration = 0.2f;  // シェイクの持続時間
        
        private Vector3 originalPosition;
        private CameraShake cameraShake;
        private bool isShaking = false;
        
        void Start()
        {
            // プレイヤーを自動で検索（targetが設定されていない場合）
            if (target == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    target = player.transform;
                }
            }
            
            // CameraShakeインスタンスを初期化
            cameraShake = new CameraShake();
            
            // プレイヤーダメージイベントを購読（引数を無視してシェイクを発動）
            GameEvents.OnPlayerDamage += (currentHP, maxHP) => TriggerShake();
        }
        
        void OnDestroy()
        {
            // イベント購読解除
            GameEvents.OnPlayerDamage -= (currentHP, maxHP) => TriggerShake();
        }
        
        void LateUpdate()
        {
            if (target == null) return;
            
            // X軸の目標位置を計算
            float targetX = target.position.x + offset.x;
            
            // Y軸の目標位置を計算（プレイヤー位置 + groundOffset + offset.y）
            float targetY = target.position.y + groundOffset + offset.y;
            
            // カメラの範囲制限
            if (useBounds)
            {
                targetX = Mathf.Clamp(targetX, minBounds.x, maxBounds.x);
                targetY = Mathf.Clamp(targetY, minBounds.y, maxBounds.y);
            }
            
            // X軸とY軸を滑らかに追従
            Vector3 basePosition = new Vector3(
                Mathf.Lerp(transform.position.x, targetX, followSpeed * Time.deltaTime),
                Mathf.Lerp(transform.position.y, targetY, verticalFollowSpeed * Time.deltaTime),
                transform.position.z
            );
            
            // ショイク処理（基準位置にオフセットを追加）
            if (isShaking)
            {
                Vector3 shakeOffset = cameraShake.UpdateShake(Time.deltaTime);
                transform.position = basePosition + shakeOffset;
                
                // シェイク終了チェック
                if (!cameraShake.IsShaking)
                {
                    isShaking = false;
                }
            }
            else
            {
                transform.position = basePosition;
            }
        }
        
        /// <summary>
        /// シェイクを発動する
        /// </summary>
        public void TriggerShake()
        {
            cameraShake.StartShake(shakeIntensity, shakeDuration);
            isShaking = true;
        }
        
        /// <summary>
        /// シェイクを発動する（カスタムパラメータ）
        /// </summary>
        /// <param name="intensity">強度</param>
        /// <param name="duration">持続時間</param>
        public void TriggerShake(float intensity, float duration)
        {
            cameraShake.StartShake(intensity, duration);
            isShaking = true;
        }
        
        /// <summary>
        /// 追従ターゲットを設定
        /// </summary>
        /// <param name="newTarget">新しいターゲット</param>
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }
        
        /// <summary>
        /// オフセットを設定
        /// </summary>
        /// <param name="newOffset">新しいオフセット</param>
        public void SetOffset(Vector3 newOffset)
        {
            offset = newOffset;
        }
        
        // エディタでのデバッグ用
        void OnDrawGizmosSelected()
        {
            if (useBounds)
            {
                Gizmos.color = Color.yellow;
                Vector3 center = new Vector3((minBounds.x + maxBounds.x) * 0.5f, (minBounds.y + maxBounds.y) * 0.5f, transform.position.z);
                Vector3 size = new Vector3(maxBounds.x - minBounds.x, maxBounds.y - minBounds.y, 0);
                Gizmos.DrawWireCube(center, size);
            }
        }
    }
}
