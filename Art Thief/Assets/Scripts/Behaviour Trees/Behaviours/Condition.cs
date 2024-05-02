using UnityEngine;

public class Condition : BehaviourNode
{
    private Blackboard board;

    private delegate bool ConditionCheckDelegate(NodeParameter a, NodeParameter b);

    private string[] leftTokens;

    private string[] operatorTokens;

    private string[] rightTokens;

    public Condition(BehaviourTree parentTree, NodeParameter[] parameters) : base(parentTree)
    {
        if (parameters == null)
            return;

        board = GetTargetBlackboard(parameters[0]);

        leftTokens = new string[parameters.Length-1];
        operatorTokens = new string[parameters.Length-1];
        rightTokens = new string[parameters.Length-1];
        for(int i = 0; i < parameters.Length-1; ++i)
        {
            string parsing = parameters[i+1];
            string[] split = parsing.Split(' ', 3);
            if (split.Length != 3)
            {
                Debug.Log("Condition parsing does not have correct split number: " + split.Length);
                return;
            }
            leftTokens[i] = split[0];
            operatorTokens[i] = split[1];
            rightTokens[i] = split[2];
        }
    }

    public override Consts.NodeStatus Update()
    {
        NodeParameter[] leftValues = new NodeParameter[leftTokens.Length];
        ConditionCheckDelegate[] operatorDelegates = new ConditionCheckDelegate[operatorTokens.Length];
        NodeParameter[] rightValues = new NodeParameter[rightTokens.Length];
        for(int i = 0; i < operatorTokens.Length; ++i)
        {
            leftValues[i] = ParseStringToValue(leftTokens[i]);
            rightValues[i] = ParseStringToValue(rightTokens[i]);
            operatorDelegates[i] = GetMatchingDelegate(rightValues[i].type, operatorTokens[i]);
        }
        Consts.NodeStatus conditionStatus = Consts.NodeStatus.SUCCESS;
        for (int i = 0; i < leftTokens.Length; ++i)
        {
            if (!operatorDelegates[i](leftValues[i], rightValues[i]))
            {
                conditionStatus = Consts.NodeStatus.FAILURE;
                break;
            }
        }
        return conditionStatus;
    }

    private NodeParameter ParseStringToValue(string newString)
    {
        Blackboard workingBoard = board;
        // First check if there is an accessor to a different board
        if(HandleStatementAccessor(newString, board, out Blackboard checkBoard, out string newStringName))
        {
            workingBoard = checkBoard;
            newString = newStringName;
        }

        // Parse normally for our primitives
        if(int.TryParse(newString, out int newInt))
            return newInt;

        if (float.TryParse(newString, out float newFloat))
            return newFloat;

        if (bool.TryParse(newString, out bool newBool))
            return newBool;

        if(newString.Contains("Vector3"))
        {
            int leftBracketIndex = newString.IndexOf('(') + 1;
            int rightBracketIndex = newString.IndexOf(')');
            string vectorValueString = newString.Substring(leftBracketIndex, rightBracketIndex - leftBracketIndex);
            string[] vectorSplit = vectorValueString.Split(',');
            float[] vectorComponents = new float[3];
            for(int i = 0; i < Mathf.Min(vectorSplit.Length, 3); ++i)
            {
                if (float.TryParse(vectorSplit[i].Trim(), out float vecComponent))
                    vectorComponents[i] = vecComponent;
            }
            return new Vector3(vectorComponents[0], vectorComponents[1], vectorComponents[2]);
        }

        // If our value is not any of the above then
        // we check if it's actually a string or if it's
        // a blackboard key as well
        if (newString.StartsWith('"') && newString.EndsWith('"'))
            return newString.Trim('"');

        System.Type type = board.GetVariableType(newString);
        if (type == null)
            return newString;

        if (type == typeof(int))
            return workingBoard.GetVariable<int>(newString);
        else if (type == typeof(float))
            return workingBoard.GetVariable<float>(newString);
        else if (type == typeof(bool))
            return workingBoard.GetVariable<bool>(newString);
        else if (type == typeof(string))
            return workingBoard.GetVariable<string>(newString);
        else if (type == typeof(Vector3))
            return workingBoard.GetVariable<Vector3>(newString);

        Debug.LogError($"Condition failed value parsing on key: {newString}");
        return newString;
    }

    #region Condition Delegates
    private ConditionCheckDelegate GetMatchingDelegate(NodeParameter.ParamType valueType, string token)
    {
        if (valueType == NodeParameter.ParamType.Int)
        {
            switch (token)
            {
                case ">":
                return GreaterThanInt;

                case ">=":
                return GreaterThanOrEqualInt;

                case "<":
                return LessThanInt;

                case "<=":
                return LessThanOrEqualInt;

                case "==":
                return EqualToInt;

                case "!=":
                return NotEqualToInt;
            }
        }
        else if(valueType == NodeParameter.ParamType.Float)
        {
            switch (token)
            {
                case ">":
                return GreaterThanFloat;

                case ">=":
                return GreaterThanOrEqualFloat;

                case "<":
                return LessThanFloat;

                case "<=":
                return LessThanOrEqualFloat;

                case "==":
                return EqualToFloat;

                case "!=":
                return NotEqualToFloat;
            }
        }
        else if(valueType == NodeParameter.ParamType.Bool)
        {
            switch (token)
            {
                case "==":
                return EqualToBool;

                case "!=":
                return NotEqualToBool;
            }
        }
        else if(valueType == NodeParameter.ParamType.String)
        {
            switch (token)
            {
                case "==":
                return EqualToString;

                case "!=":
                return NotEqualToString;
            }
        }
        else if(valueType == NodeParameter.ParamType.Vector3)
        {
            switch (token)
            {
                case "==":
                return EqualToVector3;

                case "!=":
                return NotEqualToVector3;
            }
        }
        return null;
    }

    private bool GreaterThanInt(NodeParameter a, NodeParameter b) => a > b;
    private bool GreaterThanOrEqualInt(NodeParameter a, NodeParameter b) => a >= b;
    private bool LessThanInt(NodeParameter a, NodeParameter b) => a < b;
    private bool LessThanOrEqualInt(NodeParameter a, NodeParameter b) => a <= b;
    private bool EqualToInt(NodeParameter a, NodeParameter b) => a == b;
    private bool NotEqualToInt(NodeParameter a, NodeParameter b) => a != b;


    private bool GreaterThanFloat(NodeParameter a, NodeParameter b) => a > (float)b;
    private bool GreaterThanOrEqualFloat(NodeParameter a, NodeParameter b) => a >= (float)b;
    private bool LessThanFloat(NodeParameter a, NodeParameter b) => a < (float)b;
    private bool LessThanOrEqualFloat(NodeParameter a, NodeParameter b) => a <= (float)b;
    private bool EqualToFloat(NodeParameter a, NodeParameter b) => a == (float)b;
    private bool NotEqualToFloat(NodeParameter a, NodeParameter b) => a != (float)b;


    private bool EqualToBool(NodeParameter a, NodeParameter b) => a == (bool)b;
    private bool NotEqualToBool(NodeParameter a, NodeParameter b) => a != (bool)b;


    private bool EqualToString(NodeParameter a, NodeParameter b) => a == (string)b;
    private bool NotEqualToString(NodeParameter a, NodeParameter b) => a != (string)b;

    private bool EqualToVector3(NodeParameter a, NodeParameter b) => a == (Vector3)b;
    private bool NotEqualToVector3(NodeParameter a, NodeParameter b) => a != (Vector3)b;
    #endregion
}
