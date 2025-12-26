using UnityEngine;

namespace MoreHit.UI
{
    /// <summary>
    /// ゲーム開始時に背景を縦3×横20のグリッド状に複製配置するクラス
    /// </summary>
    public class BackgroundRepeater : MonoBehaviour
    {
        [Header("背景設定")]
        [SerializeField] private GameObject backgroundPrefab; // 複製する背景プレハブ
        [SerializeField] private Transform parentTransform; // 親オブジェクト（nullの場合は自動作成）
        
        [Header("配置設定")]
        [SerializeField] private int verticalCount = 3; // 縦の数
        [SerializeField] private int horizontalCount = 20; // 横の数
        [SerializeField] private Vector2 spacing = Vector2.zero; // 追加の隙間（通常は0,0）
        
        private Vector2 bgSize; // 背景のサイズ
        
        void Start()
        {
            if (backgroundPrefab == null)
            {
                Debug.LogError("BackgroundPrefabが設定されていません");
                return;
            }
            
            // 背景サイズを取得
            GetBackgroundSize();
            
            // 背景を配置
            GenerateBackgroundGrid();
        }
        
        /// <summary>
        /// 背景プレハブのサイズを取得
        /// </summary>
        void GetBackgroundSize()
        {
            // SpriteRendererからサイズを取得
            SpriteRenderer spriteRenderer = backgroundPrefab.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                // 実際の表示サイズを取得（Pixels Per Unitを考慮）
                Sprite sprite = spriteRenderer.sprite;
                bgSize = new Vector2(
                    sprite.rect.width / sprite.pixelsPerUnit,
                    sprite.rect.height / sprite.pixelsPerUnit
                );
                
                // Transform.scaleも考慮
                Vector3 scale = backgroundPrefab.transform.localScale;
                bgSize.x *= scale.x;
                bgSize.y *= scale.y;
                
                Debug.Log($"背景サイズ: {bgSize} (sprite: {sprite.rect.size}, PPU: {sprite.pixelsPerUnit}, scale: {scale})");
                return;
            }
            
            // RectTransformからサイズを取得（UI要素の場合）
            RectTransform rectTransform = backgroundPrefab.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                bgSize = rectTransform.sizeDelta;
                return;
            }
            
            // デフォルトサイズ
            bgSize = Vector2.one;
            Debug.LogWarning("背景のサイズを自動取得できませんでした。デフォルトサイズ(1,1)を使用します。");
        }
        
        /// <summary>
        /// 縦3×横20のグリッド状に背景を配置
        /// </summary>
        void GenerateBackgroundGrid()
        {
            // 親オブジェクトを作成または取得
            Transform parent = GetOrCreateParent();
            
            // グリッド配置の開始位置を計算（中央揃え）
            Vector3 startPosition = CalculateStartPosition();
            
            // 縦3×横20で配置
            for (int row = 0; row < verticalCount; row++)
            {
                for (int col = 0; col < horizontalCount; col++)
                {
                    // 配置位置を計算
                    Vector3 position = startPosition + new Vector3(
                        col * (bgSize.x + spacing.x),
                        -row * (bgSize.y + spacing.y), // Y軸は下向きに配置
                        0
                    );
                    
                    // 背景プレハブを複製
                    GameObject bgInstance = Instantiate(backgroundPrefab, position, Quaternion.identity, parent);
                    bgInstance.name = $"Background_{row}_{col}";
                }
            }
            
            Debug.Log($"背景を {verticalCount}×{horizontalCount} = {verticalCount * horizontalCount} 個生成しました");
        }
        
        /// <summary>
        /// 親オブジェクトを取得または作成
        /// </summary>
        Transform GetOrCreateParent()
        {
            if (parentTransform != null)
                return parentTransform;
            
            // 親オブジェクトを自動作成
            GameObject parentObj = new GameObject("Background Grid");
            parentObj.transform.SetParent(transform);
            return parentObj.transform;
        }
        
        /// <summary>
        /// グリッドの開始位置を計算（アタッチされたオブジェクトの位置から右に展開）
        /// </summary>
        Vector3 CalculateStartPosition()
        {
            // アタッチされたオブジェクトの位置を開始点とする
            Vector3 startPos = transform.position;
            
            // 最初の背景の中心を開始位置に合わせるため、半分ずらす
            startPos.x += bgSize.x * 0.5f;
            startPos.y -= bgSize.y * 0.5f;
            
            return startPos;
        }
        
        /// <summary>
        /// 生成された背景をすべて削除
        /// </summary>
        [ContextMenu("Clear All Backgrounds")]
        public void ClearAllBackgrounds()
        {
            Transform parent = GetOrCreateParent();
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                if (Application.isPlaying)
                {
                    Destroy(parent.GetChild(i).gameObject);
                }
                else
                {
                    DestroyImmediate(parent.GetChild(i).gameObject);
                }
            }
            
            Debug.Log("すべての背景を削除しました");
        }
        
        /// <summary>
        /// エディタでの確認用：配置予定範囲を描画
        /// </summary>
        void OnDrawGizmosSelected()
        {
            if (backgroundPrefab == null) return;
            
            // 仮のサイズで描画
            Vector2 tempSize = bgSize == Vector2.zero ? Vector2.one : bgSize;
            
            Vector3 startPos = CalculateStartPosition();
            
            Gizmos.color = Color.green;
            for (int row = 0; row < verticalCount; row++)
            {
                for (int col = 0; col < horizontalCount; col++)
                {
                    Vector3 position = startPos + new Vector3(
                        col * (tempSize.x + spacing.x),
                        -row * (tempSize.y + spacing.y),
                        0
                    );
                    
                    Gizmos.DrawWireCube(position, tempSize);
                }
            }
        }
    }
}