using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Decorator : BehaviourNode
{
    protected BehaviourNode childNode;

    public Decorator(BehaviourTree parentTree) : base(parentTree)
    {

    }

    public override void AddChild(BehaviourNode addNode, string portName = "")
    {
        childNode = addNode;
    }

    public override bool TryGetChildNodes(out List<BehaviourNode> children)
    {
        children = new() { childNode };
        return true;
    }
}