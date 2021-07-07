using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class GazeCircleHandler : MonoBehaviour
{
    Vector2 pointOrigSize = new Vector2(24f, 24f);
    Vector2 pointEnlargeSize = new Vector2(100f, 100f);

    bool isPointAnimPlaying = false;
    bool objectGazed = false;

    // Point animation props
    float dura = 0.4f;
    float valDelta;
    Vector2 sizeDelta;

    public RawImage pointMask;
    public RawImage point;
    public Image circleFill;

    void Start()
    {

    }

    public void Reset()
    {
        pointMask.rectTransform.sizeDelta = Vector2.zero;
        point.rectTransform.sizeDelta = pointOrigSize;
        circleFill.fillAmount = 0f;
    }

    public async void StartGazing(Collider obj)
    {
        objectGazed = true;
        valDelta = (pointEnlargeSize.x - pointOrigSize.x) * Time.deltaTime / dura;
        sizeDelta = new Vector2(valDelta, valDelta);

        if (isPointAnimPlaying == false)
            await PlayPointAnim();
    }

    public async void EndGazing(Collider obj)
    {
        objectGazed = false;
        valDelta = -((pointEnlargeSize.x - pointOrigSize.x) * Time.deltaTime / dura);
        sizeDelta = new Vector2(valDelta, valDelta);

        if (isPointAnimPlaying == false)
            await PlayPointAnim();
    }

    async Task PlayPointAnim()
    {
        isPointAnimPlaying = true;
        Vector2 size;

        while (true)
        {
            size = pointMask.rectTransform.sizeDelta;
            size += sizeDelta;
            pointMask.rectTransform.sizeDelta = size;

            size = point.rectTransform.sizeDelta;
            size += sizeDelta;
            point.rectTransform.sizeDelta = size;

            if (objectGazed)
            {
                if (point.rectTransform.sizeDelta.x > pointEnlargeSize.x &&
                    point.rectTransform.sizeDelta.y > pointEnlargeSize.y)
                {
                    pointMask.rectTransform.sizeDelta = pointEnlargeSize - pointOrigSize;
                    point.rectTransform.sizeDelta = pointEnlargeSize;
                    break;
                }
            }
            else
            {
                if (point.rectTransform.sizeDelta.x < pointOrigSize.x &&
                    point.rectTransform.sizeDelta.y < pointOrigSize.y)
                {
                    pointMask.rectTransform.sizeDelta = Vector2.zero;
                    point.rectTransform.sizeDelta = pointOrigSize;
                    break;
                }
            }

            await Task.Yield();
        }

        isPointAnimPlaying = false;
    }
}
