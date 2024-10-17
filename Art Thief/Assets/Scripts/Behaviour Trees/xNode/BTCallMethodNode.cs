using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using NaughtyAttributes;

[CreateNodeMenu("Action/Call Method")]
public class BTCallMethodNode : BTActionNode
{
    [SerializeField]
    private string methodName;

    protected override void Init()
    {
        type = Consts.BehaviourType.CallMethod;
        base.Init();
    }

    public override NodeParameter[] GetParameters()
    {
        return new NodeParameter[] { methodName };
    }

    public override string GetNodeDetailsText()
    {
        return "Method name: " + methodName;
    }
}