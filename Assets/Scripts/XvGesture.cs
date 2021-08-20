using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Runtime.InteropServices;

public struct GestureInfo
{
    public int id;
    public int dis_diff_;  // distance of double pinch
    public float x;
    public float y;
    public int count;
};

public struct EventInfo
{
    public int id;
    public int dis_diff_;
    public float x;
    public float y;
    public int count;
};
enum GestureID
{
    GES_OTHER = 0,
    GES_PALM_BACK = 2,
    GES_SIX_BACK = 5,
    GES_TWO_BACK = 6,
    GES_OK_BACK = 8,
    GES_THUMP_UP_FRONT = 12,
    GES_ROCK_FRONT = 16,
    GES_FIST_FRONT = 60,
    GES_PALM_DOWN = 70
};

enum EventID
{
    EVE_DRAG_UP = 11,
    EVE_DRAG_DOWN = 12,
    EVE_DRAG_LEFT = 13,
    EVE_DRAG_RIGHT = 14,
    EVE_DOUBLE_PINCH = 15,

    GES_FIST_FRONT = 10,
    GES_PALM_DOWN = 20,
    EVE_NODEF = 21
};

public class XvGesture : MonoBehaviour
{
    [Header("Gesture Trust Criteria")]
    public int checkTime = 5;

    [SerializeField]
    private bool enable_Gesture_Plam_Back = true;
    [SerializeField]
    private bool enable_Gesture_Six_Back = true;
    [SerializeField]
    private bool enable_Gesture_Two_Back = true;
    [SerializeField]
    private bool enable_Gesture_OK_Back = true;
    [SerializeField]
    private bool enable_Gesture_Thumb_Up_Front = true;
    [SerializeField]
    private bool enable_Gesture_Rock_Front = true;
    [SerializeField]
    private bool enable_Gesture_Fist_Front = true;
    [SerializeField]
    private bool enable_Gesture_Plum_Down = true;
    [SerializeField]
    private bool enable_Event_Drag_Up = true;
    [SerializeField]
    private bool enable_Event_Drag_Down = true;
    [SerializeField]
    private bool enable_Event_Drag_Right = true;
    [SerializeField]
    private bool enable_Event_Drag_Left = true;
    [SerializeField]
    private bool enable_Event_Double_Pinch = false;


    private static XvGesture firstInstance;
    private AndroidJavaObject ges = null;
    private Queue q = new Queue(); // event use queue to avoid miss
    private GestureInfo currGes;
    private GestureInfo lastGes;

    private EventInfo lastEven;
    private float gesStay = 0.0f;
    private int noGestureCount = 0;
    private bool inited = false;

    void Awake()
    {
        if (firstInstance == null)
        {
            firstInstance = this;
        }
    }

    void Start()
    {
    }

