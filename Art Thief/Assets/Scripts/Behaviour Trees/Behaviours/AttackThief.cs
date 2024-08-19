using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class AttackThief : BehaviourNode
{
    public AttackThief(BehaviourTree parentTree) : base(parentTree)
    {
        
    }

    public override Consts.NodeStatus Update()
    {
        Consts.NodeStatus status = Consts.NodeStatus.SUCCESS;

        ParentTree.Owner.AttackAgent(Level.Instance.Thief);

        return status;
    }
}