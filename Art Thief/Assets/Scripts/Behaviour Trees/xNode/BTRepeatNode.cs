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
}