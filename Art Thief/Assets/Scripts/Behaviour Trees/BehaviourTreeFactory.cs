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
        Dictionary<BTGraphNode, BehaviourNode> nodeMap = new Dictionary<BTGraphNode, BehaviourNode>();

        BTGraphNode node = (BTGraphNode)nodes.Find(n => n.GetInputPort("parentNode").ConnectionCount == 0);
        currentTree.SetRoot(ProcessNode(node, currentTree, nodeStack, nodeMap));

        if(nodeStack.Count != 0)
            Debug.Log("Stack wasn't processed correctly");
        Debug.Log("Processed nodes: " + processedNodes);
        Debug.Log("Nodes length: " + nodes.Count);
        processedNodes = 0;
        return currentTree;
    }

    private static BehaviourNode ProcessNode(BTGraphNode nextNode, BehaviourTree tree, Stack<BehaviourNode> stack,
    Dictionary<BTGraphNode, BehaviourNode> map, NodePort outputSource = null)
    {
        // Check if we already have a BehaviourNode instance that
        // maps to this graph node, otherwise make a new one
        BehaviourNode thisNode;
        bool alreadyCached = false;
        if (map.TryGetValue(nextNode, out BehaviourNode cachedNode))
        {
            thisNode = cachedNode;
            alreadyCached = true;
        }
        else
        {
            thisNode = CreateBehaviourInstance(nextNode, tree);
            map.Add(nextNode, thisNode);
            ++processedNodes;
        }

        Debug.Assert(thisNode != null, $"BehaviourTreeFactory node instance wasn't found: {nextNode.BehaviourType}");

        if (stack.Count > 0)
            stack.Peek().AddChild(thisNode, outputSource.fieldName);

        if (!nextNode.IsLeaf && !alreadyCached)
        {
            stack.Push(thisNode);
            foreach (NodePort n in nextNode.Outputs)
            {
                if (n.ConnectionCount == 0)
                    continue;

                ProcessNode((BTGraphNode)n.Connection.node, tree, stack, map, n);
            }
            stack.Pop();
        }
        return thisNode;
    }

    private static BehaviourNode CreateBehaviourInstance(BTGraphNode dataNode, BehaviourTree tree)
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
                return new HasArrived(tree, dataNode.GetParameters());

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

            case Consts.BehaviourType.TurnHead:
                return new TurnHead(tree, dataNode.GetParameters());

            case Consts.BehaviourType.IsTurningHead:
                return new IsTurningHead(tree);
        }
        return newNode;
    }
}