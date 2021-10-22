using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using UnityEditor;

using System;
using System.Runtime.InteropServices;
using System.Text;


public class XSlamCameraController : MonoBehaviour
{
    private int m_fd = -1;
    

    [Header("Movement Settings")]
    [Tooltip("Exponential boost factor on translation"), Range(0.05f, 25f)]
    public float boost = 1.0f;

    [Header("Origin Settings")]
    [Tooltip("Position of the origin")]
    public Vector3 positionOrigin = new Vector3(0.0f, 0.0f, 0.0f);
    //public Vector3 rotationOrigin = new Vector3(0.0f, -99.10201f, 0.0f);

	
	public enum SlamModes // your custom enumeration
	{
		Device = 0, 
		Host
	};
    [Header("SLAM")]
	public SlamModes slamMode = SlamModes.Device;  // this public var should appear as a drop down


	public enum CnnSources // your custom enumeration
	{
		Left = 0, 
		Right,
		RGB,
		TOF
	};
	
    [Header("CNN")]
    public string cnnModel = "";
    public string cnnDescriptor = "";
    public CnnSources cnnSource = CnnSources.Left;

    [Header("Custom Settings")]
    public bool enableGesture = true;
    public bool enableRGBFrame = true;
    public bool enableTOFFrame = true;
    public bool enableVuforia = true;

    void OnEnable()
    {
    
    }

    void Awake()
    {
        Debug.Log("XSlamCameraController.Awake()");
    }

    void Start()
    {
        Debug.Log("XSlamCameraController.Start()");
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
	}

    void Quit()
	{
	
	}
	
	public void setSlamMode( SlamModes mode )
	{
		slamMode = mode;
		API.xslam_slam_type( slamMode == SlamModes.Device ? 0 : 1 );
	}
    
	public void ResetSlam()
	{
		API.xslam_reset_slam();
	}
    
    void Update()
    {
        DetectWhichKeyDown();

        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        if( Input.GetKeyDown(KeyCode.R) )
        {
            ResetSlam();
        }
        
        if( Input.GetKeyDown(KeyCode.S) )
        {
            setSlamMode( slamMode == SlamModes.Device ? SlamModes.Host : SlamModes.Device );
        }

        if( m_fd < 0 )
        {
#if UNITY_ANDROID

            AndroidJavaClass cls = new AndroidJavaClass("com.xvisio.unity.XVisioSDKDemo");
            int fd = cls.CallStatic<int>("getFd");
            if (fd < 0)
            {
                Debug.Log("Failed to get fd");
                return;
            }
            Debug.Log("got fd=" + fd);

            m_fd = fd;
            //Must init gesture before call API.xslam_init_with_fd
            if (enableGesture)
            {
                try
                {
                    Debug.Log("init ges");
                    XvGesture.InitGes(fd);
                }
                catch (Exception e)
                {
                    print("error: " + e);
                }
            }

            Debug.Log("+init xvsdk");
            // bool ok = API.xslam_init_with_fd( m_fd );
            bool ok = API.xslam_init_components_with_fd(m_fd,
                    (int)(API.Component.TOF | API.Component.RGB |
                        API.Component.VSC));

            Debug.Log("-init xvsdk");

            if (!ok)
            {
                Debug.Log("Failed to init xvsdk with fd=" + m_fd);
                return;
            }
#else
			if( !API.xslam_init() ){
                Debug.Log("Failed to init slam");
                return;
            }
			m_fd = 1;
#endif

#if False
            Debug.Log("Test HID read");
            byte[] cmd = {0x02, 0xde, 0x78};
            byte[] rdata = API.HidWriteAndRead(cmd, cmd.Length);
            if (rdata != null) {
                int _temp = rdata[3];
                Debug.Log( "current temperature: " + _temp );
            }
#endif

            /*
            Debug.Log("Set CNN source: " + (int)cnnSource);
            if (!API.xslam_set_cnn_source((int)cnnSource))
            {
                Debug.Log("Failed to set CNN source");
            }
            Debug.Log("Set CNN descriptor: " + cnnDescriptor);
            if (!API.xslam_set_cnn_descriptor(cnnDescriptor))
            {
                Debug.Log("Failed to set CNN descriptor");
            }
            Debug.Log("Set CNN model: " + cnnModel);
            if (!API.xslam_set_cnn_model(cnnModel))
            {
                Debug.Log("Failed to set CNN model");
            }
            */

            // Stop streams due to firmware not stable
            // Debug.Log("stop streams");

            // if (enableRGBFrame)
            //     API.xslam_stop_rgb_stream();

            // if (enableTOFFrame)
            //     API.xslam_stop_tof_stream();

            // Start image streams
            Debug.Log("start streams");
            if (enableRGBFrame)
                API.xslam_start_rgb_stream();

            if (enableTOFFrame)
                API.xslam_start_tof_stream();

            Debug.LogFormat("set slam type: {0}", slamMode == SlamModes.Device ? "edge" : "mixed");
            API.xslam_slam_type(slamMode == SlamModes.Device ? 0 : 1);

            if (enableVuforia)
            {
                Debug.Log("init vuforia");
                UVCManager.InitVuforia();
            }
        }

        /*
        Matrix4x4 mt = Matrix4x4.identity;
        long ts = 0;
        int status = 0;
        if (!API.xslam_get_transform(ref mt, ref ts, ref status))
        {
            mt = Matrix4x4.identity;
        }
        else
        {
            //Debug.Log( mt );
            //Debug.Log( ts );
        }

        //int device_state = status >> 4;
        //if( device_state == 2 ||  device_state == 4 || device_state == 5 )
        {
            Quaternion rot = mt.rotation;
            transform.position = new Vector3(mt[0, 3], mt[1, 3], mt[2, 3]) * boost + positionOrigin;
            transform.rotation = new Quaternion(rot.x, -rot.y, rot.z, -rot.w);
            transform.localScale = new Vector3(1, 1, 1); //= mt[0].lossyScale;
        }
        */
    }

    void OnApplicationQuit()
    {
        if (enableRGBFrame)
            API.xslam_stop_rgb_stream();

        if (enableTOFFrame)
            API.xslam_stop_tof_stream();
    }

    void DetectWhichKeyDown()
    {
        foreach (KeyCode vKey in System.Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(vKey))
            {
                string value = " ======== " + vKey + " is pressed!";
                Debug.Log(value);
            }
        }
    }

}
