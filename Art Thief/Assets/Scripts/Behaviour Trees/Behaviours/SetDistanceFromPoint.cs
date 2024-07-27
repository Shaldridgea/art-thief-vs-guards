using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class SetDistanceFromPoint : BehaviourNode
{
    private Blackboard board;

    private string pointKey;

    private string distanceKey;

    public SetDistanceFromPoint(BehaviourTree parentTree, NodeParameter[] parameters) : base(parentTree)
    {
        board = GetTargetBlackboard(parameters[0]);
        pointKey = parameters[1];
        distanceKey = parameters[2];
    }

    public override Consts.NodeStatus Update()
    {
        Consts.NodeStatus status = Consts.NodeStatus.SUCCESS;

        Vector3 point = board.GetVariable<Vector3>(pointKey);

        board.SetVariable(distanceKey, Vector3.Distance(ParentTree.Owner.transform.position.ZeroY(), point.ZeroY()));

        return status;
    }
}