using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

[CreateNodeMenu("Variables/Set Point From Patrol")]
public class BTSetPointFromPatrolNode : BTActionNode
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