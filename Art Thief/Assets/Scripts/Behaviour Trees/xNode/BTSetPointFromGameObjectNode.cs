using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using NaughtyAttributes;

[CreateNodeMenu("Variables/Set Point From GameObject")]
public class BTSetPointFromGameObjectNode : BTActionNode
{
    [SerializeField]
    private Consts.BlackboardSource source;

    [SerializeField]
    private string objectKey;

    [SerializeField]
    private string pointKey;

    [SerializeField]
    private Vector3 offset;

    [SerializeField]
    private Consts.OffsetType offsetSpace;

    protected override void Init()
    {
        type = Consts.BehaviourType.SetPointFromGameObject;
        base.Init();
    }

    public override NodeParameter[] GetParameters()
    {
        return new NodeParameter[] { (int)source, objectKey, pointKey, offset, (int)offsetSpace };
    }

    public override string GetNodeDetailsText()
    {
        return "Object key: " + objectKey + ", Point key: " + pointKey;
    }
}