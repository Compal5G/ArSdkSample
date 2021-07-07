using UnityEngine;

public class FreeCameraUpRight : MonoBehaviour
{
    bool enableRotation = false;

    public float sensitivity = 10f;
    public float keyboardRotationSensitivity = 10f;
    public float maxYAngle = 80f;
    private Vector2 currentRotation;

    void Update()
    {
        // if (Input.GetMouseButtonDown(1))
        // {
        //     enableRotation = !enableRotation;
        // }

        // if (enableRotation)
        // {
        //     currentRotation.x += Input.GetAxis("Mouse X") * sensitivity;
        //     currentRotation.y -= Input.GetAxis("Mouse Y") * sensitivity;
        //     currentRotation.x = Mathf.Repeat(currentRotation.x, 360);
        //     currentRotation.y = Mathf.Clamp(currentRotation.y, -maxYAngle, maxYAngle);
        //     Camera.main.transform.rotation = Quaternion.Euler(currentRotation.y, currentRotation.x, 0);
        //     // Camera.main.transform.Rotate(-Input.GetAxis("Mouse Y") * sensitivity, Input.GetAxis("Mouse X") * sensitivity, 0f);
        //     if (Input.GetMouseButtonDown(0))
        //         Cursor.lockState = CursorLockMode.Locked;
        // }

        if (Input.GetKey(KeyCode.UpArrow))
        {
            currentRotation.y -= sensitivity * Time.deltaTime * keyboardRotationSensitivity;
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            currentRotation.y += sensitivity * Time.deltaTime * keyboardRotationSensitivity;
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            currentRotation.x -= sensitivity * Time.deltaTime * keyboardRotationSensitivity;
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            currentRotation.x += sensitivity * Time.deltaTime * keyboardRotationSensitivity;
        }

        currentRotation.x = Mathf.Repeat(currentRotation.x, 360);
        currentRotation.y = Mathf.Clamp(currentRotation.y, -maxYAngle, maxYAngle);
        Camera.main.transform.rotation = Quaternion.Euler(currentRotation.y, currentRotation.x, 0);

        if (Input.GetKey(KeyCode.W))
        {
            // transform.position += transform.forward * sensitivity * Time.deltaTime;
            transform.Translate(Vector3.forward * sensitivity * Time.deltaTime);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            transform.position -= transform.forward * sensitivity * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.A))
        {
            transform.position -= transform.right * sensitivity * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            transform.position += transform.right * sensitivity * Time.deltaTime;
        }
    }
}