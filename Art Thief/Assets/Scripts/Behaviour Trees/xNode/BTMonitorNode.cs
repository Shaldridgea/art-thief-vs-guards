using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateNodeMenu("Monitor")]
public class BTMonitorNode : BTDecoratorNode
{
    [Output(connectionType = ConnectionType.Override)]
    [SerializeField]
    private Empty checkNode;

    protected override void Init()
    {
        type = Consts.BehaviourType.Monitor;
        base.Init();
    }
}
