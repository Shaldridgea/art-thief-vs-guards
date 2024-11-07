using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using NaughtyAttributes;

[CreateNodeMenu("Variables/Lock Variable")]
public class BTLockVariable : BTActionNode
{
    [SerializeField]
    private Consts.BlackboardSource source;

    [SerializeField]
    private string lockKey;

    [SerializeField]
    private float lockLength;

    protected override void Init()
    {
        type = Consts.BehaviourType.LockVariable;
        base.Init();
    }

    public override NodeParameter[] GetParameters()
    {
        return new NodeParameter[] { (int)source, lockKey, lockLength };
    }

    public override string GetNodeDetailsText()
    {
        return $"Source: {source}, Key: {lockKey}, Length: {lockLength}";
    }
}