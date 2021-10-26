using UnityEngine;

namespace SystemMessageSdk.Client
{
    abstract public class IMessageCallback : AndroidJavaProxy
    {
        public IMessageCallback() : base("com.compal.system.messagesdk.IMessageCallback")
        {
        }

        abstract public void onReceivedToast(string toast, int duration);

        abstract public void onReceivedNotification(
            int notificationId, string title, string text, string subText);
    }
}