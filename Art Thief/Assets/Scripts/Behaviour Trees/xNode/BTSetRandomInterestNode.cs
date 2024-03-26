using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using NaughtyAttributes;

[CreateNodeMenu("Variables/Set Random Interest")]
public class BTSetRandomInterestNode : BTActionNode
{
    [SerializeField]
    private Consts.BlackboardSource source;

    [SerializeField]
    private string sourceKey;

    [SerializeField]
    private string destinationKey;

    protected override void Init()
    {
        type = Consts.BehaviourType.SetRandomInterest;
        base.Init();
    }

    public override NodeParameter[] GetParameters()
    {
        return new NodeParameter[] { (int)source, sourceKey, destinationKey };
    }
}