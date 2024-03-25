using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class Cooldown : Decorator
{
    private float cooldownTime;

    private float cooldownStart;

    public Cooldown(BehaviourTree parentTree, NodeParameter[] parameters) : base(parentTree)
    {
        cooldownTime = parameters[0];
        cooldownStart = 1000000f;
    }

    public override Consts.NodeStatus Update()
    {
        Consts.NodeStatus status = Consts.NodeStatus.FAILURE;
        if (Status == Consts.NodeStatus.RUNNING)
        {
            if (childNode.Status != Consts.NodeStatus.RUNNING)
            {
                status = childNode.Status;
                cooldownStart = Time.time;
            }
            else
                status = Consts.NodeStatus.RUNNING;
        }
        else
        {
            float elapsedTime = Time.time - cooldownStart;
            if (elapsedTime >= cooldownTime)
            {
                status = childNode.Tick();
                cooldownStart = Time.time;
            }
        }

        return status;
    }
}