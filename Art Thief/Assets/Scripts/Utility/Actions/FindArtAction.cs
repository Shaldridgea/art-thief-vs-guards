using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindArtAction : UtilityAction
{
    public FindArtAction(ActionData newData) : base(newData) { }

    public override void EnterAction(ThiefAgent thief)
    {
        thief.AgentBlackboard.SetVariable("artPosition", GameController.Instance.ArtGoal.transform.position);
        Debug.Log(thief.AgentBlackboard.GetVariable<Vector3>("artPosition"));
    }

    public override void PerformAction(ThiefAgent thief)
    {
        if(!thief.NavAgent.hasPath)
            thief.MoveAgent(thief.AgentBlackboard.GetVariable<Vector3>("artPosition"));
    }

    public override void ExitAction(ThiefAgent thief)
    {
        thief.NavAgent.ResetPath();
    }

    public override void OnSceneGUI()
    {
        return;
    }
}
