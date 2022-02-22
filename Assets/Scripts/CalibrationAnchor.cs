using UnityEngine;
using System;
using System.Runtime.InteropServices;
using OpenCVForUnity.ArucoModule;
using Apal_CalibrationCore;


public class CalibrationAnchor : MonoBehaviour
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

    [SerializeField]
    private CalibrationCore calibrationCore;
    [SerializeField]
    private Transform MarkQuad;

    bool m_bClicked = false;
    float m_fSingleClicktime = 0;
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
        if (SingleClick())
        {
            GetHeadCalibrationValue();
            calibrationCore.DetectMarkers();

            for (int i = 0; i < calibrationCore.detectARModel.Length; i++)
            {
                if (calibrationCore.detectARModel[i].ARTransform)
                {
                    calibrationCore.detectARModel[i].ARModel.SetActive(true);
                    calibrationCore.detectARModel[i].IsDetected = true;
                }
            }
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
        calibrationCore.SetTexture(tex);
        calibrationCore.InitMatrix(RGBCalibValue, HeadCalibValueR, HeadCalibValueT);
        MarkQuad.localScale = new Vector3(0, 0, 0);
        hasValue = true;
    }

    //private void OnApplicationPause(bool pause)
    //{
    //    if (pause)
    //    {
    //        AndroidJavaObject activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
    //        activity.Call("finish");
    //    }
    //}
}
