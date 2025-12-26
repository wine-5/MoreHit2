using UnityEngine;
using UnityEditor;

namespace MoreHit.Editor
{
    public class PathCreatorTool : EditorWindow
    {
        public Sprite pathSprite;
        public Material pathMaterial;
        public Transform parentTransform;
        
        // ランダム配置用の複数スプライト
        public Sprite[] randomSprites = new Sprite[3];
        public float segmentLength = 1f;
        public int pathSegments = 5;
        
        [MenuItem("Tools/Path Creator")]
        public static void ShowWindow()
        {
            GetWindow<PathCreatorTool>("Path Creator");
        }
        
        void OnGUI()
        {
            GUILayout.Label("Path Creator Tool", EditorStyles.boldLabel);
            
            EditorGUILayout.LabelField("単一スプライト道路:", EditorStyles.boldLabel);
            pathSprite = (Sprite)EditorGUILayout.ObjectField("Path Sprite", pathSprite, typeof(Sprite), false);
            pathMaterial = (Material)EditorGUILayout.ObjectField("Material (Optional)", pathMaterial, typeof(Material), false);
            parentTransform = (Transform)EditorGUILayout.ObjectField("Parent Transform", parentTransform, typeof(Transform), true);
            
            if (pathSprite != null)
            {
                EditorGUILayout.LabelField($"Sprite: {pathSprite.name}");
                
                EditorGUILayout.HelpBox(
                    "使い方:\n" +
                    "1. 道のスプライトを選択\n" +
                    "2. 'Create Path Segment' で均一な濃度の道を作成\n" +
                    "3. SceneビューでSizeを調整（Tiledモードで濃度が保持される）",
                    MessageType.Info);
                
                if (GUILayout.Button("Create Path Segment"))
                {
                    CreatePathSegment();
                }
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("ランダム配置道路:", EditorStyles.boldLabel);
            
            // 3つのスプライト設定
            for (int i = 0; i < randomSprites.Length; i++)
            {
                randomSprites[i] = (Sprite)EditorGUILayout.ObjectField($"Random Sprite {i + 1}", randomSprites[i], typeof(Sprite), false);
            }
            
            segmentLength = EditorGUILayout.FloatField("Segment Length", segmentLength);
            pathSegments = EditorGUILayout.IntField("Number of Segments", pathSegments);
            
            bool hasRandomSprites = System.Array.Exists(randomSprites, sprite => sprite != null);
            
            if (hasRandomSprites)
            {
                EditorGUILayout.HelpBox(
                    "ランダム道路:\n" +
                    "• 複数のスプライトからランダムに選択\n" +
                    "• セグメント数で道の長さを調整\n" +
                    "• 各セグメントが異なるスプライトになる可能性",
                    MessageType.Info);
                
                if (GUILayout.Button("Create Random Path"))
                {
                    CreateRandomPath();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("ランダム道路を作成するには、最低1つのスプライトを設定してください", MessageType.Warning);
            }
        }
        
        void Setup9SliceSprite()
        {
            string path = AssetDatabase.GetAssetPath(pathSprite);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            
            if (importer != null)
            {
                // テクスチャ設定を確認
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                
                // 9-Slice境界を設定（ピクセル単位で指定）
                // より適切な境界値を設定
                float width = pathSprite.texture.width;
                float height = pathSprite.texture.height;
                
                Vector4 border = new Vector4(
                    Mathf.Max(width * 0.1f, 4f),   // left - 最低4ピクセル
                    Mathf.Max(height * 0.1f, 4f),  // bottom - 最低4ピクセル
                    Mathf.Max(width * 0.1f, 4f),   // right - 最低4ピクセル
                    Mathf.Max(height * 0.1f, 4f)   // top - 最低4ピクセル
                );
                
                importer.spriteBorder = border;
                
                // 再インポート
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                
                Debug.Log($"9-Slice設定完了: {pathSprite.name} - Border: {border}");
                EditorUtility.DisplayDialog("9-Slice設定", $"スプライト '{pathSprite.name}' を9-Slice用に設定しました。\n境界: {border}", "OK");
            }
        }
        
        void CreatePathSegment()
        {
            GameObject pathObj = new GameObject("Ground");
            
            if (parentTransform != null)
            {
                pathObj.transform.SetParent(parentTransform);
            }
            
            SpriteRenderer sr = pathObj.AddComponent<SpriteRenderer>();
            sr.sprite = pathSprite;
            
            // 均一な見た目にするためTiledモードを使用
            sr.drawMode = SpriteDrawMode.Tiled; // タイルモード（濃度が均一）
            sr.size = new Vector2(5f, 1f); // デフォルトサイズ
            
            // タイル化の設定
            sr.tileMode = SpriteTileMode.Continuous;
            
            Debug.Log("Tiledモードで作成（濃度均一）");
            
            if (pathMaterial != null)
            {
                sr.material = pathMaterial;
            }
            
            // Colliderも追加（サイズを同期）
            BoxCollider2D collider = pathObj.AddComponent<BoxCollider2D>();
            collider.size = sr.size;
            
            // カスタムコンポーネントを追加して動的サイズ調整を可能にする
            TiledPath tiledPath = pathObj.AddComponent<TiledPath>();
            
            // 選択状態にする
            Selection.activeGameObject = pathObj;
            
            // Scene ViewにフォーカスしてTransformが編集できるようにする
            SceneView.FrameLastActiveSceneView();
            
            Debug.Log("道のセグメントを作成しました。Inspector の Size または TiledPath コンポーネントで長さを調整してください。");
        }
        
        void CreateRandomPath()
        {
            // 利用可能なスプライトをフィルタリング
            var availableSprites = System.Array.FindAll(randomSprites, sprite => sprite != null);
            
            if (availableSprites.Length == 0)
            {
                Debug.LogWarning("利用可能なスプライトがありません");
                return;
            }
            
            // 親オブジェクトを作成
            GameObject pathContainer = new GameObject("Ground");
            if (parentTransform != null)
            {
                pathContainer.transform.SetParent(parentTransform);
            }
            
            // 各セグメントを作成
            for (int i = 0; i < pathSegments; i++)
            {
                // ランダムにスプライトを選択
                Sprite selectedSprite = availableSprites[Random.Range(0, availableSprites.Length)];
                
                GameObject segment = new GameObject($"Ground {i + 1}");
                segment.transform.SetParent(pathContainer.transform);
                segment.transform.localPosition = new Vector3(i * segmentLength, 0, 0);
                
                SpriteRenderer sr = segment.AddComponent<SpriteRenderer>();
                sr.sprite = selectedSprite;
                sr.drawMode = SpriteDrawMode.Tiled;
                sr.size = new Vector2(segmentLength, 1f);
                sr.tileMode = SpriteTileMode.Continuous;
                
                if (pathMaterial != null)
                {
                    sr.material = pathMaterial;
                }
                
                // Collider追加
                BoxCollider2D collider = segment.AddComponent<BoxCollider2D>();
                collider.size = sr.size;
            }
            
            // 全体のコライダーを追加（任意）
            BoxCollider2D mainCollider = pathContainer.AddComponent<BoxCollider2D>();
            mainCollider.size = new Vector2(pathSegments * segmentLength, 1f);
            mainCollider.offset = new Vector2((pathSegments - 1) * segmentLength * 0.5f, 0);
            
            // 選択状態にする
            Selection.activeGameObject = pathContainer;
            SceneView.FrameLastActiveSceneView();
            
            Debug.Log($"ランダム道路を作成しました。{pathSegments}セグメント、長さ: {pathSegments * segmentLength}");
        }
    }
}