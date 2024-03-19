using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetVariable : BehaviourNode
{
    private Blackboard source;

    private string[] variableNames;

    private NodeParameter[] values;

    public SetVariable(BehaviourTree parentTree, NodeParameter[] parameters) : base(parentTree)
    {
        if (parameters[0] == (int)Consts.BlackboardSource.GLOBAL)
            source = parentTree.GlobalBlackboard;
        else
            source = parentTree.Owner.AgentBlackboard;
        if (parameters == null)
            return;

        variableNames = new string[parameters.Length / 2];
        values = new NodeParameter[parameters.Length / 2];
        for (int i = 0; i < parameters.Length; i += 2)
        {
            variableNames[i] = parameters[i+1];
            values[i] = parameters[i+2];
        }
    }

    public override Consts.NodeStatus Update()
    {
        for (int i = 0; i < values.Length; ++i)
        {
            switch (values[i].type)
            {
                case NodeParameter.ParamType.INT:
                source.SetVariable<int>(variableNames[i], values[i]);
                break;
                case NodeParameter.ParamType.FLOAT:
                source.SetVariable<float>(variableNames[i], values[i]);
                break;
                case NodeParameter.ParamType.BOOL:
                source.SetVariable<bool>(variableNames[i], values[i]);
                break;
                case NodeParameter.ParamType.STRING:
                source.SetVariable<string>(variableNames[i], values[i]);
                break;
            }
        }
        return Consts.NodeStatus.SUCCESS;
    }
}