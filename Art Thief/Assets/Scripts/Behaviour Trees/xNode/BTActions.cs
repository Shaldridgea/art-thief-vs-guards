using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

// File to hold miscellaneous action overrides so the editor menu shows them individually

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
        return new NodeParameter[] { new NodeParameter(waitTime) };
    }
}