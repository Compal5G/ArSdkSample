using System;
using UnityEngine;

namespace SystemMessageSdk.Client
{
    public class SystemMessageUtils
    {
        private readonly static string TAG = "[SystemMessageUtils]";

        public static void registerSystemMessage(IMessageCallback callback)
        {
            Log.Instance.V(TAG, "registerSystemMessage");
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaClass proxyClass = new AndroidJavaClass("com.compal.system.messagesdk.MessageUnityProxy");
            proxyClass.CallStatic("registerMessageCallback", activity, callback);
        }

        public static void unRegisterSystemMessage(IMessageCallback callback)
        {
            Log.Instance.V(TAG, "unRegisterSystemMessage");
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaClass proxyClass = new AndroidJavaClass("com.compal.system.messagesdk.MessageUnityProxy");
            proxyClass.CallStatic("unRegisterMessageCallback", activity, callback);
        }
    }
}