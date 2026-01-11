using UnityEngine;
using MoreHit.Events;

namespace MoreHit.Player
{
    /// <summary>
    /// プレイヤーのダメージ時のビジュアル効果を管理するクラス
    /// 通常時とダメージ時のスプライトGameObjectのSetActiveを切り替える
    /// </summary>
    public class PlayerDamageVisualManager : MonoBehaviour
    {
        [Header("スプライト設定")]
        [SerializeField] private GameObject normalSprite;
        [SerializeField] private GameObject damagedSprite;
        
        private void Awake()
        {
            if (normalSprite != null) normalSprite.SetActive(true);
            if (damagedSprite != null) damagedSprite.SetActive(false);
        }
        
        private void OnEnable()
        {
            GameEvents.OnPlayerDamage += OnPlayerDamaged;
            GameEvents.OnPlayerInvincibilityEnded += OnInvincibilityEnded;
        }
        
        private void OnDisable()
        {
            GameEvents.OnPlayerDamage -= OnPlayerDamaged;
            GameEvents.OnPlayerInvincibilityEnded -= OnInvincibilityEnded;
        }
        
        /// <summary>
        /// プレイヤーがダメージを受けた時の処理
        /// </summary>
        private void OnPlayerDamaged(int damage, int currentHealth)
        {
            if (normalSprite != null) normalSprite.SetActive(false);
            if (damagedSprite != null) damagedSprite.SetActive(true);
        }
        
        /// <summary>
        /// 無敵時間が終了した時の処理
        /// </summary>
        private void OnInvincibilityEnded()
        {
            if (normalSprite != null) normalSprite.SetActive(true);
            if (damagedSprite != null) damagedSprite.SetActive(false);
        }
    }
}