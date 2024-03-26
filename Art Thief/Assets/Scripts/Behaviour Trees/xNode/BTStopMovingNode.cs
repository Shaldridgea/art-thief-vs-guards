using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using NaughtyAttributes;

[CreateNodeMenu("Action/Stop Moving")]
public class BTStopMovingNode : BTActionNode
{
    protected override void Init()
    {
        type = Consts.BehaviourType.StopMoving;
        base.Init();
    }
}