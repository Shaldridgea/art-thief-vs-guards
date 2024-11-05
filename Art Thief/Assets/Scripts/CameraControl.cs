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

    [SerializeField]
    private float maxCameraDistanceFromMiddle = 70f;

    public Agent CameraTarget { get; set; }

    private Camera cam;

    private CameraMode mode;

    private Vector3 freeEuler, orbitEuler;

    private Vector3 cameraVector;

    private bool controllingCamera;

    void Start()
    {
        CameraTarget = Level.Instance.Thief;
        cam = Camera.main;
        orbitEuler = new Vector3(-45f, 0f, 0f);
        cameraVector = new Vector3(0f, 0f, cameraDistance);
    }

    private void OnEnable()
    {
        freeEuler = transform.rotation.eulerAngles;
    }

    void LateUpdate()
    {
        // Toggle controlling camera while holding down right click
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
        if (controllingCamera)
        {
            cam.transform.position += camSpeed * Time.unscaledDeltaTime * cam.transform.TransformVector(movementVector);

            Vector3 vectorFromMiddle = cam.transform.position - Level.Instance.LevelMiddlePoint;

            // We don't want the free cam to fly away from the level and get lost forever,
            // so clamp how far it can be from the middle of the level
            if (vectorFromMiddle.magnitude > maxCameraDistanceFromMiddle)
                cam.transform.position = Level.Instance.LevelMiddlePoint +
                    Vector3.ClampMagnitude(vectorFromMiddle, maxCameraDistanceFromMiddle);
        }

        if (controllingCamera)
        {
            Vector3 turnVector =
            new Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0f) * turnSensitivity;
            freeEuler = new Vector3(Mathf.Clamp(freeEuler.x + turnVector.x, -90f, 90f), freeEuler.y + turnVector.y);
            cam.transform.rotation = Quaternion.Euler(freeEuler);
        }
    }

    private void UpdateOrbitingCam()
    {
        if (controllingCamera)
        {
            orbitEuler += turnSensitivity * new Vector3(Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0f);
            
            // Zoom the camera in or out
            if (Input.mouseScrollDelta.y != 0f)
            {
                cameraDistance += (-Input.mouseScrollDelta.y * scrollSensitivity) * Time.unscaledDeltaTime;
                cameraDistance = Mathf.Clamp(cameraDistance, 1f, 10f);
            }
        }

        orbitEuler.x = Mathf.Clamp(orbitEuler.x, -89f, 89f);

        Vector3 thiefOrigin = CameraTarget.AgentView.AgentHeadRoot.position;
        float rayDistance = cameraDistance;

        // Don't want the camera to be clipping through walls,
        // so make a raycast from the agent's head to push it away
        Vector3 orbitPoint = thiefOrigin + Quaternion.Euler(orbitEuler) * cameraVector;

        if (Physics.Raycast(thiefOrigin, (orbitPoint - thiefOrigin).normalized,
            out RaycastHit info, cameraDistance, LayerMask.GetMask("Default", "Floor")))
        {
            rayDistance = Mathf.Max(info.distance - 0.1f, 0.05f);
        }

        if (rayDistance > cameraVector.z)
        {
            // We lerp quickly if the camera is far away, and move it to the
            // correct distance when it's much smaller/unnoticeable. We want the
            // movement to be smooth but not be infinitely lerping
            if (Mathf.Abs(cameraVector.z - rayDistance) > 0.05f)
                cameraVector.z = Mathf.Lerp(cameraVector.z, rayDistance, 4f * Time.unscaledDeltaTime);
            else
                cameraVector.z = Mathf.MoveTowards(cameraVector.z, rayDistance, 0.05f * Time.unscaledDeltaTime);
        }
        else // If our distance is closer to the agent we instantly change it so as to not clip through walls
            cameraVector.z = rayDistance;

        cam.transform.position = thiefOrigin + Quaternion.Euler(orbitEuler) * cameraVector;
        cam.transform.LookAt(thiefOrigin);

        // Cache our camera's current angle in the free cam's angles so the
        // camera angle doesn't snap when we change mode
        freeEuler = cam.transform.eulerAngles;
    }
}