using System.Collections.Generic;
using UnityEngine;
using XNode;
using NaughtyAttributes;

[NodeTint(0.7f, 0.2f, 0.2f)]
public abstract class BTActionNode : BTGraphNode
{
    public override bool IsLeaf => true;

    public override NodeParameter[] GetParameters() { return null; }
}