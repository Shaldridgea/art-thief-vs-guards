using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using NaughtyAttributes;

[CreateNodeMenu("Condition/Is Turning Head")]
public class BTIsTurningHead : BTConditionNode
{
    protected override void Init()
    {
        type = Consts.BehaviourType.IsTurningHead;
        base.Init();
    }

    public override NodeParameter[] GetParameters()
    {
        return null;
    }
}