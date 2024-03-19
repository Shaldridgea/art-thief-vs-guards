using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    [SerializeField]
    private ThiefAgent playerAgent;

    [SerializeField]
    private Vector3 cameraDistance;

    [SerializeField]
    private float scrollSensitivity = 1f;

    [SerializeField]
    private float turnSensitivity = 10f;

    private Camera cam;

    private Quaternion cameraAngle;

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
        cameraAngle = Quaternion.Euler(-45f, 0f, 0f);
    }

    // Update is called once per frame
    void Update()
    {
        // Turn camera using the mouse
        cameraAngle.eulerAngles += new Vector3(
            Input.GetAxis("Mouse Y") * turnSensitivity,
            Input.GetAxis("Mouse X") * turnSensitivity, 0f) * Time.deltaTime;

        // Move camera around the spy and make the camera look at the spy
        cam.transform.position = playerAgent.transform.position + cameraAngle * cameraDistance;
        cam.transform.LookAt(playerAgent.transform);

        // Zoom the camera in or out
        if(Input.mouseScrollDelta.y != 0f)
            cameraDistance.z += (Input.mouseScrollDelta.y * scrollSensitivity) * Time.deltaTime;

        // Toggle the cursor locking
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.visible = !Cursor.visible;
            Cursor.lockState = Cursor.visible ? CursorLockMode.None : CursorLockMode.Locked;
        }
    }
}