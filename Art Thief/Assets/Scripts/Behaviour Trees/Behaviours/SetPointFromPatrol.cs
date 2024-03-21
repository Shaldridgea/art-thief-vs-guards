using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetPointFromPatrol : BehaviourNode
{
    private string variableKey;

    public SetPointFromPatrol(BehaviourTree parentTree, NodeParameter[] parameters) : base(parentTree)
    {
        variableKey = parameters[0];
    }

    public override Consts.NodeStatus Update()
    {
        ParentTree.Owner.AgentBlackboard.SetVariable<Vector3>(variableKey, ParentTree.Owner.GetNextPatrolPoint());
        return Consts.NodeStatus.SUCCESS;
    }
}
