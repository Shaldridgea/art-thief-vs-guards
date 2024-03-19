using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monitor : Decorator
{
    private BehaviourNode checkNode;

    public Monitor(BehaviourTree parentTree) : base(parentTree)
    {

    }

    public override Consts.NodeStatus Update()
    {
        Consts.NodeStatus checkStatus = checkNode.Tick();

        if (checkStatus == Consts.NodeStatus.FAILURE)
        {
            if (Status == Consts.NodeStatus.RUNNING)
                ParentTree.Interrupt(this);
            return checkStatus;
        }
        if (Status == Consts.NodeStatus.RUNNING)
        {
            if (childNode.Status != Consts.NodeStatus.RUNNING)
                return childNode.Status;

            return checkStatus;
        }

        Consts.NodeStatus status = childNode.Tick();

        return status;
    }

    public override void OnEnter()
    {
        base.OnEnter();
        ParentTree.RegisterMonitoringNode(this);
    }

    public override void OnExit()
    {
        base.OnExit();
        ParentTree.DeregisterMonitoringNode(this);
    }

    public override void AddChild(BehaviourNode addNode)
    {
        if (childNode == null)
            base.AddChild(addNode);
        else
            checkNode = addNode;
    }
}
