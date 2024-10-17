using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;

public class BTGridBasedGraph : MonoBehaviour
{
    [SerializeField]
    private RectTransform graphContent;

    [SerializeField]
    private Vector2Int snapSize = new Vector2Int(150, 50);

    [SerializeField]
    private Vector2 cellSize = new Vector2(200, 100);

    public Vector2 CellSize => cellSize;

    [SerializeField]
    private Vector2 spacing = new Vector2(20, 5);

    public Vector2 Spacing => spacing;

    [SerializeField]
    private RectOffset padding;

    public RectOffset Padding => padding;

    public void CreateGraph(BTRuntimeView viewOwner)
    {
        Rect gridRect = GetDefaultGridRect();

        ProcessNodes(viewOwner, ref gridRect);

        gridRect = OptimiseGrid(viewOwner, gridRect);
        UpdateGraphVisuals(viewOwner, gridRect);
        Debug.Log(gridRect);
    }
    
    private void ProcessNodes(BTRuntimeView viewOwner, ref Rect gridRect)
    {
        Dictionary<Vector2, Guid> posGuidMap = new();

        for (int i = 0; i < viewOwner.AllNodesList.Count; ++i)
        {
            Guid nodeGuid = viewOwner.AllNodesList[i];
            BTGraphNode sourceNode = viewOwner.GetDataNode(nodeGuid);

            Vector2 gridPos = new(Mathf.Ceil(sourceNode.position.x / snapSize.x), Mathf.Ceil(sourceNode.position.y / snapSize.y));

            // Set the outer extents of our grid if the node position changes it
            if (gridRect.xMin == float.MaxValue)
                gridRect.xMin = gridPos.x;

            if (gridRect.xMax == float.MaxValue)
                gridRect.xMax = gridPos.x;

            if (gridRect.yMin == float.MaxValue)
                gridRect.yMin = gridPos.y;

            if (gridRect.yMax == float.MaxValue)
                gridRect.yMax = gridPos.y;

            if (gridPos.y < gridRect.yMin)
                gridRect.yMin = gridPos.y;
            else if (gridPos.y > gridRect.yMax)
                gridRect.yMax = gridPos.y;

            if (gridPos.x < gridRect.xMin)
                gridRect.xMin = gridPos.x;
            else if (gridPos.x > gridRect.xMax)
                gridRect.xMax = gridPos.x;

#if UNITY_EDITOR
            if (posGuidMap.ContainsKey(gridPos))
            {
                BTGraphNode dataNode = viewOwner.GetDataNode(posGuidMap[gridPos]);

                Debug.Log($"Node clash: {sourceNode.name}, {sourceNode.position} -> " +
                    $"{gridPos} clashes with {dataNode.name}, {dataNode.position}");
            }
            else
#endif
            if(!posGuidMap.ContainsKey(gridPos))
                posGuidMap.Add(gridPos, nodeGuid);

            BTRuntimeViewNode viewNode = viewOwner.GetViewNode(nodeGuid);
            viewNode.SetGridPosition(gridPos);
        }
    }

