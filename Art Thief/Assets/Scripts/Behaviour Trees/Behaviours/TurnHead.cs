using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class TurnHead : BehaviourNode
{
    private bool isAngleTurn;

    private float toAngle;

    private string pointKey;

    private float animLength;

    public TurnHead(BehaviourTree parentTree, NodeParameter[] parameters) : base(parentTree)
    {
        isAngleTurn = parameters[0];
        if (isAngleTurn)
            toAngle = parameters[1];
        else
            pointKey = parameters[1];
        animLength = parameters[2];
    }

    public override Consts.NodeStatus Update()
    {
        Consts.NodeStatus status = Consts.NodeStatus.SUCCESS;

        if(isAngleTurn)
            ParentTree.Owner.TurnHead(toAngle, animLength);
        else
        {
            Vector3 toPoint = ParentTree.Owner.AgentBlackboard.GetVariable<Vector3>(pointKey);
            ParentTree.Owner.TurnHeadToPoint(toPoint, animLength);
        }

        return status;
    }
}