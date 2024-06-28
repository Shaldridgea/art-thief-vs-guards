using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class CallMethod : BehaviourNode
{
    private string methodName;

    public CallMethod(BehaviourTree parentTree, NodeParameter[] parameters) : base(parentTree)
    {
        methodName = parameters[0];
    }

    public override Consts.NodeStatus Update()
    {
        Consts.NodeStatus status = Consts.NodeStatus.SUCCESS;

        ParentTree.Owner.SendMessage(methodName);

        return status;
    }
}