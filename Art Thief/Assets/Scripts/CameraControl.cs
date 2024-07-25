using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public enum CameraMode
    {
        Orbit,
        Free
    }

    [Header("Free")]
    [SerializeField]
    private float speed = 5f;

    [Header("Orbiting")]
    [SerializeField]
    private float cameraDistance;

    [SerializeField]
    private float scrollSensitivity = 1f;

    [SerializeField]
    private float turnSensitivity = 10f;

    private ThiefAgent thief;

    private Camera cam;

    private CameraMode mode;

    private Vector3 freeEuler, orbitEuler;

    private Vector3 cameraVector;

    // Start is called before the first frame update
    void Start()
    {
        thief = Level.Instance.Thief;
        cam = Camera.main;
        orbitEuler = new Vector3(-45f, 0f, 0f);
        cameraVector = new Vector3(0f,0f,cameraDistance);
    }

    private void OnEnable()
    {
        freeEuler = transform.rotation.eulerAngles;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        // Toggle the cursor locking
        if (Input.GetKeyDown(KeyCode.C))
        {
            Cursor.visible = !Cursor.visible;
            Cursor.lockState = Cursor.visible ? CursorLockMode.None : CursorLockMode.Locked;
        }

        if (Cursor.lockState != CursorLockMode.Locked)
            return;

        switch (mode)
        {
            case CameraMode.Free:
                UpdateFreeCam();
                break;

            case CameraMode.Orbit:
                UpdateOrbitingCam();
                break;
        }
    }

    public void SetCameraMode(CameraMode newMode) => mode = newMode;

    private void UpdateFreeCam()
    {
        Vector3 movementVector = new Vector3(
            Input.GetAxisRaw("FreeCam_X"), Input.GetAxisRaw("FreeCam_Y"), Input.GetAxisRaw("FreeCam_Z")).normalized;

        float camSpeed = speed * (Input.GetKey(KeyCode.LeftShift) ? 2f : 1f);
        cam.transform.position += camSpeed * Time.unscaledDeltaTime * cam.transform.TransformVector(movementVector);

        Vector3 turnVector =
            new Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0f) * turnSensitivity * Time.unscaledDeltaTime;
        freeEuler = new Vector3(Mathf.Clamp(freeEuler.x+turnVector.x, -90f, 90f), freeEuler.y + turnVector.y);
        cam.transform.rotation = Quaternion.Euler(freeEuler);
    }

    private void UpdateOrbitingCam()
    {
        // Turn camera using the mouse
        orbitEuler += Time.unscaledDeltaTime * turnSensitivity * new Vector3(
            Input.GetAxis("Mouse Y"),
            Input.GetAxis("Mouse X"), 0f);

        orbitEuler.x = Mathf.Clamp(orbitEuler.x, -89f, 89f);

        Vector3 thiefOrigin = thief.AgentView.AgentHeadRoot.position;
        // Move camera around the spy and make the camera look at the spy
        float rayDistance = cameraDistance;
        Vector3 desiredPoint = thiefOrigin + Quaternion.Euler(orbitEuler) * cameraVector;
        if (Physics.Raycast(thiefOrigin, (desiredPoint - thiefOrigin).normalized,
            out RaycastHit info, cameraDistance, LayerMask.GetMask("Default", "Floor")))
            rayDistance = info.distance-0.1f;

        if (Mathf.Abs(cameraVector.z-rayDistance) > 0.1f)
            cameraVector.z = Mathf.Lerp(cameraVector.z, rayDistance, 4f * Time.unscaledDeltaTime);
        else
            cameraVector.z = Mathf.MoveTowards(cameraVector.z, rayDistance, 0.1f * Time.unscaledDeltaTime);
        cam.transform.position = thiefOrigin + Quaternion.Euler(orbitEuler) * cameraVector;
        cam.transform.LookAt(thiefOrigin);

        // Zoom the camera in or out
        if (Input.mouseScrollDelta.y != 0f)
        {
            cameraDistance += (-Input.mouseScrollDelta.y * scrollSensitivity) * Time.unscaledDeltaTime;
            cameraDistance = Mathf.Clamp(cameraDistance, 1f, 10f);
        }
    }
}