using System.Collections.Generic;
using UnityEngine;
using XNode;

[CreateNodeMenu("Decorator/Invert")]
[NodeWidth(180)]
public class BTInvertNode : BTDecoratorNode
{
    protected override void Init()
    {
        type = Consts.BehaviourType.Invert;
        base.Init();
    }
}