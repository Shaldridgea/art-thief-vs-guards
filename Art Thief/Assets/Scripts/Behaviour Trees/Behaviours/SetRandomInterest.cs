using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class SetRandomInterest : BehaviourNode
{
    private Blackboard board;

    private string sourceKey;

    private string destinationKey;

    public SetRandomInterest(BehaviourTree parentTree, NodeParameter[] parameters) : base(parentTree)
    {
        board = GetTargetBlackboard(parameters[0]);
        sourceKey = parameters[1];
        destinationKey = parameters[2];
    }

    public override Consts.NodeStatus Update()
    {
        Consts.NodeStatus status = Consts.NodeStatus.FAILURE;

        List<GameObject> interestList = board.GetVariable<List<GameObject>>(sourceKey);
        if(interestList != null && interestList.Count > 0)
        {
            board.SetVariable(destinationKey, interestList[Random.Range(0, interestList.Count)]);
            status = Consts.NodeStatus.SUCCESS;
        }

        return status;
    }
}