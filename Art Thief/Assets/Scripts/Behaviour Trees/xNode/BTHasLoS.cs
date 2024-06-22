using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using NaughtyAttributes;

[CreateNodeMenu("Condition/Has Line Of Sight")]
public class BTHasLoS : BTConditionNode
{
    [SerializeField]
    private string targetKey;

    protected override void Init()
    {
        type = Consts.BehaviourType.HasLineOfSight;
        base.Init();
    }

    public override NodeParameter[] GetParameters()
    {
        return new NodeParameter[] { targetKey };
    }
}