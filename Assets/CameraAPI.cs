using System;
using System.Collections.Generic;
using UnityEngine;

public static class CameraAPI {

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
        jc = new AndroidJavaClass("com.compal.service.camera.unity.CameraAPI2");
        jc.CallStatic("setAPIListener", new CameraListener());
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject context = activity.Call<AndroidJavaObject>("getApplicationContext");
        AndroidJavaObject api = jc.CallStatic<AndroidJavaObject>("Init", context);
#else
        if (isInitialized) {
            return;
        }
        isInitialized = true;
        Debug.Log("CameraAPI.init()");
#endif
    }

    public static void destroy() {
        stopUsbCamera();

#if UNITY_ANDROID && !UNITY_EDITOR
        onFrame = null;
        onError = null;
        apiClass.CallStatic("Destroy");
        jc = null;
#else
        Debug.Log("CameraAPI.destroy()");
        onFrame = null;
        onError = null;
        isInitialized = false;
#endif
    }

    public static void startUsbCamera() {
#if UNITY_ANDROID && !UNITY_EDITOR
        apiClass.CallStatic("startUsbCameraEx2");
#else
        Debug.Log("CameraAPI.startUsbCamera(leo)");
        if (onFrame != null) {

            // Create a 16x16 texture with PVRTC RGBA4 format
            // and fill it with raw PVRTC bytes.
            Texture2D tex = new Texture2D(16, 16, TextureFormat.PVRTC_RGBA4, false);
            // Raw PVRTC4 data for a 16x16 texture. This format is four bits
            // per pixel, so data should be 16*16/2=128 bytes in size.
            // Texture that is encoded here is mostly green with some angular
            // blue and red lines.
            byte[] pvrtcBytes = new byte[]
            {
                0x30, 0x32, 0x32, 0x32, 0xe7, 0x30, 0xaa, 0x7f, 0x32, 0x32, 0x32, 0x32, 0xf9, 0x40, 0xbc, 0x7f,
                0x03, 0x03, 0x03, 0x03, 0xf6, 0x30, 0x02, 0x05, 0x03, 0x03, 0x03, 0x03, 0xf4, 0x30, 0x03, 0x06,
                0x32, 0x32, 0x32, 0x32, 0xf7, 0x40, 0xaa, 0x7f, 0x32, 0xf2, 0x02, 0xa8, 0xe7, 0x30, 0xff, 0xff,
                0x03, 0x03, 0x03, 0xff, 0xe6, 0x40, 0x00, 0x0f, 0x00, 0xff, 0x00, 0xaa, 0xe9, 0x40, 0x9f, 0xff,
                0x5b, 0x03, 0x03, 0x03, 0xca, 0x6a, 0x0f, 0x30, 0x03, 0x03, 0x03, 0xff, 0xca, 0x68, 0x0f, 0x30,
                0xaa, 0x94, 0x90, 0x40, 0xba, 0x5b, 0xaf, 0x68, 0x40, 0x00, 0x00, 0xff, 0xca, 0x58, 0x0f, 0x20,
                0x00, 0x00, 0x00, 0xff, 0xe6, 0x40, 0x01, 0x2c, 0x00, 0xff, 0x00, 0xaa, 0xdb, 0x41, 0xff, 0xff,
                0x00, 0x00, 0x00, 0xff, 0xe8, 0x40, 0x01, 0x1c, 0x00, 0xff, 0x00, 0xaa, 0xbb, 0x40, 0xff, 0xff,
            };
            // Load data into the texture and upload it to the GPU.
            tex.LoadRawTextureData(pvrtcBytes);
            tex.Apply();

            onFrame(pvrtcBytes, 16, 16, 0);
        }
#endif
    }

    public static void stopUsbCamera()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        apiClass.CallStatic("stopUsbCamera");
#else
        Debug.Log("CameraAPI.stopUsbCamera(leo)");
#endif
    }

    public static void rotate180FrameBuffer(Boolean flag)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        // only for test
        apiClass.CallStatic("rotate180FrameBuffer", flag);
#else
        Debug.Log("CameraAPI.rotate180FrameBuffer(leo)" + flag);
#endif
    }

    public static event Action<byte[], int, int, int> onFrame = null;
    public static event Action<int, string> onError = null;

    class CameraListener : AndroidJavaProxy {
        public CameraListener() : base("com.compal.service.camera.unity.APIListener2") {}

        void onError(int errorcode, string msg) {
            Debug.Log("CameraAPI.onError(): " + msg);
            if (CameraAPI.onError != null) {
                CameraAPI.onError(errorcode, msg);
            }
        }

        // only for test
        void onConfig(AndroidJavaObject jo)
        {
            Debug.Log("CameraAPI.onConfig(): " + jo);

            // http://blog.trsquarelab.com/2015/06/setting-fields-of-android-java-object.html
            // https://answers.unity.com/questions/863297/how-do-i-pass-a-string-array-to-java-via-jni.html
            // https://forum.unity.com/threads/passing-arrays-through-the-jni.91757/
            AndroidJavaObject fpsObjects = jo.Get<AndroidJavaObject>("mISizeFps");

            // Make sure the resulting java object is not null
            if (fpsObjects != null && fpsObjects.GetRawObject().ToInt32() != 0)
            {
                AndroidJavaObject[] fpsArray = AndroidJNIHelper.ConvertFromJNIArray<AndroidJavaObject[]>(fpsObjects.GetRawObject());
                for (int i = 0; i < fpsArray.Length; i++)
                {
                    AndroidJavaObject item = fpsArray[i];
                    int frametype = item.Get<int>("frameType");
                    int width = item.Get<int>("width");
                    int height = item.Get<int>("height");
                    string[] fp = AndroidJNIHelper.ConvertFromJNIArray<string[]>(item.Get<AndroidJavaObject>("fps").GetRawObject());
                    string output = String.Join(" ", fp);
                    Debug.Log("CameraAPI.onConfig(): type" + frametype + ", w:" + width + ", h:" + height + ", fps:" + output);
                }
                
            }
        }

        void onFrame(AndroidJavaObject jo, int width, int height, int reserved) {
            //Debug.Log("CameraAPI.onFrame(): " + jo);
            AndroidJavaObject bufferObject = jo.Get<AndroidJavaObject>("mBuffer");
            byte[] bytes = AndroidJNIHelper.ConvertFromJNIArray<byte[]>(bufferObject.GetRawObject());
            if (bytes == null) {
                Debug.Log("CameraAPI.onFrame(): bytes:" + bytes);
                return;
            }

            if (CameraAPI.onFrame != null) {
                CameraAPI.onFrame(bytes, width, height, reserved);
            }
        }
    }
}