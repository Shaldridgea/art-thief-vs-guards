using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class StopMoving : BehaviourNode
{
    public StopMoving(BehaviourTree parentTree) : base(parentTree)
    {
        
    }

    public override Consts.NodeStatus Update()
    {
        ParentTree.Owner.NavAgent.ResetPath();

        return Consts.NodeStatus.SUCCESS;
    }
}