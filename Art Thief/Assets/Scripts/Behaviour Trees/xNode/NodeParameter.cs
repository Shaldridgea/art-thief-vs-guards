using UnityEngine;
using NaughtyAttributes;
using XNode;

[System.Serializable]
public class NodeParameter
{
    public enum ParamType
    {
        INT,
        FLOAT,
        BOOL,
        STRING
    }

    public NodeParameter()
    {

    }

    public NodeParameter(int newInt)
    {
        intValue = newInt;
    }

    public NodeParameter(float newFloat)
    {
        floatValue = newFloat;
        type = ParamType.FLOAT;
    }

    public NodeParameter(bool newBool)
    {
        boolValue = newBool;
        type = ParamType.BOOL;
    }

    public NodeParameter(string newString)
    {
        stringValue = newString;
        type = ParamType.STRING;
    }

    [NodeEnum]
    public ParamType type;
    [SerializeField]
    [ShowIf("IsInt")]
    [Label("Value")]
    [AllowNesting]
    private int intValue;
    [SerializeField]
    [ShowIf("IsFloat")]
    [Label("Value")]
    [AllowNesting]
    private float floatValue;
    [SerializeField]
    [ShowIf("IsBool")]
    [Label("Value")]
    [AllowNesting]
    private bool boolValue;
    [SerializeField]
    [ShowIf("IsString")]
    [Label("Value")]
    [AllowNesting]
    private string stringValue;

    public static implicit operator int(NodeParameter p) => p.intValue;
    public static implicit operator float(NodeParameter p) => p.floatValue;
    public static implicit operator bool(NodeParameter p) => p.boolValue;
    public static implicit operator string(NodeParameter p) => p.stringValue;

    public static implicit operator NodeParameter(int i) => new NodeParameter(i);
    public static implicit operator NodeParameter(float f) => new NodeParameter(f);
    public static implicit operator NodeParameter(bool b) => new NodeParameter(b);
    public static implicit operator NodeParameter(string s) => new NodeParameter(s);

    private bool IsInt => type == ParamType.INT;
    private bool IsFloat => type == ParamType.FLOAT;
    private bool IsBool => type == ParamType.BOOL;
    private bool IsString => type == ParamType.STRING;
}