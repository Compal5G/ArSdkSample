using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeEventForGazeAndGesture : MonoBehaviour
{
    [SerializeField] CameraGazeHandler gazeHandler;
    [SerializeField] GameObject cube0, cube1;
    [SerializeField] Animator unityChanAnimator;
    [SerializeField] bool oneShot = false;
    [SerializeField] bool test = false;
    [SerializeField] GameObject oneShotGameObject = null;
    private GestureInfo currGes;

    void Start()
    {
        gazeHandler.StartGazeEvent += StartGazeEvent;
        gazeHandler.EndGazeEvent += EndGazeEvent;
    }


    void Update()
    {
        GestureInfo newGes = XvGesture.GetGesture();
        if (newGes.id != currGes.id)
        {
            if (!unityChanAnimator.GetBool("YA") && newGes.id == 6)
            {
                unityChanAnimator.SetBool("YA", true);
            }
            else if (newGes.id == 11)//上
            {
                unityChanAnimator.gameObject.transform.Rotate(new Vector3(-10,0,0));
            }
            else if (newGes.id == 12)//下
            {
                unityChanAnimator.gameObject.transform.Rotate(new Vector3(10, 0, 0));
            }
            else if (newGes.id == 13)//左
            {
                unityChanAnimator.gameObject.transform.Rotate(new Vector3(0, 10, 0));
            }
            else if (newGes.id == 14)//右
            {
                unityChanAnimator.gameObject.transform.Rotate(new Vector3(0, -10, 0));
            }
        }
        currGes = newGes;
    }
    void StartGazeEvent(Collider obj)
    {
        if (oneShot == false && obj.gameObject != oneShotGameObject)
        {
            oneShot = true;
            oneShotGameObject = obj.gameObject;
            if (obj.gameObject == cube0)
            {
                unityChanAnimator.SetBool("Next", true);
            }
            else if (obj.gameObject == cube1)
            {
                unityChanAnimator.SetBool("Back", true);
            }
            else
            {
                oneShot = false;
                oneShotGameObject = null;
            }
        }

       
    }

    void EndGazeEvent(Collider obj)
    {
        oneShot = false;
        oneShotGameObject = null;
    }

    public void testYA()
    {
        if (!unityChanAnimator.GetBool("YA"))
        {
            unityChanAnimator.SetBool("YA", true);
        }
    }
}
