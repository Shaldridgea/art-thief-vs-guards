using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Invert : Decorator
{
    public Invert(BehaviourTree parentTree) : base(parentTree)
    {

    }

    public override Consts.NodeStatus Update()
    {
        Consts.NodeStatus status = Consts.NodeStatus.FAILURE;
        if (Status == Consts.NodeStatus.RUNNING)
        {
            if (childNode.Status != Consts.NodeStatus.RUNNING)
                status = childNode.Status;
        }
        else
            status = childNode.Tick();

        if (status == Consts.NodeStatus.SUCCESS)
            status = Consts.NodeStatus.FAILURE;
        else if (status == Consts.NodeStatus.FAILURE)
            status = Consts.NodeStatus.SUCCESS;
        return status;
    }
}