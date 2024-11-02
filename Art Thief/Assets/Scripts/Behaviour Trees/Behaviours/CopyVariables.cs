using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class CopyVariables : BehaviourNode
{
    private Blackboard board;

    private string copyKey;

    public CopyVariables(BehaviourTree parentTree, NodeParameter[] parameters) : base(parentTree)
    {
        board = GetTargetBlackboard(parameters[0]);
        copyKey = parameters[1];
    }

    public override Consts.NodeStatus Update()
    {
        Consts.NodeStatus status = Consts.NodeStatus.SUCCESS;

        string[] copyKeys = copyKey.Split(",");

        // Copy variable values from the target board to our own board with the same key
        for (int i = 0; i < copyKeys.Length; ++i)
        {
            string key = copyKeys[i].Trim();
            object gottenValue = board.GetVariable<object>(key);
            // Don't copy values that we don't have
            if (gottenValue == default)
                continue;

            ParentTree.Owner.AgentBlackboard.SetVariable(key, gottenValue);
        }

        return status;
    }
}