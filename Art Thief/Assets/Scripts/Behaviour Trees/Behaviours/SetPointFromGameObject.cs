using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class SetPointFromGameObject : BehaviourNode
{
    private Blackboard board;

    private string objectKey;

    private string pointKey;

    public SetPointFromGameObject(BehaviourTree parentTree, NodeParameter[] parameters) : base(parentTree)
    {
        board = GetTargetBlackboard(parameters[0]);
        objectKey = parameters[1];
        pointKey = parameters[2];
    }

    public override Consts.NodeStatus Update()
    {
        Consts.NodeStatus status = Consts.NodeStatus.FAILURE;

        GameObject targetGobject = board.GetVariable<GameObject>(objectKey);
        if(targetGobject != null)
        {
            board.SetVariable(pointKey, targetGobject.transform.position);
            status = Consts.NodeStatus.SUCCESS;
        }

        return status;
    }
}