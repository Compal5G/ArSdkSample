using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using Object = UnityEngine.Object;
using System.Collections;

public class LoadRGB : MonoBehaviour, SvrManager.SvrEventListener
{
    private Texture2D tex = null;
    private Color32[] pixel32;
    private GCHandle pixelHandle;
    private IntPtr pixelPtr;    
    private long rgbTimestamp = 0;
    private int lastWidth = 0;
    private int lastHeight = 0;
    private Thread newThread;
    private Thread newThread2;
    private Boolean isTextureReady = false;
    private Object thisLock = new Object();

    volatile bool keepThreadAlive = true;
    private bool isSVRReady = false;

    void Start()
    {
        // use uvc rgb
        //API.xslam_set_rgb_source( 0 );
        //Register for SvrEvents
        SvrManager.Instance.AddEventListener (this);
        // set to 720p
        newThread = new Thread(GetRGBFrame);
        newThread2 = new Thread(GetRGBFrame);
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
                isSVRReady = true;
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
                                //Update the Texture2D with array updated in C++
                                //tex.SetPixels32(pixel32);
                                //tex.Apply();
                            }
                            // else
                            // {
                            //     Debug.Log("Invalid texture");
                            // }
                        }
                        catch (Exception e)
                        {
                            Debug.LogException(e, this);
                            //yield return new WaitForSeconds(0.3f);
                            return;
                        }
                        //Debug.Log("thread E id " + Thread.CurrentThread.ManagedThreadId);
                    }
                }
            }
            Thread.Sleep(100);
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
                    Debug.Log("xslam_set_rgb_resolution");
                    API.xslam_set_rgb_resolution(1);
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
                        newThread2.Start();
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
                }
            }
        }
    }

    void OnApplicationQuit()
    {
        //Free handle
        newThread.Abort();
        newThread2.Abort();
        //newThread3.Abort();
        //StopAllCoroutines();
        pixelHandle.Free();
    }
}
