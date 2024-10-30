using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class StoreVisibleInterests : BehaviourNode
{
    private string interestTag;

    private Blackboard board;

    private string listKey;

    public StoreVisibleInterests(BehaviourTree parentTree, NodeParameter[] parameters) : base(parentTree)
    {
        interestTag = parameters[0];
        board = GetTargetBlackboard(parameters[1]);
        listKey = parameters[2];
    }

    public override Consts.NodeStatus Update()
    {
        Consts.NodeStatus status = Consts.NodeStatus.FAILURE;

        if (ParentTree.Owner.Senses.StoreVisibleInterests(interestTag, board, listKey))
            status = Consts.NodeStatus.SUCCESS;

        return status;
    }
}