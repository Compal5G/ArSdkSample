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

    public Material mat;

    public int alpha = 150;

    public bool applyEstimationPose = true;
    public ArUcoDictionary dictionaryId = ArUcoDictionary.DICT_6X6_250;
    public Camera arCamera;
    public float markerLength = 0.161f;
    public bool shouldMoveARCamera = true;
    public GameObject arGameObject;
    public bool showRejectedCorners = false;
    public Text CubeFixed;

    [SerializeField]
    private Transform MarkQuad;
    [SerializeField]
    private Transform ARGameModel;

    Texture2D texture;

    int m_iClickCount = 0;
    bool m_bClicked = false;
    float m_fSingleClicktime = 0;
    float m_fDoubleClicktime = 0;
    float m_fClickDelay = 0.5f;

    private bool m_bMarkQuadHide = false;
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

    

    private bool DoubleClick()
    {
        //return false;
        if (Input.GetKeyDown(KeyCode.JoystickButton0))
        {
            m_iClickCount++;
            if (m_iClickCount == 1)
                m_fDoubleClicktime = Time.time;
        }
        if (m_iClickCount > 1 && Time.time - m_fDoubleClicktime < m_fClickDelay)
        {
            m_iClickCount = 0;
            m_fDoubleClicktime = 0;
            m_fSingleClicktime = 0;
            return true;
        }
        else if (Time.time - m_fDoubleClicktime > 1)
            m_iClickCount = 0;
        return false;
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

    private void ClearText()
    {
        CubeFixed.text = "";
    }

    void Update()
    {
        //滑鼠輸入控制
        if(SingleClick())
        {
            DetectMarkers();
            MarkQuad.localScale = new Vector3(0, 0, 0);
            m_bMarkQuadHide = true;
            ARGameModel.localScale = new Vector3(0, 0, 0);
        }
        else if (DoubleClick())
        {
            ARGameModel.localScale = arGameObject.transform.localScale;
            ARGameModel.position = arGameObject.transform.position;
            ARGameModel.rotation = arGameObject.transform.rotation;
            arGameObject.transform.localScale = new Vector3(0,0,0);
            CubeFixed.text = "Cube Fixed";
            Invoke("ClearText",3.0f);
        }
        else if (Input.GetMouseButtonDown(1))
        {
            m_bMarkQuadHide = !m_bMarkQuadHide;
            if (m_bMarkQuadHide)
                MarkQuad.localScale = new Vector3(0, 0, 0);
            else
                MarkQuad.localScale = new Vector3(0.04f, 0.0225f, 1.0f);
        }


        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            Vector3 RGBPlaneOppoVector3 = MarkQuad.localPosition;
            MarkQuad.localPosition = new Vector3(RGBPlaneOppoVector3.x, RGBPlaneOppoVector3.y, RGBPlaneOppoVector3.z + 0.0005f);
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            Vector3 RGBPlaneOppoVector3 = MarkQuad.localPosition;
            MarkQuad.localPosition = new Vector3(RGBPlaneOppoVector3.x, RGBPlaneOppoVector3.y, RGBPlaneOppoVector3.z - 0.0005f);
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

    private void DetectMarkers()
    {
#if !UNITY_EDITOR
        GetHeadCalibrationValue();
        Mat rgbMat = new Mat(tex.height, tex.width, CvType.CV_8UC3);
        Utils.texture2DToMat(tex, rgbMat, true, 1);
#else
        Mat rgbMat = Imgcodecs.imread(@"D:\\aruco_sample.png", 1);
#endif
        texture = new Texture2D(rgbMat.cols(), rgbMat.rows(), TextureFormat.RGBA32, false);
        Debug.Log("imgMat dst ToString " + rgbMat.ToString());
        Debug.Log("rgbMat.cols " + rgbMat.cols() + " rgbMat.rows " + rgbMat.rows());
        Debug.Log("Screen.height " + Screen.height + " Screen.width " + Screen.width);
        float width = rgbMat.width();
        float height = rgbMat.height();

        float imageSizeScale = 1.0f;
        float widthScale = (float)Screen.width / width;
        float heightScale = (float)Screen.height / height;
        if (widthScale < heightScale)
        {
            imageSizeScale = (float)Screen.height / (float)Screen.width;
        }
        ResetObjectTransform();

        // set camera parameters.
        int max_d = (int)Mathf.Max(width, height);
        double fx = max_d;
        double fy = max_d;
        double cx = width / 2.0f;
        double cy = height / 2.0f;
        Mat camMatrix = new Mat(3, 3, CvType.CV_64FC1);

#if !UNITY_EDITOR

        camMatrix.put(0, 0, RGBCalibValue[0]);
        camMatrix.put(0, 1, RGBCalibValue[1]);
        camMatrix.put(0, 2, RGBCalibValue[2]);
        camMatrix.put(1, 0, RGBCalibValue[3]);
        camMatrix.put(1, 1, RGBCalibValue[4]);
        camMatrix.put(1, 2, RGBCalibValue[5]);
        camMatrix.put(2, 0, RGBCalibValue[6]);
        camMatrix.put(2, 1, RGBCalibValue[7]);
        camMatrix.put(2, 2, RGBCalibValue[8]);
#else
        camMatrix.put(0, 0, 1.4056614744087947e+03);
        camMatrix.put(0, 1, 0);
        camMatrix.put(0, 2, 9.6027175454744895e+02);
        camMatrix.put(1, 0, 0);
        camMatrix.put(1, 1, 1.4129000939180419e+03);
        camMatrix.put(1, 2, 5.2808188122521926e+02);
        camMatrix.put(2, 0, 0);
        camMatrix.put(2, 1, 0);
        camMatrix.put(2, 2, 1.0f);
#endif

        MatOfDouble distCoeffs = new MatOfDouble(0, 0, 0, 0);

        // calibration camera matrix values.
        Size imageSize = new Size(width * imageSizeScale, height * imageSizeScale);
        double apertureWidth = 0;
        double apertureHeight = 0;
        double[] fovx = new double[1];
        double[] fovy = new double[1];
        double[] focalLength = new double[1];
        Point principalPoint = new Point(0, 0);
        double[] aspectratio = new double[1];

        Calib3d.calibrationMatrixValues(camMatrix, imageSize, apertureWidth, apertureHeight, fovx, fovy, focalLength, principalPoint, aspectratio);

        // To convert the difference of the FOV value of the OpenCV and Unity. 
        double fovXScale = (2.0 * Mathf.Atan((float)(imageSize.width / (2.0 * fx)))) / (Mathf.Atan2((float)cx, (float)fx) + Mathf.Atan2((float)(imageSize.width - cx), (float)fx));
        double fovYScale = (2.0 * Mathf.Atan((float)(imageSize.height / (2.0 * fy)))) / (Mathf.Atan2((float)cy, (float)fy) + Mathf.Atan2((float)(imageSize.height - cy), (float)fy));

        // Display objects near the camera.
        arCamera.nearClipPlane = 0.01f;

        Mat ids = new Mat();
        List<Mat> corners = new List<Mat>();
        List<Mat> rejectedCorners = new List<Mat>();
        Mat rvecs = new Mat();
        Mat tvecs = new Mat();
        Mat rotMat = new Mat(3, 3, CvType.CV_64FC1);

        DetectorParameters detectorParams = DetectorParameters.create();
        Dictionary dictionary = Aruco.getPredefinedDictionary((int)dictionaryId);

        // detect markers.
        Aruco.detectMarkers(rgbMat, dictionary, corners, ids, detectorParams, rejectedCorners, camMatrix, distCoeffs);
        if (ids.total() > 0)
        {
            Aruco.drawDetectedMarkers(rgbMat, corners, ids, new Scalar(0, 255, 0));
            // estimate pose.
            if (applyEstimationPose)
            {
                Aruco.estimatePoseSingleMarkers(corners, markerLength, camMatrix, distCoeffs, rvecs, tvecs);
                for (int i = 0; i < ids.total(); i++)
                {
                    using (Mat rvec = new Mat(rvecs, new OpenCVForUnity.CoreModule.Rect(0, i, 1, 1)))
                    using (Mat tvec = new Mat(tvecs, new OpenCVForUnity.CoreModule.Rect(0, i, 1, 1)))
                    {
                        // In this example we are processing with RGB color image, so Axis-color correspondences are X: blue, Y: green, Z: red. (Usually X: red, Y: green, Z: blue)
                        Calib3d.drawFrameAxes(rgbMat, camMatrix, distCoeffs, rvec, tvec, markerLength * 0.5f);
                    }

                    // This example can display the ARObject on only first detected marker.
                    if (i == 0)
                    {
                        // Get translation vector
                        double[] tvecArr = tvecs.get(i, 0);
                        // Get rotation vector
                        double[] rvecArr = rvecs.get(i, 0);
                        Mat rvec = new Mat(3, 1, CvType.CV_64FC1);
                        rvec.put(0, 0, rvecArr);

                        // Convert rotation vector to rotation matrix.
                        Calib3d.Rodrigues(rvec, rotMat);
                        double[] rotMatArr = new double[rotMat.total()];
                        rotMat.get(0, 0, rotMatArr);

                        // Convert OpenCV camera extrinsic parameters to Unity Matrix4x4.
                        Matrix4x4 transformationM = new Matrix4x4(); // from OpenCV
                        transformationM.SetRow(0, new Vector4((float)rotMatArr[0], (float)rotMatArr[1], (float)rotMatArr[2], (float)tvecArr[0]));
                        transformationM.SetRow(1, new Vector4((float)rotMatArr[3], (float)rotMatArr[4], (float)rotMatArr[5], (float)tvecArr[1]));
                        transformationM.SetRow(2, new Vector4((float)rotMatArr[6], (float)rotMatArr[7], (float)rotMatArr[8], (float)tvecArr[2]));
                        transformationM.SetRow(3, new Vector4(0, 0, 0, 1));


                        Matrix4x4 transformation_RGB_to_Head_M = new Matrix4x4(); // from OpenCV RGB camera space to head space

#if !UNITY_EDITOR                      
                        transformation_RGB_to_Head_M.SetRow(0, new Vector4(HeadCalibValueR[0], HeadCalibValueR[1], HeadCalibValueR[2], HeadCalibValueT[0]));
                        transformation_RGB_to_Head_M.SetRow(1, new Vector4(HeadCalibValueR[3], HeadCalibValueR[4], HeadCalibValueR[5], HeadCalibValueT[1]));
                        transformation_RGB_to_Head_M.SetRow(2, new Vector4(HeadCalibValueR[6], HeadCalibValueR[7], HeadCalibValueR[8], HeadCalibValueT[2]));
                        
                        
                        transformation_RGB_to_Head_M.SetRow(3, new Vector4(0, 0, 0, 1));
#else
                        transformation_RGB_to_Head_M.SetRow(0, new Vector4(9.9989647315145669e-01f, -1.4067075219682145e-04f, 1.4388300490943334e-02f, 9.5930311914902160e-04f));
                        transformation_RGB_to_Head_M.SetRow(1, new Vector4(-3.4648524379076947e-04f, 9.9942687499944038e-01f, 3.3849689464288596e-02f, -1.2421289754088825e-02f));
                        transformation_RGB_to_Head_M.SetRow(2, new Vector4(-1.4384815857494980e-02f, -3.3851170446417528e-02f, 9.9932335874438238e-01f, 3.9562007474565251e-02f));
                        transformation_RGB_to_Head_M.SetRow(3, new Vector4(0, 0, 0, 1));
#endif


                        Matrix4x4 invertYM = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, -1, 1));
                        Matrix4x4 ARM = invertYM * transformation_RGB_to_Head_M * transformationM * invertYM;

                        if (shouldMoveARCamera)
                        {
                            ARM = arGameObject.transform.localToWorldMatrix * ARM.inverse;
                            ARUtils.SetTransformFromMatrix(arCamera.transform, ref ARM);
                        }
                        else
                        {
                            ARM = arCamera.transform.localToWorldMatrix * ARM;
                            
                            ARUtils.SetTransformFromMatrixForMarker(arGameObject.transform, ref ARM);

                            arGameObject.transform.localScale = new Vector3(0.16f, 0.16f, 0.16f);
                        }     
                    }
                }
            }
        }

        if (showRejectedCorners && rejectedCorners.Count > 0)
            Aruco.drawDetectedMarkers(rgbMat, rejectedCorners, new Mat(), new Scalar(255, 0, 0));
        Utils.matToTexture2D(rgbMat, texture, true, 1);
        MarkQuad.GetComponent<MeshRenderer>().material.mainTexture = texture;
    }

    private void ResetObjectTransform()
    {
        // reset AR object transform.
        Matrix4x4 i = Matrix4x4.identity;
        ARUtils.SetTransformFromMatrix(arCamera.transform, ref i);
        ARUtils.SetTransformFromMatrix(arGameObject.transform, ref i);
    }

    private void GetHeadCalibrationValue()
    {
        if (hasValue)
            return;
        for (int i = 0; i < 9; i++)
        {
            HeadCalibValueR[i] = SvrManager.Instance.GetSvrHeadCalibrationValueR_CH(i);
            //Debug.Log("LONGLONG UNITY HeadCalibValueR[" + i + "] = " + HeadCalibValueR[i]);
        }

        for (int i = 0; i < 3; i++)
        {
            HeadCalibValueT[i] = SvrManager.Instance.GetSvrHeadCalibrationValueT_CH(i);
            //Debug.Log("LONGLONG UNITY HeadCalibValueT[" + i + "] = " + HeadCalibValueT[i]);
        }

        for (int i = 0; i < 9; i++)
        {
            RGBCalibValue[i] = SvrManager.Instance.GetSvrRGBCalibrationValue(i);
            //Debug.Log("LONGLONG UNITY RGBCalibValue[" + i + "] = " + RGBCalibValue[i]);
        }
        hasValue = true;
    }
}
