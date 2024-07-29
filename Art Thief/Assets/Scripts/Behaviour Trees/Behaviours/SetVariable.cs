using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetVariable : BehaviourNode
{
    private Blackboard board;

    private string keyStatement;

    private NodeParameter valueStatement;

    public SetVariable(BehaviourTree parentTree, NodeParameter[] parameters) : base(parentTree)
    {
        if (parameters == null)
            return;

        board = GetTargetBlackboard(parameters[0]);
        keyStatement = parameters[1];
        valueStatement = parameters[2];
    }

    public override Consts.NodeStatus Update()
    {
        string variableName = keyStatement;
        NodeParameter value = valueStatement;
        Blackboard targetBoard = board;

        // If we're setting on a different agent by accessor e.g. storedAgent.newVariable
        if (HandleStatementAccessor(variableName, board, out Blackboard newBoard, out string newVariableName))
        {
            variableName = newVariableName;
            targetBoard = newBoard;
        }

        if (value.type == NodeParameter.ParamType.String)
        {
            string valueString = value;
            if (HandleStatementAccessor(valueString, board, out Blackboard tempBoard, out string newValueName))
            {
                object tempValue = tempBoard.GetVariable<object>(newValueName);
                System.Type type = tempBoard.GetVariableType(newValueName);
                if (type == typeof(int))
                    value = (int)tempValue;
                else if (type == typeof(float))
                    value = (float)tempValue;
                else if (type == typeof(bool))
                    value = (bool)tempValue;
                else if (type == typeof(string))
                    value = (string)tempValue;
                else if (type == typeof(Vector3))
                    value = (Vector3)tempValue;
                else
                    value = newValueName;
            }
        }

        switch (value.type)
        {
            case NodeParameter.ParamType.Int:
                targetBoard.SetVariable<int>(variableName, value);
            break;
            case NodeParameter.ParamType.Float:
                targetBoard.SetVariable<float>(variableName, value);
            break;
            case NodeParameter.ParamType.Bool:
                targetBoard.SetVariable<bool>(variableName, value);
            break;
            case NodeParameter.ParamType.String:
                targetBoard.SetVariable<string>(variableName, value);
            break;
            case NodeParameter.ParamType.Vector3:
                targetBoard.SetVariable<Vector3>(variableName, value);
            break;
        }
        return Consts.NodeStatus.SUCCESS;
    }
}