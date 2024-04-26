using UnityEngine;

public class Condition : BehaviourNode
{
    private Blackboard board;

    private delegate bool ConditionCheckDelegate(string a, NodeParameter b);

    private string[] checkNames;

    private ConditionCheckDelegate[] checkConditions;

    private NodeParameter[] checkValues;

    public Condition(BehaviourTree parentTree, NodeParameter[] parameters) : base(parentTree)
    {
        if (parameters == null)
            return;

        if (parameters[0] == (int)Consts.BlackboardSource.GLOBAL)
            board = parentTree.GlobalBlackboard;
        else
            board = parentTree.Owner.AgentBlackboard;

        checkNames = new string[parameters.Length-1];
        checkConditions = new ConditionCheckDelegate[parameters.Length-1];
        checkValues = new NodeParameter[parameters.Length-1];
        for(int i = 0; i < checkNames.Length; ++i)
        {
            string parsing = parameters[i+1];
            string[] split = parsing.Split(' ');
            if (split.Length != 3)
            {
                Debug.Log("Condition parsing does not have correct split number: " + split.Length);
                return;
            }
            checkNames[i] = split[0];
            checkValues[i] = ParseStringToValue(split[2]);
            checkConditions[i] = GetMatchingDelegate(checkValues[i].type, split[1]);
        }
    }

    public override Consts.NodeStatus Update()
    {
        Consts.NodeStatus conditionStatus = Consts.NodeStatus.SUCCESS;
        for (int i = 0; i < checkNames.Length; ++i)
        {
            if (!checkConditions[i](checkNames[i], checkValues[i]))
            {
                conditionStatus = Consts.NodeStatus.FAILURE;
                break;
            }
        }
        return conditionStatus;
    }

    private NodeParameter ParseStringToValue(string newString)
    {
        if(int.TryParse(newString, out int newInt))
            return newInt;

        if (float.TryParse(newString, out float newFloat))
            return newFloat;

        if (bool.TryParse(newString, out bool newBool))
            return newBool;

        // If our value is not any of the above then
        // we check if it's actually a string or if it's
        // a blackboard key as well
        if (newString.StartsWith('"') && newString.EndsWith('"'))
            return newString.Trim('"');

        System.Type type = board.GetVariableType(newString);
        if (type == null)
            return newString;

        if (type.Name.Contains("Int"))
            return board.GetVariable<int>(newString);
        else if (type.Name.Contains("Single") || type.Name.Contains("Float"))
            return board.GetVariable<float>(newString);
        else if (type.Name.Contains("Bool"))
            return board.GetVariable<bool>(newString);
        else if (type.Name.Contains("String"))
            return board.GetVariable<string>(newString);

        Debug.LogError($"Condition failed value parsing on key: {newString}");
        return newString;
    }

    #region Condition Delegates
    private ConditionCheckDelegate GetMatchingDelegate(NodeParameter.ParamType valueType, string conditionOperator)
    {
        if (valueType == NodeParameter.ParamType.Int)
        {
            switch (conditionOperator)
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
            switch (conditionOperator)
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
            switch (conditionOperator)
            {
                case "==":
                return EqualToBool;

                case "!=":
                return NotEqualToBool;
            }
        }
        else if(valueType == NodeParameter.ParamType.String)
        {
            switch (conditionOperator)
            {
                case "==":
                return EqualToString;

                case "!=":
                return NotEqualToString;
            }
        }
        else if(valueType == NodeParameter.ParamType.Vector3)
        {
            switch (conditionOperator)
            {
                case "==":
                return EqualToVector3;

                case "!=":
                return NotEqualToVector3;
            }
        }
        return null;
    }

    private bool GreaterThanInt(string a, NodeParameter b) => board.GetVariable<int>(a) > b;
    private bool GreaterThanOrEqualInt(string a, NodeParameter b) => board.GetVariable<int>(a) >= b;
    private bool LessThanInt(string a, NodeParameter b) => board.GetVariable<int>(a) < b;
    private bool LessThanOrEqualInt(string a, NodeParameter b) => board.GetVariable<int>(a) <= b;
    private bool EqualToInt(string a, NodeParameter b) => board.GetVariable<int>(a) == b;
    private bool NotEqualToInt(string a, NodeParameter b) => board.GetVariable<int>(a) != b;


    private bool GreaterThanFloat(string a, NodeParameter b) => board.GetVariable<float>(a) > b;
    private bool GreaterThanOrEqualFloat(string a, NodeParameter b) => board.GetVariable<float>(a) >= b;
    private bool LessThanFloat(string a, NodeParameter b) => board.GetVariable<float>(a) < b;
    private bool LessThanOrEqualFloat(string a, NodeParameter b) => board.GetVariable<float>(a) <= b;
    private bool EqualToFloat(string a, NodeParameter b) => board.GetVariable<float>(a) == b;
    private bool NotEqualToFloat(string a, NodeParameter b) => board.GetVariable<float>(a) != b;


    private bool EqualToBool(string a, NodeParameter b) => board.GetVariable<bool>(a) == b;
    private bool NotEqualToBool(string a, NodeParameter b) => board.GetVariable<bool>(a) != b;


    private bool EqualToString(string a, NodeParameter b) => board.GetVariable<string>(a) == b;
    private bool NotEqualToString(string a, NodeParameter b) => board.GetVariable<string>(a) != b;

    private bool EqualToVector3(string a, NodeParameter b) => board.GetVariable<Vector3>(a) == b;
    private bool NotEqualToVector3(string a, NodeParameter b) => board.GetVariable<Vector3>(a) != b;
    #endregion
}