    void FixedUpdate()
    {
        if (!inited) {
            return;
        }

        GestureInfo tmpGes;
        tmpGes.id = -1;
        tmpGes.dis_diff_ = 0;
        tmpGes.x = 0.0f;
        tmpGes.y = 0.0f;
	    tmpGes.count = 0;

        AndroidJavaClass arrayClass  = new AndroidJavaClass("java.lang.reflect.Array");
        AndroidJavaObject xvInfos = ges.Call<AndroidJavaObject>("getXvHandInfos");
        if (xvInfos != null) {
            int n = arrayClass.CallStatic<int>("getLength", xvInfos);
            for (int i = 0; i < n; i++) {
                AndroidJavaObject xvInfo = arrayClass.CallStatic<AndroidJavaObject>("get", xvInfos, i);
                AndroidJavaObject handInfos = xvInfo.Get<AndroidJavaObject>("handInfos");
                long timestamp = xvInfo.Get<long>("timestamp");
                long width = xvInfo.Get<long>("width");
                long height = xvInfo.Get<long>("height");
                if (handInfos != null) {
                    int m = arrayClass.CallStatic<int>("getLength", handInfos);
                    Debug.Log("handInfos width:" + width );
                    Debug.Log("handInfos height:" + height );
                    for (int j = 0; j < m; j++) {
    //class HandInfo:
    //    public static final int LR_LEFT = 240;
    //    public static final int LR_RIGHT = 15;
    //    public static final int GES_OTHER = 0;
    //    public static final int GES_FIST_BACK = 1;
    //    public static final int GES_PALM_BACK = 2;
    //    public static final int GES_THUMP_UP_BACK = 3;
    //    public static final int GES_INDEX_BACK = 4;
    //    public static final int GES_SIX_BACK = 5;
    //    public static final int GES_YES_BACK = 6;
    //    public static final int GES_ROCK_BACK = 7;
    //    public static final int GES_OK_BACK = 8;
    //    public static final int GES_GUN_BACK = 9;
    //    public static final int GES_FIST_FRONT = 10;
    //    public static final int GES_PALM_FRONT = 11;
    //    public static final int GES_THUMP_UP_FRONT = 12;
    //    public static final int GES_INDEX_FRONT = 13;
    //    public static final int GES_SIX_FRONT = 14;
    //    public static final int GES_YES_FRONT = 15;
    //    public static final int GES_ROCK_FRONT = 16;
    //    public static final int GES_OK_FRONT = 17;
    //    public static final int GES_GUN_FRONT = 18;
    //    public static final int GES_PALM_UP = 19;
    //    public static final int GES_PALM_DOWN = 20;
    //    public static final int GES_THREE_ROTATE_FRONT = 21;
    //    public static final int GES_THREE_ROTATE_BACK = 22;
    //    public static final int GES_LOVE_FRONT = 23;
    //    public static final int GES_LOVE_BACK = 24;
    //    public static final int GES_PINCH = 25;
    //    public static final int GES_FOUR_FINGER_FRONT = 26;
    //    public static final int GES_FOUR_FINGER_BACK = 27;
    //    public static final int EVE_NODEFINE = -1;
    //    public static final int EVE_PINCH = 10;
    //    public static final int EVE_DRAG_UP = 11;
    //    public static final int EVE_DRAG_DOWN = 12;
    //    public static final int EVE_DRAG_LEFT = 13;
    //    public static final int EVE_DRAG_RIGHT = 14;
    //    public static final int EVE_DOUBLE_PINCH = 15;
    //    public static final int EVE_START = 16;
    //    public static final int EVE_END = 17;
    //    public static final int EVE_MENU = 18;
    //    public static final int EVE_CLOSE = 19;
    //    public static final int EVE_PLAM_UP = 20;
    //    public static final int EVE_NODEF = 21;
    //    public static final int STUDY_EVE_ONE_FINGER = 9;
    //    public static final int STUDY_EVE_TWO_FINGERS = 10;
    //    public static final int STUDY_EVE_POINT_UP = 11;
    //    public static final int STUDY_EVE_POINT_DOWN = 12;
    //    public static final int STUDY_EVE_POINT_LEFT = 13;
    //    public static final int STUDY_EVE_POINT_RIGHT = 14;
    //    public static final int STUDY_EVE_PINCH_UP = 15;
    //    public static final int STUDY_EVE_GRAB = 16;
    //    public static final int STUDY_EVE_NODEF = 17;
    //    public static final int STUDY_SLIDE_LEFT = 18;
    //    public static final int STUDY_SLIDE_RIGHT = 19;
    //    public static final int STUDY_SLIDE_UP = 20;
    //    public static final int STUDY_SLIDE_DOWN = 21;
    //    public static final int STUDY_TRACE_NODEF = 22;
    //    public static final String[] GestureName = new String[]{"other", "fist back", "palm back", "thumb up back", "index back", "six back", "yes back", "rock back", "ok back", "gun back", "fist front", "palm front", "thumb up front", "index front", "six front", "yes front", "rock front", "ok front", "gun front", "palm up", "palm down", "three rotate front", "three rotate back", "love front", "love back", "pinch", "four finger front", "four finger back"};
    //    public float prob_;
    //    public float[] box_ = new float[4];
    //    public float[] box_3d_ = new float[24];
    //    public float[] box_pad_ = new float[4];
    //    public int have_skeleton_;
    //    public float[] skeleton_2d_ = new float[66];
    //    public float[] skeleton_3d_position_ = new float[66];
    //    public float[] skeleton_3d_orientation_ = new float[88];
    //    public float[] skeleton_prob_ = new float[22];
    //    public int lr_;
    //    public int gesture_;
    //    public int event_;
    //    public int dis_diff_;
    //    public int trace_;
    //    public float deltax;
    //    public float deltay;
    //    public float deltaz;
    //    public float deltar;
    //    public int id;
    //    public float[] orinted_box = new float[8];
    //    public float[] skeleton_2d_raw_ = new float[66];

                        AndroidJavaObject handInfo = arrayClass.CallStatic<AndroidJavaObject>("get", handInfos, j);
                        AndroidJavaObject pos_2d = handInfo.Get<AndroidJavaObject>("skeleton_2d_");
                        float x1 = arrayClass.CallStatic<AndroidJavaObject>("get", pos_2d, 0).Call<float>("floatValue");
                        float y1 = arrayClass.CallStatic<AndroidJavaObject>("get", pos_2d, 1).Call<float>("floatValue");

                        int id = handInfo.Get<int>("gesture_");
                        tmpGes.id = id;
                        tmpGes.dis_diff_ = handInfo.Get<int>("dis_diff_");
                        tmpGes.x = x1 * width;
                        tmpGes.y = y1 * height;

                        EventInfo ev;
                        ev.id = handInfo.Get<int>("event_");
                        ev.dis_diff_ = handInfo.Get<int>("dis_diff_");
                        ev.x = x1 * width;
                        ev.y = y1 * height;
                        ev.count = 0;
                        //Filter Unchecked Gesture and Event
                        switch (tmpGes.id) {
                            case (int)GestureID.GES_FIST_FRONT:
                                if(!enable_Gesture_Fist_Front) tmpGes.id = -1;
                                break;

                            case (int)GestureID.GES_OK_BACK:
                                if(!enable_Gesture_OK_Back) tmpGes.id = -1;
                                break;

                            case (int)GestureID.GES_PALM_BACK:
                                if(!enable_Gesture_Plam_Back) tmpGes.id = -1;
                                break;

                            case (int)GestureID.GES_PALM_DOWN:
                                if(!enable_Gesture_Plum_Down) tmpGes.id = -1;
                                break;

                            case (int)GestureID.GES_ROCK_FRONT:
                                if(!enable_Gesture_Rock_Front) tmpGes.id = -1;
                                break;

                            case (int)GestureID.GES_SIX_BACK:
                                if(!enable_Gesture_Six_Back) tmpGes.id = -1;
                                break;

                            case (int)GestureID.GES_THUMP_UP_FRONT:
                                if(!enable_Gesture_Thumb_Up_Front) tmpGes.id = -1;
                                break;

                            case (int)GestureID.GES_TWO_BACK:
                                if(!enable_Gesture_Two_Back) tmpGes.id = -1;
                                break;
                            
                            case (int)GestureID.GES_OTHER:
                                tmpGes.id = -1;
                                break;
                                
                            default:
                                break;
                        }

                        switch (ev.id) {
                            case (int)EventID.EVE_DOUBLE_PINCH:
                                if(!enable_Event_Double_Pinch) ev.id = -1;
                                break;

                            case (int)EventID.EVE_DRAG_DOWN:
                                if(!enable_Event_Drag_Down) ev.id = -1;
                                break;

                            case (int)EventID.EVE_DRAG_LEFT:
                                if(!enable_Event_Drag_Left) ev.id = -1;
                                break;

                            case (int)EventID.EVE_DRAG_RIGHT:
                                if(!enable_Event_Drag_Right) ev.id = -1;
                                break;

                            case (int)EventID.EVE_DRAG_UP:
                                if(!enable_Event_Drag_Up) ev.id = -1;
                                break;

                            case (int)EventID.GES_FIST_FRONT:
                                if(!enable_Gesture_Fist_Front) ev.id = (int)EventID.EVE_NODEF;
                                break;

                            case (int)EventID.GES_PALM_DOWN:
                                if(!enable_Gesture_Plum_Down) ev.id = (int)EventID.EVE_NODEF;
                                break;
                            
                            default:
                                break;
                        }
                        // There are static gestures in usens event
                        if (ev.id == (int)EventID.GES_FIST_FRONT || ev.id == (int)EventID.GES_PALM_DOWN) {
                            tmpGes.id = ev.id + 50;
                            tmpGes.dis_diff_ = ev.dis_diff_;
                        } else {
                            Debug.Log("Event before q = " + ev.id);
                            if(lastEven.id == ev.id) {
                                lastEven.count++;
                            } else {
                                lastEven = ev;
                                lastEven.count++;
                            }
                            if(lastEven.id != (int)EventID.EVE_NODEF && lastEven.count >= checkTime) {
                                q.Enqueue(ev);
                            }
                        }
                    }
                }
            }
        }

        if (tmpGes.id > 0) {  //get gesture.
            gesStay = 0;
            if(lastGes.id ==  tmpGes.id) {  //make sure if the gesture can be trust.
                lastGes.count++;
            } else {  // the new gesture.
                lastGes = tmpGes;
                lastGes.count++;
            }
            if(lastGes.count >= checkTime) {  //count is greater than checkTime, the gesutre is credible.
                noGestureCount = 0;  //clean the count of noGesture case.
                currGes = lastGes;
            }
        } else { //detect no gesture.
            noGestureCount++;
            if(gesStay < 1.0) {
                gesStay += Time.fixedDeltaTime;
            } else {
                if(noGestureCount >= checkTime) {
                    currGes = tmpGes;
                }
            }
        }
    }

