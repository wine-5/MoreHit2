using UnityEngine;
using UnityEditor;
namespace MoreHit.Editor
{

    public class StageGeneratorEditor : EditorWindow
    {
        public Texture2D stageTexture;
        public GameObject[] blockPrefabs = new GameObject[3]; // 3種類のプレハブ
        public float blockScale = 0.1f; // ブロックのスケール調整（デフォルト0.1）
        public int blockSpacing = 1; // ブロック配置間隔（ピクセル単位）※隙間なし
        public Transform parentTransform; // 生成先の親オブジェクト
        public UnityEngine.Camera targetCamera; // 参照カメラ
        
        // 生成範囲設定
        public int startX = 0;
        public int startY = 0; 
        public int generateWidth = 50; // より小さなテスト範囲
        public int generateHeight = 50; // より小さなテスト範囲 
        
        private int estimatedBlocks = 0;

        [MenuItem("Tools/Stage Generator")]
        public static void ShowWindow()
        {
            GetWindow<StageGeneratorEditor>("Stage Generator");
        }

        void OnGUI()
        {
            stageTexture = (Texture2D)EditorGUILayout.ObjectField("Stage Image", stageTexture, typeof(Texture2D), false);
            targetCamera = (UnityEngine.Camera)EditorGUILayout.ObjectField("Target Camera", targetCamera, typeof(UnityEngine.Camera), true);
            
            // 画像情報とブロック数予測を表示
            if (stageTexture != null)
            {
                EditorGUILayout.LabelField($"Image Size: {stageTexture.width} x {stageTexture.height}");
                
                // カメラ範囲取得ボタン
                if (targetCamera != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Get Camera Bounds", GUILayout.Width(150)))
                    {
                        SetGenerationAreaFromCamera();
                    }
                    EditorGUILayout.LabelField($"Camera Size: {targetCamera.orthographicSize * 2:F1} x {targetCamera.orthographicSize * 2 * targetCamera.aspect:F1}");
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    EditorGUILayout.HelpBox("Assign a Camera to auto-calculate generation area", MessageType.Info);
                }
                
                // 生成範囲設定
                GUILayout.Label("Generation Area:", EditorStyles.boldLabel);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Start Position:", GUILayout.Width(100));
                startX = EditorGUILayout.IntField("X", startX, GUILayout.Width(80));
                startY = EditorGUILayout.IntField("Y", startY, GUILayout.Width(80));
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Size:", GUILayout.Width(100));
                generateWidth = EditorGUILayout.IntField("W", generateWidth, GUILayout.Width(80));
                generateHeight = EditorGUILayout.IntField("H", generateHeight, GUILayout.Width(80));
                EditorGUILayout.EndHorizontal();
                
                // クイックセットボタン
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Reset Center", GUILayout.Width(100)))
                {
                    startX = 0; startY = 0;
                }
                if (GUILayout.Button("Expand Right", GUILayout.Width(100)))
                {
                    startX += generateWidth; 
                }
                if (GUILayout.Button("Expand Down", GUILayout.Width(100)))
                {
                    startY += generateHeight;
                }
                EditorGUILayout.EndHorizontal();
                
                EstimateBlocks();
                
                if (estimatedBlocks > 10000)
                {
                    EditorGUILayout.HelpBox($"Warning: Estimated {estimatedBlocks} blocks! Consider reducing generation area.", MessageType.Warning);
                }
                else
                {
                    EditorGUILayout.LabelField($"Estimated blocks: {estimatedBlocks}");
                }
                
                // 生成範囲の視覚的表示
                EditorGUILayout.LabelField($"Generating: ({startX},{startY}) to ({startX + generateWidth},{startY + generateHeight})");
            }
            
            GUILayout.Label("Block Prefabs (3 types):", EditorStyles.boldLabel);
            for (int i = 0; i < blockPrefabs.Length; i++)
            {
                blockPrefabs[i] = (GameObject)EditorGUILayout.ObjectField($"Block Type {i + 1}", blockPrefabs[i], typeof(GameObject), false);
            }
            
            blockScale = EditorGUILayout.FloatField("Block Scale", blockScale);
            blockSpacing = EditorGUILayout.IntSlider("Block Spacing (pixels)", blockSpacing, 1, 10);
            parentTransform = (Transform)EditorGUILayout.ObjectField("Parent Transform (Stage/Ground)", parentTransform, typeof(Transform), true);

            // 生成ボタン（警告がある場合は色を変更）
            if (estimatedBlocks > 5000)
            {
                GUI.backgroundColor = Color.yellow;
            }
            
            if (GUILayout.Button("Generate Area"))
            {
                if (estimatedBlocks > 5000)
                {
                    if (EditorUtility.DisplayDialog("Warning", 
                        $"You are about to generate {estimatedBlocks} blocks. This may cause performance issues. Continue?", 
                        "Generate", "Cancel"))
                    {
                        GenerateStage();
                    }
                }
                else
                {
                    GenerateStage();
                }
            }
            
            GUI.backgroundColor = Color.white;
            
            if (GUILayout.Button("Clear Stage"))
            {
                ClearStage();
            }
        }

