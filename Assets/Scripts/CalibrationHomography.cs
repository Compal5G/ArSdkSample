using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Runtime.InteropServices;

using OpenCVForUnity;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.ImgcodecsModule;

using System.IO;

public class CalibrationHomography : MonoBehaviour
{
    private Texture2D tex = null;
    private Color32[] pixel32;
    private GCHandle pixelHandle;
    private IntPtr pixelPtr;
    private long rgbTimestamp = 0;
    private int lastWidth = 0;
    private int lastHeight = 0;

    private float[] ColorCalibrationR = new float[9];
    private float[] ColorCalibrationL = new float[9];
    private bool hasValue = false;

    private Mat ColorR = new Mat(3, 3, CvType.CV_64FC1);
    private Mat ColorL = new Mat(3, 3, CvType.CV_64FC1);
    private Mat inputMat;
    private Mat outputMat = new Mat(1280, 720, CvType.CV_8UC4);

    [SerializeField]
    private Transform RGBPlaneR;
    [SerializeField]
    private Transform RGBPlaneL;

    public Material mat;

    public int alpha = 150;

    void Start()
    {
        
        // use uvc rgb
        //API.xslam_set_rgb_source( 0 );

        // set to 720p
        //API.xslam_set_rgb_resolution( 1 );
    }
    //對相機取得的圖片進行2D Homography轉換
    public void OpenCVHomography()
    {
        
        Utils.texture2DToMat(tex, inputMat, true, 1);

        Imgproc.warpPerspective(inputMat, outputMat, ColorL, new Size(1280, 720));
        Texture2D texThisLeft = new Texture2D(outputMat.cols(), outputMat.rows(), TextureFormat.RGBA32, false);
        Utils.matToTexture2D(outputMat, texThisLeft, true, 1);
        RGBPlaneL.GetComponent<MeshRenderer>().material.mainTexture = texThisLeft;

        Imgproc.warpPerspective(inputMat, outputMat, ColorR, new Size(1280, 720));
        Texture2D texThisRight = new Texture2D(outputMat.cols(), outputMat.rows(), TextureFormat.RGBA32, false);
        Utils.matToTexture2D(outputMat, texThisRight, true, 1);
        RGBPlaneR.GetComponent<MeshRenderer>().material.mainTexture = texThisRight;
    }

    void Update()
    {
        //滑鼠輸入控制
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.JoystickButton0))
        {
            GetHeadCalibrationValue();
            OpenCVHomography();
        }    
        //if (hasValue)
        //{
        //    OpenCVHomography();
        //}

        if( Input.GetAxis("Mouse ScrollWheel") > 0 )
        {
            Vector3 RGBPlaneOppoLeftVector3 = RGBPlaneL.localPosition;
            RGBPlaneL.transform.localPosition = new Vector3(RGBPlaneOppoLeftVector3.x, RGBPlaneOppoLeftVector3.y, RGBPlaneOppoLeftVector3.z + 0.0005f);
            Vector3 RGBPlaneOppoRightVector3 = RGBPlaneR.localPosition;
            RGBPlaneR.localPosition = new Vector3(RGBPlaneOppoRightVector3.x, RGBPlaneOppoRightVector3.y, RGBPlaneOppoRightVector3.z + 0.0005f);
        }
            
        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            Vector3 RGBPlaneOppoLeftVector3 = RGBPlaneL.localPosition;
            RGBPlaneL.localPosition = new Vector3(RGBPlaneOppoLeftVector3.x, RGBPlaneOppoLeftVector3.y, RGBPlaneOppoLeftVector3.z - 0.0005f);
            Vector3 RGBPlaneOppoRightVector3 = RGBPlaneR.localPosition;
            RGBPlaneR.localPosition = new Vector3(RGBPlaneOppoRightVector3.x, RGBPlaneOppoRightVector3.y, RGBPlaneOppoRightVector3.z - 0.0005f);
        }
        //////
        if (API.xslam_ready())
        {
            int width = API.xslam_get_rgb_width();
            int height = API.xslam_get_rgb_height();

            if (width > 0 && height > 0)
            {

                if (lastWidth != width || lastHeight != height)
                {
                    try
                    {
                        double r = 1.0;//0.25;
                        if (width < 1280 && height < 720)
                        {
                            r = 1.0;
                        }
                        int w = (int)(width * r);
                        int h = (int)(height * r);
                        Debug.Log("Create RGB texture " + w + "x" + h);
                        TextureFormat format = TextureFormat.RGBA32;
                        tex = new Texture2D(w, h, format, false);

                        try
                        {
                            pixelHandle.Free();
                        }
                        catch { }
                        pixel32 = tex.GetPixels32();
                        pixelHandle = GCHandle.Alloc(pixel32, GCHandleType.Pinned);
                        pixelPtr = pixelHandle.AddrOfPinnedObject();

                        GetComponent<Renderer>().material.mainTexture = tex;
                        if (mat != null)
                        {
                            //讓其他平面的材質也獲得影像
                            mat.mainTexture = tex;
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e, this);
                        return;
                    }

                    lastWidth = width;
                    lastHeight = height;
                }

                try
                {
                    
                    if (API.xslam_get_rgb_image_RGBA(pixelPtr, tex.width, tex.height, ref rgbTimestamp))
                    {
                        //Update the Texture2D with array updated in C++
                        tex.SetPixels32(pixel32);
                        tex.Apply();
                    }
                    else
                    {
                        Debug.Log("Invalid texture");
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

    private void GetHeadCalibrationValue()
    {
        if (hasValue)
            return;

        RGBPlaneL.transform.localPosition = new Vector3(0.0015f, 0, 0.256f);
        RGBPlaneR.localPosition = new Vector3(0.0015f, 0, 0.256f);

        for (int i = 0; i < 9; i++)
        {
            ColorCalibrationR[i] = (float)SvrManager.Instance.GetSvrColorCalibrationValueR(i);
            Debug.Log("LONGLONG UNITY ColorCalibrationR[" + i + "] = " + ColorCalibrationR[i]);
        }

        for (int i = 0; i < 9; i++)
        {
            ColorCalibrationL[i] = (float)SvrManager.Instance.GetSvrColorCalibrationValueL(i);
            Debug.Log("LONGLONG UNITY ColorCalibrationL[" + i + "] = " + ColorCalibrationL[i]);
        }

        inputMat = new Mat(tex.height, tex.width, CvType.CV_8UC4);

        ColorL = new Mat(3, 3, CvType.CV_64FC1);
        ColorL.put(0, 0,
        ColorCalibrationL[0], ColorCalibrationL[1], ColorCalibrationL[2],
        ColorCalibrationL[3], ColorCalibrationL[4], ColorCalibrationL[5],
        ColorCalibrationL[6], ColorCalibrationL[7], ColorCalibrationL[8]);

        ColorR = new Mat(3, 3, CvType.CV_64FC1);
        ColorR.put(0, 0,
        ColorCalibrationR[0], ColorCalibrationR[1], ColorCalibrationR[2],
        ColorCalibrationR[3], ColorCalibrationR[4], ColorCalibrationR[5],
        ColorCalibrationR[6], ColorCalibrationR[7], ColorCalibrationR[8]);

        hasValue = true;
    }

    void OnApplicationQuit()
    {
        //Free handle
        pixelHandle.Free();
    }
}
