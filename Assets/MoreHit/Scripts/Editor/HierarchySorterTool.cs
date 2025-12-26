using UnityEngine;
using UnityEditor;
using System.Linq;

namespace MoreHit.Editor
{
    public class HierarchySorterTool : EditorWindow
    {
        public Transform targetFolder;
        
        [MenuItem("Tools/Hierarchy Sorter")]
        public static void ShowWindow()
        {
            GetWindow<HierarchySorterTool>("Hierarchy Sorter");
        }
        
        void OnGUI()
        {
            GUILayout.Label("Hierarchy Sorter Tool", EditorStyles.boldLabel);
            
            targetFolder = (Transform)EditorGUILayout.ObjectField("Target Folder", targetFolder, typeof(Transform), true);
            
            if (targetFolder != null)
            {
                EditorGUILayout.LabelField($"対象フォルダ: {targetFolder.name}");
                EditorGUILayout.LabelField($"子オブジェクト数: {targetFolder.childCount}");
                
                EditorGUILayout.Space();
                
                // 現在の順序を表示
                EditorGUILayout.LabelField("現在の順序:", EditorStyles.boldLabel);
                for (int i = 0; i < targetFolder.childCount && i < 10; i++) // 最大10個まで表示
                {
                    EditorGUILayout.LabelField($"  {i + 1}. {targetFolder.GetChild(i).name}");
                }
                
                if (targetFolder.childCount > 10)
                {
                    EditorGUILayout.LabelField($"  ... 他 {targetFolder.childCount - 10} 個");
                }
                
                EditorGUILayout.Space();
                
                EditorGUILayout.HelpBox(
                    "使い方:\n" +
                    "1. ヒエラルキーから対象フォルダを選択\n" +
                    "2. 'Sort by Name' で名前順にソート\n" +
                    "3. 数字込みの自然な順序でソートされます",
                    MessageType.Info);
                
                if (GUILayout.Button("Sort by Name (Natural Order)"))
                {
                    SortChildrenByName();
                }
                
                if (GUILayout.Button("Sort by Name (Alphabetical)"))
                {
                    SortChildrenAlphabetical();
                }
                
                if (GUILayout.Button("Reverse Order"))
                {
                    ReverseChildrenOrder();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("ソートしたいフォルダ（親オブジェクト）をヒエラルキーから選択してください", MessageType.Warning);
            }
        }
        
        void SortChildrenByName()
        {
            if (targetFolder == null) return;
            
            // Undo登録
            Undo.RegisterCompleteObjectUndo(targetFolder, "Sort Children by Name");
            
            // 子オブジェクトを取得
            Transform[] children = new Transform[targetFolder.childCount];
            for (int i = 0; i < targetFolder.childCount; i++)
            {
                children[i] = targetFolder.GetChild(i);
            }
            
            // 自然順序でソート（数字を考慮）
            var sortedChildren = children.OrderBy(child => child.name, new NaturalStringComparer()).ToArray();
            
            // 順序を再配置
            for (int i = 0; i < sortedChildren.Length; i++)
            {
                sortedChildren[i].SetSiblingIndex(i);
            }
            
            Debug.Log($"{targetFolder.name} 内の {children.Length} 個のオブジェクトを自然順序でソートしました。");
        }
        
        void SortChildrenAlphabetical()
        {
            if (targetFolder == null) return;
            
            Undo.RegisterCompleteObjectUndo(targetFolder, "Sort Children Alphabetically");
            
            Transform[] children = new Transform[targetFolder.childCount];
            for (int i = 0; i < targetFolder.childCount; i++)
            {
                children[i] = targetFolder.GetChild(i);
            }
            
            // アルファベット順でソート
            var sortedChildren = children.OrderBy(child => child.name).ToArray();
            
            for (int i = 0; i < sortedChildren.Length; i++)
            {
                sortedChildren[i].SetSiblingIndex(i);
            }
            
            Debug.Log($"{targetFolder.name} 内の {children.Length} 個のオブジェクトをアルファベット順でソートしました。");
        }
        
        void ReverseChildrenOrder()
        {
            if (targetFolder == null) return;
            
            Undo.RegisterCompleteObjectUndo(targetFolder, "Reverse Children Order");
            
            Transform[] children = new Transform[targetFolder.childCount];
            for (int i = 0; i < targetFolder.childCount; i++)
            {
                children[i] = targetFolder.GetChild(i);
            }
            
            // 逆順に並び替え
            for (int i = 0; i < children.Length; i++)
            {
                children[children.Length - 1 - i].SetSiblingIndex(i);
            }
            
            Debug.Log($"{targetFolder.name} 内のオブジェクトを逆順にしました。");
        }
    }
    
    // 自然順序比較（数字を考慮したソート）
    public class NaturalStringComparer : System.Collections.Generic.IComparer<string>
    {
        public int Compare(string x, string y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;
            
            int ix = 0, iy = 0;
            
            while (ix < x.Length && iy < y.Length)
            {
                if (char.IsDigit(x[ix]) && char.IsDigit(y[iy]))
                {
                    // 数字の比較
                    int numX = 0, numY = 0;
                    
                    while (ix < x.Length && char.IsDigit(x[ix]))
                    {
                        numX = numX * 10 + (x[ix] - '0');
                        ix++;
                    }
                    
                    while (iy < y.Length && char.IsDigit(y[iy]))
                    {
                        numY = numY * 10 + (y[iy] - '0');
                        iy++;
                    }
                    
                    if (numX != numY)
                        return numX.CompareTo(numY);
                }
                else
                {
                    // 文字の比較
                    if (x[ix] != y[iy])
                        return x[ix].CompareTo(y[iy]);
                    
                    ix++;
                    iy++;
                }
            }
            
            return x.Length.CompareTo(y.Length);
        }
    }
}