        void GenerateStage()
        {
            if (stageTexture == null)
            {
                Debug.LogError("Stage texture is not assigned!");
                return;
            }
            
            // テクスチャが読み込み可能かチェックし、必要に応じて設定を変更
            if (!IsTextureReadable(stageTexture))
            {
                if (!MakeTextureReadable(stageTexture))
                {
                    Debug.LogError("Failed to make texture readable. Please enable 'Read/Write Enabled' in Texture Import Settings manually.");
                    return;
                }
            }
            
            // 有効なプレハブがあるかチェック
            var validPrefabs = System.Array.FindAll(blockPrefabs, prefab => prefab != null);
            if (validPrefabs.Length == 0)
            {
                Debug.LogError("No block prefabs assigned!");
                return;
            }
            
            // 生成先の親オブジェクトを決定
            Transform targetParent;
            if (parentTransform != null)
            {
                targetParent = parentTransform;

            }
            else
            {
                // 親が指定されていない場合は新しく作成
                GameObject stageParent = new GameObject("Generated Stage");
                targetParent = stageParent.transform;

            }
            
            int generatedCount = 0;
            
            // 指定された範囲のみ処理
            int endX = Mathf.Min(startX + generateWidth, stageTexture.width);
            int endY = Mathf.Min(startY + generateHeight, stageTexture.height);
            
            for (int x = startX; x < endX; x += blockSpacing)
            {
                for (int y = startY; y < endY; y += blockSpacing)
                {
                    Color pixelColor = stageTexture.GetPixel(x, y);
                    
                    // 黒い部分（grayscale < 0.5f）にブロックを配置
                    if (pixelColor.grayscale < 0.5f)
                    {
                        // ランダムに有効なプレハブを選択
                        GameObject selectedPrefab = validPrefabs[Random.Range(0, validPrefabs.Length)];
                        
                        // Y座標を反転（画像の上下とUnityの座標系を合わせる）
                        Vector3 pos = new Vector3(x * blockScale, (stageTexture.height - 1 - y) * blockScale, 0);
                        
                        GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(selectedPrefab);
                        obj.transform.position = pos;
                        obj.transform.localScale = Vector3.one * blockScale;
                        obj.transform.SetParent(targetParent);
                        generatedCount++;
                    }
                }
            }
            

        }
        
        void ClearStage()
        {
            if (parentTransform != null)
            {
                // 指定されたparentTransformの子オブジェクトをすべて削除
                while (parentTransform.childCount > 0)
                {
                    DestroyImmediate(parentTransform.GetChild(0).gameObject);
                }

            }
            else
            {
                // 従来の方法でGenerated Stageを削除
                GameObject stageParent = GameObject.Find("Generated Stage");
                if (stageParent != null)
                {
                    DestroyImmediate(stageParent);

                }
                else
                {
                    Debug.LogWarning("No Generated Stage found or Parent Transform not assigned!");
                }
            }
        }
        
        bool IsTextureReadable(Texture2D texture)
        {
            try
            {
                texture.GetPixel(0, 0);
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        bool MakeTextureReadable(Texture2D texture)
        {
            string path = AssetDatabase.GetAssetPath(texture);
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }
            
            TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
            if (textureImporter == null)
            {
                return false;
            }
            
            if (!textureImporter.isReadable)
            {
                textureImporter.isReadable = true;
                AssetDatabase.ImportAsset(path);

            }
            
            return true;
        }
        
        void EstimateBlocks()
        {
            if (stageTexture == null || !IsTextureReadable(stageTexture))
            {
                estimatedBlocks = 0;
                return;
            }
            
            int blackPixels = 0;
            
            // 指定された範囲のみカウント
            int endX = Mathf.Min(startX + generateWidth, stageTexture.width);
            int endY = Mathf.Min(startY + generateHeight, stageTexture.height);
            
            for (int x = startX; x < endX; x += blockSpacing)
            {
                for (int y = startY; y < endY; y += blockSpacing)
                {
                    Color pixelColor = stageTexture.GetPixel(x, y);
                    if (pixelColor.grayscale < 0.5f)
                    {
                        blackPixels++;
                    }
                }
            }
            
            estimatedBlocks = blackPixels;
        }
        
        void SetGenerationAreaFromCamera()
        {
            if (targetCamera == null || stageTexture == null) return;
            
            // カメラのワールド座標での範囲を計算
            float cameraHeight = targetCamera.orthographicSize * 2f;
            float cameraWidth = cameraHeight * targetCamera.aspect;
            
            // カメラの中心位置を取得
            Vector3 cameraCenter = targetCamera.transform.position;
            
            // ワールド座標をピクセル座標に変換
            // blockScaleを考慮してピクセル座標を計算
            float worldToPixel = 1f / blockScale;
            
            // 画像の中心を基準にした座標系でカメラ範囲を計算
            int centerX = stageTexture.width / 2;
            int centerY = stageTexture.height / 2;
            
            // カメラ中心からの相対位置をピクセル座標に変換
            int cameraCenterPixelX = centerX + Mathf.RoundToInt(cameraCenter.x * worldToPixel);
            int cameraCenterPixelY = centerY + Mathf.RoundToInt(cameraCenter.y * worldToPixel);
            
            // カメラ範囲をピクセル座標に変換
            int pixelWidth = Mathf.RoundToInt(cameraWidth * worldToPixel);
            int pixelHeight = Mathf.RoundToInt(cameraHeight * worldToPixel);
            
            // 生成範囲を設定（カメラ範囲の左下角から開始）
            startX = Mathf.Max(0, cameraCenterPixelX - pixelWidth / 2);
            startY = Mathf.Max(0, cameraCenterPixelY - pixelHeight / 2);
            generateWidth = Mathf.Min(pixelWidth, stageTexture.width - startX);
            generateHeight = Mathf.Min(pixelHeight, stageTexture.height - startY);
            

        }
    }
}