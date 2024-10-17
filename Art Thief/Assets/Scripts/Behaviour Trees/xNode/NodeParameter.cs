using UnityEngine;
using NaughtyAttributes;
using XNode;

[System.Serializable]
public class NodeParameter
{
    public enum ParamType
    {
        Int,
        Float,
        Bool,
        String,
        Vector3
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
        type = ParamType.Float;
    }

    public NodeParameter(bool newBool)
    {
        boolValue = newBool;
        type = ParamType.Bool;
    }

    public NodeParameter(string newString)
    {
        stringValue = newString;
        type = ParamType.String;
    }

    public NodeParameter(Vector3 newVector)
    {
        vector3Value = newVector;
        type = ParamType.Vector3;
    }

    public override string ToString()
    {
        switch (type)
        {
            case ParamType.Int:
                return intValue.ToString();
            case ParamType.Float:
                return floatValue.ToString();
            case ParamType.Bool:
                return boolValue.ToString();
            case ParamType.String:
                return stringValue;
            case ParamType.Vector3:
                return vector3Value.ToString();
        }
        return "NodeParameter";
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
    [SerializeField]
    [ShowIf("IsVector3")]
    [Label("Value")]
    [AllowNesting]
    private Vector3 vector3Value;

    public static implicit operator int(NodeParameter p) => p.intValue;
    public static implicit operator float(NodeParameter p) => p.floatValue;
    public static implicit operator bool(NodeParameter p) => p.boolValue;
    public static implicit operator string(NodeParameter p) => p.stringValue;
    public static implicit operator Vector3(NodeParameter p) => p.vector3Value;

    public static implicit operator NodeParameter(int i) => new NodeParameter(i);
    public static implicit operator NodeParameter(float f) => new NodeParameter(f);
    public static implicit operator NodeParameter(bool b) => new NodeParameter(b);
    public static implicit operator NodeParameter(string s) => new NodeParameter(s);
    public static implicit operator NodeParameter(Vector3 v) => new NodeParameter(v);

    private bool IsInt => type == ParamType.Int;
    private bool IsFloat => type == ParamType.Float;
    private bool IsBool => type == ParamType.Bool;
    private bool IsString => type == ParamType.String;
    private bool IsVector3 => type == ParamType.Vector3;
}