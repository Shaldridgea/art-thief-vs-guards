using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class HasLoS : BehaviourNode
{
    private string targetKey;

    public HasLoS(BehaviourTree parentTree, NodeParameter[] parameters) : base(parentTree)
    {
        targetKey = parameters[0];
    }

    public override Consts.NodeStatus Update()
    {
        Consts.NodeStatus status = Consts.NodeStatus.FAILURE;

        GameObject target = ParentTree.Owner.AgentBlackboard.GetVariable<GameObject>(targetKey);
        if (target != null)
            if (ParentTree.Owner.GuardSenses.IsSeen(target.transform.position))
                status = Consts.NodeStatus.SUCCESS;

        return status;
    }
}