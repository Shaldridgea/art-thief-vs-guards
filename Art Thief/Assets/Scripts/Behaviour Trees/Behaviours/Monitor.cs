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

        if (checkStatus != Consts.NodeStatus.SUCCESS)
        {
            // The check node can only ever be a SUCCESS or FAILURE
            // We do not want a check node on the running stack, it
            // is not part of the usual BT path
            if (checkStatus == Consts.NodeStatus.RUNNING)
            {
                checkNode.OnExit();
                Debug.LogError($"Monitor node's check returned a running status, this is not allowed.", ParentTree.Owner);
            }

            // Force our check status to be FAILURE as running isn't allowed
            checkStatus = Consts.NodeStatus.FAILURE;

            if (Status == Consts.NodeStatus.RUNNING)
                ParentTree.Interrupt(this);

            return checkStatus;
        }

        if (Status == Consts.NodeStatus.RUNNING)
            return childNode.Status;

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

    public override void AddChild(BehaviourNode addNode, string portName = "")
    {
        if (portName == "childNode")
            base.AddChild(addNode);
        else
            checkNode = addNode;
    }
}
