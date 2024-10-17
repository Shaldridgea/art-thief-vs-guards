using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

[CreateNodeMenu("Action/Wait")]
public class BTWaitNode : BTActionNode
{
    [SerializeField]
    private float waitTime;

    protected override void Init()
    {
        type = Consts.BehaviourType.Wait;
        base.Init();
    }

    public override NodeParameter[] GetParameters()
    {
        return new NodeParameter[] { waitTime };
    }

    public override string GetNodeDetailsText()
    {
        return "Wait time: " + waitTime;
    }
}