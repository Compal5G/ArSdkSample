using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Runtime.InteropServices;

using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.ImgcodecsModule;

using OpenCVForUnity.Calib3dModule;
using OpenCVForUnity.ArucoModule;

using System.IO;
using System.Text.RegularExpressions;
using Apal_CalibrationCore;


public class CalibrationAnker : MonoBehaviour
{
    private Texture2D tex = null;
    private Color32[] pixel32;
    private GCHandle pixelHandle;
    private IntPtr pixelPtr;
    private long rgbTimestamp = 0;
    private int lastWidth = 0;
    private int lastHeight = 0;
    private float[] HeadCalibValueR = new float[9];
    private float[] HeadCalibValueT = new float[3];
    private double[] RGBCalibValue = new double[9];
    private bool hasValue = false;
    private Material mat;

    private int alpha = 150;

    private bool applyEstimationPose = true;
    //public ArUcoDictionary dictionaryId = ArUcoDictionary.DICT_7X7_250;
    //public Camera arCamera;
    //public float markerLength = 0.161f;
    private bool showRejectedCorners = false;
    [SerializeField]
    private Transform MarkQuad;
    //[SerializeField]
    //private Transform[] ARGameObj;

    int m_iClickCount = 0;
    bool m_bClicked = false;
    float m_fSingleClicktime = 0;
    float m_fDoubleClicktime = 0;
    float m_fClickDelay = 0.5f;
    Quaternion qxyz;
    public enum ArUcoDictionary
    {
        DICT_4X4_50 = Aruco.DICT_4X4_50,
        DICT_4X4_100 = Aruco.DICT_4X4_100,
        DICT_4X4_250 = Aruco.DICT_4X4_250,
        DICT_4X4_1000 = Aruco.DICT_4X4_1000,
        DICT_5X5_50 = Aruco.DICT_5X5_50,
        DICT_5X5_100 = Aruco.DICT_5X5_100,
        DICT_5X5_250 = Aruco.DICT_5X5_250,
        DICT_5X5_1000 = Aruco.DICT_5X5_1000,
        DICT_6X6_50 = Aruco.DICT_6X6_50,
        DICT_6X6_100 = Aruco.DICT_6X6_100,
        DICT_6X6_250 = Aruco.DICT_6X6_250,
        DICT_6X6_1000 = Aruco.DICT_6X6_1000,
        DICT_7X7_50 = Aruco.DICT_7X7_50,
        DICT_7X7_100 = Aruco.DICT_7X7_100,
        DICT_7X7_250 = Aruco.DICT_7X7_250,
        DICT_7X7_1000 = Aruco.DICT_7X7_1000,
        DICT_ARUCO_ORIGINAL = Aruco.DICT_ARUCO_ORIGINAL,
    }

    void Start()
    {
        //var qx = Quaternion.AngleAxis(-90, Vector3.right);
        //var qy = Quaternion.AngleAxis(180, Vector3.up);
        //var qz = Quaternion.AngleAxis(180, Vector3.forward);
        //qxyz = qx * qy * qz;
        //
        //for (int i = 0; i < ARGameObj.Length; i++)
        //{
        //    if (ARGameObj[i])
        //    {
        //        ARGameObj[i].localScale = Vector3.zero;
        //    }
        //}

        // use uvc rgb
        //API.xslam_set_rgb_source( 0 );

        // set to 720p
        //API.xslam_set_rgb_resolution( 1 );
    }
    //對相機取得的圖片進行2D Homography轉換
    public void OpenCVHomography()
    {
        Mat inputMat = new Mat(tex.height, tex.width, CvType.CV_8UC4);
        Utils.texture2DToMat(tex, inputMat, true, 1);

        Mat outputMat = new Mat(1280, 720, CvType.CV_8UC4);
        Mat perspectiveTransform = new Mat(3, 3, CvType.CV_64FC1);
        perspectiveTransform.put(0,0, 1.355878779283638, -0.02591061968965643, -607.5356800237507,
        0.01867744854896194, 1.363350078538568, -377.5589247155547,
        -1.06168313224652e-05, -4.47573951089898e-06, 0.9997850073703513);
        Imgproc.warpPerspective(inputMat, outputMat, perspectiveTransform, new Size(1280, 720));
        Texture2D texThisLeft = new Texture2D(outputMat.cols(), outputMat.rows(), TextureFormat.RGBA32, false);
        Utils.matToTexture2D(outputMat, texThisLeft, true, 1);
        GameObject.Find("RGBPlaneOppoLeft").GetComponent<MeshRenderer>().material.mainTexture = texThisLeft;

        perspectiveTransform.put(0, 0, 1.369831985432881, -0.02842386366966552, -704.327081931564,
        0.01207212075557339, 1.372866877440848, -371.806804424705,
        -4.412241088259502e-06, -1.472150541641732e-05, 1.013364995174869);
        Imgproc.warpPerspective(inputMat, outputMat, perspectiveTransform, new Size(1280, 720));
        Texture2D texThisRight = new Texture2D(outputMat.cols(), outputMat.rows(), TextureFormat.RGBA32, false);
        Utils.matToTexture2D(outputMat, texThisRight, true, 1);
        GameObject.Find("RGBPlaneOppoRight").GetComponent<MeshRenderer>().material.mainTexture = texThisRight;
    }

    private bool SingleClick()
    {     
        if (Input.GetKeyDown(KeyCode.JoystickButton0))
        {  
            if (m_bClicked == false)
            {
                m_bClicked = true;
                m_fSingleClicktime = Time.time;
            }      
        }
        if (m_bClicked == true && Input.GetKeyUp(KeyCode.JoystickButton0) && Time.time - m_fSingleClicktime < m_fClickDelay)
        {
            m_bClicked = false;
            m_fSingleClicktime = 0;
            return true;
        }
        else if (Time.time - m_fSingleClicktime > 1)
            m_bClicked = false;
        return false;
    }

    void Update()
    {
        //滑鼠輸入控制
        if(SingleClick())
        {
            GetHeadCalibrationValue();
            GetComponent<CalibrationCore>().SetTexture(tex);
            GetComponent<CalibrationCore>().DetectMarkers(RGBCalibValue,HeadCalibValueR,HeadCalibValueT);
            MarkQuad.localScale = new Vector3(0, 0, 0);
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

                        MarkQuad.GetComponent<Renderer>().material.mainTexture = tex;
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
                        //DetectMarkers();
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

    void OnApplicationQuit()
    {
        //Free handle
        pixelHandle.Free();
    }

    private void GetHeadCalibrationValue()
    {
        if (hasValue)
            return;
        for (int i = 0; i < 9; i++)
        {
            HeadCalibValueR[i] = SvrManager.Instance.GetSvrHeadCalibrationValueR_CH(i);
        }

        for (int i = 0; i < 3; i++)
        {
            HeadCalibValueT[i] = SvrManager.Instance.GetSvrHeadCalibrationValueT_CH(i);
        }

        for (int i = 0; i < 9; i++)
        {
            RGBCalibValue[i] = SvrManager.Instance.GetSvrRGBCalibrationValue(i);
        }
        hasValue = true;
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            AndroidJavaObject activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
            activity.Call("finish");
        }
    }
}
