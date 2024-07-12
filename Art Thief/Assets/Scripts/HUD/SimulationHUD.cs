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

    private void Update()
    {
        if(Input.GetMouseButtonDown(1))
        {
            if(Physics.Raycast(
                Camera.main.ScreenPointToRay(Input.mousePosition),
                out RaycastHit hitInfo, 300f, agentLayerMask.value))
            {
                Debug.Log(hitInfo.transform.gameObject);
                contextMenu.SetActive(true);
                if(hitInfo.transform.TryGetComponent(out ThiefAgent thief))
                {
                    contextAgentTarget = thief;
                    monitorAgentButton.GetComponentInChildren<TextMeshProUGUI>().text = "Monitor Utility";
                }
                else
                if (hitInfo.transform.TryGetComponent(out GuardAgent guard))
                {
                    contextAgentTarget = guard;
                    monitorAgentButton.GetComponentInChildren<TextMeshProUGUI>().text = "Monitor Behaviour Tree";
                }
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            contextMenu.SetActive(false);
        }
    }
}
