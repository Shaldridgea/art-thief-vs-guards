using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class IsTurningHead : Condition
{
    public IsTurningHead(BehaviourTree parentTree) : base(parentTree, null)
    {
        
    }

    public override Consts.NodeStatus Update()
    {
        Consts.NodeStatus status = Consts.NodeStatus.FAILURE;

        if (ParentTree.Owner.IsTweeningHead())
            status = Consts.NodeStatus.SUCCESS;

        return status;
    }
}