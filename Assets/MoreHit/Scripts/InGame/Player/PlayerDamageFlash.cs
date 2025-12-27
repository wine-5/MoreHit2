using UnityEngine;
using System.Collections;
using MoreHit.Events;

namespace MoreHit.Player
{
    public class PlayerDamageFlash : MonoBehaviour
    {
        [Header("点滅設定")]
        [SerializeField] private PlayerData playerData;
        [SerializeField] private float flashInterval = 0.1f;
        [SerializeField] private Color flashColor = Color.red;
        
        private SpriteRenderer spriteRenderer;
        private Color originalColor;
        private Coroutine flashCoroutine;
        
        private const int FLASH_CYCLE_PHASES = 2; // 点滅1サイクルのフェーズ数（色変更 + 戻す）
        
        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            originalColor = spriteRenderer.color;
            
            if (playerData == null)
                Debug.LogError($"PlayerDamageFlash: PlayerDataが設定されていません！ {gameObject.name}");
        }
        
        private void OnEnable()
        {
            GameEvents.OnPlayerDamage += OnPlayerDamaged;
        }
        
        private void OnDisable()
        {
            GameEvents.OnPlayerDamage -= OnPlayerDamaged;
        }
        
        private void OnPlayerDamaged(int damage, int currentHealth)
        {
            if (flashCoroutine != null)
                StopCoroutine(flashCoroutine);
                
            flashCoroutine = StartCoroutine(FlashRoutine());
        }
        
        private IEnumerator FlashRoutine()
        {
            if (playerData == null)
            {
                Debug.LogError("PlayerData が null です。点滅をスキップします。");
                yield break;
            }
            
            float flashDuration = playerData.InvincibleTimeAfterDamage;
            float elapsed = 0f;
            
            while (elapsed < flashDuration)
            {
                // 点滅色に変更
                spriteRenderer.color = flashColor;
                yield return new WaitForSeconds(flashInterval);
                
                // 元の色に戻す
                spriteRenderer.color = originalColor;
                yield return new WaitForSeconds(flashInterval);
                
                elapsed += flashInterval * FLASH_CYCLE_PHASES;
            }
            
            // 確実に元の色に戻す
            spriteRenderer.color = originalColor;
            flashCoroutine = null;
        }
    }
}