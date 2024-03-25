using System.Collections.Generic;
using UnityEngine;
using XNode;
using NaughtyAttributes;

[NodeTint(0.1f, 0.7f, 0.3f)]
public abstract class BTConditionNode : BTGraphNode
{
    public override bool IsLeaf => true;
}


[CreateNodeMenu("Condition/Custom")]
public class BTCustomConditionNode : BTConditionNode
{
    [SerializeField]
    [NodeEnum]
    private Consts.BlackboardSource source;

    [SerializeField]
    private string[] checkConditions;

    protected override void Init()
    {
        type = Consts.BehaviourType.Condition;
        base.Init();
    }

    public override NodeParameter[] GetParameters()
    {
        NodeParameter[] parameters = new NodeParameter[checkConditions.Length+1];
        parameters[0] = (int)source;
        for (int i = 0; i < checkConditions.Length; ++i)
            parameters[i+1] = checkConditions[i];

        return parameters;
    }
}