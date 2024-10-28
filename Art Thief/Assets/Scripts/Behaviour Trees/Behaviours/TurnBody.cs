using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class TurnBody : BehaviourNode
{
    private bool isAngleTurn;

    private float addAngle;

    private string pointKey;

    private float animLength;

    public TurnBody(BehaviourTree parentTree, NodeParameter[] parameters) : base(parentTree)
    {
        isAngleTurn = parameters[0];
        if (isAngleTurn)
            addAngle = parameters[1];
        else
            pointKey = parameters[1];
        animLength = parameters[2];
    }

    public override Consts.NodeStatus Update()
    {
        Consts.NodeStatus status = Consts.NodeStatus.SUCCESS;

        if(isAngleTurn)
            ParentTree.Owner.TurnBody(addAngle, animLength);
        else
        {
            Vector3 toPoint = ParentTree.Owner.AgentBlackboard.GetVariable<Vector3>(pointKey);
            ParentTree.Owner.TurnBodyToPoint(toPoint, animLength);
        }

        return status;
    }
}