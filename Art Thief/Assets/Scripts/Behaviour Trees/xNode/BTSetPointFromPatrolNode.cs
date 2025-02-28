﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using XNode;

[CreateNodeMenu("Variables/Set Next Point From Patrol")]
public class BTSetPointFromPatrolNode : BTActionNode
{
    [SerializeField]
    private Consts.PatrolPathType pathType;

    [SerializeField]
    private Consts.PatrolGetType pointType;

    [SerializeField]
    private string variableName;

    [SerializeField]
    [ShowIf("isRoom")]
    [AllowNesting]
    private string roomPointKey;

    private bool isRoom => pathType == Consts.PatrolPathType.Room;
    
    protected override void Init()
    {
        type = Consts.BehaviourType.SetPointFromPatrol;
        base.Init();
    }

    public override NodeParameter[] GetParameters()
    {
        return new NodeParameter[] { variableName, (int)pathType, (int)pointType, roomPointKey };
    }

    public override string GetNodeDetailsText()
    {
        string details;
        details = "Path type: " + pathType.ToString() + ", Point type: " + pointType.ToString();
        details += ", Variable key: " + variableName;
        return details;
    }
}