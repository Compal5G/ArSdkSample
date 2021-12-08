using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class LoadRGB2 : MonoBehaviour
{

    private Texture2D texture = null;

    public SocketClient socketClient = null;

    //ntPtr _buff;

    // Start is called before the first frame update
    void Start()
    {
        Renderer rend = GetComponent<Renderer>();
        // duplicate the original texture and assign to the material
        //Texture2D texture = (Texture2D)Instantiate(rend.material.mainTexture);
        TextureFormat format = TextureFormat.ARGB32;
        texture = new Texture2D(1280, 720, format, false);
        //_buff = Marshal.AllocHGlobal(1280 * 720*4);
    }

    void LateUpdate()
    {
        Debug.Log("lateUpdate");
        texture.LoadRawTextureData(socketClient.GetRGB());
        texture.Apply();
        GetComponent<Renderer>().material.mainTexture = texture;
    }
}
