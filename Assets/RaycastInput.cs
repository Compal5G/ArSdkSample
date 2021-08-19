using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RaycastInput : MonoBehaviour
{
    Material headPointMaterial;
    RaycastHit hitInfo;
    Transform hitTransform;
    protected Transform headPointTransform;
    protected Transform camTransform;

    protected virtual void Awake()
    {
        GameObject headPointPrefab = Resources.Load<GameObject>("HeadPoint");
        headPointTransform = Instantiate(headPointPrefab, transform).transform;
        headPointMaterial = headPointTransform.GetComponent<MeshRenderer>().material;
        headPointMaterial.SetColor("_Color", Color.white);
        camTransform = transform;
    }

    protected virtual void Update()
    {
        Debug.DrawLine(camTransform.position, camTransform.position + camTransform.forward * 800, Color.blue);

        if (Physics.Raycast(camTransform.position, camTransform.forward, out hitInfo))
        {
            if (hitTransform != hitInfo.transform)
            {
                // 射線打到新的東西
                if (hitTransform != null)
                {
                    Exit();  // 上一個 Exit
                }
                Button button = hitInfo.transform.GetComponent<Button>();
                Toggle toggle = hitInfo.transform.GetComponent<Toggle>();
                EventTrigger eventTrigger = hitInfo.transform.GetComponent<EventTrigger>();
                InputField inputField = hitInfo.transform.GetComponent<InputField>();
                if (button != null || toggle != null || eventTrigger != null || inputField != null)
                {
                    Enter();
                }
            }
        }
        else
        {
            Exit();
        }
    }

    protected virtual void Enter()
    {
        hitTransform = hitInfo.transform;
        headPointMaterial.SetColor("_Color", Color.cyan);
    }

    protected virtual void Exit()
    {
        hitTransform = null;
        headPointMaterial.SetColor("_Color", Color.white);
    }

    protected void Click()
    {
        if (hitTransform != null)
        {
            Button button = hitTransform.GetComponent<Button>();
            if (button != null)
            {
                if (button.interactable)
                {
                    button.onClick.Invoke();
                }
            }
            Toggle toggle = hitTransform.GetComponent<Toggle>();
            if (toggle != null)
            {
                toggle.isOn = !toggle.isOn;
            }
            EventTrigger eventTrigger = hitTransform.GetComponent<EventTrigger>();
            if (eventTrigger != null)
            {
                foreach (EventTrigger.Entry entry in eventTrigger.triggers)
                {
                    if (entry.eventID == EventTriggerType.PointerClick)
                    {
                        entry.callback.Invoke(null);
                    }
                }
            }
            InputField inputField = hitInfo.transform.GetComponent<InputField>();
            if (inputField != null)
            {
                inputField.Select();
            }
        }
    }

    protected void Sumbit()
    {
        if (hitTransform != null)
        {
            EventTrigger eventTrigger = hitTransform.GetComponent<EventTrigger>();
            if (eventTrigger != null)
            {
                foreach (EventTrigger.Entry entry in eventTrigger.triggers)
                {
                    if (entry.eventID == EventTriggerType.Submit)
                    {
                        entry.callback.Invoke(null);
                    }
                }
            }
        }
    }
}
