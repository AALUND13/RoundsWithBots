namespace RoundsWithBots.utils {
    public static class Logger {
        public static void Log(string message) {
            if(ConfigHandler.DebugMode.Value) {
                UnityEngine.Debug.Log(message);
            }
        }

        public static void LogWarning(string message) {
            if(ConfigHandler.DebugMode.Value) {
                UnityEngine.Debug.LogWarning(message);
            }
        }

        public static void Error(string message) {
            if(ConfigHandler.DebugMode.Value) {
                UnityEngine.Debug.LogError(message);
            }
        }
    }
}
