using UnityEngine;

/// <summary>
/// MonoBehaviourベースのSingletonパターン基底クラス
/// </summary>
/// <typeparam name="T">Singletonにするクラスの型</typeparam>
public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;
    private static readonly object lockObject = new object();

    /// <summary>
    /// DontDestroyOnLoadを適用するかどうか
    /// </summary>
    protected virtual bool UseDontDestroyOnLoad => true;
    
    /// <summary>
    /// Singletonインスタンスにアクセスするプロパティ
    /// </summary>
    public static T Instance
    {
        get
        {
            lock (lockObject)
            {
                if (instance == null)
                {
#if UNITY_2023_1_OR_NEWER
                    instance = FindFirstObjectByType<T>();
#else
                    instance = FindObjectOfType<T>();
#endif

                    if (instance == null)
                    {
                        GameObject singletonObject = new GameObject($"{typeof(T).Name} (Singleton)");
                        instance = singletonObject.AddComponent<T>();
                        
                        // UseDontDestroyOnLoadプロパティをチェックしてからDontDestroyOnLoadを適用
                        T singletonComponent = instance as T;
                        if (singletonComponent != null && (singletonComponent as Singleton<T>).UseDontDestroyOnLoad)
                            DontDestroyOnLoad(singletonObject);
                    }
                }
                return instance;
            }
        }
    }

    /// <summary>
    /// Singletonインスタンスにアクセスするための短縮プロパティ（Instanceと同じ）
    /// </summary>
    public static T I => Instance;

    protected virtual void Awake()
    {
        if (instance == null)
        {
            instance = this as T;
            if (UseDontDestroyOnLoad)
                DontDestroyOnLoad(transform.root.gameObject);
        }
        else if (instance != this)
        {
            // 既存のインスタンスが動的生成されたもの（親がない）で、
            // 新しいインスタンスがシーンに配置されたもの（参照がある可能性）の場合、
            // 新しい方を優先
            bool existingIsDynamic = instance.transform.parent == null && instance.name.Contains("(Singleton)");
            bool newIsFromScene = !gameObject.name.Contains("(Singleton)");
            
            if (existingIsDynamic && newIsFromScene)
            {
                Debug.LogWarning($"[Singleton] {typeof(T).Name}の複数のインスタンスが存在します。動的生成されたインスタンスを削除し、シーン配置のインスタンスを使用します。");
                Destroy(instance.gameObject);
                instance = this as T;
                if (UseDontDestroyOnLoad)
                    DontDestroyOnLoad(transform.root.gameObject);
            }
            else
            {
                Debug.LogWarning($"[Singleton] {typeof(T).Name}の複数のインスタンスが存在します。重複したオブジェクトを削除します。");
                Destroy(gameObject);
            }
        }
    }

    protected virtual void OnDestroy()
    {
        // シーン切り替え時にインスタンスをクリア（UseDontDestroyOnLoadがfalseの場合）
        if (instance == this && !UseDontDestroyOnLoad)
        {
            instance = null;
        }
    }

    /// <summary>
    /// インスタンスが存在するかチェック
    /// </summary>
    public static bool HasInstance => instance != null;
}
