using System;
using UnityEngine;

namespace SystemMessageSdk.Client
{
    public class Log
    {
        private readonly static string TAG = "[SystemMessageSDK]";

        private bool _isDebugMode = false;
        public static Log Instance { get; private set; }
        public static void CreateInstance(bool isDebugMode)
        {
            if (Instance == null)
            {
                Instance = new Log(isDebugMode);
                AndroidJavaClass proxyClass = new AndroidJavaClass("com.compal.system.messagesdk.utils.Log");
				proxyClass.CallStatic("setLogDebug", isDebugMode);
            }
        }

        private Log(bool isDebugMode)
        {
            _isDebugMode = isDebugMode;
        }

        public void V(string tag, string message)
        {
            if (_isDebugMode)
            {
                Debug.Log(GetLogString(tag, message));
            }
        }

        public void D(string tag, string message)
        {
            if (_isDebugMode)
            {
                Debug.Log(GetLogString(tag, message));
            }
        }

        public void W(string tag, string message)
        {
            Debug.LogWarning(GetLogString(tag, message));
        }

        public void E(string tag, string message)
        {
            Debug.LogError(GetLogString(tag, message));
        }

        public void Ex(Exception message)
        {
            Debug.LogError(message);
        }

        private string GetLogString(string tag, string message)
        {
            return TAG + tag + " " + message;
        }
    }
}