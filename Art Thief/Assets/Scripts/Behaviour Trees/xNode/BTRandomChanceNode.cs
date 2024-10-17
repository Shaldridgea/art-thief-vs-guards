using System.Collections.Generic;
using UnityEngine;
using XNode;

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

    public override string GetNodeDetailsText()
    {
        return "Chance: " + chanceValue;
    }
}