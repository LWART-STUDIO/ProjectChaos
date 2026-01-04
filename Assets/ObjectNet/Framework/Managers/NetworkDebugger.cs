using UnityEngine;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// A singleton class for network debugging that allows for logging debug messages, errors, and warnings.
    /// </summary>
    public class NetworkDebugger {

        /// <summary>
        /// Flag to enable or disable console logging.
        /// </summary>
        public bool Console = true;

        /// <summary>
        /// Flag to enable or disable console logging on build.
        /// </summary>
        public bool OnBuild = true;

        /// <summary>
        /// Flag iof need to trace send and received messages
        /// </summary>
        public bool TraceMessages = false;

        /// <summary>
        /// Flag to enable or disable gizmos
        /// </summary>
        public bool ShowGizmos = true;

        /// <summary>
        /// Store the Debugger manager responsible to handle with log
        /// </summary>
        private NetworkDebuggerManager debuggerManager;

        /// <summary>
        /// The singleton instance of the NetworkDebugger.
        /// </summary>
        private static NetworkDebugger instance;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="manager"></param>
        public NetworkDebugger(NetworkDebuggerManager manager) {
            NetworkDebugger.instance = this;
            this.debuggerManager = manager;
        }

        /// <summary>
        /// Retrieves the singleton instance of the NetworkDebugger, creating it if it does not already exist.
        /// </summary>
        /// <returns>The singleton instance of the NetworkDebugger.</returns>
        public static NetworkDebugger Instance() {
            if (!InstanceExists()) {
                if (Application.isPlaying) {
                    NetworkDebugger.instance = new NetworkDebugger(null);                    
                }
            }
            return NetworkDebugger.instance;
        }

        /// <summary>
        /// Sets the console logging flag.
        /// </summary>
        /// <param name="enabled">If set to true, enables console logging.</param>
        public void SetConsoleLog(bool enabled) {
            this.Console = enabled;
        }

        /// <summary>
        /// Checks if the singleton instance of the NetworkDebugger exists.
        /// </summary>
        /// <returns>True if the instance exists, false otherwise.</returns>
        private static bool InstanceExists() {
            return (NetworkDebugger.instance != null);
        }

        /// <summary>
        /// Logs a message to the console if console logging is enabled.
        /// </summary>
        /// <param name="text">The text to log.</param>
        /// <param name="arguments">Optional arguments to format the text.</param>
        public static void Log(string text, params object[] arguments) {
            if (NetworkDebugger.Instance() != null) {
                if ((NetworkDebugger.Instance().Console) || (NetworkDebugger.Instance().OnBuild)) {
                    UnityEngine.Debug.Log((arguments.Length > 0) ? string.Format(text, arguments) : text);
                }                
            } else {
                UnityEngine.Debug.Log((arguments.Length > 0) ? string.Format(text, arguments) : text);
            }
        }

        /// <summary>
        /// Logs a debug message to the console if console logging is enabled.
        /// </summary>
        /// <param name="text">The text to log.</param>
        /// <param name="arguments">Optional arguments to format the text.</param>
        public static void LogDebug(string text, params object[] arguments) {
            if (NetworkDebugger.Instance() != null) {
                if ((NetworkDebugger.Instance().Console) || (NetworkDebugger.Instance().OnBuild)) {
                    UnityEngine.Debug.Log((arguments.Length > 0) ? string.Format(text, arguments) : text);
                }
            } else {
                UnityEngine.Debug.Log((arguments.Length > 0) ? string.Format(text, arguments) : text);
            }
        }

        /// <summary>
        /// Logs an error message to the console if console logging is enabled.
        /// </summary>
        /// <param name="text">The text to log as an error.</param>
        /// <param name="arguments">Optional arguments to format the text.</param>
        public static void LogError(string text, params object[] arguments) {
            if (NetworkDebugger.Instance() != null) {
                if ((NetworkDebugger.Instance().Console) || (NetworkDebugger.Instance().OnBuild)) {
                    UnityEngine.Debug.LogError((arguments.Length > 0) ? string.Format(text, arguments) : text);
                }
            } else {
                UnityEngine.Debug.LogError((arguments.Length > 0) ? string.Format(text, arguments) : text);
            }
}

        /// <summary>
        /// Logs a warning message to the console if console logging is enabled.
        /// </summary>
        /// <param name="text">The text to log as a warning.</param>
        /// <param name="arguments">Optional arguments to format the text.</param>
        public static void LogWarning(string text, params object[] arguments) {
            if (NetworkDebugger.Instance() != null) {
                if ((NetworkDebugger.Instance().Console) || (NetworkDebugger.Instance().OnBuild)) {
                    UnityEngine.Debug.LogWarning((arguments.Length > 0) ? string.Format(text, arguments) : text);
                }
            } else {
                UnityEngine.Debug.LogWarning((arguments.Length > 0) ? string.Format(text, arguments) : text);
            }
        }

        /// <summary>
        /// Logs some special traced code
        /// </summary>
        /// <param name="text">The text to log as a warning.</param>
        /// <param name="arguments">Optional arguments to format the text.</param>
        public static void LogTrace(string text, params object[] arguments) {
            if (NetworkDebugger.Instance() != null) {
                if ((NetworkDebugger.Instance().Console) || (NetworkDebugger.Instance().OnBuild)) {
                    UnityEngine.Debug.Log((arguments.Length > 0) ? string.Format(text, arguments) : text);
                }
            } else {
                UnityEngine.Debug.Log((arguments.Length > 0) ? string.Format(text, arguments) : text);
            }
        }

    }

}