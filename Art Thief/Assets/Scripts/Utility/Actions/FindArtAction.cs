using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Utility action that handles the thief agent pathing to its target art piece
/// </summary>
public class FindArtAction : UtilityAction
{
    public FindArtAction(ActionData newData) : base(newData) { }

    public override void EnterAction(ThiefAgent thief)
    {
        if (thief.ArtGoal == null)
            return;

        thief.AgentBlackboard.SetVariable("artPosition", thief.ArtGoal.transform.position);
        GameEventLog.Log("Thief is finding the art to steal");
    }

    public override void PerformAction(ThiefAgent thief)
    {
        if (thief.ArtGoal == null)
            return;

        if (!thief.NavAgent.hasPath)
            thief.MoveAgent(thief.AgentBlackboard.GetVariable<Vector3>("artPosition"));
    }

    public override void ExitAction(ThiefAgent thief)
    {
        thief.NavAgent.ResetPath();
    }
}
