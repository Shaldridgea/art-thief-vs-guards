using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SimulationHUD : MonoBehaviour
{
    [SerializeField]
    private Slider simulationSpeedSlider;

    [SerializeField]
    private TextMeshProUGUI speedText;

    [SerializeField]
    private LayerMask agentLayerMask;

    [SerializeField]
    private Toggle lockCameraToggle;

    [SerializeField]
    private CameraControl gameCameraControl;

    [Header("Agent Info Views")]
    [SerializeField]
    private GameObject blackboardView;

    [SerializeField]
    private GameObject utilityView;

    [SerializeField]
    private GameObject treeView;

    [Header("Context Menu")]
    [SerializeField]
    private GameObject contextMenu;

    [SerializeField]
    private Button monitorBoardButton;

    [SerializeField]
    private Button monitorAgentButton;

    private Agent contextAgentTarget;

    // Start is called before the first frame update
    void Start()
    {
        simulationSpeedSlider.onValueChanged.AddListener(HandleSpeedSliderChange);
        monitorBoardButton.onClick.AddListener(HandleMonitorBlackboard);
        monitorAgentButton.onClick.AddListener(HandleMonitorAgent);
        lockCameraToggle.onValueChanged.AddListener(HandleLockCameraToggle);
    }

    private void HandleSpeedSliderChange(float newValue)
    {
        Time.timeScale = newValue;
        speedText.text = $"Simulation speed: {newValue}x";
    }

    private void HandleMonitorBlackboard()
    {
        blackboardView.SetActive(true);
        blackboardView.GetComponent<BlackboardView>().SetTarget(contextAgentTarget);
    }

    private void HandleMonitorAgent()
    {
        if(contextAgentTarget is ThiefAgent thief)
        {
            utilityView.SetActive(true);
            utilityView.GetComponent<ThiefUtilityView>().SetTarget(thief);
        }
        else
        if(contextAgentTarget is GuardAgent guard)
        {
            treeView.SetActive(true);
            treeView.GetComponentInChildren<BTRuntimeViewGraph>().SetTarget(guard);
        }
    }

    private void HandleLockCameraToggle(bool toggle)
    {
        gameCameraControl.SetCameraMode(toggle ? CameraControl.CameraMode.Orbit : CameraControl.CameraMode.Free);
    }

    private void Update()
    {
        // Right click context menu for bringing up visual aids
        if(Input.GetMouseButtonDown(1))
        {
            // Raycast for any agents we can right click on
            if(Physics.Raycast(
                Camera.main.ScreenPointToRay(Input.mousePosition),
                out RaycastHit hitInfo, 300f, agentLayerMask.value))
            {
                contextMenu.SetActive(true);

                // Set the context menu's position to be where we clicked
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(transform as RectTransform,
                    Input.mousePosition, null, out Vector2 localPoint))
                {
                    (contextMenu.transform as RectTransform).anchoredPosition = localPoint;
                }

                // Change context menu prompts based on thief or guard
                if(hitInfo.transform.TryGetComponent(out ThiefAgent thief))
                {
                    contextAgentTarget = thief;
                    monitorAgentButton.GetComponentInChildren<TextMeshProUGUI>().text = "View Utility";
                }
                else
                if (hitInfo.transform.TryGetComponent(out GuardAgent guard))
                {
                    contextAgentTarget = guard;
                    monitorAgentButton.GetComponentInChildren<TextMeshProUGUI>().text = "View Behaviour Tree";
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
            contextMenu.SetActive(false);

        if (Input.GetKeyDown(KeyCode.Space))
            simulationSpeedSlider.value = Time.timeScale >= 1f ? 0f : 1f;
    }
}
