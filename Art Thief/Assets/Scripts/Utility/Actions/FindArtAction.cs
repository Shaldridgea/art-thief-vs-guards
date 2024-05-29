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

        if (thief.NavAgent.hasPath)
            thief.AgentBlackboard.SetVariable("nearToArt", thief.NavAgent.remainingDistance <= 1f ? 1f : 0f);
    }

    public override void ExitAction(ThiefAgent thief)
    {
        thief.NavAgent.ResetPath();
    }
}
