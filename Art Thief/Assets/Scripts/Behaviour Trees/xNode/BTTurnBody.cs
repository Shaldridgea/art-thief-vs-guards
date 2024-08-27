using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using NaughtyAttributes;

[CreateNodeMenu("Action/Turn Body")]
public class BTTurnBody : BTActionNode
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
    private float addAngle;

    [SerializeField]
    [AllowNesting]
    [HideIf("IsAngle")]
    private string pointKey;

    [SerializeField]
    private float time;

    protected override void Init()
    {
        type = Consts.BehaviourType.TurnBody;
        base.Init();
    }

    public override NodeParameter[] GetParameters()
    {
        return new NodeParameter[] { IsAngle, IsAngle ? addAngle : pointKey, time };
    }
}