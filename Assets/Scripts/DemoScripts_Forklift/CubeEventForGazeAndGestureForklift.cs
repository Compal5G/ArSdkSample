using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeEventForGazeAndGestureForklift : MonoBehaviour
{
    [SerializeField] CameraGazeHandler gazeHandler;
    [SerializeField] GameObject cube0, cube1;
    [SerializeField] Transform fork;
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
            fork.localPosition = new Vector3(0.9635f, 2.2295f, 0);
        }
        currGes = newGes;

        if (startUpMoving)
        {
            if (fork.localPosition.y <= 6)
            {
                fork.position += new Vector3(0, 0.01f, 0);
            }
        }
        if (startDownMoving)
        {
            if (fork.localPosition.y >= 2.2295f)
            {
                fork.position -= new Vector3(0, 0.01f, 0);
            }
        }
    }

    bool startUpMoving = false;
    bool startDownMoving = false;
    void StartGazeEvent(Collider obj)
    {
        if (oneShot == false && obj.gameObject != oneShotGameObject)
        {
            oneShot = true;
            oneShotGameObject = obj.gameObject;
            if (obj.gameObject == cube0)
            {
                startUpMoving = true;
            }
            else if (obj.gameObject == cube1)
            {
                startDownMoving = true;
            }
            else
            {
                startUpMoving = false;
                startDownMoving = false;
                oneShot = false;
                oneShotGameObject = null;
            }
        }
    }

    void EndGazeEvent(Collider obj)
    {
        startUpMoving = false;
        startDownMoving = false;   
        oneShot = false;
        oneShotGameObject = null;
    }

    public void testYA()
    {
        
    }
}
