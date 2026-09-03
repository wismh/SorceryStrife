using UnityEngine;

namespace Game
{
    /// <summary>Single seam for console logging, so call sites don't call UnityEngine.Debug directly.</summary>
    public static class LoggingSystem
    {
        public static void Log(string message)
        {
            Debug.Log(message);
        }

        public static void LogWarning(string message)
        {
            Debug.LogWarning(message);
        }

        public static void LogError(string message)
        {
            Debug.LogError(message);
        }
    }
}
