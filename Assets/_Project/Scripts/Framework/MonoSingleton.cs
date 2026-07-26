using UnityEngine;

public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
{
    protected static T _instance;
    private static bool _isQuitting;

    public static T Instance
    {
        get
        {
            if (_isQuitting)
                return null;
            if (_instance == null)
                _instance = FindFirstObjectByType<T>();
            return _instance;
        }
    }

    public static bool HasInstance => _instance != null && !_isQuitting;

    protected virtual void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = (T)this;
    }

    protected virtual void OnDestroy()
    {
        if (_instance == this)
            _instance = null;
    }

    private void OnApplicationQuit()
    {
        _isQuitting = true;
    }
}

public abstract class DontDestroyMonoSingleton<T> : MonoSingleton<T> where T : DontDestroyMonoSingleton<T>
{
    protected override void Awake()
    {
        base.Awake();
        if (_instance == this)
            DontDestroyOnLoad(gameObject);
    }
}
