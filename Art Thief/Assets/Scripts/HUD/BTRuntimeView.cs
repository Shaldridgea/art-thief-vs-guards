using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BTRuntimeView : MonoBehaviour
{
    public class NodeModel
    {
        public readonly Guid Guid;

        private readonly BTGraphNode dataNode;

        private readonly BehaviourNode logicNode;

        private BTRuntimeViewNode viewNode;

        public NodeModel(BTGraphNode newData, BehaviourNode newLogic, BTRuntimeViewNode newView)
        {
            Guid = Guid.NewGuid();

            dataNode = newData;
            dataNode.SetGuid(Guid);

            logicNode = newLogic;
            logicNode.SetGuid(Guid);

            viewNode = newView;
            viewNode.SetGuid(Guid);
        }

        public BTGraphNode GetDataNode() => dataNode;

        public BehaviourNode GetLogicNode() => logicNode;

        public BTRuntimeViewNode GetViewNode() => viewNode;
    }

    enum TreeTraversal
    {
        Ascend,
        Descend
    }

    [SerializeField]
    private ZoomScrollRect zoomScrollRect;

    [SerializeField]
    private BTGridBasedGraph gridGraph;

    [SerializeField]
    private NodePoolLoader nodePoolLoader;

    private BehaviourTree tree;

    private Dictionary<Guid, NodeModel> nodeGuidMap = new();

    private List<Guid> nodeGuidList = new();

    public List<Guid> AllNodesList => nodeGuidList;

    private List<BehaviourNode> updatedNodesList = new();

    private List<Guid> liveUpdateNodesList = new();

    private bool resetUpdatedNodes;

    private BehaviourNode mostRecentRunningNode;

    private GameObjectPool<BTRuntimeViewNode> nodePool;

    public void SetTarget(GuardAgent guard)
    {
        if (guard.BehaviourTree == tree)
            return;

        if (nodePool == null)
            nodePool = nodePoolLoader.GetNodePool();

        // Clean up event listener from previous targets
        if (tree != null)
            tree.NodeRanEvent -= OnNodeRun;

        // Set our new target tree and listen to nodes it runs
        tree = guard.BehaviourTree;
        tree.NodeRanEvent += OnNodeRun;

        // Clean up previous nodes
        nodeGuidMap.Clear();
        updatedNodesList.Clear();
        liveUpdateNodesList.Clear();

        nodePool.ResetCounter();

        Debug.Log("Tree graph: " + tree.GraphSource.name);
        for (int i = 0; i < tree.GraphSource.nodes.Count; ++i)
        {
            BTGraphNode node = (BTGraphNode)tree.GraphSource.nodes[i];

            // Get our UI nodes and create our relational model of each node with a guid
            var viewNode = nodePool.GetFromPool(true);
            NodeModel nodeModel = new NodeModel(node, tree.NodeMap[node], viewNode);

            if (viewNode.transform.parent == null)
                viewNode.transform.SetParent(gridGraph.transform, false);

            nodeGuidMap.Add(nodeModel.Guid, nodeModel);
            nodeGuidList.Add(nodeModel.Guid);
        }
        nodePool.DeactivateUnused();

        gridGraph.CreateGraph(this);

        mostRecentRunningNode = tree.PeekRunningStack();
        zoomScrollRect.SetZoom(0.35f);
        FocusScrollRectView(mostRecentRunningNode.GetGuid());
    }

    public BTGraphNode GetDataNode(Guid g) => nodeGuidMap[g].GetDataNode();

    public BehaviourNode GetLogicNode(Guid g) => nodeGuidMap[g].GetLogicNode();

    public BTRuntimeViewNode GetViewNode(Guid g) => nodeGuidMap[g].GetViewNode();

    // Update is called once per frame
    void LateUpdate()
    {
        if (!resetUpdatedNodes && updatedNodesList.Count > 0)
        {
            BehaviourNode currentPeek = tree.PeekRunningStack();
            // Focus on the last updated node when our running stack changes
            if (mostRecentRunningNode != currentPeek)
            {
                zoomScrollRect.SetZoom(0.5f);

                FocusScrollRectView(updatedNodesList[^1].GetGuid());

                mostRecentRunningNode = currentPeek;
            }
        }

        // Update visuals of most recently updated nodes
        foreach (var logicNode in updatedNodesList)
        {
            var viewNode = nodeGuidMap[logicNode.GetGuid()].GetViewNode();
            viewNode.UpdateLiveNodeVisuals(updatedNodesList.Contains(logicNode), logicNode);
        }

        // Update visuals of nodes that have live updates like timers
        foreach (var guid in liveUpdateNodesList)
        {
            var viewNode = nodeGuidMap[guid].GetViewNode();
            var logicNode = nodeGuidMap[guid].GetLogicNode();
            viewNode.UpdateLiveNodeVisuals(updatedNodesList.Contains(logicNode), logicNode);
        }
        resetUpdatedNodes = true;
    }

    private void FocusScrollRectView(Guid focusNode)
    {
        var viewNodeTransform = nodeGuidMap[focusNode].GetViewNode().transform as RectTransform;
        // Set scroll rect's content position to center on the focused node
        zoomScrollRect.content.anchoredPosition =
                (-viewNodeTransform.anchoredPosition - gridGraph.CellSize * new Vector2(0.5f, -0.5f))
                    * zoomScrollRect.content.localScale;

        // Clamp the content position to the 0-1 bounds
        Vector2 normPos = zoomScrollRect.normalizedPosition;
        normPos = new Vector2(Mathf.Clamp(normPos.x, 0f, 1f), Mathf.Clamp(normPos.y, 0f, 1f));
        zoomScrollRect.normalizedPosition = normPos;
    }

    public void UpdateNodeView(BTRuntimeViewNode viewNode)
    {
        BTGraphNode dataNode = nodeGuidMap[viewNode.GetGuid()].GetDataNode();

        viewNode.gameObject.SetActive(true);

        var logicNode = nodeGuidMap[viewNode.GetGuid()].GetLogicNode();

        if (viewNode.InitNodeInfoVisuals(dataNode, logicNode))
            liveUpdateNodesList.Add(viewNode.GetGuid());

        viewNode.UpdateLiveNodeVisuals(false, logicNode);

        if (!viewNode.gameObject.activeSelf)
            viewNode.gameObject.SetActive(true);

        if (dataNode.IsLeaf)
        {
            viewNode.ResetLineConnectors();
            return;
        }

        int j = 0;
        // Set the line connectors from this node to its children
        foreach (var n in dataNode.Outputs)
        {
            if (n.ConnectionCount == 0)
                continue;

            BTGraphNode nextNode = n.Connection.node as BTGraphNode;

            var nextViewNode = nodeGuidMap[nextNode.GetGuid()].GetViewNode();
            viewNode.SetLineConnectorVisuals(j, nextViewNode, gridGraph.CellSize, gridGraph.Spacing);
            ++j;
        }
        viewNode.ResetLineConnectors(j);
    }

    private void OnNodeRun(BehaviourNode node)
    {
        if (!isActiveAndEnabled)
            return;

        if(resetUpdatedNodes)
        {
            resetUpdatedNodes = false;
            // Update last updated nodes visuals to reflect no longer being recent
            foreach (var logicNode in updatedNodesList)
            {
                var viewNode = nodeGuidMap[logicNode.GetGuid()].GetViewNode();
                viewNode.UpdateLiveNodeVisuals(false);
            }
            updatedNodesList.Clear();
        }
        updatedNodesList.Add(node);
    }

    private void OnDisable()
    {
        if(tree != null)
        {
            tree.NodeRanEvent -= OnNodeRun;
            tree = null;
        }
    }
}
