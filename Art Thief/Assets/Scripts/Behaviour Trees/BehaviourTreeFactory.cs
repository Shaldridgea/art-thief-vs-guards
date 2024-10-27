using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

public static class BehaviourTreeFactory
{
    public static BehaviourTree MakeTree(BTGraph graph, Agent owner)
    {
        Stack<BehaviourNode> nodeStack = new Stack<BehaviourNode>();
        BehaviourTree currentTree = new BehaviourTree(owner);
        var nodes = graph.nodes;
        Dictionary<BTGraphNode, BehaviourNode> nodeMap = new Dictionary<BTGraphNode, BehaviourNode>();

        // Find the root node, which is the only node with no input connections
        BTGraphNode rootNode = (BTGraphNode)nodes.Find(n => n.GetInputPort("parentNode").ConnectionCount == 0);
        currentTree.SetRoot(ProcessNode(rootNode, currentTree, nodeStack, nodeMap));
        currentTree.SetNodeMap(nodeMap);
        currentTree.SetGraph(graph);

        if(nodeStack.Count != 0)
            Debug.Log("Stack wasn't processed correctly");
        Debug.Log("Nodes length: " + nodes.Count);
        return currentTree;
    }

    private static BehaviourNode ProcessNode(BTGraphNode nextNode, BehaviourTree tree, Stack<BehaviourNode> stack,
    Dictionary<BTGraphNode, BehaviourNode> map, NodePort outputSource = null)
    {
        // Check if this source node already has its
        // BehaviourNode cached, otherwise make a new one
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
        }

        Debug.Assert(thisNode != null, $"BehaviourTreeFactory node instance wasn't found: {nextNode.BehaviourType}");

        // Add children nodes to their parents
        if (stack.Count > 0)
            stack.Peek().AddChild(thisNode, outputSource.fieldName);

        // Recurse through output connections
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
        return dataNode.BehaviourType switch
        {
            Consts.BehaviourType.Sequence => new Sequence(tree, dataNode.GetParameters()),
            Consts.BehaviourType.Selector => new Selector(tree, dataNode.GetParameters()),
            Consts.BehaviourType.Repeat => new Repeat(tree, dataNode.GetParameters()),
            Consts.BehaviourType.Invert => new Invert(tree),
            Consts.BehaviourType.ForceSuccess => new ForceSuccess(tree),
            Consts.BehaviourType.RandomChance => new RandomChance(tree, dataNode.GetParameters()),
            Consts.BehaviourType.Monitor => new Monitor(tree),
            Consts.BehaviourType.SetVariable => new SetVariable(tree, dataNode.GetParameters()),
            Consts.BehaviourType.Wait => new Wait(tree, dataNode.GetParameters()),
            Consts.BehaviourType.Condition => new Condition(tree, dataNode.GetParameters()),
            Consts.BehaviourType.HasArrived => new HasArrived(tree, dataNode.GetParameters()),
            Consts.BehaviourType.MoveToPoint => new MoveToPoint(tree, dataNode.GetParameters()),
            Consts.BehaviourType.SetPointFromPatrol => new SetPointFromPatrol(tree, dataNode.GetParameters()),
            Consts.BehaviourType.Cooldown => new Cooldown(tree, dataNode.GetParameters()),
            Consts.BehaviourType.StoreVisibleInterests => new StoreVisibleInterests(tree, dataNode.GetParameters()),
            Consts.BehaviourType.SetRandomInterest => new SetRandomInterest(tree, dataNode.GetParameters()),
            Consts.BehaviourType.SetPointFromGameObject => new SetPointFromGameObject(tree, dataNode.GetParameters()),
            Consts.BehaviourType.StopMoving => new StopMoving(tree),
            Consts.BehaviourType.TurnHead => new TurnHead(tree, dataNode.GetParameters()),
            Consts.BehaviourType.IsTurningHead => new IsTurningHead(tree),
            Consts.BehaviourType.HasLineOfSight => new HasLoS(tree, dataNode.GetParameters()),
            Consts.BehaviourType.CallMethod => new CallMethod(tree, dataNode.GetParameters()),
            Consts.BehaviourType.SetDistanceFromPoint => new SetDistanceFromPoint(tree, dataNode.GetParameters()),
            Consts.BehaviourType.CopyVariablesToSelf => new CopyVariables(tree, dataNode.GetParameters()),
            Consts.BehaviourType.IsThiefHeard => new IsThiefHeard(tree),
            Consts.BehaviourType.CanAttackThief => new CanAttackThief(tree),
            Consts.BehaviourType.AttackThief => new AttackThief(tree),
            Consts.BehaviourType.TurnBody => new TurnBody(tree, dataNode.GetParameters()),
            _ => null,
        };
    }
}