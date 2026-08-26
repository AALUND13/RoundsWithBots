using BepInEx.Logging;
using RoundsWithBots.Menu;
using System.Diagnostics;

namespace RoundsWithBots.Utils.Debug {
    public static class LoggerUtils {
        private static void Loginternal(LogLevel level, string message, bool bypassCheck = false) {
            if (LogLevel.Debug > level || RWBMenu.EnableDebugLogging.Value || bypassCheck) {
                var callerFrame = new StackFrame(2);
                var callerMethod = callerFrame.GetMethod();
                string className = callerMethod.DeclaringType.Name;

                RoundsWithBots.ModLogger.Log(level, $"[{className}] {message}");
            }
        }

        public static void Log(LogLevel level, string message, bool bypassCheck = false) =>
            Loginternal(level, message, bypassCheck);
        public static void LogInfo(string message, bool bypassCheck = false) =>
            Loginternal(LogLevel.Info, message, bypassCheck);
        public static void LogWarn(string message) =>
            Loginternal(LogLevel.Warning, message, true);
        public static void LogError(string message) =>
            Loginternal(LogLevel.Error, message, true);
    }
}
