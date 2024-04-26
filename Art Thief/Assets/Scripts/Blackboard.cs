﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blackboard
{
    private int setFrameCount = 0;

    private HashSet<string> lockedVariables = new HashSet<string>();

    private Dictionary<string, object> variablesDict = new Dictionary<string, object>();

    private Dictionary<string, Type> typeDict = new Dictionary<string, Type>();

    public void SetVariable<T>(string variableName, T newValue)
    {
        // If it's been more than a frame since we last set a value
        if (setFrameCount < Time.frameCount)
            lockedVariables.Clear(); // Clear the locked variables hash set

        // If a variable is not locked
        if (!lockedVariables.Contains(variableName))
        {
            variablesDict[variableName] = newValue;
            Type type = newValue.GetType();
            typeDict[variableName] = type;
            // Lock this variable after setting so it can't be set more than once per frame
            lockedVariables.Add(variableName);
        }

        // Set the frame this variable was set on
        setFrameCount = Time.frameCount;
    }

    public T GetVariable<T>(string variableName)
    {
        if (variablesDict.TryGetValue(variableName, out object val))
            return (T)val;
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