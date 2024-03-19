using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Decorator : BehaviourNode
{
    protected BehaviourNode childNode;

    public Decorator(BehaviourTree parentTree) : base(parentTree)
    {

    }

    public override void AddChild(BehaviourNode addNode)
    {
        childNode = addNode;
    }
}