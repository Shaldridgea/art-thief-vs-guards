﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BehaviourNode : INodeGuid
{
    protected BehaviourTree ParentTree { get; private set; }

    public Consts.NodeStatus Status { get; private set; }

    private System.Guid guid;

    public BehaviourNode(BehaviourTree parentTree)
    {
        ParentTree = parentTree;
    }

    public Consts.NodeStatus Tick()
    {
        if (Status != Consts.NodeStatus.RUNNING)
            OnEnter();
        Status = Update();
        if (Status != Consts.NodeStatus.RUNNING)
            OnExit();
        return Status;
    }

    public virtual void OnEnter()
    {
        ParentTree.PushRunningNode(this);
    }

    public abstract Consts.NodeStatus Update();

    public virtual void OnExit()
    {
        ParentTree.PopRunningNode();
        Reset();
    }

    public virtual void Reset()
    {
        // Set our status to something if we have to reset
        // while still running (usually because of an interrupt)
        if (Status == Consts.NodeStatus.RUNNING)
            Status = Consts.NodeStatus.FAILURE;
    }

    public virtual void AddChild(BehaviourNode addNode, string portName = "") { }

    /// <summary>
    /// Helper function to handle changing board targets in variable statements by using an accessor operator
    /// </summary>
    protected bool HandleStatementAccessor(string statement, Blackboard sourceBoard,
        out Blackboard newTargetBoard, out string modifiedStatement)
    {
        newTargetBoard = null;
        modifiedStatement = null;

        if (statement.Contains('.'))
        {
            string[] splitResult = statement.Split('.');
            string left = splitResult[0];
            string right = splitResult[1];

            modifiedStatement = right;
            if(left.ToLower() == "self")
            {
                newTargetBoard = ParentTree.Owner.AgentBlackboard;
                return true;
            }

            if (left.ToLower() == "global")
            {
                newTargetBoard = ParentTree.GlobalBlackboard;
                return true;
            }

            // Check if our source board has the GameObject and associated Agent we expect it to have stored
            object retrievedValue = sourceBoard.GetVariable<object>(left);
            if (retrievedValue != default)
            {
                if (retrievedValue.GetType() == typeof(GameObject))
                {
                    GameObject target = retrievedValue as GameObject;
                    if (target != null)
                    {
                        if (target.TryGetComponent(out Agent targetAgent))
                        {
                            newTargetBoard = targetAgent.AgentBlackboard;
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    protected Blackboard GetTargetBlackboard(Consts.BlackboardSource source)
    {
        return source == Consts.BlackboardSource.AGENT ?
            ParentTree.Owner.AgentBlackboard : ParentTree.GlobalBlackboard;
    }

    protected Blackboard GetTargetBlackboard(NodeParameter source)
    {
        return ((Consts.BlackboardSource)(int)source) == Consts.BlackboardSource.AGENT ?
            ParentTree.Owner.AgentBlackboard : ParentTree.GlobalBlackboard;
    }

    /// <summary>
    /// Get a string of any live changes to the node's variables. Can be empty if it has no live changes
    /// </summary>
    public virtual string GetLiveVisualsText()
    {
        return string.Empty;
    }

    public void SetGuid(Guid newGuid) { guid = newGuid; }

    public Guid GetGuid() => guid;
}