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
        // Find nearest valid guard we're looking at to attack
        var guards = thief.ThiefSenses.AwareGuards;
        float beatDistance = float.MaxValue;
        foreach(var g in guards)
        {
            if (g.AgentBlackboard.GetVariable<bool>("isStunned"))
                continue;

            float angleToGuard = Vector3.Angle(thief.transform.forward,
                    (g.transform.position - thief.transform.position).normalized);

            if (angleToGuard <= thief.AggroAngle)
            {
                float distanceToGuard = Vector3.Distance(thief.transform.position, g.transform.position);
                if (distanceToGuard <= thief.AggroRadius)
                {
                    if (distanceToGuard <= beatDistance)
                    {
                        beatDistance = distanceToGuard;
                        targetGuard = g;
                    }
                }
            }
        }
    }

    public override void PerformAction(ThiefAgent thief)
    {
        if (targetGuard == null)
            return;

        if (animationStarted)
            return;

        thief.AttackAgent(targetGuard);
        animationStarted = true;
    }

    public override void ExitAction(ThiefAgent thief)
    {
        // Warp NavAgents back to actual position as they can drift when the animation starts
        thief.NavAgent.Warp(thief.transform.position);

        if (targetGuard != null)
            targetGuard.NavAgent.Warp(targetGuard.transform.position);

        animationStarted = false;
        targetGuard = null;
    }

    public override void OnSceneGUI()
    {
        return;
    }
}