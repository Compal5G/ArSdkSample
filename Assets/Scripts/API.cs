using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Runtime.InteropServices;

public class API : MonoBehaviour
{

	// ** Struct **

    /**
     * @brief Quaternion structure
     */
    public struct Quaternion
    {
        public double x;
        public double y;
        public double z;
        public double w;
    };

    /**
     * @brief 3DOF structure
     */
    public struct Orientation {
        public long hostTimestamp; //!<Timestamp in µs read on host
        public long deviceTimestamp; //!<Timestamp in µs read on the device
        public Quaternion quaternion; //!< Absolute quaternion (3DoF)
        public double roll; //!< Absolute roll euler angle (3DoF)
        public double pitch; //!< Absolute pitch euler angle (3DoF)
        public double yaw; //!< Absolute yaw euler angle (3DoF)

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public double[] angularVelocity; //!< Instantaneous angular velocity (radian/second)
    };

    /**
     * @brief Rotation and translation structure
     */
    [StructLayout(LayoutKind.Sequential)]
    public struct ctransform
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
        public double[] rotation; //!< Rotation matrix (row major)

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public double[] translation; //!< Translation vector
    };

    /**
     * @brief Polynomial Distortion Model
     */
    public struct pdm
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)]
        public double[] K;
    /**
        Projection and raytrace formula can be found here:
        https://docs.opencv.org/3.4.0/d4/d94/tutorial_camera_calibration.html

        K[0] : fx
        K[1] : fy
        K[2] : u0
        K[3] : v0
        K[4] : k1
        K[5] : k2
        K[6] : p1
        K[7] : p2
        K[8] : k3
        K[9] : image width
        K[10] : image height
    */
    };

    /**
     * @brief Unified camera model
     */
    public struct unified
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
        public double[] K;
    /**
      Projection and raytrace formula can be found here:
      1.  C. Geyer and K. Daniilidis, “A unifying theory for central panoramic systems and practical applications,” in Proc. 6th Eur. Conf. Comput. Vis.
    II (ECCV’00), Jul. 26, 2000, pp. 445–461
      or
      2. "J.P. Barreto. General central projection systems, modeling, calibration and visual
    servoing. Ph.D., University of Coimbra, 2003". Section 2.2.2.

      K[0] : fx
      K[1] : fy
      K[2] : u0
      K[3] : v0
      K[4] : xi
      K[5] : image width
      K[6] : image height

      More details,
      Projection:
        The simplest camera model is represented by projection relation:    p = 1/z K X
        where p=(u v)^T is an image point, X = (x y z)^T is a spatial point to be projected
        and K is a projection matrix: K = (fx 0 u0; 0 fy v0).

        The distortion model is added in the following manner.
        First we project all the points onto the unit sphere S
            Qs = X / ||X|| = 1/rho (x y z)   where rho = sqrt(X^2+Y^2+Z^2)
        and then we apply the perspective projection with center (0 0 -xi)^T of Qs onto plan image
            p = 1/(z/rho + xi) K (x/rho  y/rho).

      Back-projection/raytrace:
        The normalized coordinate of a pixel is (x y 1)^1.
        We know that a line joining this normalized point and the projection center intersects the unit sphere
        at a point Qs. This point is defined as
            Qs = (eta*x  eta*y  eta-xi)
        where scale factor    eta = (xi + sqrt(1 + (x^2+y^2)(1-xi^2))) / (x^2+y^2+1).
    */
    };

    public struct unified_calibration
    {
        public ctransform extrinsic;
        public unified intrinsic;
    };

    public struct stereo_fisheyes
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public unified_calibration[] calibrations;
    };

    public struct pdm_calibration
    {
        public ctransform extrinsic;
        public pdm intrinsic;
    };

    public struct stereo_pdm_calibration
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public pdm_calibration[] calibrations;
    };

    public struct rgb_calibration
    {
        public ctransform extrinsic;
        public pdm intrinsic1080; //!< 1920x1080
        public pdm intrinsic720; //!< 1280x720
        public pdm intrinsic480; //!< 640x480
    };

    public struct imu_bias
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public double[] gyro_offset;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public double[] accel_offset;
    };


	// ** Init **

	// Init SDK, the device will be found automatically
	// Note: Not working on non rooted Android device
	[DllImport ("xslam-unity-wrapper")]
	public static extern bool xslam_init();

	// Uninit SDK
	[DllImport ("xslam-unity-wrapper")]
	public static extern bool xslam_uninit();

	// Init SDK, the device will be open using the giving file descriptor
	[DllImport ("xslam-unity-wrapper")]
	public static extern bool xslam_init_with_fd(int fd);
	
    public enum Component {
        ALL    = 0xFFFF,

        // init callbacks
        IMU    = 0x0001,
        POSE   = 0x0002,
        STEREO = 0x0004,
        RGB    = 0x0008,
        TOF    = 0x0010,
        EVENTS = 0x0040,
        CNN    = 0x0080,

        // channels
        HID    = 0x0100,
        UVC    = 0x0200,
        VSC    = 0x0400,
        SLAM   = 0x0800,  // depends on HID, UVC and VSC
        EDGEP  = 0x1000,  // depends on SLAM, supply 3DOF(temp)
    };

	// Init SDK, the device will be found automatically
	// Usage: xslam_init_components( (int)(Component.IMU | Component.STEREO | Component.EVENTS | Component.HID | Component.UVC) )
	// Note: Not working on non rooted Android device, should use xslam_init_components_with_fd
    // Note: If not ALL, must specify channels and streams to use
	[DllImport ("xslam-unity-wrapper")]
	public static extern bool xslam_init_components(int components);

	// Init SDK, the device will be open using the giving file descriptor
	// Usage: xslam_init_components( (int)(Component.IMU | Component.STEREO | Component.EVENTS | Component.HID | Component.UVC) )
    // Note:  If not ALL, must specify channels and streams to use
	[DllImport ("xslam-unity-wrapper")]
	public static extern bool xslam_init_components_with_fd(int fd, int components);

	// Return true if the device is open and the SDK ready
	[DllImport ("xslam-unity-wrapper")]
	public static extern bool xslam_ready();




	// ** SLAM **

	// Reset the slam to zero
	[DllImport ("xslam-unity-wrapper")]
	public static extern bool xslam_reset_slam();

	// Set the SLAM source
	// - 0: Edge (SLAM on the device)
	// - 1: Mixed mode (SLAM on the host)
	[DllImport ("xslam-unity-wrapper")]
	public static extern void xslam_slam_type(int type);

	// Get the transformation matrix and the corresponding timestamp form the SLAM source
	[DllImport ("xslam-unity-wrapper")]
    public static extern bool xslam_get_transform(ref Matrix4x4 matrix, ref long timestamp, ref int status);

	// Get the transformation matrix and the corresponding timestamp form the SLAM source
	[DllImport ("xslam-unity-wrapper")]
    public static extern bool xslam_get_transform_matrix([In, Out] float[] matrix, ref long timestamp, ref int status);

	// Get the position vector, the orientation (euler angles) and the corresponding timestamp form the SLAM source
	[DllImport ("xslam-unity-wrapper")]
	public static extern bool xslam_get_6dof(ref Vector3 position, ref Vector3 orientation, ref long timestamp);




	// ** IMU **

	// Get the IMU data as array of Vector3 and the corresponding timestamp
	// Note: Initialize the array with a size of 3
	[DllImport ("xslam-unity-wrapper")]
	public static extern bool xslam_get_imu_array([In, Out] Vector3[] imu, ref long timestamp);

	// Get the IMU data as three Vector3 and the corresponding timestamp
	[DllImport ("xslam-unity-wrapper")]
	public static extern bool xslam_get_imu(ref Vector3 accel, ref Vector3 gyro, ref Vector3 magn, ref long timestamp);



	// ** 3DOF **

	// Get the 3DOF data
	[DllImport ("xslam-unity-wrapper")]
	public static extern bool xslam_get_3dof(ref Orientation o);




	// ** Stereo **

	// Get the stereo image width, can return 0 if no image is available
	[DllImport ("xslam-unity-wrapper")]
	public static extern int xslam_get_stereo_width();

	// Get the stereo image height, can return 0 if no image is available
	[DllImport ("xslam-unity-wrapper")]
	public static extern int xslam_get_stereo_height();

	// Get the stereo left image with RGBA format, set width and height to resize the image or 0 to keep the original size
	// Return false if no image is available
	[DllImport ("xslam-unity-wrapper")]
	public static extern bool xslam_get_left_image(System.IntPtr data, int width, int height);

	// Get the stereo right image with RGBA format, set width and height to resize the image or 0 to keep the original size
	// Return false if no image is available
	[DllImport ("xslam-unity-wrapper")]
	public static extern bool xslam_get_right_image(System.IntPtr data, int width, int height);

	// Get the maximal number of points which can be detected in the image
	[DllImport ("xslam-unity-wrapper")]
	public static extern int xslam_get_stereo_max_points();

	// Get the points detected in the left image, size will be set to the current number of points
	// Note: Initialize the array with a size of N Vector2 (get N form xslam_get_stereo_max_points)
	// Return false if the data is not available
	[DllImport ("xslam-unity-wrapper")]
	public static extern bool xslam_get_left_points([In, Out] Vector2[] points, ref int size);

	// Get the points detected in the right image, size will be set to the current number of points
	// Note: Initialize the array with a size of N Vector2 (get N form xslam_get_stereo_max_points)
	// Return false if the data is not available
	[DllImport ("xslam-unity-wrapper")]
	public static extern bool xslam_get_right_points([In, Out] Vector2[] points, ref int size);

	// Start stereo stream.
	[DllImport ("xslam-unity-wrapper")]
    public static extern bool xslam_start_stereo_stream();

	// Stop stereo stream.
	[DllImport ("xslam-unity-wrapper")]
    public static extern bool xslam_stop_stereo_stream();



	// ** RGB **

    // 0: UVC / 1:VSC
    [DllImport ("xslam-unity-wrapper")]
    public static extern bool xslam_set_rgb_source( int source );

    // Should same to XSlam::VSC::RgbResolution
    // RGB_1920x1080 = 0, ///< RGB 1080p
    // RGB_1280x720  = 1, ///< RGB 720p
    // RGB_640x480   = 2, ///< RGB 480p
    // RGB_320x240   = 3, ///< RGB QVGA
    // RGB_2560x1920 = 4, ///< RGB 5m
    // TOF           = 5, ///< TOF YUYV 224x172
    // TOF only support in uvc rgb
    [DllImport ("xslam-unity-wrapper")]
    public static extern bool xslam_set_rgb_resolution(int res);

	// Get the RGB image width, return 0 if no image is available
	[DllImport ("xslam-unity-wrapper")]
	public static extern int xslam_get_rgb_width();

	// Get the RGB image height, return 0 if no image is available
	[DllImport ("xslam-unity-wrapper")]
	public static extern int xslam_get_rgb_height();

	// Get the RGB image with RGBA format, set width and height to resize the image or 0 to keep the original size
	// `timestamp` should be saved for next call to avoid get same image
	// Return false if no image is available
	[DllImport ("xslam-unity-wrapper")]
    public static extern bool xslam_get_rgb_image_RGBA(System.IntPtr data, int width, int height, ref long timestamp);

	// Get the RGB image with YUV format(I420), set width and height to resize the image or 0 to keep the original size
	// `timestamp` should be saved for next call to avoid get same image
	// Return false if no image is available
	[DllImport ("xslam-unity-wrapper")]
	public static extern bool xslam_get_rgb_image_YUV(System.IntPtr data, int width, int height, ref long timestamp);

	// Start RGB stream.
	[DllImport ("xslam-unity-wrapper")]
    public static extern bool xslam_start_rgb_stream();

	// Stop RGB stream.
	[DllImport ("xslam-unity-wrapper")]
    public static extern bool xslam_stop_rgb_stream();



	// ** TOF **

	// Get the TOF image width, return 0 if no image is available
	[DllImport ("xslam-unity-wrapper")]
	public static extern int xslam_get_tof_width();

	// Get the TOF image height, return 0 if no image is available
	[DllImport ("xslam-unity-wrapper")]
	public static extern int xslam_get_tof_height();

	// Get the TOF image with RGBA format, set width and height to resize the image or 0 to keep the original size
	// Return false if no image is available
	[DllImport ("xslam-unity-wrapper")]
	public static extern bool xslam_get_tof_image(System.IntPtr data, int width, int height);

	// Get the TOF depth data
	// Note: Initialize the array with a size of N float (get N form xslam_get_tof_width * xslam_get_tof_height)
	[DllImport ("xslam-unity-wrapper")]
	public static extern bool xslam_get_depth_data([In, Out] float[] data);

	// Get the TOF cloud point
	// Note: Initialize the array with a size of N Vector3 (get N form xslam_get_tof_width * xslam_get_tof_height)
	[DllImport ("xslam-unity-wrapper")]
	public static extern bool xslam_get_cloud_data([In, Out] Vector3[] data);

	// Start TOF stream.
	[DllImport ("xslam-unity-wrapper")]
    public static extern bool xslam_start_tof_stream();

	// Stop TOF stream.
	[DllImport ("xslam-unity-wrapper")]
    public static extern bool xslam_stop_tof_stream();



	// ** Other **

	// Get HID event
    // return event info and timestamp.
    // For GrooveX controller:
    //        key      action   type state
    //        GPIO 3   key down  08   01
    //        GPIO 3   key up    08   02
    //        GPIO 43  key down  08   03
    //        GPIO 43  key up    08   04
    [DllImport ("xslam-unity-wrapper")]
    public static extern bool xslam_get_event( ref int type, ref int state, ref long timestamp );



	// ** HID **

    [DllImport ("xslam-unity-wrapper")]
    public static extern bool xslam_write_and_read_hid(IntPtr wdata, int wlen, IntPtr rdata, int rlen);

	// Write HID command and get result
    public static byte[] HidWriteAndRead(byte[] wdata, int wlen)
    {
        try
        {
            byte[] rdata = new byte[128];
            GCHandle wh = GCHandle.Alloc(wdata, GCHandleType.Pinned);
            GCHandle rh = GCHandle.Alloc(rdata, GCHandleType.Pinned);
            bool ret = xslam_write_and_read_hid(wh.AddrOfPinnedObject(), wlen, rh.AddrOfPinnedObject(), 128);
            wh.Free();
            rh.Free();
            if (ret)
            {
                return rdata;
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
        return null;
    }


	// ** Read Calibrations **

    [DllImport ("xslam-unity-wrapper")]
    public static extern bool readIMUBias(ref imu_bias bias);

    [DllImport ("xslam-unity-wrapper")]
    public static extern bool readStereoFisheyesCalibration(ref stereo_fisheyes calib, ref int imu_fisheye_shift_us);

    [DllImport ("xslam-unity-wrapper")]
    public static extern bool readDisplayCalibration(ref pdm_calibration calib);

    [DllImport ("xslam-unity-wrapper")]
    public static extern bool readToFCalibration(ref pdm_calibration calib);

    [DllImport ("xslam-unity-wrapper")]
    public static extern bool readRGBCalibration(ref rgb_calibration calib);
    
    [DllImport ("xslam-unity-wrapper")]
    public static extern bool readStereoFisheyesPDMCalibration(ref stereo_pdm_calibration calib);

    [DllImport ("xslam-unity-wrapper")]
    public static extern bool readStereoDisplayCalibration(ref stereo_pdm_calibration calib);


	// ** CNN **

    [DllImport ("xslam-unity-wrapper")]
    public static extern bool xslam_set_cnn_model(string path);
    
    [DllImport ("xslam-unity-wrapper")]
    public static extern bool xslam_set_cnn_descriptor(string path);
    
    [DllImport ("xslam-unity-wrapper")]
    public static extern bool xslam_set_cnn_source(int source);


	// ** Sound **

    // Play buff
    [DllImport ("xslam-unity-wrapper")]
    public static extern bool xslam_play_sound( IntPtr ptr, int len );

    // Play file(Raw path like /sdcard/a.pcm)
    [DllImport ("xslam-unity-wrapper")]
    public static extern bool xslam_play_sound_file( string path );

    [DllImport ("xslam-unity-wrapper")]
    public static extern bool xslam_is_playing();

    // Stop play
    [DllImport ("xslam-unity-wrapper")]
    public static extern void xslam_stop_play();

    // Start record
    [DllImport ("xslam-unity-wrapper")]
    public static extern bool xslam_set_mic_callback( DataCallback cb );

    // Stop record
    [DllImport ("xslam-unity-wrapper")]
    public static extern void xslam_unset_mic_callback();

    public delegate void DataCallback( IntPtr data, int len );


    // ** Play Expert **

    // Enable speaker
	[DllImport ("xslam-unity-wrapper")]
    public static extern bool xslam_start_speaker_stream();

    // Disable speaker
	[DllImport ("xslam-unity-wrapper")]
    public static extern bool xslam_stop_speaker_stream();

    // Send small sound data (<= 7680 bytes) to device
    [DllImport ("xslam-unity-wrapper")]
    public static extern int xslam_transfer_speaker_buffer( IntPtr ptr, int len );

}
