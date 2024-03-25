using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blackboard
{
    private int setFrameCount = 0;

    private HashSet<string> lockedVariables = new HashSet<string>();

    private Dictionary<string, object> variablesDict = new Dictionary<string, object>();

    public void SetVariable<T>(string variableName, T newValue)
    {
        // If it's been more than a frame since we last set a value
        if (setFrameCount < Time.frameCount)
            lockedVariables.Clear(); // Clear the locked variables hash set

        // If a variable is not locked
        if (!lockedVariables.Contains(variableName))
        {
            variablesDict[variableName] = newValue;
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

    public Dictionary<string, object> GetData() => variablesDict;
}