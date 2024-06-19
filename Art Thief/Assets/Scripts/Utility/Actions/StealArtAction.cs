using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StealArtAction : UtilityAction
{
    public StealArtAction(ActionData newData) : base(newData) { }

    public override void EnterAction(ThiefAgent thief)
    {
        return;
    }

    public override void PerformAction(ThiefAgent thief)
    {
        float stealProgress = thief.AgentBlackboard.GetVariable<float>("stealProgress");

        stealProgress += 0.5f * Time.deltaTime;

        thief.AgentBlackboard.SetVariable("stealProgress", stealProgress);

        if (stealProgress >= 1f)
            thief.TakeArt();
    }

    public override void ExitAction(ThiefAgent thief)
    {
        return;
    }

    public override void OnSceneGUI()
    {
        return;
    }
}
