using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

[CreateNodeMenu("Set Variable")]
public class BTSetVariableNode : BTActionNode {

	[SerializeField]
	[NodeEnum]
	private Consts.BlackboardSource source;

    [SerializeField]
    private string[] variableNames;

    [SerializeField]
    private NodeParameter[] variableValues;

	protected override void Init() {
		type = Consts.BehaviourType.SetVariable;
		base.Init();
	}

    public override NodeParameter[] GetParameters()
    {
        int varSize = variableNames.Length;
        if (variableValues.Length < varSize)
            varSize = variableValues.Length;
        varSize *= 2;
        NodeParameter[] nodeParams = new NodeParameter[varSize + 1];
        nodeParams[0] = (int)source;
        for(int i = 0; i < varSize; i += 2)
        {
            nodeParams[i + 1] = variableNames[i];
            nodeParams[i + 2] = variableValues[i];
        }
        return nodeParams;
    }
}