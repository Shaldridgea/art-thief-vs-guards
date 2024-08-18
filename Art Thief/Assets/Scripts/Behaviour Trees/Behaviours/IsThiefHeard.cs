using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class IsThiefHeard : Condition
{
    public IsThiefHeard(BehaviourTree parentTree) : base(parentTree, null)
    {
        
    }

    public override Consts.NodeStatus Update()
    {
        Consts.NodeStatus status = Consts.NodeStatus.FAILURE;

        if (ParentTree.Owner.Suspicion.IsThiefHeard())
            status = Consts.NodeStatus.SUCCESS;

        return status;
    }
}