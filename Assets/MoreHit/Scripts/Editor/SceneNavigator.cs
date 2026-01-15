using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;
using System.Linq;

namespace MoreHit.Editor
{
    /// <summary>
    /// シーン移動を簡単にするためのEditorウィンドウ
    /// </summary>
    public class SceneNavigator : EditorWindow
    {
        private string sceneFolderPath = "Assets/MoreHit/Scenes"; // デフォルトのシーンフォルダ
        private Vector2 scrollPosition;
        private string[] sceneFiles;
        private GUIStyle buttonStyle;

        [MenuItem("Tools/Scene Navigator")]
        public static void ShowWindow()
        {
            GetWindow<SceneNavigator>("Scene Navigator");
        }

        private void OnEnable()
        {
            RefreshSceneList();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            
            // フォルダパス選択
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Scene Folder:", GUILayout.Width(100));
            sceneFolderPath = EditorGUILayout.TextField(sceneFolderPath);
            
            if (GUILayout.Button("Browse", GUILayout.Width(70)))
            {
                string selectedPath = EditorUtility.OpenFolderPanel("Select Scene Folder", "Assets", "");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    // プロジェクトの相対パスに変換
                    if (selectedPath.StartsWith(Application.dataPath))
                    {
                        sceneFolderPath = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                        RefreshSceneList();
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Error", "Please select a folder inside the Assets folder.", "OK");
                    }
                }
            }
            
            if (GUILayout.Button("Refresh", GUILayout.Width(70)))
            {
                RefreshSceneList();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField($"Found {sceneFiles?.Length ?? 0} scene(s)", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            // シーンリスト表示
            if (sceneFiles != null && sceneFiles.Length > 0)
            {
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                
                InitializeButtonStyle();
                
                foreach (string scenePath in sceneFiles)
                {
                    DrawSceneButton(scenePath);
                }
                
                EditorGUILayout.EndScrollView();
            }
            else
            {
                EditorGUILayout.HelpBox("No scenes found in the specified folder.", MessageType.Info);
            }

            EditorGUILayout.Space(10);
            
            // 現在のシーン情報
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Current Scene:", EditorStyles.boldLabel, GUILayout.Width(100));
            EditorGUILayout.LabelField(SceneManager.GetActiveScene().name);
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// ボタンスタイルの初期化
        /// </summary>
        private void InitializeButtonStyle()
        {
            if (buttonStyle == null)
            {
                buttonStyle = new GUIStyle(GUI.skin.button)
                {
                    alignment = TextAnchor.MiddleLeft,
                    padding = new RectOffset(10, 10, 8, 8),
                    fontSize = 12
                };
            }
        }

        /// <summary>
        /// シーンボタンを描画
        /// </summary>
        private void DrawSceneButton(string scenePath)
        {
            string sceneName = Path.GetFileNameWithoutExtension(scenePath);
            bool isCurrentScene = SceneManager.GetActiveScene().path == scenePath;
            
            EditorGUILayout.BeginHorizontal();
            
            // 現在のシーンの場合は色を変える
            Color originalColor = GUI.backgroundColor;
            if (isCurrentScene)
            {
                GUI.backgroundColor = Color.green;
            }
            
            // シーン移動ボタン
            if (GUILayout.Button($"▶ {sceneName}", buttonStyle, GUILayout.Height(30)))
            {
                LoadSceneWithSavePrompt(scenePath);
            }
            
            GUI.backgroundColor = originalColor;
            
            // Build Settingsに追加ボタン
            if (GUILayout.Button("Add to Build", GUILayout.Width(100), GUILayout.Height(30)))
            {
                AddSceneToBuildSettings(scenePath);
            }
            
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(2);
        }

        /// <summary>
        /// 指定フォルダからシーンファイルを検索
        /// </summary>
        private void RefreshSceneList()
        {
            if (Directory.Exists(sceneFolderPath))
            {
                sceneFiles = Directory.GetFiles(sceneFolderPath, "*.unity", SearchOption.AllDirectories)
                    .OrderBy(path => Path.GetFileName(path))
                    .ToArray();
            }
            else
            {
                sceneFiles = new string[0];
                Debug.LogWarning($"Folder not found: {sceneFolderPath}");
            }
        }

        /// <summary>
        /// セーブ確認ダイアログを表示してシーンを読み込み
        /// </summary>
        private void LoadSceneWithSavePrompt(string scenePath)
        {
            // 既に同じシーンの場合は何もしない
            if (SceneManager.GetActiveScene().path == scenePath)
            {

                return;
            }

            // 現在のシーンに未保存の変更があるかチェック
            if (SceneManager.GetActiveScene().isDirty)
            {
                int option = EditorUtility.DisplayDialogComplex(
                    "Scene Has Been Modified",
                    $"Do you want to save the changes to '{SceneManager.GetActiveScene().name}'?",
                    "Save",      // option 0
                    "Don't Save", // option 1
                    "Cancel"     // option 2
                );

                switch (option)
                {
                    case 0: // Save
                        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
                        OpenScene(scenePath);
                        break;
                    case 1: // Don't Save
                        OpenScene(scenePath);
                        break;
                    case 2: // Cancel

                        break;
                }
            }
            else
            {
                // 変更がない場合はそのまま開く
                OpenScene(scenePath);
            }
        }

        /// <summary>
        /// シーンを開く
        /// </summary>
        private void OpenScene(string scenePath)
        {
            try
            {
                EditorSceneManager.OpenScene(scenePath);
                // Debug.Log($"Opened scene: {Path.GetFileNameWithoutExtension(scenePath)}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to open scene: {scenePath} - {e.Message}");
            }
        }

        /// <summary>
        /// Build Settingsにシーンを追加
        /// </summary>
        private void AddSceneToBuildSettings(string scenePath)
        {
            var scenes = EditorBuildSettings.scenes.ToList();
            
            // 既に追加されているかチェック
            if (scenes.Any(s => s.path == scenePath))
            {
                EditorUtility.DisplayDialog("Info", "Scene is already in Build Settings.", "OK");
                return;
            }
            
            scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            

            EditorUtility.DisplayDialog("Success", "Scene added to Build Settings.", "OK");
        }
    }
}