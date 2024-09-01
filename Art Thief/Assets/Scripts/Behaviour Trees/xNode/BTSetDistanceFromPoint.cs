using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using NaughtyAttributes;

[CreateNodeMenu("Variables/Set Distance From Point")]
public class BTSetDistanceFromPoint : BTActionNode
{
    [SerializeField]
    private Consts.BlackboardSource source;

    [SerializeField]
    private string pointKey;

    [SerializeField]
    private string distanceKey;

    protected override void Init()
    {
        type = Consts.BehaviourType.SetDistanceFromPoint;
        base.Init();
    }

    public override NodeParameter[] GetParameters()
    {
        return new NodeParameter[] { (int)source, pointKey, distanceKey };
    }
}