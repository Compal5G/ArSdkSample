using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;

public class LoadTOF : MonoBehaviour, SvrManager.SvrEventListener
{
    private Texture2D tex = null;
    private bool isSVRReady = false;
	    
    // Start is called before the first frame update
    void Start()
    {
        SvrManager.Instance.AddEventListener (this);
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

    private Color32[] pixel32;
    private GCHandle pixelHandle;
    private IntPtr pixelPtr;
    
    // Update is called once per frame
    void Update()
    {
    	if( API.xslam_ready() ){
    	
    		int width = API.xslam_get_tof_width();
    		int height = API.xslam_get_tof_height();
    		
    		if( width > 0 && height > 0){
			
				if( !tex ){
					Debug.Log("Create TOF texture " + width + "x" + height);
					TextureFormat format = TextureFormat.RGBA32;
		    		tex = new Texture2D(width, height, format, false);
		    		
		    		
			        pixel32 = tex.GetPixels32();
        			pixelHandle = GCHandle.Alloc(pixel32, GCHandleType.Pinned);
        			pixelPtr = pixelHandle.AddrOfPinnedObject();
        			
        			GetComponent<Renderer>().material.mainTexture = tex;
				}
				
                if(isSVRReady) {
                    if( API.xslam_get_tof_image(pixelPtr, tex.width, tex.height) ){
                        //Update the Texture2D with array updated in C++
                        tex.SetPixels32(pixel32);
                        tex.Apply();
                    }else{
                        Debug.Log("Invalid TOF texture");
                    }
                }
							
				//Vector3[] cloudData = new Vector3[ width * height ];
				//if( API.xslam_get_cloud_data( cloudData) ){
				//	int c = Convert.ToInt32(width*0.5*(height+1));
				//	Vector3 v = cloudData[c];
				//	Debug.Log("Center of depth = " + v );
				//}else{
				//	Debug.Log("Failed to get CLOUD");
				//}				
			}
	    }
    }
    
    void OnApplicationQuit()
    {
        //Free handle
        try {
            pixelHandle.Free();
        } catch {}
    }
}
