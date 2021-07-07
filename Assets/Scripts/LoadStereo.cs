using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;

public class LoadStereo : MonoBehaviour
{
    private Texture2D tex = null;
    
    private Vector2[] leftPoints;
    private Vector2[] rightPoints;
	    
    // Start is called before the first frame update
    void Start()
    {
    	int max = API.xslam_get_stereo_max_points();
		leftPoints = new Vector2[max];
		rightPoints = new Vector2[max];
    }

    private Color32[] pixel32;
    private GCHandle pixelHandle;
    private IntPtr pixelPtr;
    
    // Update is called once per frame
    void Update()
    {
    	if( API.xslam_ready() ){
    	
    		int width = API.xslam_get_stereo_width();
    		int height = API.xslam_get_stereo_height();
			int size = width * height;
    		
    		    		
    		
    		if( width > 0 && height > 0 && size > 0){				
			
				if( !tex ){
					Debug.Log("Create STEREO texture " + width + "x" + height);
					TextureFormat format = TextureFormat.RGBA32;
		    		tex = new Texture2D(width, height, format, false);
		    		
		    		
			        pixel32 = tex.GetPixels32();
        			pixelHandle = GCHandle.Alloc(pixel32, GCHandleType.Pinned);
        			pixelPtr = pixelHandle.AddrOfPinnedObject();
        			
        			GetComponent<Renderer>().material.mainTexture = tex;
				}
			
                if(GameObject.Find("TogglePanel").GetComponent<StreamToggle>().StereoOn()) {
                    if( API.xslam_get_left_image(pixelPtr, tex.width, tex.height) ){
                        //Update the Texture2D with array updated in C++
                        tex.SetPixels32(pixel32);
                        tex.Apply();
                    }else{
                        Debug.Log("Invalid Stereo texture");
                    }
                }
				
				/*int leftPointsCount = 0;
				if( API.xslam_get_left_points( leftPoints, ref leftPointsCount) ){
					Debug.Log("leftPointsCount " + leftPointsCount);
				}*/
			}		    
	    }
    }
    
    void OnApplicationQuit()
    {
        //Free handle
        pixelHandle.Free();
    }
}
