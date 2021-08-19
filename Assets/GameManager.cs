
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public Text resultText = null;
    public Text triggerWords = null;
    private ConcurrentQueue<string> mQueuedMsgs = new ConcurrentQueue<string>();
    private ConcurrentQueue<string> mQueuedMsgs2 = new ConcurrentQueue<string>();

    // Start is called before the first frame update
    void Start() {
        // register callback from java
        SpeechAPI.onSpeechResult += onSpeechResult;
        SpeechAPI.onError += onError;
        SpeechAPI.onLoadLanguage += onLoadLanguage;
        SpeechAPI.onSupportTriggerWords += onSupportTriggerWords;

        // get tirggere words list
        SpeechAPI.stopSpeech(); // alwayse stop recognizer before switch language
        SpeechAPI.switchLanguage("1"); // set chinese as default
        SpeechAPI.getTriggerWords(); // after switch language, we cound get trogger word list
    }

    // Update is called once per frame
    void Update() {
        // modify message in UGUI in main thread!
        while (mQueuedMsgs.TryDequeue(out string message))
        {
            resultText.text = message;
        }
        while (mQueuedMsgs2.TryDequeue(out string message))
        {
            triggerWords.text = message;
        }
    }

    void OnGUI() {
        Event e = Event.current;
        if (e.isKey)
        {
            Debug.Log("Detected key code: " + e.keyCode);
        }
    }

    void OnDestroy() {
        // un-register callback from java
        SpeechAPI.onSpeechResult -= onSpeechResult;
        SpeechAPI.onError -= onError;
        SpeechAPI.onLoadLanguage -= onLoadLanguage;
        SpeechAPI.onSupportTriggerWords -= onSupportTriggerWords;
        SpeechAPI.destroy();
    }

    public void btnStartSpeech() {
        addMessage("請開始說話");
        SpeechAPI.startSpeech();
    }

    public void btnStopSpeech() {
        addMessage("停止辯識");
        SpeechAPI.stopSpeech();
    }

    public void btnSwitchLanguageCh() {
        addMessage("切為換為中文辯識");
        SpeechAPI.stopSpeech();
        SpeechAPI.switchLanguage("1");
        SpeechAPI.getTriggerWords(); // optional
        SpeechAPI.startSpeech();
    }

    public void btnSwitchLanguageCh2()
    {
        addMessage("切為換為中文v2辯識");
        SpeechAPI.stopSpeech();
        SpeechAPI.switchLanguage("3");
        SpeechAPI.getTriggerWords(); // optional
        SpeechAPI.startSpeech();
    }

    public void btnSwitchLanguageEn() {
        addMessage("切為換為英文辯識");
        SpeechAPI.stopSpeech();
        SpeechAPI.switchLanguage("2");
        SpeechAPI.getTriggerWords(); // optional
        SpeechAPI.startSpeech();
    }

    // Compal Speech API callback
    void onSpeechResult(string val, int gmm, int sg, int fil, int energy) {
        Debug.Log("onSpeechResult:" + val);

        // java call callback in background thread and cause below issue:
        // UnityException: get_isActiveAndEnabled can only be called from the main thread.
        //resultText.text = val;
        addMessage(val + ", gmm:" + gmm + ", sg:" + sg + ", fil:" + fil + ", energy:" + energy);
    }

    void onError(int errorcode, string msg) {
        Debug.Log("onError:" + errorcode + ", msg:" + msg);
        string str = "onError:" + errorcode + ", msg:" + msg;
        addMessage(str);
    }

    void onLoadLanguage(int resultcode, string msg) {
        Debug.Log("onLoadLanguage:" + resultcode + ", msg:" + msg);
        string str = "onLoadLanguage:" + resultcode + ", msg:" + msg;
        addMessage(str);
    }

    void onSupportTriggerWords(string[] words)
    {
        string text = null;
        for (int i = 0; i < words.Length; ++i)
        {
            Debug.Log("onSupportTriggerWords:" + words[i]);
            text += words[i];
            text += "\n";
        }
        addMessage2(text);
    }

    void addMessage(string msg) {
        mQueuedMsgs.Enqueue(msg);
    }

    void addMessage2(string msg)
    {
        mQueuedMsgs2.Enqueue(msg);
    }
}
