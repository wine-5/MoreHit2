using UnityEngine;

/// <summary>
/// MonoBehaviourベースのSingletonパターン基底クラス
/// </summary>
/// <typeparam name="T">Singletonにするクラスの型</typeparam>
public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;
    private static readonly object lockObject = new object();
    private static bool isDestroying = false;

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
            if (isDestroying)
            {
                Debug.LogWarning($"[Singleton] {typeof(T).Name}のインスタンスはすでに破棄されています。");
                return null;
            }

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
                        
                        // IsPersistentプロパティをチェックしてからDontDestroyOnLoadを適用
                        T singletonComponent = instance as T;
                        if (singletonComponent != null && (singletonComponent as Singleton<T>).UseDontDestroyOnLoad)
                        {
                            DontDestroyOnLoad(singletonObject);
                        }
                    }
                }
                return instance;
            }
        }
    }

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
            Debug.LogWarning($"[Singleton] {typeof(T).Name}の複数のインスタンスが存在します。重複したオブジェクトを削除します。");
            Destroy(gameObject);
        }
    }

    protected virtual void OnDestroy()
    {
        if (instance == this)
        {
            isDestroying = true;
        }
    }

    /// <summary>
    /// インスタンスが存在するかチェック
    /// </summary>
    public static bool HasInstance => instance != null && !isDestroying;
}
