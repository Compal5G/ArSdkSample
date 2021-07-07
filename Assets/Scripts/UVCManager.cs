#define USE_VUFORIA

using UnityEngine;

#if USE_VUFORIA
using Vuforia;
#endif
 
/// <summary>
/// 用于获得调用 USBCamera 摄像头的权限
/// </summary>
public class UVCManager : MonoBehaviour
{
    public static void InitVuforia()
    {
#if (UNITY_ANDROID && USE_VUFORIA)
        Debug.Log("loading libUVCDriver.so");
        bool driverLibrarySet = false;
        driverLibrarySet = VuforiaUnity.SetDriverLibrary("libUVCDriver.so");
 
        Debug.Log("loading libUVCDriver.so end");
        if (driverLibrarySet)
        {
            Debug.Log("acquring usb permission");
            // Load your applications scene here 
            // InitAndLoadScene(VUFORIA_DRIVER_CAMERA_SCENE_INDEX);
 
            // The application needs to ask for USB permissions to run the USB camera
            // this is done after the driver is loaded. We call a method in the UVC driver
            // Java code to request permissions, passing in the Unity app's activity.
            AndroidJavaClass unityJC = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject unityActivity = unityJC.GetStatic<AndroidJavaObject>("currentActivity");
 
            AndroidJavaClass usbControllerJC = new AndroidJavaClass("com.vuforia.samples.uvcDriver.USBController");
            usbControllerJC.CallStatic("requestUSBPermission", unityActivity);
            Debug.Log("acquring usb permission end");

            VuforiaRuntime.Instance.InitVuforia();
        }
        else
        {
            Debug.Log("Failed to initialize the UVC driver - defaulting to the standard scene");
 
            // Fall back to the in-built camera
        }
#endif
    }
 
    // Start is called before the first frame update
    void Start()
    {
        
    }
 
    
}