    void OnApplicationQuit()
    {
        if (ges != null) {
            ges.Call("stop");
        }
    }

    public static void InitGes(int fd)
    {
#if UNITY_ANDROID
        if (firstInstance != null) {
            AndroidJavaObject context = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject> ("currentActivity");
            firstInstance.ges = new AndroidJavaObject("com.xvisio.hand.Gesture", context);
            if (firstInstance.ges.Call<bool>("init", fd)) {
                firstInstance.ges.Call("start");
                firstInstance.inited = true;
            } else {
                Debug.Log("Gesture init failed!");
                ShowAndroidToast("Gesture init failed!");
                Toggle tog = GameObject.Find("ToggleGes").GetComponent<Toggle>();
                tog.isOn = false;
                tog.interactable = false;
            }
        } else {
            Debug.Log("Please put a XvGesture object into the scene!");
        }
#endif
    }

    public static bool Ready()
    {
        if (firstInstance != null) {
            return firstInstance.inited;
        } else {
            throw new Exception("Please put a XvGesture object into the scene!");
        }
    }

    // Start Gesture capture, must open RGB first
    public static void StartGes()
    {
        if (firstInstance != null) {
            firstInstance.ges.Call("start");
        } else {
            throw new Exception("Please put a XvGesture object into the scene!");
        }
    }

    // Stop Gesture capture
    public static void StopGes()
    {
        if (firstInstance != null) {
            firstInstance.ges.Call("stop");
        } else {
            throw new Exception("Please put a XvGesture object into the scene!");
        }
    }

    // Return latest valid gesture within previous 1s, or empty gesture
    public static GestureInfo GetGesture()
    {
        if (firstInstance != null) {
            return firstInstance.currGes;
        } else {
            throw new Exception("Please put a XvGesture object into the scene!");
        }
    }

    // Return event list, first is oldest
    public static EventInfo[] GetEvents()
    {
        if (firstInstance != null) {
            EventInfo[] evs = new EventInfo[firstInstance.q.Count];
            firstInstance.q.CopyTo(evs, 0);
            firstInstance.q.Clear();
            return evs;
        } else {
            throw new Exception("Please put a XvGesture object into the scene!");
        }
    }

    public static void ShowAndroidToast(string message)
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        if (unityActivity != null)
        {
            AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
            unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", unityActivity, message, 0);
                toastObject.Call("show");
            }));
        }
    }
}
