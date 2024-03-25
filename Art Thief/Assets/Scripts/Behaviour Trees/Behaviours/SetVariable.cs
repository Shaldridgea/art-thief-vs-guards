using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetVariable : BehaviourNode
{
    private Blackboard source;

    private string variableName;

    private NodeParameter value;

    public SetVariable(BehaviourTree parentTree, NodeParameter[] parameters) : base(parentTree)
    {
        if (parameters == null)
            return;

        if (parameters[0] == (int)Consts.BlackboardSource.GLOBAL)
            source = parentTree.GlobalBlackboard;
        else
            source = parentTree.Owner.AgentBlackboard;

        variableName = parameters[1];
        value = parameters[2];
    }

    public override Consts.NodeStatus Update()
    {
        switch (value.type)
        {
            case NodeParameter.ParamType.Int:
            source.SetVariable<int>(variableName, value);
            break;
            case NodeParameter.ParamType.Float:
            source.SetVariable<float>(variableName, value);
            break;
            case NodeParameter.ParamType.Bool:
            source.SetVariable<bool>(variableName, value);
            break;
            case NodeParameter.ParamType.String:
            source.SetVariable<string>(variableName, value);
            break;
            case NodeParameter.ParamType.Vector3:
            source.SetVariable<Vector3>(variableName, value);
            break;
        }
        return Consts.NodeStatus.SUCCESS;
    }
}