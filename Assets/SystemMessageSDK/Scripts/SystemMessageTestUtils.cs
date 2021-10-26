using System;
using UnityEngine;

namespace SystemMessageSdk.Client
{
    public class SystemMessageTestUtils
    {
        private readonly static string TAG = "[SystemMessageTestUtils]";

        public static void testShortToast()
        {
            Log.Instance.V(TAG, "testShortToast");
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaClass proxyClass = new AndroidJavaClass("com.compal.system.messagesdk.MessageTestProxy");
            proxyClass.CallStatic("sendShortToast", activity);
        }

        public static void testLongToast()
        {
            Log.Instance.V(TAG, "testLongToast");
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaClass proxyClass = new AndroidJavaClass("com.compal.system.messagesdk.MessageTestProxy");
            proxyClass.CallStatic("sendLongToast", activity);
        }

        public static void testUnimportantNotification()
        {
            Log.Instance.V(TAG, "testUnimportantNotification");
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaClass proxyClass = new AndroidJavaClass("com.compal.system.messagesdk.MessageTestProxy");
            proxyClass.CallStatic("sendUnimportantNotification", activity);
        }

        public static void testImportantNotification()
        {
            Log.Instance.V(TAG, "testImportantNotification");
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaClass proxyClass = new AndroidJavaClass("com.compal.system.messagesdk.MessageTestProxy");
            proxyClass.CallStatic("sendImportantNotification", activity);
        }
    }
}