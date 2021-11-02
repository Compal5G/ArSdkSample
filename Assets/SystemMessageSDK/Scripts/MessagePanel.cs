using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class MessagePanel : MonoBehaviour, SvrManager.SvrEventListener
{
    private readonly static string TAG = "[MessagePanel]";

    private Vector3 _LocationNotification = new Vector3(0, 0.135f, 1);
    private Vector3 _LocationTitleNotification = new Vector3(0, 0.158f, 1);
    private Vector3 _LocationTextNotification = new Vector3(0, 0.128f, 1);
    private Vector3 _LocationToast = new Vector3(0, -0.04f, 1);
    private Vector3 _LocationGoneNotification = new Vector3(0, 135, 0);
    private Vector3 _LocationGoneToast = new Vector3(0, -130, 0);

    private SvrManager _SvrManager;
    private MessageCallback _MessageCallback;

    private Image _ToastImage;
    private Image _NotificationImage;
    private Text _ToastText;
    private Text _NotificationTitle;
    private Text _NotificationText;

    private float _ToastTimer = 0f;
    private float _NotificationTimer = 0f;
    private int _ToastSeconds = 0;
    private int _NotificationSeconds = 0;

    private LinkedList<ToastInfo> _QueueToast = new LinkedList<ToastInfo>();
    private LinkedList<NotificationInfo> _QueueNotification =
            new LinkedList<NotificationInfo>();

    private class ToastInfo
    {
        public string message;
        public int duration;
    }

    private class NotificationInfo
    {
        public int id;
        public string title;
        public string text;
        public string subText;
    }

    private void _SetNotificationObjects()
    {
        //New notification background object
        GameObject bgObject = new GameObject();
        RectTransform bgTrans = bgObject.AddComponent<RectTransform>();
        bgTrans.transform.SetParent(this.transform);
        bgTrans.localScale = Vector3.one;
        bgTrans.sizeDelta = new Vector2(300, 70);
        bgTrans.localScale = new Vector3(0.001f, 0.001f, 0.001f);
        bgTrans.localPosition = _LocationGoneNotification;
        _NotificationImage = bgObject.AddComponent<Image>();
        bgObject.transform.SetParent(this.transform);
        bgObject.SetActive(false);
        Color notificationBg = Color.white;
        notificationBg.a = 0.9f;
        _NotificationImage.color = notificationBg;

        Font arial = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
        //New notification titls object 
        GameObject titleObject = new GameObject();
        RectTransform titleTrans = titleObject.AddComponent<RectTransform>();
        titleTrans.transform.SetParent(this.transform);
        titleTrans.localScale = Vector3.one;
        titleTrans.sizeDelta = new Vector2(250, 18);
        titleTrans.localScale = new Vector3(0.001f, 0.001f, 0.001f);
        titleTrans.localPosition = _LocationGoneNotification;
        _NotificationTitle = titleObject.AddComponent<Text>();
        titleObject.transform.SetParent(this.transform);
        titleObject.SetActive(true);

        _NotificationTitle.font = arial;
        _NotificationTitle.color = Color.black;
        _NotificationTitle.fontSize = 13;
        _NotificationTitle.alignment = TextAnchor.MiddleLeft;

        //New notification text object 
        GameObject textObject = new GameObject();
        RectTransform textTrans = textObject.AddComponent<RectTransform>();
        textTrans.transform.SetParent(this.transform);
        textTrans.localScale = Vector3.one;
        textTrans.sizeDelta = new Vector2(250, 50);
        textTrans.localScale = new Vector3(0.001f, 0.001f, 0.001f);
        textTrans.localPosition = _LocationGoneNotification;
        _NotificationText = textObject.AddComponent<Text>();
        textObject.transform.SetParent(this.transform);
        textObject.SetActive(true);
        _NotificationText.font = arial;
        _NotificationText.color = Color.black;
        _NotificationText.fontSize = 11;
        _NotificationText.alignment = TextAnchor.MiddleLeft;
    }
    private void _SetToastObjects()
    {
        //New toast background object
        GameObject bgObject = new GameObject();
        RectTransform bgTrans = bgObject.AddComponent<RectTransform>();
        bgTrans.transform.SetParent(this.transform);
        bgTrans.localScale = Vector3.one;
        bgTrans.sizeDelta = new Vector2(300, 80);
        bgTrans.localScale = new Vector3(0.001f, 0.001f, 0.001f);
        bgTrans.localPosition = _LocationGoneToast;
        _ToastImage = bgObject.AddComponent<Image>();
        bgObject.transform.SetParent(this.transform);
        bgObject.SetActive(false);
        Color toastBg = Color.white;
        toastBg.a = 0.6f;
        _ToastImage.color = toastBg;

        //New toast text object 
        GameObject textObject = new GameObject();
        RectTransform textTrans = textObject.AddComponent<RectTransform>();
        textTrans.transform.SetParent(this.transform);
        textTrans.localScale = Vector3.one;
        textTrans.sizeDelta = new Vector2(300, 80);
        textTrans.localScale = new Vector3(0.001f, 0.001f, 0.001f);
        textTrans.localPosition = _LocationGoneToast;
        _ToastText = textObject.AddComponent<Text>();
        textObject.transform.SetParent(this.transform);
        textObject.SetActive(true);
        Font arial = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
        _ToastText.font = arial;
        _ToastText.color = Color.black;
        _ToastText.fontSize = 13;
        _ToastText.alignment = TextAnchor.MiddleCenter;
    }

    private void Awake()
    {
        SystemMessageSdk.Client.Log.CreateInstance(false);
#if UNITY_ANDROID
        _MessageCallback = new MessageCallback(this);
#endif
        _SetNotificationObjects();
        _SetToastObjects();

        //test
        //StartCoroutine(_TestMessages());
    }

    void Start()
    {
        StartCoroutine(_TryToSetSvrManager());
    }

    void OnApplicationPause(bool isPause)
    {
#if UNITY_ANDROID
        if (isPause)
        {
            SystemMessageSdk.Client.SystemMessageUtils
                    .unRegisterSystemMessage(_MessageCallback);
        }
        else
        {
            SystemMessageSdk.Client.SystemMessageUtils
                    .registerSystemMessage(_MessageCallback);
        }
#endif
    }

    void Update()
    {
        if (_QueueToast.Count > 0)
        {
            //Show toast
            _ToastText.gameObject.GetComponent<RectTransform>()
                    .localPosition = _LocationToast;
            _ToastImage.gameObject.SetActive(true);
            _ToastImage.gameObject.GetComponent<RectTransform>()
                    .localPosition = _LocationToast;
            ToastInfo info = _QueueToast.First.Value;
            _ToastText.text = string.Format(info.message);

            //Set time to hide toast
            _ToastTimer += Time.deltaTime;
            _ToastSeconds = (int)(_ToastTimer % 60);
            if (_ToastSeconds >= info.duration)
            {
                _QueueToast.RemoveFirst();
                _ToastTimer = 0f;
                _ToastSeconds = 0;
            }
        }
        else
        {
            //Hide toast
            if (!string.IsNullOrEmpty(_ToastText.text))
            {
                _ToastText.text = "";
            }
            if (_ToastText.gameObject.GetComponent<RectTransform>()
                    .localPosition != _LocationGoneToast)
            {
                _ToastText.gameObject.GetComponent<RectTransform>()
                        .localPosition = _LocationGoneToast;
            }
            if (_ToastImage.gameObject.GetComponent<RectTransform>()
                    .localPosition != _LocationGoneToast)
            {
                _ToastImage.gameObject.GetComponent<RectTransform>()
                        .localPosition = _LocationGoneToast;
                _ToastImage.gameObject.SetActive(false);
            }
        }

        if (_QueueNotification.Count > 0)
        {
            //Show notification
            _NotificationTitle.gameObject.GetComponent<RectTransform>()
                    .localPosition = _LocationTitleNotification;
            _NotificationText.gameObject.GetComponent<RectTransform>()
                    .localPosition = _LocationTextNotification;
            _NotificationImage.gameObject.SetActive(true);
            _NotificationImage.gameObject.GetComponent<RectTransform>()
                    .localPosition = _LocationNotification;
            NotificationInfo info = _QueueNotification.First.Value;
            _NotificationTitle.text = null == info.title ? "" : string.Format(info.title);
            _NotificationText.text = null == info.text ? "" : string.Format(info.text);

            //Set time to hide notification
            _NotificationTimer += Time.deltaTime;
            _NotificationSeconds = (int)(_NotificationTimer % 60);
            if (_NotificationSeconds >= 4)
            {
                _QueueNotification.RemoveFirst();
                _NotificationTimer = 0f;
                _NotificationSeconds = 0;
            }
        }
        else
        {
            //Hide notification
            if (!string.IsNullOrEmpty(_NotificationTitle.text))
            {
                _NotificationTitle.text = "";
            }
            if (!string.IsNullOrEmpty(_NotificationText.text))
            {
                _NotificationText.text = "";
            }
            if (_NotificationTitle.gameObject.GetComponent<RectTransform>()
                    .localPosition != _LocationGoneNotification)
            {
                _NotificationTitle.gameObject.GetComponent<RectTransform>()
                        .localPosition = _LocationGoneNotification;
            }
            if (_NotificationText.gameObject.GetComponent<RectTransform>()
                    .localPosition != _LocationGoneNotification)
            {
                _NotificationText.gameObject.GetComponent<RectTransform>()
                        .localPosition = _LocationGoneNotification;
            }
            if (_NotificationImage.gameObject.GetComponent<RectTransform>()
                    .localPosition != _LocationGoneNotification)
            {
                _NotificationImage.gameObject.GetComponent<RectTransform>()
                        .localPosition = _LocationGoneNotification;
                _NotificationImage.gameObject.SetActive(false);
            }
        }
    }

    private IEnumerator _TryToSetSvrManager()
    {
        while (null == _SvrManager)
        {
            yield return new WaitForSecondsRealtime(3);
            _SvrManager = SvrManager.Instance;
            Debug.Assert(_SvrManager != null, "SvrManager object not found");
            if (_SvrManager != null)
            {
                _SvrManager.AddEventListener(this); // Register for SvrEvents
            }
        }
        Debug.Log(string.Format("Stop set SvrManager."));
    }

    private void LateUpdate()
    {
        if (_SvrManager == null)
            return;

        var headTransform = _SvrManager.head;

        transform.position = headTransform.position;
        transform.rotation = headTransform.rotation;

        var position = headTransform.localPosition;
        var orientation = headTransform.localRotation;
    }

    public void OnSvrEvent(SvrManager.SvrEvent ev)
    {

    }

    private class MessageCallback : SystemMessageSdk.Client.IMessageCallback
    {
        MessagePanel mMessagePanel;

        public MessageCallback(MessagePanel messagePanel)
        {
            mMessagePanel = messagePanel;
        }

        override
        public void onReceivedToast(string toast, int duration)
        {
            SystemMessageSdk.Client.Log.Instance.D(TAG, "-----onReceivedToast toast:"
                + toast + ", duration:" + duration);
            int time = 2;
            if (duration == 1)
            {
                time = 4;
            }
            ToastInfo info = new ToastInfo();
            info.message = toast;
            info.duration = time;
            mMessagePanel._QueueToast.AddLast(info);
        }

        override
        public void onReceivedNotification(
            int notificationId, string title, string text, string subText)
        {
            SystemMessageSdk.Client.Log.Instance.D(TAG,
                    "-----onReceivedNotification notificationId:" + notificationId +
                    ", title:" + title + ", text:" + text + ", subText:" + subText);
            NotificationInfo info = new NotificationInfo();
            info.id = notificationId;
            info.title = title;
            info.text = text;
            info.subText = subText;
            mMessagePanel._QueueNotification.AddLast(info);
        }
    }

    private IEnumerator _TestMessages()
    {
        SystemMessageSdk.Client.Log.Instance.D(TAG, "testMessages");
        while (true)
        {
            yield return new WaitForSeconds(3);
            SystemMessageSdk.Client.SystemMessageTestUtils.testShortToast();
            SystemMessageSdk.Client.SystemMessageTestUtils.testLongToast();
            SystemMessageSdk.Client.SystemMessageTestUtils.testUnimportantNotification();
            SystemMessageSdk.Client.SystemMessageTestUtils.testImportantNotification();
        }
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}
