using BepInEx.Logging;
using UnityEngine;

namespace FastSwim
{
    public static class Logger
    {
        public static ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource(FastSwim.NAME);
        internal static void Log(object s)
        {
            if (!ConfigurationFile.debug.Value)
            {
                return;
            }

            logger.LogInfo(s?.ToString());
        }

        internal static void LogInfo(object s)
        {
            logger.LogInfo(s?.ToString());
        }

        internal static void LogWarning(object s)
        {
            var toPrint = $"{FastSwim.NAME} {FastSwim.VERSION}: {(s != null ? s.ToString() : "null")}";

            Debug.LogWarning(toPrint);
        }

        internal static void LogError(object s)
        {
            var toPrint = $"{FastSwim.NAME} {FastSwim.VERSION}: {(s != null ? s.ToString() : "null")}";

            Debug.LogError(toPrint);
        }
    }
}
