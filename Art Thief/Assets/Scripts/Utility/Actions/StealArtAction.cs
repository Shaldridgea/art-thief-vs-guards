using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StealArtAction : UtilityAction
{
    const float STEALING_SPEED = 10f;

    public StealArtAction(ActionData newData) : base(newData) { }

    public override void EnterAction(ThiefAgent thief)
    {
        thief.StealingProgressContainer.SetActive(true);
    }

    public override void PerformAction(ThiefAgent thief)
    {
        float stealProgress = thief.AgentBlackboard.GetVariable<float>("stealProgress");

        stealProgress += Time.deltaTime / STEALING_SPEED;

        thief.AgentBlackboard.SetVariable("stealProgress", stealProgress);
        thief.StealingProgressImage.fillAmount = stealProgress;

        if (stealProgress >= 1f)
            thief.TakeArt();
    }

    public override void ExitAction(ThiefAgent thief)
    {
        thief.StealingProgressContainer.SetActive(false);
    }

    public override void OnSceneGUI()
    {
        return;
    }
}
