using UnityEngine;
using UnityEngine.Tilemaps;

namespace MoreHit
{
    /// <summary>
    /// 基本的な道タイル
    /// TilemapPathToolで使用します
    /// </summary>
    [CreateAssetMenu(fileName = "PathTile", menuName = "Tiles/PathTile")]
    public class PathTile : TileBase
    {
        public Sprite sprite;
        
        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
        {
            tileData.transform = Matrix4x4.identity;
            tileData.flags = TileFlags.LockTransform;
            tileData.colliderType = Tile.ColliderType.None;
            tileData.sprite = sprite;
        }
        
        public override bool GetTileAnimationData(Vector3Int position, ITilemap tilemap, ref TileAnimationData tileAnimationData)
        {
            return false; // アニメーションなし
        }
        
        public override void RefreshTile(Vector3Int position, ITilemap tilemap)
        {
            // 特別なリフレッシュ処理は不要
        }
    }
}