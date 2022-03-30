using NatSuite.Recorders;
using NatSuite.Recorders.Clocks;
using NatSuite.Recorders.Inputs;
using System;
using System.Collections;
using System.IO;
using UnityEngine;

public class RecordManager
{
    private RecordManager()
    {
        m_videoWidth = 0;
        m_videoHeight = 0;
        m_recordMicrophone = false;
        m_sampleRate = m_channelCount = 0;
        m_recordCamera = null;
        
    }

    private static RecordManager m_instance = null;

    public static RecordManager GetInstance()
    {
        if (null == m_instance)
        {
            m_instance = new RecordManager();
        }
        return m_instance;
    }

    private int m_videoWidth, m_videoHeight;
    private RealtimeClock m_clock = null;
    private bool m_recordMicrophone;
    private int m_sampleRate;
    private int m_channelCount;
    private IMediaRecorder m_recorder = null;
    private CameraInput m_cameraInput = null;
    private Camera m_recordCamera = null;
    private AudioInput m_audioInput = null;
    private AudioSource m_microphoneSource = null;
    private string m_resultPath;
    private string m_microphoneName;

    

    public void StartMicrophone()
    {
        if (Microphone.devices.Length == 0)
        {
            Debug.Log("No Microphone");
        }
        else
        {
            foreach (string _DeviceName in Microphone.devices)
            {
                Debug.Log("Microphone Name: " + _DeviceName);
                m_microphoneName = _DeviceName;
            }
        }

        Debug.Log("Microphone final Name: "+ m_microphoneName);
        if (m_recordCamera)
        {
            if (!m_microphoneSource)
            {
                m_microphoneSource = m_recordCamera.gameObject.AddComponent<AudioSource>();
            }
        }
        m_microphoneSource.mute = m_microphoneSource.loop = m_recordMicrophone = true;
        m_microphoneSource.bypassEffects = m_microphoneSource.bypassListenerEffects = false;
        m_sampleRate = AudioSettings.outputSampleRate;
        m_channelCount = (int)AudioSettings.speakerMode;
        m_microphoneSource.clip = Microphone.Start(m_microphoneName, true, 10, AudioSettings.outputSampleRate);//Open microphone
        while (Microphone.GetPosition(null) <= 0)
        {

        }
        m_microphoneSource.Play();
        Debug.Log("Microphone play");
    }
    

    public void StopMicrophone()
    {
        Microphone.End(null);//Close microphone
    }


    public void StartRecording()
    {
        m_clock = new RealtimeClock();
        m_recorder = new MP4Recorder(m_videoWidth, m_videoHeight, 100, m_sampleRate, m_channelCount);//new Recorder
        m_cameraInput = new CameraInput(m_recorder, m_clock, m_recordCamera);//Create camera input
        if (m_recordMicrophone)
        {
            m_audioInput = new AudioInput(m_recorder, m_clock, m_microphoneSource, true);//Create audio input
            m_microphoneSource.mute = false;
        }
        Debug.Log("Record start");
    }
    

    public async void StopRecording(string saveName)
    {
        if (m_recordMicrophone)
        {
            m_microphoneSource.mute = true;
            m_audioInput.Dispose();
        }
        m_cameraInput.Dispose();
        m_resultPath = await m_recorder.FinishWriting();//Finish and save
        
        Debug.Log("Record stop");
        RenameAndMove(saveName);
    }


    public void RemoveRecording()
    {
        if (File.Exists(m_resultPath))
        {
            File.Delete(m_resultPath);
        }
    }

    private void RenameAndMove(string name)
    {
        string time = DateTime.Now.ToString("yyyyMMddHHmmssfff");
        string path = Path.Combine(GetAndroidExternalStoragePath(), name +"_"+time+ ".mp4");
        File.Move(m_resultPath, path);
        Debug.Log($"Saved recording to: {path}");
    }

    public void SetCamera(Camera camera)
    {
        m_recordCamera = camera;
    }

    public void SetWidthAndHeight(int width, int height)
    {
        m_videoWidth = width;
        m_videoHeight = height;
    }

    public string GetAndroidExternalStoragePath()
    {
        if (Application.platform != RuntimePlatform.Android)
            return Application.persistentDataPath;

        var jc = new AndroidJavaClass("android.os.Environment");
        var path = jc.CallStatic<AndroidJavaObject>("getExternalStoragePublicDirectory",
            jc.GetStatic<string>("DIRECTORY_DCIM"))
            .Call<string>("getAbsolutePath");
        return path;
    }
}