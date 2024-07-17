using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindArtAction : UtilityAction
{
    public FindArtAction(ActionData newData) : base(newData) { }

    public override void EnterAction(ThiefAgent thief)
    {
        if (thief.ArtGoal == null)
            return;

        thief.AgentBlackboard.SetVariable("artPosition", thief.ArtGoal.transform.position);
    }

    public override void PerformAction(ThiefAgent thief)
    {
        if (thief.ArtGoal == null)
            return;

        if (!thief.NavAgent.hasPath)
            thief.MoveAgent(thief.AgentBlackboard.GetVariable<Vector3>("artPosition"));

        thief.AgentBlackboard.SetVariable("nearToArt",
            Vector3.Distance(thief.transform.position.ZeroY(), thief.ArtGoal.transform.position.ZeroY()) <= 1f ? 1f : 0f);
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
