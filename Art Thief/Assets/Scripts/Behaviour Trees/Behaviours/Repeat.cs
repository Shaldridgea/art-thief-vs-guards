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
                // If we're running then it's either because our child node is running
                // and doing something that we were waiting on,
                // OR because our child node didn't give the result we wanted, so we're
                // still checking till it does
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
            while(repeatCounter < repeatNumber)
            {
                if (Status == Consts.NodeStatus.RUNNING)
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
    }
}
