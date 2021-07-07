using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Runtime.InteropServices;

public class StreamToggle : MonoBehaviour {
    public Toggle toggleRgb;
    public Toggle toggleTof;
    public Toggle toggleStereo;
    public Toggle toggleGesture;
    public Toggle toggleMixed;
    public Toggle toggleRgbSource;

    void Start()
    {
        //toggleMixed.isOn = GameObject.Find("Cube").GetComponent<XSlamCameraController>().slamMode == XSlamCameraController.SlamModes.Host;

        toggleRgb.onValueChanged.AddListener(OnValueChangedRgb);
        toggleTof.onValueChanged.AddListener(OnValueChangedTof);
        toggleStereo.onValueChanged.AddListener(OnValueChangedStereo);
        toggleGesture.onValueChanged.AddListener(OnValueChangedGesture);
        toggleMixed.onValueChanged.AddListener(OnValueChangedMixed);
        toggleRgbSource.onValueChanged.AddListener(OnValueChangedRgbSource);
    }

    void OnValueChangedRgb(bool check)
    {
        Debug.Log("OnValueChangedRgb " + check);
        if (check) {
            API.xslam_start_rgb_stream();
        } else {
            API.xslam_stop_rgb_stream();
        }
    }

    void OnValueChangedTof(bool check)
    {
        Debug.Log("OnValueChangedTof " + check);
        if (check) {
            API.xslam_start_tof_stream();
        } else {
            API.xslam_stop_tof_stream();
        }
        //API.xslam_set_rgb_resolution( check ? 5 : 1 ); // UVC TOF
    }

    void OnValueChangedStereo(bool check)
    {
        Debug.Log("OnValueChangedStereo " + check);
        if (check) {
            API.xslam_start_stereo_stream();
        } else {
            API.xslam_stop_stereo_stream();
        }
    }

    void OnValueChangedGesture(bool check)
    {
        Debug.Log("OnValueChangedGesture " + check);
        if (check) {
            XvGesture.StartGes();
        } else {
            XvGesture.StopGes();
        }
    }

    void OnValueChangedMixed(bool check)
    {
        Debug.Log("OnValueChangedMixed " + check);
        GameObject.Find("Cube").GetComponent<XSlamCameraController>().setSlamMode(check ? XSlamCameraController.SlamModes.Host : XSlamCameraController.SlamModes.Device );
    }

    void OnValueChangedRgbSource(bool check)
    {
        Debug.Log("OnValueChangeRgbSource " + check);
        API.xslam_set_rgb_source( check ? 0 : 1 );
    }

    public bool RgbOn()
    {
        return toggleRgb.isOn;
    }

    public bool TofOn()
    {
        return toggleTof.isOn;
    }

    public bool StereoOn()
    {
        return toggleStereo.isOn;
    }

}
