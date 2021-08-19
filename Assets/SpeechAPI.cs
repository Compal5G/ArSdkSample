using System;
using System.Collections.Generic;
using UnityEngine;

public static class SpeechAPI {

#if UNITY_ANDROID && !UNITY_EDITOR
    static AndroidJavaClass apiClass {
        get {
            init();
            return jc;
        }
    }
    static AndroidJavaClass jc = null;
#else
    static bool isInitialized = false;
#endif

    private static void init() {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (jc != null) {
            return;
        }
        jc = new AndroidJavaClass("com.compal.service.speech.unity.SpeechAPI");
        jc.CallStatic("setAPIListener", new SpeechListener());
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject context = activity.Call<AndroidJavaObject>("getApplicationContext");
        AndroidJavaObject api = jc.CallStatic<AndroidJavaObject>("Init", context);
#else
        if (isInitialized) {
            return;
        }
        isInitialized = true;
        Debug.Log("SpeechAPI.init()");
#endif
    }

    public static void destroy() {
#if UNITY_ANDROID && !UNITY_EDITOR
        onSpeechResult = null;
        onError = null;
        onLoadLanguage = null;
        onSupportTriggerWords = null;
        apiClass.CallStatic("Destroy");
        jc = null;
#else
        Debug.Log("SpeechAPI.destroy()");
        onSpeechResult = null;
        onError = null;
        onLoadLanguage = null;
        onSupportTriggerWords = null;
        isInitialized = false;
#endif
    }

     public static void switchLanguage(string index) {
#if UNITY_ANDROID && !UNITY_EDITOR
        apiClass.CallStatic("switchLanguage", index);
#else
        Debug.Log("SpeechAPI.switchLanguage(" + index + ")");
#endif
    }

    public static void startSpeech() {
#if UNITY_ANDROID && !UNITY_EDITOR
        apiClass.CallStatic("startSpeech");
#else
        Debug.Log("SpeechAPI.startSpeech(leo)");
        if (onSpeechResult != null) {
            onSpeechResult("API is fine ^_^", 99, 99, 99, 99);
        }
#endif
    }

    public static void stopSpeech()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        apiClass.CallStatic("stopSpeech");
#else
        Debug.Log("SpeechAPI.stopSpeech(leo)");
#endif
    }

    public static void getTriggerWords()
    {
        Debug.Log("SpeechAPI.getTriggerWords()");
#if UNITY_ANDROID && !UNITY_EDITOR
        apiClass.CallStatic("getTriggerWords");
#else
        Debug.Log("SpeechAPI.getTriggerWords()");
        if (onSupportTriggerWords != null)
        {
            string[] words = new string[] { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };
            onSupportTriggerWords(words);
        }
#endif
    }

    public static event Action<string, int, int, int, int> onSpeechResult = null;
    public static event Action<int, string> onError = null;
    public static event Action<int, string> onLoadLanguage = null;
    public static event Action<string[]> onSupportTriggerWords = null;

    class SpeechListener : AndroidJavaProxy {
        public SpeechListener() : base("com.compal.service.speech.unity.APIListener") {}

        void onSpeechResult(string result, int gmm, int sg, int fil, int energy)
        {
            if (SpeechAPI.onSpeechResult != null) {
                SpeechAPI.onSpeechResult(result, gmm, sg, fil, energy);
            }
        }

        void onError(int errorcode, string msg) {
            if (SpeechAPI.onError != null) {
                SpeechAPI.onError(errorcode, msg);
            }
        }

        void onLoadLanguage(int resultcode, string msg) {
            if (SpeechAPI.onLoadLanguage != null) {
                SpeechAPI.onLoadLanguage(resultcode, msg);
            }
        }

        void onSupportTriggerWords(string[] words)
        {
            if (SpeechAPI.onSupportTriggerWords != null) {
                SpeechAPI.onSupportTriggerWords(words);
            }
        }
    }
}