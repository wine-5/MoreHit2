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
        
        private const int FLASH_CYCLE_PHASES = 2;
        
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
            
            float flashDuration = playerData.InvincibleTime;
            float elapsed = 0f;
            
            while (elapsed < flashDuration)
            {
                spriteRenderer.color = flashColor;
                yield return new WaitForSeconds(flashInterval);
                
                spriteRenderer.color = originalColor;
                yield return new WaitForSeconds(flashInterval);
                
                elapsed += flashInterval * FLASH_CYCLE_PHASES;
            }
            
            spriteRenderer.color = originalColor;
            flashCoroutine = null;
        }
    }
}