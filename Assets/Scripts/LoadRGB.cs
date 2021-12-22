using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using Object = UnityEngine.Object;
using System.IO;

public class LoadRGB : MonoBehaviour, SvrManager.SvrEventListener
{
    [HideInInspector] public  Texture2D tex = null;
    private Color32[] pixel32;
    private GCHandle pixelHandle;
    private IntPtr pixelPtr;    
    private long rgbTimestamp = 0;
    private int lastWidth = 0;
    private int lastHeight = 0;
    private Thread newThread;
    private Boolean isTextureReady = false;
    private Object thisLock = new Object();

    volatile bool keepThreadAlive = true;
    private bool isSVRReady = false;

    private int count = 0;

    private static bool open = true;

    void Start()
    {
        // use uvc rgb
        //API.xslam_set_rgb_source( 0 );
        //Register for SvrEvents
        SvrManager.Instance.AddEventListener (this);
        // set to 720p
        newThread = new Thread(GetRGBFrame);
    }

    /// <summary>
    /// Raises the svr event event.
    /// </summary>
    /// <param name="ev">Ev.</param>
    //---------------------------------------------------------------------------------------------
    public void OnSvrEvent(SvrManager.SvrEvent ev)
    {
        switch (ev.eventType)
        {
            case SvrManager.svrEventType.kEventVrModeStarted:
                Debug.Log("xslam_set_rgb_resolution");
                API.xslam_set_rgb_resolution(1);
                isSVRReady = true;
                count = 0;
                break;
        }
    }

    void GetRGBFrame()
    {
        while (keepThreadAlive)
        {
            if (API.xslam_ready())
            {
                //API.xslam_set_rgb_resolution(1);
                if (isSVRReady)
                {
                    lock (thisLock)
                    {
                        //Debug.Log("thread B id " + Thread.CurrentThread.ManagedThreadId);
                        try
                        {
                            if (API.xslam_get_rgb_image_RGBA(pixelPtr, tex.width, tex.height, ref rgbTimestamp))
                            {

                            }
                        }
                        catch (Exception e)
                        {
                            Debug.LogException(e, this);
                            return;
                        }
                    }
                }
            }
            Thread.Sleep(30);
            count++;
        }
    }
    
    void Update()
    {
    	if( API.xslam_ready() ){
    	
    		int width = API.xslam_get_rgb_width();
    		int height = API.xslam_get_rgb_height();
    		//Debug.Log("Create RGB texture " + width + "x" + height);
    		if( width > 0 && height > 0 ){
				if( lastWidth != width || lastHeight != height ){
                    keepThreadAlive = false;
                    try{
                        double r = 0.25;
                        if (width < 1280 && height < 720) {
                            r = 1.0;
                        }
                        int w = (int)(width * r);
                        int h = (int)(height * r);
                        Debug.Log("Create RGB texture " + w + "x" + h);
                        TextureFormat format = TextureFormat.RGBA32;
                        tex = new Texture2D(w, h, format, false);
                        //tex.filterMode = FilterMode.Point;

                        try {
                            pixelHandle.Free();
                        } catch {}
                        pixel32 = tex.GetPixels32();
                        pixelHandle = GCHandle.Alloc(pixel32, GCHandleType.Pinned);
                        pixelPtr = pixelHandle.AddrOfPinnedObject();

                        GetComponent<Renderer>().material.mainTexture = tex;
                    }catch (Exception e)
                    {
                        Debug.LogException(e, this);
                        return;
                    }

                    lastWidth = width;
                    lastHeight = height;

                    if(!keepThreadAlive) {
                        keepThreadAlive = true;
                        newThread.Start();
                    }
                }


                // if (GameObject.Find("TogglePanel").GetComponent<StreamToggle>().RgbOn())
                // //if (true)
                // {
                //    try
                //    {
                //        if (API.xslam_get_rgb_image_RGBA(pixelPtr, tex.width, tex.height, ref rgbTimestamp))
                //        {
                //            //Update the Texture2D with array updated in C++
                //            tex.SetPixels32(pixel32);
                //            tex.Apply();
                //        }
                //        else
                //        {
                //            Debug.Log("Invalid texture");
                //        }
                //    }
                //    catch (Exception e)
                //    {
                //        Debug.LogException(e, this);
                //        return;
                //    }

                // }
                lock (thisLock)
                {
                   //Debug.Log("apply B");
                    tex.SetPixels32(pixel32);
                    tex.Apply();
                    //Debug.Log("apply E");
                    if(count == 50) {
                        SavePNG();
                        //count = 0;
                        //API.xslam_start_rgb_stream();
                    }
                }

            }
        }

        //if (Input.GetKeyDown(KeyCode.JoystickButton0))
        //{
        //    if(!open) {
        //        open = true;
        //        Debug.Log("xslam_start_rgb_stream");
        //        API.xslam_start_rgb_stream();
        //    } else {
        //        open = false;
        //        Debug.Log("xslam_stop_rgb_stream");
        //        API.xslam_stop_rgb_stream();              
        //    }
        //}
    }

    void SavePNG()
    {
        string time = DateTime.Now.ToString("yyyyMMddHHmmssfff");
        string path = GetAndroidExternalStoragePath();
        
        byte[] bytes = tex.EncodeToPNG();
        string tempPath1 = Path.Combine(path, time + "_texture.png");
        File.WriteAllBytes(tempPath1, bytes);

        byte[] destination = new byte[pixel32.Length * Marshal.SizeOf(typeof(Color32))];
        Marshal.Copy(pixelPtr, destination, 0, destination.Length);
        string tempPath2 = Path.Combine(path, time + "_.png");
        File.WriteAllBytes(tempPath2, bytes);
    }

    private string GetAndroidExternalStoragePath()
    {
        if (Application.platform != RuntimePlatform.Android)
            return Application.persistentDataPath;

        var jc = new AndroidJavaClass("android.os.Environment");
        var path = jc.CallStatic<AndroidJavaObject>("getExternalStoragePublicDirectory",
            jc.GetStatic<string>("DIRECTORY_DCIM"))
            .Call<string>("getAbsolutePath");
        return path;
    }

    void OnApplicationQuit()
    {
        Debug.Log("OnApplicationQuit");
        newThread.Abort();
        //Free handle
        pixelHandle.Free();
    }
}
