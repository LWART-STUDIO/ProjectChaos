using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Manages the network debugging settings and applies them to the NetworkDebugger instance.
    /// </summary>
    public class NetworkDebuggerManager : MonoBehaviour {

        // Indicates whether console logging is enabled or not.
        [SerializeField]
        private bool EnableConsoleLog = false; // Default is false, can be set in the Unity Inspector

        // Indicates whether console logging will appear on compiled vuild
        [SerializeField]
        private bool EnableOnBuild = false; // Default is false, can be set in the Unity Inspector

        // Indicated if need to capture warnings
        [SerializeField]
        private bool CaptureWarnings = true;

        // Indicated if need to capture errors
        [SerializeField]
        private bool CaptureErrors = true;

        // Indicated if need to capture logs
        [SerializeField]
        private bool CaptureLogs = true;

        // Indicated if need to disable objectnet gizmos
        [SerializeField]
        private bool ShowGizmos = true;

        // Reference to the NetworkDebugger instance
        private NetworkDebugger Debugger;

        // Store message in wueue when "EnableOnBuild" is active
        private Queue InternalLogQueue = new Queue();

        private GUIStyle LogStyle = null;

        const uint QUEUE_SIZE = 100;  // number of messages to keep

        /// <summary>
        /// Called when the script instance is being loaded.
        /// Initializes the NetworkDebugger instance with the console logging setting.
        /// </summary>
        private void Start() {
            // Initiate NetworkDebugger element
            if (NetworkDebugger.Instance() == null) {
                this.Debugger = new NetworkDebugger(this);
            } else {
                this.Debugger = NetworkDebugger.Instance();
            }
            // Set the console logging option of the NetworkDebugger instance based on the serialized field
            NetworkDebugger.Instance().Console      = this.EnableConsoleLog;
            // Set the console logging option of the NetworkDebugger instance based on the serialized field
            NetworkDebugger.Instance().OnBuild      = this.EnableOnBuild;
            // Flag if need to show gizmo
            NetworkDebugger.Instance().ShowGizmos   = this.ShowGizmos;
        }

        void OnEnable() {
            if (this.EnableOnBuild) {
                Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
                Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);
                Application.SetStackTraceLogType(LogType.Error, StackTraceLogType.Full);
                Application.logMessageReceived += HandleLog;
            }
        }

        void OnDisable() {
            if (this.EnableOnBuild) {                
                Application.logMessageReceived -= HandleLog;
            }
        }

        void HandleLog(string logString, string stackTrace, LogType type) {
            this.InternalLogQueue.Enqueue(string.Format("[{0}] {1} : {2}", System.DateTime.Now.ToString("HH:mm:ss.fff"), type.ToString().ToUpper(), logString));
            if (((type == LogType.Log) && (this.CaptureLogs)) ||
                ((type == LogType.Warning) && (this.CaptureWarnings)) ||
                (((type == LogType.Exception) || (type == LogType.Error)) && (this.CaptureErrors))) {
                this.InternalLogQueue.Enqueue(stackTrace);
            }
            while (this.InternalLogQueue.Count > QUEUE_SIZE) {
                this.InternalLogQueue.Dequeue();
            }
        }

        void OnGUI() {
            if (this.EnableOnBuild) {
                this.InitStyles();
                GUILayout.BeginArea(new Rect(Screen.width - (Screen.width / 2.0f), 10, (Screen.width / 2.0f) - 10, Screen.height - 20), this.LogStyle);
                GUILayout.Label("\n" + string.Join("\n", this.InternalLogQueue.ToArray().Reverse()));
                GUILayout.EndArea();
            }
        }

        private void InitStyles() {
            if (this.LogStyle == null) {
                this.LogStyle = new GUIStyle(GUI.skin.box);
                Color bgColor = new Color(Color.black.r, Color.black.g, Color.black.b, 0.15f);
                this.LogStyle.normal.background = MakeTex((Screen.width / 2) - 10, Screen.height - 20, bgColor);
            }
        }

        private Texture2D MakeTex(int width, int height, Color col) {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i) {
                pix[i] = col;
            }
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }
    }
}