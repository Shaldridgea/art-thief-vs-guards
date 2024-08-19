using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using NaughtyAttributes;

[CreateNodeMenu("Condition/Can Attack Thief")]
public class BTCanAttackThief : BTConditionNode
{
    protected override void Init()
    {
        type = Consts.BehaviourType.CanAttackThief;
        base.Init();
    }

    public override NodeParameter[] GetParameters()
    {
        return new NodeParameter[] { null };
    }
}