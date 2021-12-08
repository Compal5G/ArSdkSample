using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Linq;

public class SocketClient:MonoBehaviour, SvrManager.SvrEventListener
{
   string editString="hello wolrd"; //編輯框文字

    Socket serverSocket; //伺服器端socket
    IPAddress ip; //主機ip
    IPEndPoint ipEnd; 
    string recvStr; //接收的字串
    string sendStr; //傳送的字串
    private byte[] recvData=new byte[921600*4]; //接收的資料，必須為位元組

    public byte[] textureData=new byte[921600*4]; //接收的資料，必須為位元組

    byte[] sendData=new byte[1024]; //傳送的資料，必須為位元組
    int recvLen; //接收的資料長度
    Thread connectThread; //連線執行緒

        /// <summary>
    /// Raises the svr event event.
    /// </summary>
    /// <param name="ev">Ev.</param>
    //---------------------------------------------------------------------------------------------
    public void OnSvrEvent(SvrManager.SvrEvent ev)
    {
        switch (ev.eventType)
        {
            case SvrManager.svrEventType.kEventVrModeStarted:
                InitSocket();
                break;
        }
    }

    //初始化
    void InitSocket()
    {
        //定義伺服器的IP和埠，埠與伺服器對應
        ip=IPAddress.Parse("127.0.0.1"); //可以是區域網或網際網路ip，此處是本機
        ipEnd=new IPEndPoint(ip, 3456);
        
        //開啟一個執行緒連線，必須的，否則主執行緒卡死
        connectThread=new Thread(new ThreadStart(SocketReceive));
        connectThread.Start();
    }

    void SocketConnet()
    {
        Thread.Sleep(500);
        if(serverSocket!=null)
            serverSocket.Close();
        //定義套接字型別,必須在子執行緒中定義
        serverSocket=new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
        Debug.Log("ready to connect");
        //連線
        serverSocket.Connect(ipEnd);
    }

    void SocketSend(string sendStr)
    {
        //清空傳送快取
        sendData=new byte[1024];
        //資料型別轉換
        sendData=Encoding.ASCII.GetBytes(sendStr);
        //傳送
        serverSocket.Send(sendData,sendData.Length,SocketFlags.None);
    }

    void SocketReceive()
    {
        int LEN = 921600*4;  //3686400
        int temp = 0;
        SocketConnet();
        //不斷接收伺服器發來的資料
        while(true)
        {     
            byte[] recvData_t = new byte[1500000];
            recvLen=serverSocket.Receive(recvData_t);
            System.Buffer.BlockCopy(recvData_t, 0, recvData, temp, recvLen);
            temp += recvLen;
            if(temp >= LEN) {
                Debug.Log("Package combine complete !");
                temp = 0;
                textureData = recvData;
                recvData = new byte[LEN];
            }
        }
    }

    public byte[] GetRGB() {
        // for(int i = this.textureData.Length - 1; i >= this.textureData.Length - 9; i--) {
        //     Debug.Log("textureData i " + this.textureData[i]);
        // }
        return this.textureData;
    }

    void SocketQuit()
    {
        //關閉執行緒
        if(connectThread!=null)
        {
            connectThread.Interrupt();
            connectThread.Abort();
        }
        //最後關閉伺服器
        if(serverSocket!=null)
            serverSocket.Close();
        Debug.Log("diconnect");
    }

    // Use this for initialization
    void Start()
    {
        SvrManager.Instance.AddEventListener (this);
    }

    void OnGUI()
    {
        editString=GUI.TextField(new Rect(10,10,100,20),editString);
        if(GUI.Button(new Rect(10,30,60,20),"send"))
            SocketSend(editString);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnApplicationPause(bool pauseStatus)
    {
        SocketQuit();
    }

    //程式退出則關閉連線
    void OnApplicationQuit()
    {
        //SocketQuit();
    }
}
