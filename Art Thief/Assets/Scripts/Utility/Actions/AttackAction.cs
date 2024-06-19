using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackAction : UtilityAction
{
    private GuardAgent targetGuard;

    private bool animationStarted;

    public AttackAction(ActionData newData) : base(newData) { }

    public override void EnterAction(ThiefAgent thief)
    {
        var guards = thief.ThiefSenses.AwareGuards;
        float beatDistance = float.MaxValue;
        foreach(var g in guards)
        {
            float angleToGuard = Vector3.Angle(thief.transform.forward, (g.transform.position - thief.transform.position).normalized);
            if (angleToGuard <= 20f)
            {
                float distanceToGuard = Vector3.Distance(thief.transform.position, g.transform.position);
                if(distanceToGuard <= beatDistance)
                {
                    beatDistance = distanceToGuard;
                    targetGuard = g;
                }
            }
        }

        if(targetGuard != null)
        {
            thief.transform.LookAt(targetGuard.transform, Vector3.up);
            targetGuard.transform.LookAt(thief.transform, Vector3.up);
            thief.NavAgent.ResetPath();
            targetGuard.NavAgent.ResetPath();
            thief.NavAgent.updatePosition = false;
            thief.NavAgent.updateRotation = false;
            targetGuard.NavAgent.updatePosition = false;
            targetGuard.NavAgent.updateRotation = false;
        }
    }

    public override void PerformAction(ThiefAgent thief)
    {
        if (targetGuard == null)
            return;

        if (animationStarted)
            return;

        thief.PlayFightingAnimation(false);
        targetGuard.PlayFightingAnimation(true);
        targetGuard.AgentBlackboard.SetVariable("isStunned", true);
        animationStarted = true;
    }

    public override void ExitAction(ThiefAgent thief)
    {
        thief.NavAgent.updatePosition = true;
        thief.NavAgent.updateRotation = true;
        if(targetGuard != null)
        {
            targetGuard.NavAgent.updatePosition = true;
            targetGuard.NavAgent.updateRotation = true;
        }
        animationStarted = false;
        targetGuard = null;
    }

    public override void OnSceneGUI()
    {
        return;
    }
}