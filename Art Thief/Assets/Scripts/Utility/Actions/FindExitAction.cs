using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindExitAction : UtilityAction
{
    public FindExitAction(ActionData newData) : base(newData) { }

    public override void PerformAction(ThiefAgent thief)
    {
        thief.MoveAgent(new Vector3(0f, 1f, 0f));
    }

    public override void StopAction(ThiefAgent thief)
    {
        thief.NavAgent.ResetPath();
    }
}
