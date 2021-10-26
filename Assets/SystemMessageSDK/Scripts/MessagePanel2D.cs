using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class MessagePanel2D : MonoBehaviour
{
    private readonly static string TAG = "[MessagePanel2D]";

    private const int HEIGHT_NOTIFICATION = 200;
    private const int HEIGHT_NOTIFICATION_TITLE = 50;
    private const int WIDTH_NOTIFICATION = 1000;
    private const int PADDING_NOTIFICATION = 50;

    private MessageCallback mMessageCallback;

    private float _ToastTimer = 0f;
    private float _NotificationTimer = 0f;
    private int _ToastSeconds = 0;
    private int _NotificationSeconds = 0;

    private Vector3 _LocationNotification = new Vector3(0, 450, 0);
    private Vector3 _LocationTitleNotification = new Vector3(PADDING_NOTIFICATION,
            450 + HEIGHT_NOTIFICATION_TITLE, 0);
    private Vector3 _LocationTextNotification = new Vector3(PADDING_NOTIFICATION,
            450 - HEIGHT_NOTIFICATION_TITLE, 0);
    private Vector3 _LocationToast = new Vector3(0, -400, 0);
    private Vector3 _LocationGoneNotification = new Vector3(0, 4500, 0);
    private Vector3 _LocationGoneToast = new Vector3(0, -4000, 0);

    private Image _ToastImage;
    private Image _NotificationImage;
    private Text _ToastText;
    private Text _NotificationTitle;
    private Text _NotificationText;

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
        bgTrans.sizeDelta = new Vector2(WIDTH_NOTIFICATION, HEIGHT_NOTIFICATION);
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
        titleTrans.sizeDelta = new Vector2(WIDTH_NOTIFICATION
                - PADDING_NOTIFICATION * 2, HEIGHT_NOTIFICATION_TITLE);
        titleTrans.localPosition = _LocationGoneNotification;
        _NotificationTitle = titleObject.AddComponent<Text>();
        titleObject.transform.SetParent(this.transform);
        titleObject.SetActive(true);

        _NotificationTitle.font = arial;
        _NotificationTitle.color = Color.black;
        _NotificationTitle.fontSize = 36;
        _NotificationTitle.alignment = TextAnchor.MiddleLeft;

        //New notification text object 
        GameObject textObject = new GameObject();
        RectTransform textTrans = textObject.AddComponent<RectTransform>();
        textTrans.transform.SetParent(this.transform);
        textTrans.localScale = Vector3.one;
        textTrans.sizeDelta = new Vector2(WIDTH_NOTIFICATION - PADDING_NOTIFICATION * 2,
                HEIGHT_NOTIFICATION - HEIGHT_NOTIFICATION_TITLE - PADDING_NOTIFICATION / 2);
        textTrans.localPosition = _LocationGoneNotification;
        _NotificationText = textObject.AddComponent<Text>();
        textObject.transform.SetParent(this.transform);
        textObject.SetActive(true);
        _NotificationText.font = arial;
        _NotificationText.color = Color.black;
        _NotificationText.fontSize = 30;
        _NotificationText.alignment = TextAnchor.UpperLeft;
    }

    private void _SetToastObjects()
    {
        //New toast background object
        GameObject bgObject = new GameObject();
        RectTransform bgTrans = bgObject.AddComponent<RectTransform>();
        bgTrans.transform.SetParent(this.transform);
        bgTrans.localScale = Vector3.one;
        bgTrans.sizeDelta = new Vector2(1500, 80);
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
        textTrans.sizeDelta = new Vector2(1500, 80);
        textTrans.localPosition = _LocationGoneToast;
        _ToastText = textObject.AddComponent<Text>();
        textObject.transform.SetParent(this.transform);
        textObject.SetActive(true);
        Font arial = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
        _ToastText.font = arial;
        _ToastText.color = Color.black;
        _ToastText.fontSize = 35;
        _ToastText.alignment = TextAnchor.MiddleCenter;
    }

    void Awake()
    {
        SystemMessageSdk.Client.Log.CreateInstance(false);
#if UNITY_ANDROID
        mMessageCallback = new MessageCallback(this);
#endif
        _SetNotificationObjects();
        _SetToastObjects();

        //test
        //StartCoroutine(_TestMessages());
    }

    void OnApplicationPause(bool isPause)
    {
#if UNITY_ANDROID
        if (isPause)
        {
            SystemMessageSdk.Client.SystemMessageUtils
                    .unRegisterSystemMessage(mMessageCallback);
        }
        else
        {
            SystemMessageSdk.Client.SystemMessageUtils
                    .registerSystemMessage(mMessageCallback);
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

    private class MessageCallback : SystemMessageSdk.Client.IMessageCallback
    {
        MessagePanel2D mMessagePanel;

        public MessageCallback(MessagePanel2D messagePanel)
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
            yield return new WaitForSeconds(5);
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