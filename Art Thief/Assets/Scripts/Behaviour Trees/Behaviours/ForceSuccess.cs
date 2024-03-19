using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceSuccess : Decorator
{
    public ForceSuccess(BehaviourTree parentTree) : base(parentTree)
    {
        
    }

    public override Consts.NodeStatus Update()
    {
        Consts.NodeStatus status = childNode.Status;

        if (Status != Consts.NodeStatus.RUNNING)
            status = childNode.Tick();

        if (status == Consts.NodeStatus.RUNNING)
            return status;

        return Consts.NodeStatus.SUCCESS;
    }
}
