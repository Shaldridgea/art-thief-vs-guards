using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackAction : UtilityAction
{
    private GuardAgent targetGuard;

    private bool animationStarted;

    private HashSet<GuardAgent> attackedGuards = new();

    public AttackAction(ActionData newData) : base(newData) { }

    public override void EnterAction(ThiefAgent thief)
    {
        // Find nearest guard we're looking at to attack
        var guards = thief.ThiefSenses.AwareGuards;
        float beatDistance = float.MaxValue;
        foreach(var g in guards)
        {
            if (g.AgentBlackboard.GetVariable<bool>("isStunned"))
                continue;

            float angleToGuard = Vector3.Angle(thief.transform.forward, (g.transform.position - thief.transform.position).normalized);
            if (angleToGuard <= thief.AggroAngle)
            {
                float distanceToGuard = Vector3.Distance(thief.transform.position, g.transform.position);
                if(distanceToGuard <= beatDistance)
                {
                    beatDistance = distanceToGuard;
                    targetGuard = g;
                }
            }
        }

        // Immediately stop the thief and guard from moving, make them look at each other
        // and stop the NavAgent components from moving
        // or rotating the agents during their animated interaction
        if (targetGuard != null)
        {
            SetupAttack(thief);
        }
    }

    public override void PerformAction(ThiefAgent thief)
    {
        if (targetGuard == null)
            return;

        if (animationStarted)
            return;

        // Decide if thief wins the fight
        // Thief always wins the first fight against a guard,
        // after that it's a 50/50 chance
        bool doesThiefWinFight = true;
        if (attackedGuards.Contains(targetGuard))
        {
            if (Random.value <= 0.5f)
                doesThiefWinFight = false;
        }
        else
            attackedGuards.Add(targetGuard);

        // Start animated fight interaction between thief and guard
        thief.PlayFightSequence(doesThiefWinFight);
        targetGuard.PlayFightSequence(!doesThiefWinFight);
        // Let the guard know it's interacting so it won't do anything else
        targetGuard.AgentBlackboard.SetVariable("isInteracting", true);
        animationStarted = true;
    }

    public override void ExitAction(ThiefAgent thief)
    {
        // Warp agents back to actual position as they drift when the animation starts
        thief.NavAgent.Warp(thief.transform.position);

        if (targetGuard != null)
            targetGuard.NavAgent.Warp(targetGuard.transform.position);

        animationStarted = false;
        targetGuard = null;
    }

    private void SetupAttack(ThiefAgent thief)
    {
        thief.transform.LookAt(targetGuard.transform, Vector3.up);
        thief.NavAgent.ResetPath();
        thief.NavAgent.updatePosition = false;
        thief.NavAgent.updateRotation = false;
        thief.NavAgent.updateUpAxis = false;

        targetGuard.transform.LookAt(thief.transform, Vector3.up);
        targetGuard.NavAgent.ResetPath();
        targetGuard.NavAgent.updatePosition = false;
        targetGuard.NavAgent.updateRotation = false;
        targetGuard.NavAgent.updateUpAxis = false;
    }

    public override void OnSceneGUI()
    {
        return;
    }
}