using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;    



public class NavCode : MonoBehaviour
{
    [SerializeField]
    Transform target;
    [SerializeField]
    float startWidth=1, endWidth=1;
    [SerializeField]
    Material material;
    [SerializeField]
    GameObject ARGameObjectLock;

    private NavMeshPath path;
    private LineRenderer line;
    void Start()
    {
        line = gameObject.AddComponent<LineRenderer>();
        path = new NavMeshPath();
        //只有設置了材質 setColor纔有作用
        line.startWidth = startWidth;//設置直線寬度
        line.endWidth = endWidth;//設置直線寬度
        line.material = material;
    }

    void Update()
    {
        if (ARGameObjectLock.transform.localScale.x != 0)
        {
            NavMesh.CalculatePath(transform.position, target.position, NavMesh.AllAreas, path);
            line.positionCount = path.corners.Length;
            for (int i = 0; i < path.corners.Length; i++)
            {
                line.SetPosition(i, path.corners[i]);
            }
        }
           }
    }
