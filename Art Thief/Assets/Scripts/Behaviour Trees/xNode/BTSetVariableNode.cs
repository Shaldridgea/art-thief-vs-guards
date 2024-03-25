using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using XNode;

[CreateNodeMenu("Set Variable")]
public class BTSetVariableNode : BTActionNode {

	[SerializeField]
	[NodeEnum]
	private Consts.BlackboardSource source;

    [SerializeField]
    private string variableName;

    [Header("Variable Value")]
    [SerializeField]
    private NodeParameter variableValue;

	protected override void Init() {
		type = Consts.BehaviourType.SetVariable;
		base.Init();
	}

    public override NodeParameter[] GetParameters()
    {
        return new NodeParameter[] { (int)source, variableName, variableValue };
    }
}