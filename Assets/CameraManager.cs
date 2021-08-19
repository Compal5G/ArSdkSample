
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraManager : MonoBehaviour
{
    public Text resultText = null;
    public Text triggerWords = null;
    private ConcurrentQueue<string> mQueuedMsgs = new ConcurrentQueue<string>();
    private ConcurrentQueue<string> mQueuedMsgs2 = new ConcurrentQueue<string>();

    Texture2D mCameraFrame;

    void Awake()
    {
        UnityThread.initUnityThread();
    }

    // Start is called before the first frame update
    void Start()
    {
        // for dislplay
        mCameraFrame = new Texture2D(640, 480, TextureFormat.RGB565, false);
        
        // register callback from java
        CameraAPI.onFrame += onFrame;
        CameraAPI.onError += onError;

        //CameraAPI.rotate180FrameBuffer(true);
    }

    // Update is called once per frame
    void Update()
    {
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

    void OnGUI()
    {
        Event e = Event.current;
        if (e.isKey)
        {
            Debug.Log("Detected key code: " + e.keyCode);
        }

        GUI.DrawTexture(new Rect((Screen.width - 800) / 2, Screen.height / 2, 800, 600), mCameraFrame, ScaleMode.StretchToFill, true, 2.0F);
    }

    void OnDestroy()
    {
        // un-register callback from java
        CameraAPI.onFrame -= onFrame;
        CameraAPI.onError -= onError;
        CameraAPI.destroy();
    }

    public void btnStartCamera()
    {
        addMessage("UVC camera open");
        CameraAPI.startUsbCamera();
    }

    public void btnStopCamera()
    {
        addMessage("UVC camera close");
        CameraAPI.stopUsbCamera();
    }

    void onFrame(byte[] data, int width, int height, int reserve)
    {
        Debug.Log("Camera.onFrame:" + data.Length + ", width:" + width + ", height:" + height);
        
        // load buffer in background thread
        mCameraFrame.LoadRawTextureData(data);
        
        UnityThread.executeInUpdate(() =>
        {
            // Display must in UI thread 
            mCameraFrame.Apply();
        });
    }

    void onError(int errorcode, string msg)
    {
        Debug.Log("onError:" + errorcode + ", msg:" + msg);
        string str = "onError:" + errorcode + ", msg:" + msg;
        addMessage(str);
    }

    void addMessage(string msg)
    {
        mQueuedMsgs.Enqueue(msg);
    }

    void addMessage2(string msg)
    {
        mQueuedMsgs2.Enqueue(msg);
    }
}
