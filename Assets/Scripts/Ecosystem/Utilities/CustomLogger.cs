using System;
using UnityEngine;
#if UNITY_EDITOR

public static class CustomLogger
{
    private static readonly bool isLoggingEnabled = false;
    public static void Log(object message)
    {
        if (isLoggingEnabled)
        {
            Debug.Log(message);
        }
    }

    public static void LogWarning(object message)
    {
        if (isLoggingEnabled)
        {
            Debug.LogWarning(message);
        }
    }

    public static void LogError(object message)
    {
        if (isLoggingEnabled)
        {
            Debug.LogError(message);
        }
    }

    public static void LogException(Exception exception)
    {
        if (isLoggingEnabled)
        {
            Debug.LogException(exception, null);
        }
    }
    
    public static void DrawRay(Vector3 start, Vector3 dir, Color color)
    {
        if (isLoggingEnabled)
        {
            Debug.DrawRay(start, dir, color);
        }
    }
}
#else
// In builds, the CustomLogger class exists but all methods are effectively no-ops.
// This prevents any potential compiler or runtime errors due to missing methods.
public static class CustomLogger
{
    public static void Log(object message) { }

    public static void LogWarning(object message) { }

    public static void LogError(object message) { }

    public static void LogException(Exception exception) { }

    public static void DrawRay(Vector3 start, Vector3 dir, Color color) { }
}
#endif
