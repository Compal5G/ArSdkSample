using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowFPS : MonoBehaviour
{
    //更新的时间间隔
    public float UpdateInterval = 0.5F;
    //最后的时间间隔
    private float _lastInterval;
    //帧[中间变量 辅助]
    private int _frames = 0;
    //当前的帧率
    private float _fps;

    private Text _text;

    //private int _temp = 0;

    void Start()
    {
        //Application.targetFrameRate=60;

        //UpdateInterval = Time.realtimeSinceStartup;

        _frames = 0;
        _text = GameObject.Find("FPS").GetComponent<Text>();
    }

    void OnGUI()
    {
        _text.text = "FPS:" + _fps.ToString("f2");
        //_text.text = "temp:" + _temp;
    }

    void Update()
    {
        ++_frames;

        if (Time.realtimeSinceStartup > _lastInterval + UpdateInterval)
        {
            _fps = _frames / (Time.realtimeSinceStartup - _lastInterval);

            _frames = 0;

            _lastInterval = Time.realtimeSinceStartup;
        }

        //if (_temp == 0) {
        //	Debug.Log( "gettemp" );
        //    byte[] cmd = {0x02, 0xde, 0x78};
        //    byte[] rdata = API.HidWriteAndRead(cmd, cmd.Length);
        //    if (rdata != null) {
        //        _temp = rdata[3];
        //        Debug.Log( "gettemp" + _temp );
        //    }
        //}
    }
}
