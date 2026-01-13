using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

namespace MoreHit.Editor
{
    /// <summary>
    /// 不正なコンポーネントやMissingコンポーネントを検索するエディタツール
    /// </summary>
    public class FindMissingComponentsTool : EditorWindow
    {
        private class ObjectInfo
        {
            public string objectName;
            public string sceneName;
            public string scenePath;
            public string hierarchyPath;
        }
        
        private Vector2 scrollPosition;
        private Dictionary<GameObject, ObjectInfo> objectsWithIssues = new Dictionary<GameObject, ObjectInfo>();
        private bool scanComplete = false;

        [MenuItem("Tools/MoreHit/Find Missing Components")]
        public static void ShowWindow()
        {
            GetWindow<FindMissingComponentsTool>("Find Missing Components");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Missing/Invalid Components Finder", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            if (GUILayout.Button("Scan Current Scene", GUILayout.Height(30)))
                ScanCurrentScene();

            if (GUILayout.Button("Scan All Scenes", GUILayout.Height(30)))
                ScanAllScenes();

            if (GUILayout.Button("Scan All Prefabs", GUILayout.Height(30)))
                ScanAllPrefabs();

            EditorGUILayout.Space();

            if (scanComplete)
            {
                if (objectsWithIssues.Count == 0)
                {
                    EditorGUILayout.HelpBox("No issues found!", MessageType.Info);
                }
                else
                {
                    EditorGUILayout.HelpBox($"Found {objectsWithIssues.Count} objects with issues:", MessageType.Warning);
                    
                    scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                    
                    foreach (var kvp in objectsWithIssues)
                    {
                        var obj = kvp.Key;
                        var info = kvp.Value;
                        if (obj == null) continue;

                        EditorGUILayout.BeginHorizontal();
                        
                        // オブジェクト名表示とクリックで選択
                        if (GUILayout.Button($"{info.objectName} ({info.sceneName})", EditorStyles.linkLabel))
                        {
                            // Prefabの場合
                            if (info.scenePath.EndsWith(".prefab"))
                            {
                                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(info.scenePath);
                                Selection.activeObject = prefab;
                                EditorGUIUtility.PingObject(prefab);
                            }
                            // シーンの場合
                            else if (!string.IsNullOrEmpty(info.scenePath))
                            {
                                EditorSceneManager.OpenScene(info.scenePath, OpenSceneMode.Single);
                                Selection.activeGameObject = obj;
                                EditorGUIUtility.PingObject(obj);
                            }
                        }
                        
                        // 階層パス表示
                        EditorGUILayout.LabelField(info.hierarchyPath, EditorStyles.miniLabel);
                        
                        EditorGUILayout.EndHorizontal();
                    }
                    
                    EditorGUILayout.EndScrollView();
                }
            }
        }

        private void ScanCurrentScene()
        {
            objectsWithIssues.Clear();
            scanComplete = false;

            var currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            var allObjects = FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            
            foreach (var obj in allObjects)
            {
                if (HasMissingComponents(obj))
                {
                    objectsWithIssues.Add(obj, new ObjectInfo
                    {
                        objectName = obj.name,
                        sceneName = currentScene.name,
                        scenePath = currentScene.path,
                        hierarchyPath = GetGameObjectPath(obj)
                    });
                }
            }

            scanComplete = true;
            Debug.Log($"Scan complete. Found {objectsWithIssues.Count} objects with missing components.");
        }

        private void ScanAllScenes()
        {
            objectsWithIssues.Clear();
            scanComplete = false;

            var currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            var scenePaths = new List<string>();

            // Build Settingsに登録されているシーンを取得
            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled)
                    scenePaths.Add(scene.path);
            }

            foreach (var scenePath in scenePaths)
            {
                if (string.IsNullOrEmpty(scenePath)) continue;
                
                Debug.Log($"[FindMissing] Opening scene: {scenePath}");
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                Debug.Log($"[FindMissing] Scene opened: {scene.name}");
                
                var allObjects = FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                
                foreach (var obj in allObjects)
                {
                    if (HasMissingComponents(obj))
                    {
                        objectsWithIssues.Add(obj, new ObjectInfo
                        {
                            objectName = obj.name,
                            sceneName = scene.name,
                            scenePath = scenePath,
                            hierarchyPath = GetGameObjectPath(obj)
                        });
                    }
                }
            }

            // 元のシーンに戻す（パスが有効な場合のみ）
            if (!string.IsNullOrEmpty(currentScene.path))
                EditorSceneManager.OpenScene(currentScene.path, OpenSceneMode.Single);

            scanComplete = true;
            Debug.Log($"All scenes scan complete. Found {objectsWithIssues.Count} objects with missing components.");
        }

        private void ScanAllPrefabs()
        {
            objectsWithIssues.Clear();
            scanComplete = false;

            // すべてのプレハブを検索
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
            
            Debug.Log($"[FindMissing] Scanning {prefabGuids.Length} prefabs...");

            foreach (string guid in prefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                
                if (prefab == null) continue;

                // プレハブ自体とその子オブジェクトをすべてチェック
                var allObjects = new List<GameObject> { prefab };
                GetChildrenRecursive(prefab.transform, allObjects);

                foreach (var obj in allObjects)
                {
                    if (HasMissingComponents(obj))
                    {
                        objectsWithIssues.Add(obj, new ObjectInfo
                        {
                            objectName = obj.name,
                            sceneName = $"[Prefab] {path}",
                            scenePath = path,
                            hierarchyPath = GetGameObjectPath(obj)
                        });
                        
                        Debug.LogWarning($"[FindMissing] Found issue in Prefab: {path} -> {obj.name}");
                    }
                }
            }

            scanComplete = true;
            Debug.Log($"[FindMissing] Prefab scan complete. Found {objectsWithIssues.Count} objects with missing components.");
        }

        private void GetChildrenRecursive(Transform parent, List<GameObject> list)
        {
            foreach (Transform child in parent)
            {
                list.Add(child.gameObject);
                GetChildrenRecursive(child, list);
            }
        }

        private bool HasMissingComponents(GameObject obj)
        {
            var components = obj.GetComponents<Component>();
            
            // 通常のnullチェック
            for (int i = 0; i < components.Length; i++)
            {
                var component = components[i];
                
                // 完全にnullの場合
                if (component == null)
                {
                    Debug.LogWarning($"[FindMissing] Found null component at index {i} on '{obj.name}'");
                    return true;
                }
                
                // MonoBehaviourで、型名が不正な場合や、ToString()が "(Missing)" を含む場合
                if (component is MonoBehaviour)
                {
                    var typeName = component.GetType().FullName;
                    var componentString = component.ToString();
                    
                    // "Missing"や"None"を含む場合は無効
                    if (componentString.Contains("Missing") || componentString.Contains("(null)"))
                    {
                        Debug.LogWarning($"[FindMissing] Found broken MonoBehaviour on '{obj.name}': Type={typeName}, ToString={componentString}");
                        return true;
                    }
                    
                    // 型名が"UnityEngine.MonoBehaviour"そのものの場合も無効
                    if (typeName == "UnityEngine.MonoBehaviour")
                    {
                        Debug.LogWarning($"[FindMissing] Found generic MonoBehaviour on '{obj.name}'");
                        return true;
                    }
                }
            }
            
            return false;
        }



        private string GetGameObjectPath(GameObject obj)
        {
            string path = obj.name;
            Transform parent = obj.transform.parent;
            
            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }
            
            return path;
        }
    }
}
