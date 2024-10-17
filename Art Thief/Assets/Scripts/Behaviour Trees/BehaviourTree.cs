﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviourTree
{
    public GuardAgent Owner { get; private set; }

    public Blackboard GlobalBlackboard { get; private set; }

    protected BehaviourNode rootNode;

    public BehaviourNode RootNode => rootNode;

    public Dictionary<BTGraphNode, BehaviourNode> NodeMap { get; private set; }

    public BTGraph GraphSource { get; private set; }

    public delegate void NodeDelegate(BehaviourNode node);

    public event NodeDelegate NodeRanEvent;

    protected Stack<BehaviourNode> runningStack;

    protected List<BehaviourNode> monitoringList;

    public BehaviourTree(Agent newOwner)
    {
        Owner = (GuardAgent)newOwner;
        GlobalBlackboard = GameController.Instance.GlobalBlackboard;
        runningStack = new Stack<BehaviourNode>();
        monitoringList = new List<BehaviourNode>();
    }

    public void SetAgent(Agent newAgent) => Owner = (GuardAgent)newAgent;

    public void SetRoot(BehaviourNode newRoot) => rootNode = newRoot;

    public void SetNodeMap(Dictionary<BTGraphNode, BehaviourNode> map) => NodeMap = map;

    public void SetGraph(BTGraph graphSource) => GraphSource = graphSource;

    public void Update()
    {
        if(monitoringList.Count > 0)
        {
            for(int i = 0; i < monitoringList.Count; ++i)
            {
                if (monitoringList[i].Tick() == Consts.NodeStatus.FAILURE)
                    break;
            }
        }

        while(runningStack.Count > 0)
        {
            BehaviourNode currentRunning = runningStack.Peek();

            Debug.Assert(currentRunning.Status == Consts.NodeStatus.RUNNING, "Top of running stack does not have a status of RUNNING");

            Consts.NodeStatus currentNewStatus = currentRunning.Tick();
            if (currentNewStatus == Consts.NodeStatus.RUNNING)
                return;
        }

        rootNode.Tick();
    }

    public void PushRunningNode(BehaviourNode newNode)
    {
        runningStack.Push(newNode);
        NodeRanEvent?.Invoke(newNode);
    }

    public void PopRunningNode() => runningStack.Pop();

    public void RegisterMonitoringNode(BehaviourNode newNode) => monitoringList.Add(newNode);

    public void DeregisterMonitoringNode(BehaviourNode oldNode) => monitoringList.Remove(oldNode);

    public void Interrupt(BehaviourNode interruptSource)
    {
        while(runningStack.Count > 0)
        {
            BehaviourNode current = runningStack.Peek();

            if (current == interruptSource)
                break;

            current.OnExit();
        }
    }

    public BehaviourNode PeekRunningStack() => runningStack.Count > 0 ? runningStack.Peek() : null;
}