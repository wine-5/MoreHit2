using UnityEngine;

namespace MoreHit
{
    /// <summary>
    /// タイル状に繰り返されるパス用コンポーネント
    /// </summary>
    public class TiledPath : MonoBehaviour
    {
        [Header("タイル設定")]
        public bool adaptiveTiling = true; // サイズに合わせてタイル数を自動調整
        public Vector2 tileSize = Vector2.one; // 1タイルのサイズ
        
        private SpriteRenderer spriteRenderer;
        private Vector2 lastSize;
        
        void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.drawMode = SpriteDrawMode.Tiled;
                UpdateTiling();
            }
        }
        
        void Update()
        {
            // サイズが変更されたら自動でタイリングを更新
            if (spriteRenderer != null && spriteRenderer.size != lastSize)
            {
                UpdateTiling();
            }
        }
        
        void UpdateTiling()
        {
            if (spriteRenderer == null) return;
            
            if (adaptiveTiling && spriteRenderer.sprite != null)
            {
                // スプライトのサイズに基づいてタイリングを調整
                Vector2 spriteSize = spriteRenderer.sprite.bounds.size;
                spriteRenderer.tileMode = SpriteTileMode.Continuous;
            }
            
            lastSize = spriteRenderer.size;
            
            // Colliderサイズも同期
            var collider = GetComponent<BoxCollider2D>();
            if (collider != null)
            {
                collider.size = spriteRenderer.size;
            }
        }
        
        /// <summary>
        /// パスの長さを設定
        /// </summary>
        public void SetLength(float length)
        {
            if (spriteRenderer != null)
            {
                Vector2 newSize = spriteRenderer.size;
                newSize.x = length;
                spriteRenderer.size = newSize;
                UpdateTiling();
            }
        }
        
        /// <summary>
        /// パスの幅を設定
        /// </summary>
        public void SetWidth(float width)
        {
            if (spriteRenderer != null)
            {
                Vector2 newSize = spriteRenderer.size;
                newSize.y = width;
                spriteRenderer.size = newSize;
                UpdateTiling();
            }
        }
    }
}