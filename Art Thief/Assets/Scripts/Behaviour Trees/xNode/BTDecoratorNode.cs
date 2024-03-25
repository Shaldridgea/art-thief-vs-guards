using System.Collections.Generic;
using UnityEngine;
using XNode;

[NodeTint(0.7f, 0.7f, 0.2f)]
public abstract class BTDecoratorNode : BTGraphNode
{
    [Output(connectionType = ConnectionType.Override)]
    [SerializeField]
    private Empty childNode;

    public override bool IsLeaf => false;

    public override NodeParameter[] GetParameters()
    {
        return null;
    }
}