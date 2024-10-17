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
        currentTree.SetNodeMap(nodeMap);
        currentTree.SetGraph(graph);

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
                newNode = new Sequence(tree, dataNode.GetParameters());
            break;

            case Consts.BehaviourType.Selector:
                newNode = new Selector(tree, dataNode.GetParameters());
            break;

            case Consts.BehaviourType.Repeat:
                newNode = new Repeat(tree, dataNode.GetParameters());
            break;

            case Consts.BehaviourType.Invert:
                newNode = new Invert(tree);
            break;

            case Consts.BehaviourType.ForceSuccess:
                newNode = new ForceSuccess(tree);
            break;

            case Consts.BehaviourType.RandomChance:
                newNode = new RandomChance(tree, dataNode.GetParameters());
            break;

            case Consts.BehaviourType.Monitor:
                newNode = new Monitor(tree);
            break;

            case Consts.BehaviourType.SetVariable:
                newNode = new SetVariable(tree, dataNode.GetParameters());
            break;

            case Consts.BehaviourType.Wait:
                newNode = new Wait(tree, dataNode.GetParameters());
            break;

            case Consts.BehaviourType.Condition:
                newNode = new Condition(tree, dataNode.GetParameters());
            break;

            case Consts.BehaviourType.HasArrived:
                newNode = new HasArrived(tree, dataNode.GetParameters());
            break;

            case Consts.BehaviourType.MoveToPoint:
                newNode = new MoveToPoint(tree, dataNode.GetParameters());
            break;

            case Consts.BehaviourType.SetPointFromPatrol:
                newNode = new SetPointFromPatrol(tree, dataNode.GetParameters());
            break;

            case Consts.BehaviourType.Cooldown:
                newNode = new Cooldown(tree, dataNode.GetParameters());
            break;

            case Consts.BehaviourType.StoreVisibleInterests:
                newNode = new StoreVisibleInterests(tree, dataNode.GetParameters());
            break;

            case Consts.BehaviourType.SetRandomInterest:
                newNode = new SetRandomInterest(tree, dataNode.GetParameters());
            break;

            case Consts.BehaviourType.SetPointFromGameObject:
                newNode = new SetPointFromGameObject(tree, dataNode.GetParameters());
            break;

            case Consts.BehaviourType.StopMoving:
                newNode = new StopMoving(tree);
            break;

            case Consts.BehaviourType.TurnHead:
                newNode = new TurnHead(tree, dataNode.GetParameters());
            break;

            case Consts.BehaviourType.IsTurningHead:
                newNode = new IsTurningHead(tree);
            break;

            case Consts.BehaviourType.HasLineOfSight:
                newNode = new HasLoS(tree, dataNode.GetParameters());
            break;

            case Consts.BehaviourType.CallMethod:
                newNode = new CallMethod(tree, dataNode.GetParameters());
            break;

            case Consts.BehaviourType.SetDistanceFromPoint:
                newNode = new SetDistanceFromPoint(tree, dataNode.GetParameters());
            break;

            case Consts.BehaviourType.CopyVariablesToSelf:
                newNode = new CopyVariables(tree, dataNode.GetParameters());
            break;

            case Consts.BehaviourType.IsThiefHeard:
                newNode = new IsThiefHeard(tree);
            break;

            case Consts.BehaviourType.CanAttackThief:
                newNode = new CanAttackThief(tree);
            break;

            case Consts.BehaviourType.AttackThief:
                newNode = new AttackThief(tree);
            break;

            case Consts.BehaviourType.TurnBody:
                newNode = new TurnBody(tree, dataNode.GetParameters());
            break;
        }
        newNode.Name = dataNode.name;
        return newNode;
    }
}