using UnityEngine;
using System.Collections;
using ZXing;

public class CodeScanner : MonoBehaviour
{
    private BarcodeReader reader = new BarcodeReader();//ZXing的解碼
    private Result res;//儲存掃描後回傳的資訊
    private bool flag = true;//判斷掃描是否執行完畢
    Texture2D sourceTex;//暫存rgb影像

    [HeaderAttribute("放置結果的Text物件")]
    [SerializeField] UnityEngine.UI.Text resText;

    [HeaderAttribute("放置獲取rgb影像的來源")]
    [SerializeField] LoadRGB getRGB;

    [HeaderAttribute("code scanner 開始時間")]
    [SerializeField] float InvokeStartTime;

    [HeaderAttribute("code scanner 間格時間")]
    [SerializeField] float InvokeUpdateTime;

    void Start()
    {
        InvokeRepeating("scanRepeating", InvokeStartTime, InvokeUpdateTime);//hehehe 出發
    }

    void scanRepeating()
    {
        if (flag == true)//若掃描已執行完畢，則再繼續進行掃描
        {
            StartCoroutine(scan());
        }
    }

    private IEnumerator scan()
    {
        flag = false;//若掃描已執行完畢，則再繼續進行掃描，這邊紀錄一下掃描開始

        sourceTex = getRGB.tex;//用一變數暫存Texture

        Texture2D t2DTexture = TextureToTexture2D(sourceTex);//將Texture to Texture2D

        yield return new WaitForEndOfFrame();//等待

        res = reader.Decode(t2DTexture.GetPixels32(), t2DTexture.width, t2DTexture.height);//對RGB影像進行解碼，並將解碼後的資料回傳

        t2DTexture = null; sourceTex = null;//把東西null一下
        System.GC.Collect();//回收記憶體

        if (res != null)        //若是掃描不到訊息，則res為null
        {
            resText.text = res.Text;
        }
        flag = true;//若掃描已執行完畢，則再繼續進行掃描，這邊紀錄一下掃描完畢
    }


    private Texture2D TextureToTexture2D(Texture texture)
    {
        Texture2D texture2D = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture renderTexture = RenderTexture.GetTemporary(texture.width, texture.height, 32);
        Graphics.Blit(texture, renderTexture);

        RenderTexture.active = renderTexture;
        texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture2D.Apply();

        RenderTexture.active = currentRT;
        RenderTexture.ReleaseTemporary(renderTexture);

        System.GC.Collect();//回收記憶體

        return texture2D;
    }

}