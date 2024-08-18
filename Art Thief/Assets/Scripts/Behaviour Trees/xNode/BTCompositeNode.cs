using System.Collections.Generic;
using UnityEngine;
using XNode;

public abstract class BTCompositeNode : BTGraphNode
{
    [Output(dynamicPortList = true, connectionType = ConnectionType.Override)]
    [SerializeField]
    private List<Empty> childNodes;

    [SerializeField]
    protected bool randomise;

    public override bool IsLeaf => false;

    public override NodeParameter[] GetParameters()
    {
        return new NodeParameter[] { randomise };
    }
}