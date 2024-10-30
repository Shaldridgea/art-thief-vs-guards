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

    public void SetVariable<T>(string variableName, T newValue)
    {
        variablesDict[variableName] = newValue;
    }

    public T GetVariable<T>(string variableName)
    {
        if (variablesDict.TryGetValue(variableName, out object val))
            return (T)val;
        else
            return default;
    }

    public Dictionary<string, object> GetData() => variablesDict;
}