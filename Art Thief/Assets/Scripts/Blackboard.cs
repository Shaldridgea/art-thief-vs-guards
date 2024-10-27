using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Container for a Dictionary of string-object pairs to generically store values by name
/// </summary>
public class Blackboard
{
    private Dictionary<string, object> variablesDict = new Dictionary<string, object>();

    private Dictionary<string, Type> typeDict = new Dictionary<string, Type>();

    public void SetVariable<T>(string variableName, T newValue)
    {
        variablesDict[variableName] = newValue;

        Type type = newValue.GetType();
        typeDict[variableName] = type;
    }

    public T GetVariable<T>(string variableName)
    {
        if (variablesDict.TryGetValue(variableName, out object val))
            return (T)val;
        else
            return default;
    }

    public object GetVariableGeneric(string variableName)
    {
        if (variablesDict.TryGetValue(variableName, out object val))
            return val;
        else
            return default;
    }

    public Type GetVariableType(string variableName)
    {
        if (typeDict.TryGetValue(variableName, out Type val))
            return val;
        else
            return null;
    }

    public Dictionary<string, object> GetData() => variablesDict;
}