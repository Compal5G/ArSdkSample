﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;

public class CameraRecord : MonoBehaviour
{
    private RecordManager recordManager;
    [SerializeField]
    private Camera recoardCamera = null;
    [SerializeField]
    private GameObject REC = null;

    void Start()
    {
#if PLATFORM_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            Permission.RequestUserPermission(Permission.Microphone);
        }
        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
        {
            Permission.RequestUserPermission(Permission.ExternalStorageRead);
        }
        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
        {
            Permission.RequestUserPermission(Permission.ExternalStorageWrite);
        }
#endif
        recordManager = RecordManager.GetInstance();
        recordManager.SetCamera(recoardCamera);
        recordManager.SetWidthAndHeight(1280, 720);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            recordManager.StartMicrophone();
            recordManager.StartRecording();
            REC.SetActive(true);
        }
        else if (Input.GetKeyDown(KeyCode.T))
        {
            recordManager.StopMicrophone();
            recordManager.StopRecording("user01");
            
            REC.SetActive(false);
        }
    }
}
