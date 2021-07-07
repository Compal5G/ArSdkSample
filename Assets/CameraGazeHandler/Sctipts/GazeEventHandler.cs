using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GazeEventHandler : MonoBehaviour
{
    public enum GazeEventType
    {
        None,
        StartGaze,
        Gazing,
        EndGaze
    }

    public delegate void GazeHandler();

    GazeHandler StartGazeAction;
    GazeHandler GazingAction;
    GazeHandler EndGazeAction;

    public void RegisterStartGazeAction(GazeHandler action)
    {
        StartGazeAction = action;
    }

    public void RegisterGazingAction(GazeHandler action)
    {
        GazingAction = action;
    }

    public void RegisterEndGazeAction(GazeHandler action)
    {
        EndGazeAction = action;
    }

    public GazeHandler GetStartGazeAction()
    {
        return StartGazeAction;
    }

    public GazeHandler GetGazingAction()
    {
        return GazingAction;
    }

    public GazeHandler GetEndGazeAction()
    {
        return EndGazeAction;
    }
}
