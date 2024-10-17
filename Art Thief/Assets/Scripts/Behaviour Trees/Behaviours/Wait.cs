using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wait : BehaviourNode
{
    private float waitTime;

    private float waitStart;

    public Wait(BehaviourTree parentTree, NodeParameter[] parameters) : base(parentTree)
    {
        waitTime = parameters[0];
    }

    public override void OnEnter()
    {
        base.OnEnter();
        waitStart = Time.time;
    }

    public override Consts.NodeStatus Update()
    {
        Consts.NodeStatus status = Consts.NodeStatus.SUCCESS;
        float elapsedTime = Time.time - waitStart;
        if (elapsedTime < waitTime)
            status = Consts.NodeStatus.RUNNING;

        return status;
    }

    public override string GetLiveVisualsText()
    {
        float elapsedTime = Mathf.Clamp(Time.time - waitStart, 0f, waitTime);
        return $"Time: {elapsedTime:N2} sec(s)";
    }
}