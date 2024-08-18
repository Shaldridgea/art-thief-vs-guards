using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using NaughtyAttributes;

[CreateNodeMenu("Condition/Is Thief Heard")]
public class BTIsThiefHeard : BTConditionNode
{
    protected override void Init()
    {
        type = Consts.BehaviourType.IsThiefHeard;
        base.Init();
    }

    public override NodeParameter[] GetParameters()
    {
        return new NodeParameter[] { null };
    }
}