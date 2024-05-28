using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetPointFromPatrol : BehaviourNode
{
    private string variableKey;

    private Consts.PatrolPathType pathType;

    public SetPointFromPatrol(BehaviourTree parentTree, NodeParameter[] parameters) : base(parentTree)
    {
        variableKey = parameters[0];
        pathType = (Consts.PatrolPathType)(int)parameters[1];
    }

    public override Consts.NodeStatus Update()
    {
        ParentTree.Owner.AgentBlackboard.SetVariable(variableKey, ParentTree.Owner.GetNextPatrolPoint(pathType));
        return Consts.NodeStatus.SUCCESS;
    }
}
