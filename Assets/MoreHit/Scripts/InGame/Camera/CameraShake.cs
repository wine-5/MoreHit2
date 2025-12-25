using UnityEngine;

namespace MoreHit.Camera
{
    /// <summary>
    /// カメラシェイク計算クラス（MonoBehaviourを継承しない）
    /// 純粋な計算ロジックを提供
    /// </summary>
    [System.Serializable]
    public class CameraShake
    {
        private bool isShaking;
        private float shakeTimer;
        private float shakeDuration;
        private float shakeIntensity;
        private Vector3 originalPosition;
        
        public bool IsShaking => isShaking;
        
        /// <summary>
        /// シェイクを開始する
        /// </summary>
        /// <param name="intensity">シェイクの強度</param>
        /// <param name="duration">シェイクの持続時間</param>
        public void StartShake(float intensity, float duration)
        {
            shakeIntensity = intensity;
            shakeDuration = duration;
            shakeTimer = duration;
            isShaking = true;
        }
        
        /// <summary>
        /// シェイクの更新処理
        /// </summary>
        /// <param name="deltaTime">フレーム時間</param>
        /// <returns>シェイクによるオフセット</returns>
        public Vector3 UpdateShake(float deltaTime)
        {
            if (!isShaking)
                return Vector3.zero;
                
            shakeTimer -= deltaTime;
            
            if (shakeTimer <= 0f)
            {
                isShaking = false;
                return Vector3.zero;
            }
            
            // シェイクの強度を時間とともに減衰
            float currentIntensity = shakeIntensity * (shakeTimer / shakeDuration);
            
            // ランダムなシェイクオフセットを生成
            Vector3 shakeOffset = new Vector3(
                Random.Range(-1f, 1f) * currentIntensity,
                Random.Range(-1f, 1f) * currentIntensity,
                0f
            );
            
            return shakeOffset;
        }
        
        /// <summary>
        /// シェイクを即座に停止する
        /// </summary>
        public void StopShake()
        {
            isShaking = false;
            shakeTimer = 0f;
        }
        
        /// <summary>
        /// シェイクの進行度を取得（0.0～1.0）
        /// </summary>
        /// <returns>シェイクの進行度</returns>
        public float GetShakeProgress()
        {
            if (!isShaking || shakeDuration <= 0f)
                return 0f;
                
            return 1f - (shakeTimer / shakeDuration);
        }
        
        /// <summary>
        /// 現在のシェイク強度を取得
        /// </summary>
        /// <returns>現在のシェイク強度</returns>
        public float GetCurrentIntensity()
        {
            if (!isShaking)
                return 0f;
                
            return shakeIntensity * (shakeTimer / shakeDuration);
        }
    }
}