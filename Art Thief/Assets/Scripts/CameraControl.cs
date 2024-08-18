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

    private bool controllingCamera;

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
        if (Input.GetMouseButtonDown(1) && !controllingCamera)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Confined;
            controllingCamera = true;
        }
        else if(Input.GetMouseButtonUp(1) && controllingCamera)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            controllingCamera = false;
        }

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
        if(controllingCamera)
            cam.transform.position += camSpeed * Time.unscaledDeltaTime * cam.transform.TransformVector(movementVector);

        if (controllingCamera)
        {
            Vector3 turnVector =
            new Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0f) * turnSensitivity * Time.unscaledDeltaTime;
            freeEuler = new Vector3(Mathf.Clamp(freeEuler.x + turnVector.x, -90f, 90f), freeEuler.y + turnVector.y);
            cam.transform.rotation = Quaternion.Euler(freeEuler);
        }
    }

    private void UpdateOrbitingCam()
    {
        // Turn camera using the mouse
        if (controllingCamera)
            orbitEuler += Time.unscaledDeltaTime * turnSensitivity * new Vector3(
            Input.GetAxis("Mouse Y"),
            Input.GetAxis("Mouse X"), 0f);

        orbitEuler.x = Mathf.Clamp(orbitEuler.x, -89f, 89f);

        Vector3 thiefOrigin = thief.AgentView.AgentHeadRoot.position;
        float rayDistance = cameraDistance;
        // Make a raycast out from the Agent's head to find any collision with the environment,
        // so the camera will be pushed closer by walls
        Vector3 desiredPoint = thiefOrigin + Quaternion.Euler(orbitEuler) * cameraVector;
        if (Physics.Raycast(thiefOrigin, (desiredPoint - thiefOrigin).normalized,
            out RaycastHit info, cameraDistance, LayerMask.GetMask("Default", "Floor")))
            rayDistance = info.distance-0.1f;

        // If the gotten ray distance is further than current camera position,
        // lerp the camera's distance and move it to the ray's distance smoothly
        if (rayDistance > cameraVector.z)
        {
            if (Mathf.Abs(cameraVector.z - rayDistance) > 0.05f)
                cameraVector.z = Mathf.Lerp(cameraVector.z, rayDistance, 4f * Time.unscaledDeltaTime);
            else
                cameraVector.z = Mathf.MoveTowards(cameraVector.z, rayDistance, 0.05f * Time.unscaledDeltaTime);
        }
        else // If our distance is closer to the agent we instantly change it
            cameraVector.z = rayDistance;

        // Move camera around the spy and make the camera look at the spy
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