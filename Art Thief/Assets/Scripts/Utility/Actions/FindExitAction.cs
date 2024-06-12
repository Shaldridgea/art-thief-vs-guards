using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindExitAction : UtilityAction
{
    public FindExitAction(ActionData newData) : base(newData) { }

    public override void EnterAction(ThiefAgent thief)
    {
        return;
    }

    public override void PerformAction(ThiefAgent thief)
    {
        thief.MoveAgent(new Vector3(0f, 1f, 0f));
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
