using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;


[CreateNodeMenu("Condition/Has Arrived")]
[NodeWidth(180)]
public class BTHasArrivedNode : BTConditionNode
{
    protected override void Init()
    {
        type = Consts.BehaviourType.HasArrived;
        base.Init();
    }

    public override NodeParameter[] GetParameters()
    {
        return null;
    }
}