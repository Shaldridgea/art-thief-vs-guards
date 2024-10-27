using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Utility AI behaviour controller
/// </summary>
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

    public List<UtilityAction> ActionList => actionList;

    private UtilityAction currentAction;

    void Start()
    {
        // Create our actions using our associated action data
        for (int i = 0; i < actionData.Count; ++i)
            actionList.Add(Consts.GetUtilityAction(actionData[i]));
    }

    void Update()
    {
        if (!agent.AgentActivated)
            return;

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

        // If the action we want to perform is different from the one
        // we were doing before, call our Exit function on the old action,
        // and run Enter function on the new action
        if (favouredAction != currentAction)
        {
            currentAction?.ExitAction(agent);
            currentAction = favouredAction;
            currentAction.EnterAction(agent);
        }
    }

#if UNITY_EDITOR
    public UtilityAction.DebugDrawCallback GetDebugDrawCallback(){
        if (currentAction != null)
            return currentAction.OnSceneGUI;

        return null;
    }
#endif
}