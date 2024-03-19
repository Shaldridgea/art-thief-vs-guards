using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotiveValue
{
    private string name;

    private ThiefAgent parent;

    private string blackboardKey;

    public float Value => parent.AgentBlackboard.GetVariable<float>(blackboardKey);

    public MotiveValue(Agent agentParent, MotiveData newData)
    {
        parent = (ThiefAgent)agentParent;

        name = newData.Motive;
        blackboardKey = newData.BlackboardKey;
    }
}