    /// <summary>
    /// Evaluates the nodes in the grid and pushes them towards the middle to save unused space
    /// </summary>
    /// <returns>The updated smaller Rect grid</returns>
    private Rect OptimiseGrid(BTRuntimeView viewOwner, Rect gridRect)
    {
        // Use these to check and count which rows and colums nodes are occupying
        Dictionary<float, int> horizontalCountMap = new();
        Dictionary<float, int> verticalCountMap = new();

        // Lists of nodes on each side of the middle point
        List<BTRuntimeViewNode> topNodes = new();
        List<BTRuntimeViewNode> bottomNodes = new();
        List<BTRuntimeViewNode> leftNodes = new();
        List<BTRuntimeViewNode> rightNodes = new();

        Vector2 middlePoint = gridRect.position + gridRect.size / 2f;
        middlePoint.x = Mathf.Floor(middlePoint.x);
        middlePoint.y = Mathf.Floor(middlePoint.y);

        // Check all our nodes and put them in the correct lists and maps
        for (int i = 0; i < viewOwner.AllNodesList.Count; ++i)
        {
            Guid nodeGuid = viewOwner.AllNodesList[i];
            BTRuntimeViewNode viewNode = viewOwner.GetViewNode(nodeGuid);

            Vector2 gridPos = viewNode.GridPosition;
            if (!horizontalCountMap.TryAdd(gridPos.x, 1))
                horizontalCountMap[gridPos.x] += 1;

            if (!verticalCountMap.TryAdd(gridPos.y, 1))
                verticalCountMap[gridPos.y] += 1;

            if (gridPos.x < middlePoint.x)
                leftNodes.Add(viewNode);
            else if (gridPos.x > middlePoint.x)
                rightNodes.Add(viewNode);

            if (gridPos.y < middlePoint.y)
                topNodes.Add(viewNode);
            else if (gridPos.y > middlePoint.y)
                bottomNodes.Add(viewNode);
        }

        int verticalSort(BTRuntimeViewNode x, BTRuntimeViewNode y) => x.GridPosition.y.CompareTo(y.GridPosition.y);
        int horizontalSort(BTRuntimeViewNode x, BTRuntimeViewNode y) => x.GridPosition.x.CompareTo(y.GridPosition.x);

        // Sort our lists to go from closest to furthest from middle
        topNodes.Sort(verticalSort);
        topNodes.Reverse();
        bottomNodes.Sort(verticalSort);
        leftNodes.Sort(horizontalSort);
        leftNodes.Reverse();
        rightNodes.Sort(horizontalSort);

        // Local function to run on each list and move each node closer to the middle if possible
        void RemoveUnusedSpace(List<BTRuntimeViewNode> nodeList, bool isVertical, float moveSign)
        {
            var checkCountMap = isVertical ? verticalCountMap : horizontalCountMap;
            // Vectors can be accessed with 0 for the x index and 1 for the y index
            int vecIndex = isVertical ? 1 : 0;

            for (int i = 0; i < nodeList.Count; ++i)
            {
                var viewNode = nodeList[i];

                float posComponent = viewNode.GridPosition[vecIndex];
                int distanceToMiddle = (int)Mathf.Abs(middlePoint[vecIndex] - posComponent);

                // Check if we can move any closer to the middle without
                // potentially overlapping with an existing node
                // We also maintain a distance of at least a cell between
                // us and the next node to leave a gap
                float moveDistance = 0f;
                for (int distance = 1; distance <= distanceToMiddle; ++distance)
                {
                    moveDistance = Mathf.Max(0f, distance - 2);
                    if (checkCountMap.ContainsKey(posComponent + distance * moveSign))
                        break;
                }

                if (moveDistance > 0f)
                {
                    Vector2 moveVector = new();
                    moveVector[vecIndex] = moveDistance * moveSign;

                    // Check every node that is further away than the current one
                    // (as well as the current one)
                    for (int n = i; n < nodeList.Count; ++n)
                    {
                        var node = nodeList[n];

                        float checkComponent = node.GridPosition[vecIndex];

                        // Update our column/row count since we're moving this node
                        checkCountMap[checkComponent] -= 1;
                        if (checkCountMap[checkComponent] <= 0)
                            checkCountMap.Remove(checkComponent);

                        node.SetGridPosition(node.GridPosition + moveVector);

                        // Update our grid extents if this node used to be at the edges
                        if (isVertical)
                        {
                            if (gridRect.yMin == checkComponent)
                                gridRect.yMin = node.GridPosition[vecIndex];
                            else if (gridRect.yMax == checkComponent)
                                gridRect.yMax = node.GridPosition[vecIndex];
                        }
                        else
                        {
                            if (gridRect.xMin == checkComponent)
                                gridRect.xMin = node.GridPosition[vecIndex];
                            else if (gridRect.xMax == checkComponent)
                                gridRect.xMax = node.GridPosition[vecIndex];
                        }

                        checkComponent = node.GridPosition[vecIndex];

                        // Update our column/row count for the new position
                        if (!checkCountMap.TryAdd(checkComponent, 1))
                            checkCountMap[checkComponent] += 1;
                    }
                }
            }
        }

        RemoveUnusedSpace(topNodes, true, 1f);
        RemoveUnusedSpace(bottomNodes, true, -1f);
        RemoveUnusedSpace(leftNodes, false, 1f);
        RemoveUnusedSpace(rightNodes, false, -1f);

        return gridRect;
    }

    private void UpdateGraphVisuals(BTRuntimeView viewOwner, Rect gridRect)
    {
        gridRect.size += Vector2.one;
        for (int i = 0; i < viewOwner.AllNodesList.Count; ++i)
        {
            Guid nodeGuid = viewOwner.AllNodesList[i];
            BTRuntimeViewNode viewNode = viewOwner.GetViewNode(nodeGuid);

            // Calculate our grid position to the actual graph grid position in UI space,
            // adjusting our node position as if we start with the top left of the grid being 0,0
            Vector2 posDiff = viewNode.GridPosition - gridRect.position;
            Vector2 adjustedGridPosition = (posDiff - gridRect.size / 2f) * cellSize;
            // Set our position in UI space, adjusting for spacing,
            // keeping in mind that UI elements expand out from the middle
            (viewNode.transform as RectTransform).anchoredPosition = (adjustedGridPosition
                + posDiff * spacing
                - (gridRect.size * spacing / 2f))
                * new Vector2(1f, -1f); // Flip our y coords because Unity UI is y-up

            viewOwner.UpdateNodeView(viewNode);
        }

        // Calculate the size of the rect transform holding the grid view to encompass everything
        graphContent.sizeDelta =
            gridRect.size * cellSize
            + (gridRect.size - Vector2.one) * spacing
            + new Vector2(padding.horizontal, padding.vertical);
    }

    private Rect GetDefaultGridRect()
    {
        return new Rect
        {
            xMin = float.MaxValue,
            xMax = float.MaxValue,
            yMin = float.MaxValue,
            yMax = float.MaxValue
        };
    }
}
