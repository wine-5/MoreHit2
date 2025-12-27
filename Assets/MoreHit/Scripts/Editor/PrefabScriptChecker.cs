using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace MoreHit.Editor
{
    public class PrefabScriptChecker : EditorWindow
    {
        private Vector2 scrollPosition;
        private List<PrefabInfo> invalidPrefabs = new List<PrefabInfo>();
        private bool isSearching = false;
        private string searchPath = "Assets";

        private class PrefabInfo
        {
            public string path;
            public GameObject prefab;
            public List<string> missingScripts = new List<string>();
            public int missingScriptCount;
        }

        [MenuItem("Tools/Prefab Script Checker")]
        public static void ShowWindow()
        {
            GetWindow<PrefabScriptChecker>("Prefab Script Checker");
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Invalid Prefab Script Finder", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            // Search path
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Search Path:", GUILayout.Width(80));
            searchPath = EditorGUILayout.TextField(searchPath);
            if (GUILayout.Button("Browse", GUILayout.Width(70)))
            {
                string path = EditorUtility.OpenFolderPanel("Select Folder", "Assets", "");
                if (!string.IsNullOrEmpty(path))
                {
                    if (path.StartsWith(Application.dataPath))
                    {
                        searchPath = "Assets" + path.Substring(Application.dataPath.Length);
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            // Search button
            GUI.enabled = !isSearching;
            if (GUILayout.Button("Search for Invalid Prefabs", GUILayout.Height(30)))
            {
                SearchForInvalidPrefabs();
            }
            GUI.enabled = true;

            if (isSearching)
            {
                EditorGUILayout.LabelField("Searching...", EditorStyles.helpBox);
            }

            EditorGUILayout.Space(10);

            // Results
            if (invalidPrefabs.Count > 0)
            {
                EditorGUILayout.LabelField($"Found {invalidPrefabs.Count} prefabs with invalid scripts:", EditorStyles.boldLabel);
                EditorGUILayout.Space(5);

                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

                foreach (var prefabInfo in invalidPrefabs)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                    // Prefab name and path
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Select", GUILayout.Width(60)))
                    {
                        Selection.activeObject = prefabInfo.prefab;
                        EditorGUIUtility.PingObject(prefabInfo.prefab);
                    }

                    EditorGUILayout.LabelField(prefabInfo.path, EditorStyles.wordWrappedLabel);
                    EditorGUILayout.EndHorizontal();

                    // Missing script count
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField($"Missing Scripts: {prefabInfo.missingScriptCount}", EditorStyles.miniLabel);

                    // Component details
                    foreach (var component in prefabInfo.missingScripts)
                    {
                        EditorGUILayout.LabelField($"• {component}", EditorStyles.miniLabel);
                    }
                    EditorGUI.indentLevel--;

                    // Fix button
                    if (GUILayout.Button("Open Prefab", GUILayout.Height(25)))
                    {
                        AssetDatabase.OpenAsset(prefabInfo.prefab);
                    }

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space(5);
                }

                EditorGUILayout.EndScrollView();

                EditorGUILayout.Space(10);

                // Utility buttons
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Clear Results"))
                {
                    invalidPrefabs.Clear();
                }

                if (GUILayout.Button("Export to Console"))
                {
                    ExportToConsole();
                }
                EditorGUILayout.EndHorizontal();
            }
            else if (!isSearching)
            {
                EditorGUILayout.HelpBox("Click 'Search for Invalid Prefabs' to find prefabs with missing or invalid scripts.", MessageType.Info);
            }
        }

        private void SearchForInvalidPrefabs()
        {
            isSearching = true;
            invalidPrefabs.Clear();

            try
            {
                // Find all prefabs in the project
                string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { searchPath });

                int totalPrefabs = prefabGuids.Length;
                int currentIndex = 0;

                foreach (string guid in prefabGuids)
                {
                    currentIndex++;
                    string path = AssetDatabase.GUIDToAssetPath(guid);

                    if (EditorUtility.DisplayCancelableProgressBar(
                        "Searching Prefabs",
                        $"Checking: {path} ({currentIndex}/{totalPrefabs})",
                        (float)currentIndex / totalPrefabs))
                    {
                        break;
                    }

                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (prefab != null)
                    {
                        PrefabInfo info = CheckPrefabForMissingScripts(prefab, path);
                        if (info != null)
                        {
                            invalidPrefabs.Add(info);
                        }
                    }
                }

                EditorUtility.ClearProgressBar();

                Debug.Log($"<color=cyan>Prefab Script Checker:</color> Search complete. Found {invalidPrefabs.Count} prefabs with invalid scripts.");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error during search: {e.Message}");
                EditorUtility.ClearProgressBar();
            }
            finally
            {
                isSearching = false;
            }
        }

        private PrefabInfo CheckPrefabForMissingScripts(GameObject prefab, string path)
        {
            PrefabInfo info = null;

            // Check all GameObjects in the prefab hierarchy
            GameObject[] allObjects = prefab.GetComponentsInChildren<Transform>(true)
                .Select(t => t.gameObject)
                .ToArray();

            foreach (GameObject obj in allObjects)
            {
                Component[] components = obj.GetComponents<Component>();

                for (int i = 0; i < components.Length; i++)
                {
                    if (components[i] == null)
                    {
                        if (info == null)
                        {
                            info = new PrefabInfo
                            {
                                path = path,
                                prefab = prefab
                            };
                        }

                        string objectPath = GetGameObjectPath(obj, prefab);
                        info.missingScripts.Add($"{objectPath} (Component {i})");
                        info.missingScriptCount++;
                    }
                }
            }

            return info;
        }

        private string GetGameObjectPath(GameObject obj, GameObject root)
        {
            if (obj == root)
                return obj.name;

            string path = obj.name;
            Transform current = obj.transform.parent;

            while (current != null && current.gameObject != root)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }

            return path;
        }

        private void ExportToConsole()
        {
            Debug.Log("=== Invalid Prefab Scripts Report ===");
            foreach (var info in invalidPrefabs)
            {
                Debug.LogWarning($"Prefab: {info.path} - Missing Scripts: {info.missingScriptCount}", info.prefab);
                foreach (var script in info.missingScripts)
                {
                    Debug.Log($"  • {script}");
                }
            }
            Debug.Log($"=== Total: {invalidPrefabs.Count} prefabs with issues ===");
        }
    }
}