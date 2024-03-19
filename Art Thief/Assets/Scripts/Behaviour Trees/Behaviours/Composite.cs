using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public abstract class Composite : BehaviourNode
{
    protected List<BehaviourNode> ChildNodes { get; private set; }

    protected int nodeIndex = 0;

    protected Consts.NodeStatus exitStatus;

    protected bool shuffleChildren;

    public Composite(BehaviourTree parentTree, NodeParameter[] parameters) : base(parentTree)
    {
        ChildNodes = new List<BehaviourNode>();
        nodeIndex = 0;
        shuffleChildren = parameters[0];
    }

    public override void Reset()
    {
        base.Reset();
        nodeIndex = 0;
    }

    public override void AddChild(BehaviourNode node)
    {
        ChildNodes.Add(node);
    }

    public override void OnEnter()
    {
        base.OnEnter();
        if (nodeIndex == 0)
            if (shuffleChildren)
                ShuffleChildNodes();
    }

    public override Consts.NodeStatus Update()
    {
        Consts.NodeStatus status = Consts.NodeStatus.FAILURE;

        // If we WERE running and our current child has finished running
        // then exit if we've met our condition, or keep updating with the next child
        if (Status == Consts.NodeStatus.RUNNING)
            if (ChildNodes[nodeIndex].Status != Consts.NodeStatus.RUNNING)
            {
                status = ChildNodes[nodeIndex].Status;
                if (ChildNodes[nodeIndex].Status == exitStatus)
                    return status;
                else
                    nodeIndex += 1;
            }
            else
                return Consts.NodeStatus.RUNNING;
        
        for (int i = nodeIndex; i < ChildNodes.Count; ++i)
        {
            status = ChildNodes[i].Tick();
            if (status == exitStatus)
                break;

            if (status == Consts.NodeStatus.RUNNING)
            {
                nodeIndex = i;
                break;
            }
        }
        return status;
    }

    private void ShuffleChildNodes()
    {
        int n = ChildNodes.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            BehaviourNode value = ChildNodes[k];
            ChildNodes[k] = ChildNodes[n];
            ChildNodes[n] = value;
        }
    }
}