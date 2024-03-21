using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToPoint : BehaviourNode
{
    private string variableKey;

    public MoveToPoint(BehaviourTree parentTree, NodeParameter[] parameters) : base(parentTree)
    {
        variableKey = parameters[0];
    }

    public override Consts.NodeStatus Update()
    {
        ParentTree.Owner.MoveAgent(ParentTree.Owner.AgentBlackboard.GetVariable<Vector3>(variableKey));
        return Consts.NodeStatus.SUCCESS;
    }
}
