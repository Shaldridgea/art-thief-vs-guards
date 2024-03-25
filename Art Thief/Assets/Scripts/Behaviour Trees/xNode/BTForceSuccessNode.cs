using System.Collections.Generic;
using UnityEngine;
using XNode;

[NodeWidth(180)]
[CreateNodeMenu("Decorator/Force Success")]
public class BTForceSuccessNode : BTDecoratorNode
{
    protected override void Init()
    {
        type = Consts.BehaviourType.ForceSuccess;
        base.Init();
    }
}