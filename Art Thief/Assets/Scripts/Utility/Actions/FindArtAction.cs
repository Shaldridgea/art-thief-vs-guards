using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindArtAction : UtilityAction
{
    public FindArtAction(ActionData newData) : base(newData) { }

    public override void PerformAction(ThiefAgent thief)
    {
        thief.MoveAgent(thief.AgentBlackboard.GetVariable<Vector3>("artPosition"));
    }

    public override void StopAction(ThiefAgent thief)
    {
        thief.NavAgent.ResetPath();
    }
}
