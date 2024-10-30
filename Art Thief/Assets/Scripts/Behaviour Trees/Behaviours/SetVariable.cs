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
        Blackboard targetBoard = board;

        // If we're setting on a different agent by accessor e.g. storedAgent.newVariable
        if (HandleStatementAccessor(variableName, board, out Blackboard newBoard, out string newVariableName))
        {
            variableName = newVariableName;
            targetBoard = newBoard;
        }

        bool isDirectValue = true;
        // If the value we're setting is a string then we might
        // be using a key for another variable to set from
        if (valueStatement.type == NodeParameter.ParamType.String)
        {
            string valueString = valueStatement;
            if (HandleStatementAccessor(valueString, board, out Blackboard tempBoard, out string newValueName))
            {
                isDirectValue = false;

                object value = tempBoard.GetVariable<object>(newValueName);
                targetBoard.SetVariable(variableName, value);
            }
        }
        
        if(isDirectValue)
        {
            switch (valueStatement.type)
            {
                case NodeParameter.ParamType.Int:
                    targetBoard.SetVariable<int>(variableName, valueStatement);
                    break;
                case NodeParameter.ParamType.Float:
                    targetBoard.SetVariable<float>(variableName, valueStatement);
                    break;
                case NodeParameter.ParamType.Bool:
                    targetBoard.SetVariable<bool>(variableName, valueStatement);
                    break;
                case NodeParameter.ParamType.String:
                    targetBoard.SetVariable<string>(variableName, valueStatement);
                    break;
                case NodeParameter.ParamType.Vector3:
                    targetBoard.SetVariable<Vector3>(variableName, valueStatement);
                    break;
            }
        }
        return Consts.NodeStatus.SUCCESS;
    }
}