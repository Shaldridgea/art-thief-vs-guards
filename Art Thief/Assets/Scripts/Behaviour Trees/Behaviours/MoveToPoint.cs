using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToPoint : BehaviourNode
{
    private Consts.BlackboardSource blackboardSource;

    private string variableKey;

    public MoveToPoint(BehaviourTree parentTree, NodeParameter[] parameters) : base(parentTree)
    {
        blackboardSource = (Consts.BlackboardSource)(int)parameters[0];
        variableKey = parameters[1];
    }

    public override Consts.NodeStatus Update()
    {
        Blackboard blackboard = GetTargetBlackboard(blackboardSource);
        ParentTree.Owner.MoveAgent(blackboard.GetVariable<Vector3>(variableKey));
        return Consts.NodeStatus.SUCCESS;
    }
}
