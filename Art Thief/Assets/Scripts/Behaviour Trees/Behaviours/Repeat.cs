using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Repeat : Decorator
{
    private Consts.RepeatCondition repeatCondition;

    private Consts.NodeStatus desiredResult;

    private int repeatNumber;

    private int repeatCounter;

    private bool setRunningByChild;

    public Repeat(BehaviourTree parentTree, NodeParameter[] parameters) : base(parentTree)
    {
        repeatCondition = (Consts.RepeatCondition)(int)parameters[0];
        desiredResult = (Consts.NodeStatus)(int)parameters[1];
        repeatNumber = parameters[2];
    }

    public override Consts.NodeStatus Update()
    {
        Consts.NodeStatus status = Consts.NodeStatus.RUNNING;
        
        if(repeatCondition == Consts.RepeatCondition.UntilResult)
        {
            if (Status == Consts.NodeStatus.RUNNING)
            {
                // If we're running then it's either because our child node was running
                // OR because our child node didn't give the SUCCESS/FAILURE result we wanted
                if (childNode.Status != Consts.NodeStatus.RUNNING && setRunningByChild)
                    status = childNode.Status;
                else
                    status = childNode.Tick();
            }
            else
                status = childNode.Tick();

            if (status == desiredResult)
                return Consts.NodeStatus.SUCCESS;

            // If our status is going to be running because our child is running
            // while doing something, we need to know that to evaluate correctly
            setRunningByChild = status == Consts.NodeStatus.RUNNING;

            return Consts.NodeStatus.RUNNING;
        }
        else if(repeatCondition == Consts.RepeatCondition.NumberOfTimes)
        {
            status = Status;
            while(repeatCounter < repeatNumber)
            {
                if (status == Consts.NodeStatus.RUNNING)
                {
                    if (childNode.Status != Consts.NodeStatus.RUNNING)
                        status = childNode.Status;
                }
                else
                    status = childNode.Tick();

                if (status == Consts.NodeStatus.RUNNING)
                    break;

                ++repeatCounter;
            }
        }
        return status;
    }

    public override void Reset()
    {
        base.Reset();
        setRunningByChild = false;
        repeatCounter = 0;
    }

    public override string GetLiveVisualsText()
    {
        if (repeatCondition == Consts.RepeatCondition.UntilResult)
            return string.Empty;

        return $"Repeat count: {repeatCounter}";
    }
}
