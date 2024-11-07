using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class LockVariable : BehaviourNode
{
    private Blackboard board;

    private string keyName;

    private float lockLength;

    public LockVariable(BehaviourTree parentTree, NodeParameter[] parameters) : base(parentTree)
    {
        board = GetTargetBlackboard(parameters[0]);
        keyName = parameters[1];
        lockLength = parameters[2];
    }

    public override Consts.NodeStatus Update()
    {
        board.LockVariable(keyName, lockLength);

        return Consts.NodeStatus.SUCCESS;
    }
}