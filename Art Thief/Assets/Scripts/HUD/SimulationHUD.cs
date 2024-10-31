using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// HUD for controlling simulation while in play
/// </summary>
public class SimulationHUD : MonoBehaviour
{
    [SerializeField]
    private LayerMask agentLayerMask;

    [SerializeField]
    private CameraControl gameCameraControl;

    [Header("Simulation UI")]
    [SerializeField]
    private Slider simulationSpeedSlider;

    [SerializeField]
    private TextMeshProUGUI speedText;

    [SerializeField]
    private Button changeCameraLeftButton;

    [SerializeField]
    private Button changeCameraRightButton;

    [SerializeField]
    private TextMeshProUGUI cameraTargetNameText;

    [SerializeField]
    private Toggle lockCameraToggle;

    [SerializeField]
    private TextMeshProUGUI thiefWinText;

    [SerializeField]
    private TextMeshProUGUI guardsWinText;

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

    private List<Agent> cameraTargetList;

    private int cameraTargetIndex;

    void Start()
    {
        // Setup listener callbacks for all the UI
        simulationSpeedSlider.onValueChanged.AddListener(HandleSpeedSliderChange);
        monitorBoardButton.onClick.AddListener(HandleMonitorBlackboard);
        monitorAgentButton.onClick.AddListener(HandleMonitorAgent);
        changeCameraLeftButton.onClick.AddListener(HandleChangeCameraLeft);
        changeCameraRightButton.onClick.AddListener(HandleChangeCameraRight);
        lockCameraToggle.onValueChanged.AddListener(HandleLockCameraToggle);

        // Add agents to list to scroll through
        cameraTargetList = new(5);
        cameraTargetList.Add(Level.Instance.Thief);
        cameraTargetList.AddRange(Level.Instance.GuardList);
    }

    public void ShowWinnerText(Consts.Team winner)
    {
        guardsWinText.gameObject.SetActive(winner == Consts.Team.GUARD);
        thiefWinText.gameObject.SetActive(winner == Consts.Team.THIEF);
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
            treeView.GetComponentInChildren<BTRuntimeView>().SetTarget(guard);
        }
    }

    private void HandleChangeCameraLeft()
    {
        cameraTargetIndex = (cameraTargetIndex - 1) % cameraTargetList.Count - 1;

        while (!cameraTargetList[cameraTargetIndex].gameObject.activeSelf)
            cameraTargetIndex = (cameraTargetIndex - 1) % cameraTargetList.Count - 1;

        GameController.Instance.CameraController.CameraTarget = cameraTargetList[cameraTargetIndex];
        cameraTargetNameText.text = GameController.Instance.CameraController.CameraTarget.name;
    }

    private void HandleChangeCameraRight()
    {
        cameraTargetIndex = (cameraTargetIndex + 1) % cameraTargetList.Count;

        while (!cameraTargetList[cameraTargetIndex].gameObject.activeSelf)
            cameraTargetIndex = (cameraTargetIndex + 1) % cameraTargetList.Count;

        GameController.Instance.CameraController.CameraTarget = cameraTargetList[cameraTargetIndex];
        cameraTargetNameText.text = GameController.Instance.CameraController.CameraTarget.name;
    }

    private void HandleLockCameraToggle(bool toggle)
    {
        gameCameraControl.SetCameraMode(toggle ? CameraControl.CameraMode.Orbit : CameraControl.CameraMode.Free);
    }

    private void Update()
    {
        // Left click context menu for bringing up visual aids
        if(Input.GetMouseButtonUp(0) && !contextMenu.activeSelf)
        {
            // Raycast for any agents we can left click on
            if(Physics.Raycast(GameController.Instance.GameCamera.ScreenPointToRay(Input.mousePosition),
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
        else
        if (Input.GetMouseButtonUp(0) && contextMenu.activeSelf)
            contextMenu.SetActive(false);

        if (Input.GetKeyDown(KeyCode.Space))
            simulationSpeedSlider.value = Time.timeScale >= 1f ? 0f : 1f;
    }
}
