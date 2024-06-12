using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UtilityBehaviour : MonoBehaviour
{
    [SerializeField]
    private ThiefAgent agent;

    [SerializeField]
    private List<ActionData> actionData;

    [SerializeField]
    private float updateInterval;

    private float updateTimer;

    private List<UtilityAction> actionList = new List<UtilityAction>();

    private List<UtilityAction> sortedActions;

    private UtilityAction currentAction;

    private bool toggleGUI;

    // Start is called before the first frame update
    void Start()
    {
        // Create our actions and give the actions their relvant motives
        for (int i = 0; i < actionData.Count; ++i)
            actionList.Add(Consts.GetUtilityAction(actionData[i]));
        sortedActions = new List<UtilityAction>(actionList);
    }

    // Update is called once per frame
    void Update()
    {
        // Toggle visualisation of our values
        if (Input.GetKeyDown(KeyCode.G))
            toggleGUI = !toggleGUI;

        // Run our evaluation timer
        updateTimer -= Time.deltaTime;
        if (updateTimer > 0f)
            return;
        else
            updateTimer = updateInterval;

        // Calculate our new action to take
        EvaluateUtility();
        if (currentAction == null)
            return;

        // Perform our desired action
        currentAction.PerformAction(agent);
    }

    private void EvaluateUtility()
    {
        // Evaluate the utility of all our current actions and choose the one that has the highest score
        UtilityAction favouredAction = null;
        float currentUtility = float.MinValue;
        for (int i = 0; i < actionList.Count; ++i)
        {
            float comparisonUtility = actionList[i].CalculateScore(agent.AgentBlackboard);
            if(comparisonUtility > currentUtility)
            {
                currentUtility = comparisonUtility;
                favouredAction = actionList[i];
            }
        }

        // Sort our actions by their score for visualisation purposes
        sortedActions.Sort((x, y) => y.Score.CompareTo(x.Score));

        // If the action we want to perform is different from the one we were doing before
        if (favouredAction != currentAction)
        {
            // Stop our current action and set our new one to do
            currentAction?.ExitAction(agent);
            currentAction = favouredAction;
            currentAction.EnterAction(agent);
        }
    }

    private void OnGUI()
    {
        GUIStyle style = new GUIStyle("box");
        style.fontSize = 20;
        GUILayout.Box(name, style);
        style.fontSize = 15;
        for (int i = 0; i < actionList.Count; ++i)
            GUILayout.Box($"{actionData[i].Action}: {actionList[i].Score}", style);
    }

    public UtilityAction.DebugDrawCallback GetDebugDrawCallback(){
        if (currentAction != null)
            return currentAction.OnSceneGUI;

        return null;
    }
}