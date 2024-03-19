using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UtilityBehaviour : MonoBehaviour
{
    [SerializeField]
    private ThiefAgent agent;

    [SerializeField]
    private List<MotiveData> motiveData;

    [SerializeField]
    private List<ActionData> actionData;

    [SerializeField]
    private float evaluationInterval;

    private float evalTimer;

    private List<MotiveValue> motiveList = new List<MotiveValue>();

    private Dictionary<string, MotiveValue> motiveDict = new Dictionary<string, MotiveValue>();

    private List<UtilityAction> actionList = new List<UtilityAction>();

    private List<UtilityAction> sortedActions;

    private float discontentment;

    private UtilityAction currentAction;

    private bool toggleGUI;

    // Start is called before the first frame update
    void Start()
    {
        // Create all our motives
        for(int i = 0; i < motiveData.Count; ++i)
        {
            motiveList.Add(new MotiveValue(agent, motiveData[i]));
            motiveDict.Add(motiveData[i].Motive, motiveList[i]);
        }

        // Create our actions and give the actions their relvant motives
        for (int i = 0; i < actionData.Count; ++i)
        {
            MotiveValue[] thisMotives = new MotiveValue[actionData[i].Motives.Length];
            for (int j = 0; j < thisMotives.Length; ++j)
                thisMotives[j] = motiveDict[actionData[i].Motives[j].MotiveName];
            actionList.Add(Consts.GetUtilityAction(actionData[i].Action, thisMotives));
        }
        sortedActions = new List<UtilityAction>(actionList);
    }

    // Update is called once per frame
    void Update()
    {
        // Toggle visualisation of our values
        if (Input.GetKeyDown(KeyCode.G))
            toggleGUI = !toggleGUI;

        // Run our evaluation timer
        evalTimer -= Time.deltaTime;
        if (evalTimer > 0f)
            return;
        else
            evalTimer = evaluationInterval;

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
        float currentUtility = 0f;
        for (int i = 0; i < actionList.Count; ++i)
        {
            float comparisonUtility = actionList[i].EvaluateUtility();
            if(comparisonUtility > currentUtility)
            {
                currentUtility = comparisonUtility;
                favouredAction = actionList[i];
            }
        }

        // Sort our actions by their score for visualisation purposes
        sortedActions.Sort((x, y) => y.Score.CompareTo(x.Score));

        // If the action we want to perform is different from the one we were doing before
        if (currentAction != favouredAction)
        {
            // Stop our current action and set our new one to do
            currentAction?.StopAction(agent);
            currentAction = favouredAction;
        }
    }
}