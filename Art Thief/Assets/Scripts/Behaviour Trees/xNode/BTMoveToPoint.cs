using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

[CreateNodeMenu("Action/Move To Point")]
public class BTMoveToPoint : BTActionNode {

    [SerializeField]
    private string variableName;

    protected override void Init()
    {
        type = Consts.BehaviourType.MoveToPoint;
        base.Init();
    }

    public override NodeParameter[] GetParameters()
    {
        return new NodeParameter[] { variableName };
    }
}