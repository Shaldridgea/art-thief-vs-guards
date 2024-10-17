using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using NaughtyAttributes;

[CreateNodeMenu("Variables/Copy Variables To Self")]
public class BTCopyVariables : BTActionNode
{
    [SerializeField]
    private Consts.BlackboardSource source;

    [SerializeField]
    private string copyKey;

    protected override void Init()
    {
        type = Consts.BehaviourType.CopyVariablesToSelf;
        base.Init();
    }

    public override NodeParameter[] GetParameters()
    {
        return new NodeParameter[] { (int)source, copyKey };
    }

    public override string GetNodeDetailsText()
    {
        return $"Source: {source}, Copy key: {copyKey}";
    }
}