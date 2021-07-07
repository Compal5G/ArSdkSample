using UnityEngine;

public class CameraGazeHandler : MonoBehaviour
{
    public Camera[] usingCamera;

    public delegate void GazeHandler(Collider obj);

    public event GazeHandler StartGazeEvent;
    public event GazeHandler GazingEvent;
    public event GazeHandler EndGazeEvent;

    Camera targetCam;
    Canvas gazeCircleCanvas;
    GazeCircleHandler gazeCircleHandler;

    RaycastHit hit;
    Collider lastHitObject;

    Vector3 rayOrigin;
    Vector3 rayForward;
    float rayFarClipPlane;

    void Start()
    {
        // Create gaze circle ui and init
        for (int i = 0; i < usingCamera.Length; i++)
        {
            targetCam = usingCamera[i];
            gazeCircleCanvas = Instantiate<Canvas>(Resources.Load<Canvas>("CameraGazeUI"));
            gazeCircleCanvas.worldCamera = targetCam;
            gazeCircleCanvas.planeDistance = targetCam.nearClipPlane + 0.1f; // 重疊在相機的最近視界會產生順序的交錯成像, 故再往前0.1
            gazeCircleHandler = gazeCircleCanvas.GetComponent<GazeCircleHandler>();

            // Register event for circle update
            StartGazeEvent += gazeCircleHandler.StartGazing;
            EndGazeEvent += gazeCircleHandler.EndGazing;
        }
    }

    void Update()
    {
        rayOrigin = Vector3.zero;
        rayForward = Vector3.zero;
        rayFarClipPlane = 0f;

        for (int i = 0; i < usingCamera.Length; i++)
        {
            rayOrigin += usingCamera[i].transform.position;
            rayForward += usingCamera[i].transform.forward;

            if (usingCamera[i].farClipPlane > rayFarClipPlane)
                rayFarClipPlane = usingCamera[i].farClipPlane;
        }
        rayOrigin /= usingCamera.Length;

        Debug.DrawRay(rayOrigin, transform.TransformDirection(rayForward) * 1000f, Color.yellow);

        if (Physics.Raycast(rayOrigin, rayForward, out hit, rayFarClipPlane))
        {
            if (lastHitObject == null)
            {
                Debug.Log(hit.collider.name);
                StartGazeEvent?.Invoke(hit.collider);
                CheckRegisterAndInvoke(hit.collider, GazeEventHandler.GazeEventType.StartGaze);
                lastHitObject = hit.collider;
            }
            else
            {
                if (lastHitObject == hit.collider)
                {
                    GazingEvent?.Invoke(hit.collider);
                    CheckRegisterAndInvoke(hit.collider, GazeEventHandler.GazeEventType.Gazing);
                }
                else
                {
                    EndGazeEvent?.Invoke(lastHitObject);
                    CheckRegisterAndInvoke(lastHitObject, GazeEventHandler.GazeEventType.EndGaze);
                    StartGazeEvent?.Invoke(hit.collider);
                    CheckRegisterAndInvoke(hit.collider, GazeEventHandler.GazeEventType.StartGaze);
                    lastHitObject = hit.collider;
                }
            }
        }
        else
        {
            if (lastHitObject != null)
            {
                EndGazeEvent?.Invoke(lastHitObject);
                CheckRegisterAndInvoke(lastHitObject, GazeEventHandler.GazeEventType.EndGaze);
                lastHitObject = null;
            }
        }
    }

    void OnDestroy()
    {
        // Unregister circle update event
        StartGazeEvent -= gazeCircleHandler.StartGazing;
        EndGazeEvent -= gazeCircleHandler.EndGazing;
    }

    // 用來確認特定物件是否掛有"GazeEventHandler"的腳本並註冊callback
    void CheckRegisterAndInvoke(Collider obj, GazeEventHandler.GazeEventType eventType)
    {
        GazeEventHandler eventHnd = obj.GetComponent<GazeEventHandler>();

        if (eventHnd != null)
        {
            switch (eventType)
            {
                case GazeEventHandler.GazeEventType.StartGaze:
                    eventHnd.GetStartGazeAction()?.Invoke();
                    break;
                case GazeEventHandler.GazeEventType.Gazing:
                    eventHnd.GetGazingAction()?.Invoke();
                    break;
                case GazeEventHandler.GazeEventType.EndGaze:
                    eventHnd.GetEndGazeAction()?.Invoke();
                    break;
            }
        }
    }
}
