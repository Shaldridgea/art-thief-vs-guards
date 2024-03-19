using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
