using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wait : BehaviourNode
{
    private float waitMax;

    private float waitTimer;

    public Wait(BehaviourTree parentTree, NodeParameter[] parameters) : base(parentTree)
    {
        waitMax = parameters[0];
    }

    public override void Reset()
    {
        base.Reset();
        waitTimer = 0f;
    }

    public override Consts.NodeStatus Update()
    {
        Consts.NodeStatus status = Consts.NodeStatus.SUCCESS;
        waitTimer += Time.deltaTime;
        if (waitTimer < waitMax)
            status = Consts.NodeStatus.RUNNING;

        return status;
    }
}