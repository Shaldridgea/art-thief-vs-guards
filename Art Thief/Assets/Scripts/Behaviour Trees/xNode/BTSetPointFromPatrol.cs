using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

[CreateNodeMenu("Action/Set Point From Patrol")]
public class BTSetPointFromPatrol : BTActionNode
{
    [SerializeField]
    private string variableName;
    
    protected override void Init()
    {
        type = Consts.BehaviourType.SetPointFromPatrol;
        base.Init();
    }

    public override NodeParameter[] GetParameters()
    {
        return new NodeParameter[] { variableName };
    }
}