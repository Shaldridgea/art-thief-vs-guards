using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Container for a Dictionary of string-object pairs to generically store values by name
/// </summary>
public class Blackboard
{
    private Dictionary<string, object> variablesDict = new();

    private Dictionary<string, float> lockTimerDict = new();

    public void SetVariable<T>(string variableName, T newValue)
    {
        if(lockTimerDict.ContainsKey(variableName))
        {
            float endTime = lockTimerDict[variableName];
            if (Time.time >= endTime)
                lockTimerDict.Remove(variableName);
            else
                return;
        }

        variablesDict[variableName] = newValue;
    }

    public T GetVariable<T>(string variableName)
    {
        if (variablesDict.TryGetValue(variableName, out object val))
            return (T)val;
        else
            return default;
    }

    public void LockVariable(string keyName, float lockTimer)
    {
        lockTimerDict[keyName] = Time.time + lockTimer;
    }

    public Dictionary<string, object> GetData() => variablesDict;
}