using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using NaughtyAttributes;

[CreateNodeMenu("Action/Turn Head")]
public class BTTurnHead : BTActionNode
{
    enum TurnType{
        Angle,
        Point
    }

    [SerializeField]
    private TurnType turnType;

    private bool IsAngle => turnType == TurnType.Angle;

    [SerializeField]
    [AllowNesting]
    [ShowIf("IsAngle")]
    private float toAngle;

    [SerializeField]
    [AllowNesting]
    [HideIf("IsAngle")]
    private string pointKey;

    [SerializeField]
    private float time;

    protected override void Init()
    {
        type = Consts.BehaviourType.TurnHead;
        base.Init();
    }

    public override NodeParameter[] GetParameters()
    {
        return new NodeParameter[] { IsAngle, IsAngle ? toAngle : pointKey, time };
    }
}