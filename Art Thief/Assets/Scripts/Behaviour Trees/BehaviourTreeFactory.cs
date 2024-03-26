using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

public static class BehaviourTreeFactory
{
    private static int processedNodes = 0;

    public static BehaviourTree MakeTree(BTGraph graph, Agent owner)
    {
        Stack<BehaviourNode> nodeStack = new Stack<BehaviourNode>();
        BehaviourTree currentTree = new BehaviourTree(owner);
        var nodes = graph.nodes;
        BTGraphNode node = (BTGraphNode)nodes.Find(n => n.GetInputPort("parentNode").ConnectionCount == 0);
        currentTree.SetRoot(ProcessNode(node, currentTree, nodeStack));

        if(nodeStack.Count != 0)
            Debug.Log("Stack wasn't processed correctly");
        Debug.Log("Processed nodes: " + processedNodes);
        Debug.Log("Nodes length: " + nodes.Count);
        processedNodes = 0;
        return currentTree;
    }

    private static BehaviourNode ProcessNode(BTGraphNode nextNode, BehaviourTree tree, Stack<BehaviourNode> stack)
    {
        BehaviourNode thisNode = GetBehaviourInstance(nextNode, tree);

        Debug.Assert(thisNode != null, $"BehaviourTreeFactory node instance wasn't found: {nextNode.BehaviourType}");

        if (stack.Count > 0)
            stack.Peek().AddChild(thisNode);

        if (!nextNode.IsLeaf)
        {
            stack.Push(thisNode);
            foreach (NodePort n in nextNode.Outputs)
            {
                if (n.ConnectionCount == 0)
                    continue;

                ProcessNode((BTGraphNode)n.Connection.node, tree, stack);
            }
            stack.Pop();
        }
        ++processedNodes;
        return thisNode;
    }

    private static BehaviourNode GetBehaviourInstance(BTGraphNode dataNode, BehaviourTree tree)
    {
        BehaviourNode newNode = null;
        switch (dataNode.BehaviourType)
        {
            case Consts.BehaviourType.Sequence:
                return new Sequence(tree, dataNode.GetParameters());

            case Consts.BehaviourType.Selector:
                return new Selector(tree, dataNode.GetParameters());

            case Consts.BehaviourType.Repeat:
                return new Repeat(tree, dataNode.GetParameters());

            case Consts.BehaviourType.Invert:
                return new Invert(tree);

            case Consts.BehaviourType.ForceSuccess:
                return new ForceSuccess(tree);

            case Consts.BehaviourType.RandomChance:
                return new RandomChance(tree, dataNode.GetParameters());

            case Consts.BehaviourType.Monitor:
                return new Monitor(tree);

            case Consts.BehaviourType.SetVariable:
                return new SetVariable(tree, dataNode.GetParameters());

            case Consts.BehaviourType.Wait:
                return new Wait(tree, dataNode.GetParameters());

            case Consts.BehaviourType.Condition:
                return new Condition(tree, dataNode.GetParameters());

            case Consts.BehaviourType.HasArrived:
                return new HasArrived(tree, null);

            case Consts.BehaviourType.MoveToPoint:
                return new MoveToPoint(tree, dataNode.GetParameters());

            case Consts.BehaviourType.SetPointFromPatrol:
                return new SetPointFromPatrol(tree, dataNode.GetParameters());

            case Consts.BehaviourType.Cooldown:
                return new Cooldown(tree, dataNode.GetParameters());

            case Consts.BehaviourType.StoreVisibleInterests:
                return new StoreVisibleInterests(tree, dataNode.GetParameters());

            case Consts.BehaviourType.SetRandomInterest:
                return new SetRandomInterest(tree, dataNode.GetParameters());

            case Consts.BehaviourType.SetPointFromGameObject:
                return new SetPointFromGameObject(tree, dataNode.GetParameters());

            case Consts.BehaviourType.StopMoving:
                return new StopMoving(tree);
        }
        return newNode;
    }
}