using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    enum CameraMode
    {
        Free,
        Orbit
    }

    [Header("Free")]
    [SerializeField]
    private float speed = 5f;

    [Header("Orbiting")]
    [SerializeField]
    private Vector3 cameraDistance;

    [SerializeField]
    private float scrollSensitivity = 1f;

    [SerializeField]
    private float turnSensitivity = 10f;

    private ThiefAgent thief;

    private Camera cam;

    private Quaternion cameraAngle;

    private CameraMode mode;

    private Vector3 eulerAngles;

    // Start is called before the first frame update
    void Start()
    {
        thief = Level.Instance.Thief;
        cam = Camera.main;
        cameraAngle = Quaternion.Euler(-45f, 0f, 0f);
    }

    private void OnEnable()
    {
        eulerAngles = transform.rotation.eulerAngles;
    }

    // Update is called once per frame
    void Update()
    {
        switch (mode)
        {
            case CameraMode.Free:
                UpdateFreeCam();
                break;

            case CameraMode.Orbit:
                UpdateOrbitingCam();
                break;
        }

        // Toggle the cursor locking
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.visible = !Cursor.visible;
            Cursor.lockState = Cursor.visible ? CursorLockMode.None : CursorLockMode.Locked;
        }
    }

    private void UpdateFreeCam()
    {
        Vector3 movementVector = new Vector3(
            Input.GetAxisRaw("FreeCam_X"), Input.GetAxisRaw("FreeCam_Y"), Input.GetAxisRaw("FreeCam_Z")).normalized;

        float camSpeed = speed * (Input.GetKey(KeyCode.LeftShift) ? 2f : 1f);
        cam.transform.position += cam.transform.TransformVector(movementVector) * camSpeed * Time.unscaledDeltaTime;

        Vector3 turnVector =
            new Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0f) * turnSensitivity * Time.unscaledDeltaTime;
        eulerAngles = new Vector3(Mathf.Clamp(eulerAngles.x+turnVector.x, -90f, 90f), eulerAngles.y + turnVector.y);
        cam.transform.rotation = Quaternion.Euler(eulerAngles);
    }

    private void UpdateOrbitingCam()
    {
        // Turn camera using the mouse
        cameraAngle.eulerAngles += new Vector3(
            Input.GetAxis("Mouse Y") * turnSensitivity,
            Input.GetAxis("Mouse X") * turnSensitivity, 0f) * Time.unscaledDeltaTime;

        // Move camera around the spy and make the camera look at the spy
        cam.transform.position = thief.transform.position + cameraAngle * cameraDistance;
        cam.transform.LookAt(thief.transform);

        // Zoom the camera in or out
        if (Input.mouseScrollDelta.y != 0f)
            cameraDistance.z += (Input.mouseScrollDelta.y * scrollSensitivity) * Time.unscaledDeltaTime;
    }
}