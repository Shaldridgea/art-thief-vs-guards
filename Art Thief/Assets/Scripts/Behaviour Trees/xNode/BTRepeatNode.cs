using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using NaughtyAttributes;

[CreateNodeMenu("Decorator/Repeat")]
public class BTRepeatNode : BTDecoratorNode
{
    enum RestrictedNodeStatus
    {
        SUCCESS,
        FAILURE
    }

    [Header("Repeat Condition")]
    [SerializeField]
    [AllowNesting]
    [Label("")]
    private Consts.RepeatCondition repeatCondition;

    [SerializeField]
    [ShowIf("IsResultCondition")]
    [AllowNesting]
    [Label("Match Result")]
    private RestrictedNodeStatus desiredResult;

    [SerializeField]
    [HideIf("IsResultCondition")]
    [AllowNesting]
    private int repeatTimes;

    private bool IsResultCondition => repeatCondition == Consts.RepeatCondition.UntilResult;

    protected override void Init()
    {
        type = Consts.BehaviourType.Repeat;
        base.Init();
    }

    public override NodeParameter[] GetParameters()
    {
        return new NodeParameter[] { (int)repeatCondition, (int)desiredResult, repeatTimes };
    }

    public override string GetNodeDetailsText()
    {
        string details;
        details = "Repeat type: " + repeatCondition.ToString();
        if (repeatCondition == Consts.RepeatCondition.UntilResult)
            details += ", Target result: " + desiredResult;
        else
            details += ", Repeat times: " + repeatTimes;
        return details;
    }
}