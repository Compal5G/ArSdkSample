using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GazeEventHandler))]
public class TestGazeEventHandler : MonoBehaviour
{
    GazeEventHandler eventHandler;

    // Start is called before the first frame update
    void Start()
    {
        eventHandler = this.GetComponent<GazeEventHandler>();
        eventHandler.RegisterStartGazeAction(TestStartGazeAction);
        eventHandler.RegisterGazingAction(TestGazingAction);
        eventHandler.RegisterEndGazeAction(TestEndGazeAction);
    }

    public void TestStartGazeAction()
    {
        Debug.Log(this.name + " start gaze");
    }

    public void TestGazingAction()
    {
        Debug.Log(this.name + " gazing");
    }

    public void TestEndGazeAction()
    {
        Debug.Log(this.name + " end gaze");
    }
}
