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

[CreateNodeMenu("Decorator/Invert")]
[NodeWidth(180)]
public class BTInvertNode : BTDecoratorNode
{
    protected override void Init()
    {
        type = Consts.BehaviourType.Invert;
        base.Init();
    }
}

[CreateNodeMenu("Decorator/Random Chance")]
public class BTRandomChanceNode: BTDecoratorNode
{
    [SerializeField]
    private float chanceValue;

    protected override void Init()
    {
        type = Consts.BehaviourType.RandomChance;
        base.Init();
    }

    public override NodeParameter[] GetParameters()
    {
        return new NodeParameter[] { chanceValue };
    }
}

[NodeWidth(180)]
[CreateNodeMenu("Decorator/Force Success")]
public class BTForceSuccessNode : BTDecoratorNode
{
    protected override void Init()
    {
        type = Consts.BehaviourType.ForceSuccess;
        base.Init();
    }
}