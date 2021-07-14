using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using Vuforia;

public class CameraInitHandler : MonoBehaviour
{
    public CameraThirdPartyType firstInitCamera;
    public GameObject[] qualcommObjects;
    public GameObject[] xvisioObjects;
    public GameObject qualcommSVRCameraObject;
    public GameObject xvisioCameraObject;

    public CameraGazeHandler gazeHandler;
    public Material cubeNormalMat;
    public Material cubeGazedMat;

    public UnityEngine.UI.Image gesImg;
    private int lastGesId = -1;

    public Transform vuforiaARCameraTransform;
    public Transform vuforiaTargetTransform;

    static Text debugText;
    public Camera[] SVRCameras;

    private int i = 0;

    private SvrManager svrManager = null;

    // ==================================
    private List<EventInfo> events = new List<EventInfo>();
    private EventInfo currEvent;
    private DateTime currEventTime;
    private GestureInfo currGes;
    private Dictionary<string, Sprite> gesTextures = new Dictionary<string, Sprite>();
    private Sprite texGes;
    private Sprite texEv;
    // ==================================

    XSlamCameraController xvisioCameraControl;

    public enum CameraThirdPartyType
    {
        QualcommSVR,
        Xvisio
    }

    void Awake()
    {
        Debug.Log("CameraInitHandler.Awake()");
        // Disable QVR and Xvison related objects first.
        // And we will contorl the respective initialization order in Start()
        foreach (GameObject obj in qualcommObjects)
        {
            Debug.Log("CameraInitHandler.Awake(q):" + obj);
            if (obj != null)
                obj.SetActive(false);
        }

        foreach (GameObject obj in xvisioObjects)
        {
            Debug.Log("CameraInitHandler.Awake(x):" + obj);
            if (obj != null)
                obj.SetActive(false);
        }

        if (xvisioCameraObject != null)
        {
            xvisioCameraControl = xvisioCameraObject.GetComponent<XSlamCameraController>();
            if (xvisioCameraControl != null)
            {
                if (vuforiaARCameraTransform != null)
                    vuforiaARCameraTransform.gameObject.SetActive(xvisioCameraControl.enableVuforia);
                
                if (vuforiaTargetTransform != null)
                    vuforiaTargetTransform.gameObject.SetActive(xvisioCameraControl.enableVuforia);
            }
        }

        Input.backButtonLeavesApp = false;
    }

