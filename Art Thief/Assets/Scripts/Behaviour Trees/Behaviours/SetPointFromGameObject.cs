using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class SetPointFromGameObject : BehaviourNode
{
    private Blackboard board;

    private string objectKey;

    private string pointKey;

    private Vector3 offsetVector;

    private Consts.OffsetType offsetType;

    public SetPointFromGameObject(BehaviourTree parentTree, NodeParameter[] parameters) : base(parentTree)
    {
        board = GetTargetBlackboard(parameters[0]);
        objectKey = parameters[1];
        pointKey = parameters[2];
        offsetVector = parameters[3];
        offsetType = (Consts.OffsetType)(int)parameters[4];
    }

    public override Consts.NodeStatus Update()
    {
        Consts.NodeStatus status = Consts.NodeStatus.FAILURE;

        GameObject targetObject = board.GetVariable<GameObject>(objectKey);
        if(targetObject != null)
        {
            Vector3 targetPoint = targetObject.transform.position;

            if(offsetVector != Vector3.zero)
            {
                if (offsetType == Consts.OffsetType.LOCAL)
                    offsetVector = targetObject.transform.InverseTransformVector(offsetVector);

                targetPoint += offsetVector;
            }

            board.SetVariable(pointKey, targetPoint);
            status = Consts.NodeStatus.SUCCESS;
        }

        return status;
    }
}