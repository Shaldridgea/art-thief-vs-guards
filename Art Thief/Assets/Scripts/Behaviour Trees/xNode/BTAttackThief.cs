using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using NaughtyAttributes;

[CreateNodeMenu("Action/Attack Thief")]
public class BTAttackThief : BTActionNode
{
    protected override void Init()
    {
        type = Consts.BehaviourType.AttackThief;
        base.Init();
    }

    public override NodeParameter[] GetParameters()
    {
        return new NodeParameter[] { null };
    }
}