    // Start is called before the first frame update
    IEnumerator Start()
    {
        Debug.Log("CameraInitHandler.Start()");
        SvrDebugHud svrDebugHud = FindObjectOfType<SvrDebugHud>();
        if (svrDebugHud != null)
        {
            Transform debugTextTransform = svrDebugHud.transform.Find("DebugText");
            if (debugTextTransform != null)
            {
                debugText = debugTextTransform.GetComponent<Text>();
            }
        }

        if (xvisioCameraControl.enableVuforia)
        {
            VuforiaARController.Instance.RegisterVuforiaInitializedCallback(OnVuforiaInitialized);
            VuforiaARController.Instance.RegisterVuforiaStartedCallback(OnVuforiaStarted);
        }

        currEvent.id = -1;
        currEventTime = DateTime.UtcNow;
        InitGazeEvent();

        Debug.Log("gesImg=" + gesImg);
        if (gesImg != null)
        {
            gesImg.gameObject.SetActive(false);
            gesImg.sprite = null;
        }

        //+++
        // QVR is not ready, so we dislplay black screen here.
        // UI/UX: display a customized loading page here.
        // GameObject mainCameraGo = GameObject.FindWithTag("MainCamera");
        // if (mainCameraGo)
        // {
        //     mainCameraGo.SetActive(false);
        //     Debug.Log("Disabling Camera with MainCamera tag");
        // }
        // GL.Clear(false, true, Color.black);
        //---

        switch (firstInitCamera)
        {
            case CameraThirdPartyType.QualcommSVR:
                yield return new WaitForSeconds(1.5f);
                QualcommInit();
                //yield return new WaitForSeconds(1f);
                XvisioInit();
                MoveXvisioCameraToQualcommSVRCamera();
                break;
            case CameraThirdPartyType.Xvisio:
                XvisioInit(); // May casue QVR restart ! [qvrservice_main: Received signal 2], if it did cause QVR restart, we must wait here unitil QVR is ready.
                //yield return new WaitForSeconds(1.5f); // so we wait here for a while before do QVR init(). May need to increase waiting time that depends on system loading on that time.
                QualcommInit();
                MoveXvisioCameraToQualcommSVRCamera();
                break;
            default:
                Debug.Log("Init order is no set up!");
                break;
        }

        svrManager = SvrManager.Instance;
        if (SvrInput.Instance != null)
        {
            SvrInput.Instance.OnBackListener = HandleBackButton;
        }
    }
    public int GestureId; // For gesture testing
    void Update()
    {
        if (!XvGesture.Ready())
        {
            Debug.Log("XvGesture is not ready!");
            return;
        }

        // Get gesture
        GestureInfo newGes = XvGesture.GetGesture();
        if (newGes.id != currGes.id && newGes.id > 0)
        {
            Debug.LogFormat("new gesture! id:{0} x:{1} y:{2}", newGes.id, newGes.x, newGes.y);
        }
        currGes = newGes;

        // Get event
        GetCurrEvent();

        if (currGes.id != -1)
        {
            string path = string.Format("Gesture/g{0:D2}", currGes.id);
            Debug.LogFormat("path:{0} ", path);
            if (gesTextures.ContainsKey(path))
            {
                Debug.LogFormat("gest contains key");
                texGes = gesTextures[path];
            }
            else
            {
                Debug.LogFormat("gest NOT contains key");
                texGes = Resources.Load<Sprite>(path);
                gesTextures.Add(path, texGes);
            }
        }
        if (currEvent.id != -1)
        {
            string path = string.Format("Gesture/e{0:D2}", currEvent.id);
            if (gesTextures.ContainsKey(path))
            {
                texEv = gesTextures[path];
            }
            else
            {
                texEv = Resources.Load<Sprite>(path);
                gesTextures.Add(path, texEv);
            }
        }

        Debug.LogFormat("gesImg={0}, tesGest={1} , texEven={2}" , gesImg, texGes, texEv);

        if (gesImg != null)
        {
            if (currGes.id != -1)
            {
                Debug.Log("gesImg on");
                gesImg.sprite = texGes;
                gesImg.gameObject.SetActive(true);
            }

            if (currEvent.id != -1)
            {
                Debug.Log("gesImg on");
                gesImg.sprite = texEv;
                gesImg.gameObject.SetActive(true);
            }

            if (currGes.id == -1 && currEvent.id == -1)
            {
                Debug.Log("gesImg off");
                gesImg.gameObject.SetActive(false);
            }
        }
    }

    Boolean pauseXvisio;
    void OnApplicationFocus(bool focusStatus)
    {
        Debug.Log("OnApplicationFocus: " + focusStatus);
        if (focusStatus)
        {
            //
        }
    }

    void OnApplicationPause(bool pauseStatus)
    {
        Debug.Log("OnApplicationPause: " + pauseStatus);
        if (pauseStatus)
        {
            if (xvisioCameraControl.enableRGBFrame)
                API.xslam_stop_rgb_stream();

            if (xvisioCameraControl.enableTOFFrame)
                API.xslam_stop_tof_stream();

            pauseXvisio = true;
        }
        if (!pauseStatus)
        {
            if (xvisioCameraControl.enableRGBFrame)
                API.xslam_start_rgb_stream();

            if (xvisioCameraControl.enableTOFFrame)
                API.xslam_start_tof_stream();

            pauseXvisio = false;
        }
    }

    void OnDestroy()
    {
        UnregisterGazeEvent();
    }

    void OnVuforiaInitialized()
    {
        VuforiaRenderer.VideoTextureInfo textureInfo = VuforiaRenderer.Instance.GetVideoTextureInfo();
        Debug.Log("=========== imageSize.x : " + textureInfo.imageSize.x +
        ", imageSize.y : " + textureInfo.imageSize.y);
        Debug.Log("=========== textureSize.x : " + textureInfo.textureSize.x +
        ", textureSize.y : " + textureInfo.textureSize.y);
    }

    void OnVuforiaStarted()
    {
        VuforiaRenderer.VideoTextureInfo textureInfo = VuforiaRenderer.Instance.GetVideoTextureInfo();
        Debug.Log("=========== imageSize.x : " + textureInfo.imageSize.x +
        ", imageSize.y : " + textureInfo.imageSize.y);
        Debug.Log("=========== textureSize.x : " + textureInfo.textureSize.x +
        ", textureSize.y : " + textureInfo.textureSize.y);
    }

