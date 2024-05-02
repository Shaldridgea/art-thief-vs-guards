using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using XNode;

[CreateNodeMenu("Variables/Set Variable", order = -10)]
public class BTSetVariableNode : BTActionNode {

	[SerializeField]
	[NodeEnum]
	private Consts.BlackboardSource source;

    [SerializeField]
    private string variableKey;

    [Header("Variable Value")]
    [SerializeField]
    private NodeParameter variableValue;

	protected override void Init() {
		type = Consts.BehaviourType.SetVariable;
		base.Init();
	}

    public override NodeParameter[] GetParameters()
    {
        return new NodeParameter[] { (int)source, variableKey, variableValue };
    }
}