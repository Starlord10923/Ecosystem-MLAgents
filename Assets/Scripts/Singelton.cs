using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static bool _isShuttingDown = false;

    public static T Instance
    {
        get
        {
            if (_isShuttingDown)
                return null;

            if (_instance == null)
            {
                _instance = FindObjectOfType<T>();
                if (_instance == null)
                {
                    CustomLogger.LogError($"[{typeof(T).Name}] No instance found in the scene.");
                }
            }

            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
        }
        else if (_instance != this)
        {
            CustomLogger.LogWarning($"[{typeof(T).Name}] Duplicate detected. Destroying the new instance.");
            Destroy(gameObject);
        }
    }

    protected virtual void OnApplicationQuit()
    {
        _isShuttingDown = true;
    }

    protected virtual void OnDestroy()
    {
        _isShuttingDown = true;
    }
}
