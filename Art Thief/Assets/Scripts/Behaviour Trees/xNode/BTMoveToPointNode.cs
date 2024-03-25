using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

[CreateNodeMenu("Action/Move To Point")]
public class BTMoveToPointNode : BTActionNode {

    [SerializeField]
    private Consts.BlackboardSource source;

    [SerializeField]
    private string variableName;

    protected override void Init()
    {
        type = Consts.BehaviourType.MoveToPoint;
        base.Init();
    }

    public override NodeParameter[] GetParameters()
    {
        return new NodeParameter[] { (int)source, variableName };
    }
}