    void QualcommInit()
    {
        foreach (GameObject obj in qualcommObjects)
        {
            if (obj != null)
            {
                Debug.Log("QualcommInit:active: " + obj);
                obj.SetActive(true);
            }
        }
    }

    void XvisioInit()
    {
        foreach (GameObject obj in xvisioObjects)
        {
            if (obj != null) 
            {
                Debug.Log("XvisioInit:active: " + obj);
                obj.SetActive(true);
            }
        }
    }

    void MoveXvisioCameraToQualcommSVRCamera()
    {
        xvisioCameraObject.transform.SetParent(qualcommSVRCameraObject.transform);
        xvisioCameraObject.transform.position = Vector3.zero;
    }

    void InitGazeEvent()
    {
        if (gazeHandler != null)
        {
            gazeHandler.StartGazeEvent += StartGazeEvent;
            gazeHandler.EndGazeEvent += EndGazeEvent;
        }
    }

    void UnregisterGazeEvent()
    {
        if (gazeHandler != null)
        {
            gazeHandler.StartGazeEvent -= StartGazeEvent;
            gazeHandler.EndGazeEvent -= EndGazeEvent;
        }
    }

    void StartGazeEvent(Collider obj)
    {
        if (cubeGazedMat != null)
            obj.GetComponent<Renderer>().material = cubeGazedMat;
    }

    void EndGazeEvent(Collider obj)
    {
        if (cubeNormalMat != null)
            obj.GetComponent<Renderer>().material = cubeNormalMat;
    }

    IEnumerator HandleBackButton()
    {
        svrManager.SetOverlayFade(SvrManager.eFadeState.FadeOut);
        yield return new WaitUntil(() => svrManager.IsOverlayFading() == false);

        svrManager.Shutdown();
        yield return new WaitUntil(() => svrManager.Initialized == false);

        System.GC.Collect();
        Application.Quit();
    }

    void DetectWhichKeyDown()
    {
        foreach (KeyCode vKey in System.Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(vKey))
            {
                //your code here
                Debug.Log(" ======== " + vKey + " is pressed!");
            }
        }
    }

    public GestureInfo GetCurrGes()
    {
        return currGes;
    }

    private EventInfo GetCurrEvent()
    {
        // update
        i = 0;
        foreach (EventInfo ev in XvGesture.GetEvents())
        {
            //Debug.Log(" ==== ev " + i + " id = " + ev.id);
            events.Add(ev);
            i++;
        }

        // unique list, then remove bad events
        // should not be uniqued if need x,y
        events = events.Distinct().ToList();
        //Debug.Log(" ==== events count " + events.Count);
        for (int i = events.Count - 1; i >= 0; i--)
        {
            if (events[i].id == 21)
            { // ignore NODEF
                events.RemoveAt(i);
            }
        }

        // remove events too old
        for (int i = events.Count - 2; i >= 0; i--)
        {
            events.RemoveAt(i);
        }

        // no old event
        if (currEvent.id == -1)
        {
            Debug.Log("currEvent.id == -1");
            if (events.Count == 0)
            {
                return currEvent;
            }
            else
            {
                Debug.Log("events.Count > 0");
                currEvent = events[0];
                currEventTime = DateTime.UtcNow;
                events.RemoveAt(0);
                return currEvent;
            }
        }

        // if haven't showed for enough time, still show old event
        TimeSpan ts = DateTime.UtcNow - currEventTime;
        if (ts.TotalMilliseconds < 100)
        {
            return currEvent;
        }

        if (events.Count > 0)
        {
            Debug.Log("events.Count > 0");
            currEvent = events[0];
            currEventTime = DateTime.UtcNow;
            events.RemoveAt(0);
            return currEvent;
        }
        else if (ts.TotalMilliseconds < 1000)
        {
            // still show old if no new event
            return currEvent;
        }
        else
        {
            // set event empty
            currEvent.id = -1;
            return currEvent;
        }
    }

    // For debug
    public static void ShowMsg(string msg)
    {
        if (debugText == null)
            return;

        Debug.Log(msg);
        debugText.text = msg;
    }
}
