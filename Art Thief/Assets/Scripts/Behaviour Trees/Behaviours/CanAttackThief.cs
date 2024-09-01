using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class CanAttackThief : Condition
{
    public CanAttackThief(BehaviourTree parentTree) : base(parentTree, null)
    {
        
    }

    public override Consts.NodeStatus Update()
    {
        Consts.NodeStatus status = Consts.NodeStatus.FAILURE;

        if (ParentTree.Owner.CanAttackEnemy())
            status = Consts.NodeStatus.SUCCESS;

        return status;
    }
}