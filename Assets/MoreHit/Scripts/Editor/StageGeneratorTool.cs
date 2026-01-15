using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;

namespace MoreHit.Editor
{
    public class StageGeneratorTool : EditorWindow
    {
        public Texture2D stageTexture;
        public GameObject[] blockPrefabs = new GameObject[5];
        public Transform targetParent;
        public float blockSize = 1f;
        public Vector2Int generationRange = new Vector2Int(50, 50);
        public Vector2 cameraCenter;
        
        [MenuItem("Tools/Stage/Stage Generator")]
        public static void ShowWindow()
        {
            GetWindow<StageGeneratorTool>("Stage Generator");
        }
        
        void OnGUI()
        {
            GUILayout.Label("Stage Generator Tool", EditorStyles.boldLabel);
            
            stageTexture = (Texture2D)EditorGUILayout.ObjectField("Stage Texture", stageTexture, typeof(Texture2D), false);
            targetParent = (Transform)EditorGUILayout.ObjectField("Parent Object", targetParent, typeof(Transform), true);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Block Prefabs:", EditorStyles.boldLabel);
            
            for (int i = 0; i < blockPrefabs.Length; i++)
            {
                blockPrefabs[i] = (GameObject)EditorGUILayout.ObjectField($"Prefab {i + 1}", blockPrefabs[i], typeof(GameObject), false);
            }
            
            blockSize = EditorGUILayout.FloatField("Block Size", blockSize);
            generationRange = EditorGUILayout.Vector2IntField("Generation Range", generationRange);
            
            EditorGUILayout.Space();
            
            if (stageTexture != null && System.Array.Exists(blockPrefabs, prefab => prefab != null))
            {
                EditorGUILayout.HelpBox(
                    "使い方:\n" +
                    "1. ステージテクスチャを設定\n" +
                    "2. ブロックPrefabを設定\n" +
                    "3. 'Generate Stage' でステージを生成",
                    MessageType.Info);
                
                if (GUILayout.Button("Generate Stage from Texture"))
                {
                    GenerateStageFromTexture();
                }
                
                if (GUILayout.Button("Set Generation Area from Camera"))
                {
                    SetGenerationAreaFromCamera();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("ステージテクスチャと最低1つのPrefabを設定してください", MessageType.Warning);
            }
        }
        
        void GenerateStageFromTexture()
        {
            if (stageTexture == null) return;
            
            // 親オブジェクトを作成
            GameObject stageParent = new GameObject("Generated Stage");
            if (targetParent != null)
            {
                stageParent.transform.SetParent(targetParent);
            }
            
            // テクスチャの各ピクセルをチェック
            int width = Mathf.Min(stageTexture.width, generationRange.x);
            int height = Mathf.Min(stageTexture.height, generationRange.y);
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color pixelColor = stageTexture.GetPixel(x, y);
                    
                    // 黒いピクセルにブロックを配置
                    if (pixelColor.r < 0.5f && pixelColor.g < 0.5f && pixelColor.b < 0.5f)
                    {
                        // ランダムにPrefabを選択
                        var availablePrefabs = System.Array.FindAll(blockPrefabs, prefab => prefab != null);
                        if (availablePrefabs.Length > 0)
                        {
                            GameObject selectedPrefab = availablePrefabs[Random.Range(0, availablePrefabs.Length)];
                            Vector3 position = new Vector3(x * blockSize, -y * blockSize, 0);
                            
                            GameObject instance = PrefabUtility.InstantiatePrefab(selectedPrefab) as GameObject;
                            instance.transform.SetParent(stageParent.transform);
                            instance.transform.localPosition = position;
                        }
                    }
                }
            }
            

            Selection.activeGameObject = stageParent;
        }
        
        void SetGenerationAreaFromCamera()
        {
            UnityEngine.Camera mainCamera = UnityEngine.Camera.main;
            if (mainCamera == null) mainCamera = FindFirstObjectByType<UnityEngine.Camera>();
            
            if (mainCamera != null)
            {
                Vector3 bottomLeft = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, mainCamera.transform.position.z));
                Vector3 topRight = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, mainCamera.transform.position.z));
                
                generationRange = new Vector2Int(
                    Mathf.RoundToInt((topRight.x - bottomLeft.x) / blockSize),
                    Mathf.RoundToInt((topRight.y - bottomLeft.y) / blockSize)
                );
                
                cameraCenter = new Vector2(mainCamera.transform.position.x, mainCamera.transform.position.y);
                

            }
        }
    }
}